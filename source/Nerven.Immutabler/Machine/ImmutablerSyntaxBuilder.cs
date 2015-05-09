using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nerven.Assertion;

namespace Nerven.Immutabler.Machine
{
    public sealed class ImmutablerSyntaxBuilder
    {
        public CompilationUnitSyntax BuildSyntaxFromTypeDefinition(TypeDefinition typeDefinition)
        {
            Must
                .Assert(typeDefinition != null);

            var _outputClass = _CreateOutputClass(typeDefinition);

            var _outputDeclaration = SyntaxFactory
                .CompilationUnit()
                .AddUsings(typeDefinition.Usings.ToArray())
                .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(SyntaxFactory
                    .NamespaceDeclaration(SyntaxFactory.IdentifierName(typeDefinition.Namespace))
                    .AddMembers(_outputClass)));

            _outputDeclaration = _outputDeclaration
                .RemoveNodes(_outputDeclaration.DescendantNodes().OfType<EmptyStatementSyntax>(), SyntaxRemoveOptions.KeepUnbalancedDirectives)
                .NormalizeWhitespace()
                .WithTrailingTrivia(_outputDeclaration.GetTrailingTrivia().Append(SyntaxFactory.EndOfLine(string.Empty)));

            return _outputDeclaration;
        }
        
        private ClassDeclarationSyntax _CreateOutputClass(TypeDefinition typeDefinition)
        {
            return SyntaxFactory
                .ClassDeclaration(typeDefinition.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(_CreateOutputClassFields(typeDefinition))
                .AddMembers(_CreateOutputClassMainCtor(typeDefinition))
                .AddMembers(_CreateOutputClassProperties(typeDefinition))
                .AddMembers(_CreateOutputClassCreateMethod(typeDefinition))
                .AddMembers(_CreateOutputClassCreateWithMethods(typeDefinition))
                .AddMembers(_CreateOutputClassWithMethods(typeDefinition));
        }

        private MemberDeclarationSyntax[] _CreateOutputClassFields(TypeDefinition typeDefinition)
        {
            return typeDefinition.Properties.Select(_CreateOutputClassField).ToArray<MemberDeclarationSyntax>();
        }

        private FieldDeclarationSyntax _CreateOutputClassField(PropertyDefinition property)
        {
            return SyntaxFactory
                .FieldDeclaration(SyntaxFactory
                    .VariableDeclaration(
                        SyntaxFactory.IdentifierName(property.Type),
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(NameHelper.TextToPrivateFieldIdentifier(property.Name)))))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
        }

