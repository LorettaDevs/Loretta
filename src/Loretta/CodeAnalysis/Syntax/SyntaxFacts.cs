using System;

namespace Loretta.CodeAnalysis.Syntax
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
        public static Boolean IsComment ( this SyntaxKind kind ) =>
            kind is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia;
    }
}
