using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nerven.Assertion;

namespace Nerven.Immutabler.Machine
{
    public sealed class ImmutablerTypeDefinitionGenerator
    {
        public TypeDefinition GenerateTypeDefinition(CompilationUnitSyntax rootNode)
        {
            var _inputNamespaceDeclaration = rootNode
                .ChildNodes()
                .Where(_syntaxNode => _syntaxNode is CSharpSyntaxNode)
                .OfType<NamespaceDeclarationSyntax>()
                .OneOrDefault();

            var _inputClassDeclaration = _inputNamespaceDeclaration?
                .ChildNodes()
                .Where(_syntaxNode => _syntaxNode is CSharpSyntaxNode)
                .OfType<ClassDeclarationSyntax>()
                .Where(_classDeclaration => _classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                .Where(_classDeclaration => _classDeclaration.HasLeadingTrivia)
                .OneOrDefault();

            var _immutablerDocumentationXml = _inputClassDeclaration?
                .GetLeadingTrivia()
                .Where(_trivia => _trivia.Kind() == SyntaxKind.SingleLineDocumentationCommentTrivia)
                .Select(_trivia => _trivia.GetStructure().ToString())
                .Select(_ParseDocumentationCommentXml)
                .Where(_xml => string.Equals(_xml?.Name.LocalName, "Immutabler", StringComparison.Ordinal))
                .OneOrDefault();
            
            if (_immutablerDocumentationXml == null)
            {
                return null;
            }

            return _CreateTypeDefinition(_inputClassDeclaration, rootNode);
        }

        private static XElement _ParseDocumentationCommentXml(string content)
        {
            try
            {
                return XElement.Parse(content);
            }
            catch (XmlException)
            {
                return null;
            }
        }
        
        private TypeDefinition _CreateTypeDefinition(ClassDeclarationSyntax inputClassDeclaration, CompilationUnitSyntax rootNode)
        {
            var _typeDefinition = TypeDefinition.Create(
                inputClassDeclaration.Identifier.ToString(),
                ((NamespaceDeclarationSyntax)inputClassDeclaration.Parent).Name.ToString());

            _typeDefinition = _typeDefinition
                .AddUsings(rootNode.Usings);

            var _typeValidationMethod = inputClassDeclaration.Members
                .Where(_member => _member.Kind() == SyntaxKind.MethodDeclaration)
                .Cast<MethodDeclarationSyntax>()
                .Where(_method => _method.Modifiers.Any(SyntaxKind.StaticKeyword))
                .Where(_method => _method.ParameterList.Parameters.Count == 1)
                .SingleOrDefault(_method => _method.Identifier.Text == NameHelper.TextToPublicMethodIdentifier("IsValid").Text);

            if (_typeValidationMethod != null)
            {
                var _valueParameter = _typeValidationMethod.ParameterList.Parameters[_typeValidationMethod.ParameterList.Parameters.Count - 1];

                Must.Assert(_valueParameter.Type.ToString() == _typeDefinition.Name);

                _typeDefinition = _typeDefinition
                    .WithValidateMethodName(SyntaxFactory.IdentifierName(_typeValidationMethod.Identifier));
            }

            _typeDefinition = _typeDefinition.AddProperties(_CreatePropertyDefinitions(inputClassDeclaration));
            return _typeDefinition;
        }

        private IEnumerable<PropertyDefinition> _CreatePropertyDefinitions(ClassDeclarationSyntax inputClassDeclaration)
        {
            return inputClassDeclaration.Members
                .Where(_member => _member.Kind() == SyntaxKind.PropertyDeclaration)
                .Cast<PropertyDeclarationSyntax>()
                .Where(_property => _property.AccessorList?.Accessors.Count == 1)
                .Where(_property => _property.AccessorList.Accessors.All(_accessor => _accessor.Body == null && _accessor.Keyword.Kind() == SyntaxKind.GetKeyword))
                .Select(_property => _CreatePropertyDefinition(_property, inputClassDeclaration));
        }

        private PropertyDefinition _CreatePropertyDefinition(PropertyDeclarationSyntax inputPropertyDeclaration, ClassDeclarationSyntax inputClassDeclaration)
        {
            var _propertyDefinition = PropertyDefinition.Create(inputPropertyDeclaration.Identifier.Text, inputPropertyDeclaration.Type.ToString());

            var _propertyValidationMethod = inputClassDeclaration.Members
                .Where(_member => _member.Kind() == SyntaxKind.MethodDeclaration)
                .Cast<MethodDeclarationSyntax>()
                .Where(_method => _method.Modifiers.Any(SyntaxKind.StaticKeyword))
                .Where(_method => _method.ParameterList.Parameters.Count == 1)
                .SingleOrDefault(_method => _method.Identifier.Text == NameHelper.TextToPublicMethodIdentifier("Is" + _propertyDefinition.Name + "Valid").Text);

            if (_propertyValidationMethod != null)
            {
                var _valueParameter = _propertyValidationMethod.ParameterList.Parameters[_propertyValidationMethod.ParameterList.Parameters.Count - 1];

                Must.Assert(_valueParameter.Type.ToString() == _propertyDefinition.Type);

                _propertyDefinition = _propertyDefinition
                    .WithValidateMethodName(SyntaxFactory.IdentifierName(_propertyValidationMethod.Identifier));
            }

            var _defaultProperty = inputClassDeclaration.Members
                .Where(_member => _member.Kind() == SyntaxKind.PropertyDeclaration)
                .Cast<PropertyDeclarationSyntax>()
                .Where(_p => _p.Modifiers.Any(SyntaxKind.StaticKeyword))
                .SingleOrDefault(_p => _p.Identifier.Text == NameHelper.TextToPublicPropertyIdentifier("Default" + _propertyDefinition.Name).Text);

            if (_defaultProperty != null)
            {
                Must.Assert(_defaultProperty.Type.ToString() == _propertyDefinition.Type);

                _propertyDefinition = _propertyDefinition.WithDefaultValuePropertyName(SyntaxFactory.IdentifierName(_defaultProperty.Identifier));
            }

            return _propertyDefinition;
        }
    }
}
