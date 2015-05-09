using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nerven.Assertion;

namespace Nerven.Immutabler
{
    [PublicAPI]
    public sealed partial class TypeDefinition
    {
#if DEBUG
        public interface IImmutable
        {
            string Name { get; }

            string Namespace { get; }

            ImmutableList<UsingDirectiveSyntax> Usings { get; }

            ImmutableList<PropertyDefinition> Properties { get; }

            NameSyntax ValidateMethodName { get; }
        }
#endif

        public static ImmutableList<UsingDirectiveSyntax> DefaultUsings => ImmutableList.Create<UsingDirectiveSyntax>();

        public static ImmutableList<PropertyDefinition> DefaultProperties => ImmutableList.Create<PropertyDefinition>();

        public static NameSyntax DefaultValidateMethodName => null;

        public static bool IsNameValid(string name) => name != null;

        public static bool IsNamespaceValid(string @namespace) => @namespace != null;

        public static bool IsValid(TypeDefinition typeDefinition) => true;

        public TypeDefinition AddUsings(IEnumerable<UsingDirectiveSyntax> usings)
        {
            Must.Assert(usings != null);

            return WithUsings(_Usings.AddRange(usings));
        }

        public TypeDefinition AddProperties(IEnumerable<PropertyDefinition> properties)
        {
            Must.Assert(properties != null);

            return WithProperties(_Properties.AddRange(properties));
        }

        public TypeDefinition AddProperties(params PropertyDefinition[] properties)
        {
            Must.Assert(properties != null);

            return WithProperties(_Properties.AddRange(properties));
        }
    }
}
