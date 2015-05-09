using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nerven.Immutabler.Machine
{
    public static class NameHelper
    {
        public static SyntaxToken TextToPublicMethodIdentifier(string s) => SyntaxFactory.Identifier(s);

        ////public static IdentifierNameSyntax TextToPublicMethodName(string s) => SyntaxFactory.IdentifierName(s);

        public static SyntaxToken TextToPublicPropertyIdentifier(string s) => SyntaxFactory.Identifier(s);

        ////public static IdentifierNameSyntax TextToPublicPropertyName(string s) => SyntaxFactory.IdentifierName(s);

        ////public static SyntaxToken TextToProtectedMethodIdentifier(string s) => SyntaxFactory.Identifier(s);

        ////public static IdentifierNameSyntax TextToProtectedMethodName(string s) => SyntaxFactory.IdentifierName(s);

        public static SyntaxToken TextToPrivateFieldIdentifier(string s) => SyntaxFactory.Identifier("_" + s);

        public static IdentifierNameSyntax TextToPrivateFieldName(string s) => SyntaxFactory.IdentifierName("_" + s);

        public static SyntaxToken TextToMethodParameterIdentifier(string s) => SyntaxFactory.Identifier("@" + s.Substring(0, 1).ToLower() + s.Substring(1));

        public static IdentifierNameSyntax TextToMethodParameterName(string s) => SyntaxFactory.IdentifierName("@" + s.Substring(0, 1).ToLower() + s.Substring(1));
    }
}
