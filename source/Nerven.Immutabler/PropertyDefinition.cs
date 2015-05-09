using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nerven.Immutabler
{
    /// <Immutabler />
    [PublicAPI]
    public sealed partial class PropertyDefinition
    {
        public string Name { get; }

        public string Type { get; }

        public NameSyntax DefaultValuePropertyName { get; }

        public NameSyntax ValidateMethodName { get; }

        public static NameSyntax DefaultDefaultValuePropertyName => null;

        public static NameSyntax DefaultValidateMethodName => null;

        public static bool IsNameValid(string name) => name != null;

        public static bool IsTypeValid(string type) => type != null;
    }
}
