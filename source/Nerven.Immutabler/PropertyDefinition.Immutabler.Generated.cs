using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nerven.Immutabler
{
    partial class PropertyDefinition
    {
        private readonly string _Name;
        private readonly string _Type;
        private readonly NameSyntax _DefaultValuePropertyName;
        private readonly NameSyntax _ValidateMethodName;
        private PropertyDefinition(string @name, string @type, NameSyntax @defaultValuePropertyName, NameSyntax @validateMethodName)
        {
            _Name = @name;
            _Type = @type;
            _DefaultValuePropertyName = @defaultValuePropertyName;
            _ValidateMethodName = @validateMethodName;
        }

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public string Type
        {
            get
            {
                return _Type;
            }
        }

        public NameSyntax DefaultValuePropertyName
        {
            get
            {
                return _DefaultValuePropertyName;
            }
        }

        public NameSyntax ValidateMethodName
        {
            get
            {
                return _ValidateMethodName;
            }
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
            var _instance = new PropertyDefinition(@name, _Type, _DefaultValuePropertyName, _ValidateMethodName);
            return _instance;
        }

        public PropertyDefinition WithType(string @type)
        {
            if (!IsTypeValid(@type))
                throw new System.ArgumentException();
            var _instance = new PropertyDefinition(_Name, @type, _DefaultValuePropertyName, _ValidateMethodName);
            return _instance;
        }

        public PropertyDefinition WithDefaultValuePropertyName(NameSyntax @defaultValuePropertyName)
        {
            var _instance = new PropertyDefinition(_Name, _Type, @defaultValuePropertyName, _ValidateMethodName);
            return _instance;
        }

        public PropertyDefinition WithValidateMethodName(NameSyntax @validateMethodName)
        {
            var _instance = new PropertyDefinition(_Name, _Type, _DefaultValuePropertyName, @validateMethodName);
            return _instance;
        }
    }
}