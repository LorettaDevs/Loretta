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
    }
}
