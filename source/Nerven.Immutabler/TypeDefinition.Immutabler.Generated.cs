using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nerven.Assertion;

namespace Nerven.Immutabler
{
    partial class TypeDefinition
    {
        private TypeDefinition(string @name, string @namespace, ImmutableList<UsingDirectiveSyntax> @usings, ImmutableList<PropertyDefinition> @properties, NameSyntax @validateMethodName, SerializationMode @serializationMode)
        {
            Name = @name;
            Namespace = @namespace;
            Usings = @usings;
            Properties = @properties;
            ValidateMethodName = @validateMethodName;
            SerializationMode = @serializationMode;
        }

        public static TypeDefinition Create(string @name, string @namespace)
        {
            if (!IsNameValid(@name))
                throw new System.ArgumentException();
            if (!IsNamespaceValid(@namespace))
                throw new System.ArgumentException();
            var _instance = new TypeDefinition(@name, @namespace, DefaultUsings, DefaultProperties, DefaultValidateMethodName, DefaultSerializationMode);
            if (!IsValid(_instance))
                throw new System.ArgumentException();
            return _instance;
        }

        public TypeDefinition WithName(string @name)
        {
            if (!IsNameValid(@name))
                throw new System.ArgumentException();
            var _instance = new TypeDefinition(@name, Namespace, Usings, Properties, ValidateMethodName, SerializationMode);
            if (!IsValid(_instance))
                throw new System.ArgumentException();
            return _instance;
        }

        public TypeDefinition WithNamespace(string @namespace)
        {
            if (!IsNamespaceValid(@namespace))
                throw new System.ArgumentException();
            var _instance = new TypeDefinition(Name, @namespace, Usings, Properties, ValidateMethodName, SerializationMode);
            if (!IsValid(_instance))
                throw new System.ArgumentException();
            return _instance;
        }

        public TypeDefinition WithUsings(ImmutableList<UsingDirectiveSyntax> @usings)
        {
            var _instance = new TypeDefinition(Name, Namespace, @usings, Properties, ValidateMethodName, SerializationMode);
            if (!IsValid(_instance))
                throw new System.ArgumentException();
            return _instance;
        }

        public TypeDefinition WithProperties(ImmutableList<PropertyDefinition> @properties)
        {
            var _instance = new TypeDefinition(Name, Namespace, Usings, @properties, ValidateMethodName, SerializationMode);
            if (!IsValid(_instance))
                throw new System.ArgumentException();
            return _instance;
        }

        public TypeDefinition WithValidateMethodName(NameSyntax @validateMethodName)
        {
            var _instance = new TypeDefinition(Name, Namespace, Usings, Properties, @validateMethodName, SerializationMode);
            if (!IsValid(_instance))
                throw new System.ArgumentException();
            return _instance;
        }

        public TypeDefinition WithSerializationMode(SerializationMode @serializationMode)
        {
            var _instance = new TypeDefinition(Name, Namespace, Usings, Properties, ValidateMethodName, @serializationMode);
            if (!IsValid(_instance))
                throw new System.ArgumentException();
            return _instance;
        }
    }
}