using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nerven.Assertion;

namespace Nerven.Immutabler
{
    partial class TypeDefinition
    {
        private readonly string _Name;
        private readonly string _Namespace;
        private readonly ImmutableList<UsingDirectiveSyntax> _Usings;
        private readonly ImmutableList<PropertyDefinition> _Properties;
        private readonly NameSyntax _ValidateMethodName;
        private TypeDefinition(string @name, string @namespace, ImmutableList<UsingDirectiveSyntax> @usings, ImmutableList<PropertyDefinition> @properties, NameSyntax @validateMethodName)
        {
            _Name = @name;
            _Namespace = @namespace;
            _Usings = @usings;
            _Properties = @properties;
            _ValidateMethodName = @validateMethodName;
        }

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public string Namespace
        {
            get
            {
                return _Namespace;
            }
        }

        public ImmutableList<UsingDirectiveSyntax> Usings
        {
            get
            {
                return _Usings;
            }
        }

        public ImmutableList<PropertyDefinition> Properties
        {
            get
            {
                return _Properties;
            }
        }

        public NameSyntax ValidateMethodName
        {
            get
            {
                return _ValidateMethodName;
            }
        }

        public static TypeDefinition Create(string @name, string @namespace)
        {
            if (!IsNameValid(@name))
                throw new System.ArgumentException();
            if (!IsNamespaceValid(@namespace))
                throw new System.ArgumentException();
            var _instance = new TypeDefinition(@name, @namespace, DefaultUsings, DefaultProperties, DefaultValidateMethodName);
            if (!IsValid(_instance))
                throw new System.ArgumentException();
            return _instance;
        }

        public TypeDefinition WithName(string @name)
        {
            if (!IsNameValid(@name))
                throw new System.ArgumentException();
            var _instance = new TypeDefinition(@name, _Namespace, _Usings, _Properties, _ValidateMethodName);
            if (!IsValid(_instance))
                throw new System.ArgumentException();
            return _instance;
        }

        public TypeDefinition WithNamespace(string @namespace)
        {
            if (!IsNamespaceValid(@namespace))
                throw new System.ArgumentException();
            var _instance = new TypeDefinition(_Name, @namespace, _Usings, _Properties, _ValidateMethodName);
            if (!IsValid(_instance))
                throw new System.ArgumentException();
            return _instance;
        }

        public TypeDefinition WithUsings(ImmutableList<UsingDirectiveSyntax> @usings)
        {
            var _instance = new TypeDefinition(_Name, _Namespace, @usings, _Properties, _ValidateMethodName);
            if (!IsValid(_instance))
                throw new System.ArgumentException();
            return _instance;
        }

        public TypeDefinition WithProperties(ImmutableList<PropertyDefinition> @properties)
        {
            var _instance = new TypeDefinition(_Name, _Namespace, _Usings, @properties, _ValidateMethodName);
            if (!IsValid(_instance))
                throw new System.ArgumentException();
            return _instance;
        }

        public TypeDefinition WithValidateMethodName(NameSyntax @validateMethodName)
        {
            var _instance = new TypeDefinition(_Name, _Namespace, _Usings, _Properties, @validateMethodName);
            if (!IsValid(_instance))
                throw new System.ArgumentException();
            return _instance;
        }
    }
}