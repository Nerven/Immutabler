using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nerven.Immutabler
{
    [PublicAPI]
    public sealed partial class PropertyDefinition
    {
#if DEBUG
        public interface IImmutable
        {
            string Name { get; }

            string Type { get; }

            NameSyntax DefaultValuePropertyName { get; }

            NameSyntax ValidateMethodName { get; }
        }
#endif

        public static NameSyntax DefaultDefaultValuePropertyName => null;

        public static NameSyntax DefaultValidateMethodName => null;

        public static bool IsNameValid(string name) => name != null;

        public static bool IsTypeValid(string type) => type != null;
    }
}
