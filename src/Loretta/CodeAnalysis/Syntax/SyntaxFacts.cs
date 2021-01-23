using System;
using Tsu;

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

        /// <summary>
        /// Obtains the value of a keyword's <see cref="SyntaxKind"/> if any.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static Option<Object?> GetKeywordValue ( SyntaxKind kind )
        {
            return kind switch
            {
                SyntaxKind.NilKeyword => null,
                SyntaxKind.TrueKeyword => true,
                SyntaxKind.FalseKeyword => false,
                _ => Option.None<Object?> ( )
            };
        }
    }
}