        private ConstructorDeclarationSyntax _CreateOutputClassMainCtor(TypeDefinition typeDefinition)
        {
            return SyntaxFactory
                .ConstructorDeclaration(SyntaxFactory.Identifier(typeDefinition.Name))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(typeDefinition.Properties
                    .Select(_property => SyntaxFactory
                        .Parameter(NameHelper.TextToMethodParameterIdentifier(_property.Name))
                        .WithType(SyntaxFactory.IdentifierName(_property.Type))))))
                .WithBody(SyntaxFactory
                    .Block(typeDefinition.Properties
                        .Select(_property => SyntaxFactory
                            .ExpressionStatement(SyntaxFactory
                                .AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.IdentifierName(NameHelper.TextToPrivateFieldIdentifier(_property.Name)),
                                    SyntaxFactory.IdentifierName(NameHelper.TextToMethodParameterIdentifier(_property.Name)))))));
        }

        private MemberDeclarationSyntax[] _CreateOutputClassProperties(TypeDefinition typeDefinition)
        {
            return typeDefinition.Properties.Select(_CreateOutputClassProperty).ToArray<MemberDeclarationSyntax>();
        }

        private PropertyDeclarationSyntax _CreateOutputClassProperty(PropertyDefinition property)
        {
            return SyntaxFactory
                .PropertyDeclaration(
                    SyntaxFactory.IdentifierName(property.Type),
                    NameHelper.TextToPublicPropertyIdentifier(property.Name))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, SyntaxFactory.Block(SyntaxFactory.ReturnStatement(NameHelper.TextToPrivateFieldName(property.Name)))));
        }

        private MethodDeclarationSyntax _CreateOutputClassCreateMethod(TypeDefinition typeDefinition)
        {
            var _propertyValidationStatements = _CreatePropertyValidationStatements(typeDefinition, _property => NameHelper.TextToMethodParameterName(_property.Name));
            var _typeCtorArguments = _CreatePropertyArgumentList(typeDefinition, _property => (_property.DefaultValuePropertyName ?? NameHelper.TextToMethodParameterName(_property.Name)));
            var _createTypeInstanceStatement = _CreateDeclareVariableAndCreateTypeInstanceStatement(typeDefinition, _typeCtorArguments, SyntaxFactory.Identifier("_instance"));
            var _typeValidationStatement = _CreateTypeValidationStatement(typeDefinition, SyntaxFactory.IdentifierName("_instance"));
            var _returnTypeInstanceStatement = SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("_instance"));

            return SyntaxFactory
                .MethodDeclaration(SyntaxFactory.IdentifierName(typeDefinition.Name), NameHelper.TextToPublicMethodIdentifier("Create"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(typeDefinition.Properties
                    .Where(_property => _property.DefaultValuePropertyName == null)
                    .Select(_property => SyntaxFactory
                        .Parameter(NameHelper.TextToMethodParameterIdentifier(_property.Name))
                        .WithType(SyntaxFactory.IdentifierName(_property.Type))))))
                .WithBody(SyntaxFactory.Block()
                    .AddStatements(_propertyValidationStatements)
                    .AddStatements(_createTypeInstanceStatement)
                    .AddStatements(_typeValidationStatement)
                    .AddStatements(_returnTypeInstanceStatement));
        }

        private StatementSyntax[] _CreatePropertyValidationStatements(TypeDefinition typeDefinition, Func<PropertyDefinition, ExpressionSyntax> getPropertyValue)
        {
            return typeDefinition
                .Properties
                .Where(_property => _property.DefaultValuePropertyName == null)
                .Select(_property => _CreatePropertyValidationStatement(_property, getPropertyValue))
                .ToArray();
        }

        private ArgumentListSyntax _CreatePropertyArgumentList(TypeDefinition typeDefinition, Func<PropertyDefinition, NameSyntax> selector)
        {
            return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(typeDefinition.Properties.Select(_property => SyntaxFactory.Argument(selector(_property)))));
        }

        private LocalDeclarationStatementSyntax _CreateDeclareVariableAndCreateTypeInstanceStatement(TypeDefinition typeDefinition, ArgumentListSyntax argumentList, SyntaxToken variableName)
        {
            var _variableDeclarator = SyntaxFactory.VariableDeclarator(variableName)
                .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory
                    .ObjectCreationExpression(SyntaxFactory.IdentifierName(typeDefinition.Name))
                    .WithArgumentList(argumentList)));
            
            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName("var"),
                    SyntaxFactory.SingletonSeparatedList(_variableDeclarator)));
        }

        private MemberDeclarationSyntax[] _CreateOutputClassCreateWithMethods(TypeDefinition typeDefinition)
        {
            if (typeDefinition.Properties.Any(_requiredProperty => _requiredProperty.DefaultValuePropertyName == null))
            {
                return new MemberDeclarationSyntax[0];
            }

            return typeDefinition.Properties
                .Select(_property => _CreateOutputClassCreateWithMethod(typeDefinition, _property))
                .ToArray<MemberDeclarationSyntax>();
        }

        private MethodDeclarationSyntax _CreateOutputClassCreateWithMethod(TypeDefinition typeDefinition, PropertyDefinition propertyDefinition)
        {
            var _validationStatement = _CreatePropertyValidationStatement(propertyDefinition, _property => NameHelper.TextToMethodParameterName(_property.Name));
            var _typeCtorArguments = _CreatePropertyArgumentList(
                typeDefinition,
                _property => (_property != propertyDefinition ? _property.DefaultValuePropertyName : NameHelper.TextToMethodParameterName(_property.Name)));
            var _createTypeInstanceStatement = _CreateDeclareVariableAndCreateTypeInstanceStatement(typeDefinition, _typeCtorArguments, SyntaxFactory.Identifier("_instance"));
            var _typeValidationStatement = _CreateTypeValidationStatement(typeDefinition, SyntaxFactory.IdentifierName("_instance"));
            var _returnTypeInstanceStatement = SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("_instance"));

            return SyntaxFactory
                .MethodDeclaration(
                    SyntaxFactory.IdentifierName(typeDefinition.Name),
                    NameHelper.TextToPublicMethodIdentifier("CreateWith" + propertyDefinition.Name))
                .AddParameterListParameters(SyntaxFactory
                    .Parameter(NameHelper.TextToMethodParameterIdentifier(propertyDefinition.Name))
                    .WithType(SyntaxFactory.IdentifierName(propertyDefinition.Type)))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .WithBody(SyntaxFactory.Block()
                    .AddStatements(_validationStatement)
                    .AddStatements(_createTypeInstanceStatement)
                    .AddStatements(_typeValidationStatement)
                    .AddStatements(_returnTypeInstanceStatement));
        }

        private MemberDeclarationSyntax[] _CreateOutputClassWithMethods(TypeDefinition typeDefinition)
        {
            return typeDefinition.Properties
                .Select(_property => _CreateOutputClassWithMethod(typeDefinition, _property))
                .ToArray<MemberDeclarationSyntax>();
        }

        private MethodDeclarationSyntax _CreateOutputClassWithMethod(TypeDefinition typeDefinition, PropertyDefinition propertyDefinition)
        {
            var _validationStatement = _CreatePropertyValidationStatement(propertyDefinition, _property => NameHelper.TextToMethodParameterName(_property.Name));
            var _typeCtorArguments = _CreatePropertyArgumentList(
                typeDefinition,
                _property => (_property.Name == propertyDefinition.Name ? NameHelper.TextToMethodParameterName(propertyDefinition.Name) : NameHelper.TextToPrivateFieldName(_property.Name)));
            var _createTypeInstanceStatement = _CreateDeclareVariableAndCreateTypeInstanceStatement(typeDefinition, _typeCtorArguments, SyntaxFactory.Identifier("_instance"));
            var _typeValidationStatement = _CreateTypeValidationStatement(typeDefinition, SyntaxFactory.IdentifierName("_instance"));
            var _returnTypeInstanceStatement = SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("_instance"));

            return SyntaxFactory
                .MethodDeclaration(
                    SyntaxFactory.IdentifierName(typeDefinition.Name),
                    NameHelper.TextToPublicMethodIdentifier("With" + propertyDefinition.Name))
                .AddParameterListParameters(SyntaxFactory
                    .Parameter(NameHelper.TextToMethodParameterIdentifier(propertyDefinition.Name))
                    .WithType(SyntaxFactory.IdentifierName(propertyDefinition.Type)))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block()
                    .AddStatements(_validationStatement)
                    .AddStatements(_createTypeInstanceStatement)
                    .AddStatements(_typeValidationStatement)
                    .AddStatements(_returnTypeInstanceStatement));
        }

        private StatementSyntax _CreateTypeValidationStatement(TypeDefinition type, ExpressionSyntax value)
        {
            if (type.ValidateMethodName == null)
            {
                return SyntaxFactory.EmptyStatement();
            }

            var _validationCall = SyntaxFactory
                .InvocationExpression(type.ValidateMethodName).AddArgumentListArguments(SyntaxFactory.Argument(value));

            return SyntaxFactory.IfStatement(
                SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, _validationCall),
                SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("System.ArgumentException")).AddArgumentListArguments()));
        }

        private StatementSyntax _CreatePropertyValidationStatement(PropertyDefinition property, Func<PropertyDefinition, ExpressionSyntax> getPropertyValue)
        {
            if (property.ValidateMethodName == null)
            {
                return SyntaxFactory.EmptyStatement();
            }

            var _validationCall = SyntaxFactory
                .InvocationExpression(property.ValidateMethodName).AddArgumentListArguments(SyntaxFactory.Argument(getPropertyValue(property)));

            return SyntaxFactory.IfStatement(
                SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, _validationCall),
                SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("System.ArgumentException")).AddArgumentListArguments()));
        }
    }
}
