using Loretta.CodeAnalysis.PooledObjects;
using Loretta.CodeAnalysis.Syntax.InternalSyntax;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing
{
    public static class SyntaxExtensions
    {
        #region SyntaxNodeExtensions

        public static SyntaxTriviaList GetLeadingTrivia(this SyntaxNode node) =>
            node.GetFirstToken(includeSkipped: true).LeadingTrivia;

        public static SyntaxTriviaList GetTrailingTrivia(this SyntaxNode node) =>
            node.GetLastToken(includeSkipped: true).TrailingTrivia;

        internal static ImmutableArray<DiagnosticInfo> Errors(this SyntaxNode node) =>
            node.Green.ErrorsOrWarnings(errorsOnly: true);

        internal static ImmutableArray<DiagnosticInfo> Warnings(this SyntaxNode node) =>
            node.Green.ErrorsOrWarnings(errorsOnly: false);

        internal static ImmutableArray<DiagnosticInfo> ErrorsAndWarnings(this SyntaxNode node) =>
            node.Green.ErrorsAndWarnings();

        #endregion SyntaxNodeExtensions

        #region SyntaxTokenExtensions

        public static SyntaxTriviaList GetLeadingTrivia(this SyntaxToken token) => token.LeadingTrivia;

        public static SyntaxTriviaList GetTrailingTrivia(this SyntaxToken token) => token.TrailingTrivia;

        internal static ImmutableArray<DiagnosticInfo> Errors(this SyntaxToken token) =>
            ((Lua.Syntax.InternalSyntax.LuaSyntaxNode) token.Node!).ErrorsOrWarnings(errorsOnly: true);

        internal static ImmutableArray<DiagnosticInfo> Warnings(this SyntaxToken token) =>
            ((Lua.Syntax.InternalSyntax.LuaSyntaxNode) token.Node!).ErrorsOrWarnings(errorsOnly: false);

        internal static ImmutableArray<DiagnosticInfo> ErrorsAndWarnings(this SyntaxToken token) =>
            ((Lua.Syntax.InternalSyntax.LuaSyntaxNode) token.Node!).ErrorsAndWarnings();

        #endregion SyntaxTokenExtensions

        #region SyntaxNodeOrTokenExtensions

        internal static ImmutableArray<DiagnosticInfo> Errors(this SyntaxNodeOrToken nodeOrToken) =>
            nodeOrToken.UnderlyingNode!.ErrorsOrWarnings(errorsOnly: true);

        internal static ImmutableArray<DiagnosticInfo> Warnings(this SyntaxNodeOrToken nodeOrToken) =>
            nodeOrToken.UnderlyingNode!.ErrorsOrWarnings(errorsOnly: false);

        internal static ImmutableArray<DiagnosticInfo> ErrorsAndWarnings(this SyntaxNodeOrToken nodeOrToken) =>
            nodeOrToken.UnderlyingNode!.ErrorsAndWarnings();

        #endregion SyntaxNodeOrTokenExtensions

        #region SyntaxTriviaExtensions

        internal static ImmutableArray<DiagnosticInfo> Errors(this SyntaxTrivia trivia) =>
            ((Lua.Syntax.InternalSyntax.LuaSyntaxNode) trivia.UnderlyingNode!).ErrorsOrWarnings(errorsOnly: true);

        internal static ImmutableArray<DiagnosticInfo> Warnings(this SyntaxTrivia trivia) =>
            ((Lua.Syntax.InternalSyntax.LuaSyntaxNode) trivia.UnderlyingNode!).ErrorsOrWarnings(errorsOnly: false);

        internal static ImmutableArray<DiagnosticInfo> ErrorsAndWarnings(this SyntaxTrivia trivia) =>
            ((Lua.Syntax.InternalSyntax.LuaSyntaxNode) trivia.UnderlyingNode!).ErrorsAndWarnings();

        #endregion SyntaxTriviaExtensions

        private static ImmutableArray<DiagnosticInfo> ErrorsOrWarnings(this GreenNode node, bool errorsOnly)
        {
            var b = ArrayBuilder<DiagnosticInfo>.GetInstance();

            var l = new SyntaxDiagnosticInfoList(node);

            foreach (var item in l)
            {
                if (item.Severity == (errorsOnly ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning))
                    b.Add(item);
            }

            return b.ToImmutableAndFree();
        }

        private static ImmutableArray<DiagnosticInfo> ErrorsAndWarnings(this GreenNode node)
        {
            var b = ArrayBuilder<DiagnosticInfo>.GetInstance();

            var l = new SyntaxDiagnosticInfoList(node);

            foreach (var item in l)
            {
                b.Add(item);
            }

            return b.ToImmutableAndFree();
        }
    }
}
