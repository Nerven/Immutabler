using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nerven.Immutabler
{
    partial class PropertyDefinition
    {
        private PropertyDefinition(string @name, string @type, NameSyntax @defaultValuePropertyName, NameSyntax @validateMethodName)
        {
            Name = @name;
            Type = @type;
            DefaultValuePropertyName = @defaultValuePropertyName;
            ValidateMethodName = @validateMethodName;
        }

        public static PropertyDefinition Create(string @name, string @type)
        {
            if (!IsNameValid(@name))
                throw new System.ArgumentException();
            if (!IsTypeValid(@type))
                throw new System.ArgumentException();
            var _instance = new PropertyDefinition(@name, @type, DefaultDefaultValuePropertyName, DefaultValidateMethodName);
            return _instance;
        }

        public PropertyDefinition WithName(string @name)
        {
            if (!IsNameValid(@name))
                throw new System.ArgumentException();
            var _instance = new PropertyDefinition(@name, Type, DefaultValuePropertyName, ValidateMethodName);
            return _instance;
        }

        public PropertyDefinition WithType(string @type)
        {
            if (!IsTypeValid(@type))
                throw new System.ArgumentException();
            var _instance = new PropertyDefinition(Name, @type, DefaultValuePropertyName, ValidateMethodName);
            return _instance;
        }

        public PropertyDefinition WithDefaultValuePropertyName(NameSyntax @defaultValuePropertyName)
        {
            var _instance = new PropertyDefinition(Name, Type, @defaultValuePropertyName, ValidateMethodName);
            return _instance;
        }

        public PropertyDefinition WithValidateMethodName(NameSyntax @validateMethodName)
        {
            var _instance = new PropertyDefinition(Name, Type, DefaultValuePropertyName, @validateMethodName);
            return _instance;
        }
    }
}