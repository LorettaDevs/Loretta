namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// A static class containing facts about Lua's Syntax.
    /// </summary>
    public static partial class SyntaxFacts
    {
        /// <summary>
        /// Checks whether a <see cref="SyntaxKind"/> is a comment's.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static bool IsComment(SyntaxKind kind) =>
            kind is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia;

        /// <summary>
        /// Checks whether a given <see cref="SyntaxKind"/> is a right associative operator's.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static bool IsRightAssociative(SyntaxKind kind) =>
            kind is SyntaxKind.HatToken;

        /// <summary>
        /// Checks whether a given kind is a reserved keyword.
        /// </summary>
        /// <param name="actual"></param>
        /// <param name="syntaxOptions"></param>
        /// <returns></returns>
        public static bool IsReservedKeyword(SyntaxKind actual, LuaSyntaxOptions syntaxOptions) =>
            actual switch
            {
                SyntaxKind.ContinueKeyword => syntaxOptions.ContinueType == ContinueType.Keyword,
                _ => IsKeyword(actual)
            };

        /// <summary>
        /// Checks whether a given kind is a contextual keyword.
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="syntaxOptions"></param>
        /// <returns></returns>
        public static bool IsContextualKeyword(SyntaxKind kind, LuaSyntaxOptions syntaxOptions) =>
            kind switch
            {
                SyntaxKind.ContinueKeyword => syntaxOptions.ContinueType == ContinueType.ContextualKeyword,
                _ => false
            };
    }
}
