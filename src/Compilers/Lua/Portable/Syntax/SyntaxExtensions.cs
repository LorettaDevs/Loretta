using System;
using System.Collections.Generic;

namespace Loretta.CodeAnalysis.Lua
{
    public static class SyntaxExtensions
    {

        /// <summary>
        /// Creates a new syntax token with all whitespace and end of line trivia replaced with
        /// regularly formatted trivia.
        /// </summary>
        /// <param name="token">The token to normalize.</param>
        /// <param name="indentation">A sequence of whitespace characters that defines a single level of indentation.</param>
        /// <param name="elasticTrivia">If true the replaced trivia is elastic trivia.</param>
        public static SyntaxToken NormalizeWhitespace(this SyntaxToken token, string indentation, bool elasticTrivia) =>
            SyntaxNormalizer.Normalize(token, indentation, CodeAnalysis.SyntaxNodeExtensions.DefaultEOL, elasticTrivia);

        /// <summary>
        /// Creates a new syntax token with all whitespace and end of line trivia replaced with
        /// regularly formatted trivia.
        /// </summary>
        /// <param name="token">The token to normalize.</param>
        /// <param name="indentation">An optional sequence of whitespace characters that defines a
        /// single level of indentation.</param>
        /// <param name="eol">An optional sequence of whitespace characters used for end of line.</param>
        /// <param name="elasticTrivia">If true the replaced trivia is elastic trivia.</param>
        public static SyntaxToken NormalizeWhitespace(this SyntaxToken token,
            string indentation = CodeAnalysis.SyntaxNodeExtensions.DefaultIndentation,
            string eol = CodeAnalysis.SyntaxNodeExtensions.DefaultEOL,
            bool elasticTrivia = false) =>
            SyntaxNormalizer.Normalize(token, indentation, eol, elasticTrivia);

        /// <summary>
        /// Creates a new syntax trivia list with all whitespace and end of line trivia replaced with
        /// regularly formatted trivia.
        /// </summary>
        /// <param name="list">The trivia list to normalize.</param>
        /// <param name="indentation">A sequence of whitespace characters that defines a single level of indentation.</param>
        /// <param name="elasticTrivia">If true the replaced trivia is elastic trivia.</param>
        public static SyntaxTriviaList NormalizeWhitespace(this SyntaxTriviaList list, string indentation, bool elasticTrivia) =>
            SyntaxNormalizer.Normalize(list, indentation, CodeAnalysis.SyntaxNodeExtensions.DefaultEOL, elasticTrivia);

        /// <summary>
        /// Creates a new syntax trivia list with all whitespace and end of line trivia replaced with
        /// regularly formatted trivia.
        /// </summary>
        /// <param name="list">The trivia list to normalize.</param>
        /// <param name="indentation">An optional sequence of whitespace characters that defines a
        /// single level of indentation.</param>
        /// <param name="eol">An optional sequence of whitespace characters used for end of line.</param>
        /// <param name="elasticTrivia">If true the replaced trivia is elastic trivia.</param>
        public static SyntaxTriviaList NormalizeWhitespace(this SyntaxTriviaList list,
            string indentation = CodeAnalysis.SyntaxNodeExtensions.DefaultIndentation,
            string eol = CodeAnalysis.SyntaxNodeExtensions.DefaultEOL,
            bool elasticTrivia = false) =>
            SyntaxNormalizer.Normalize(list, indentation, eol, elasticTrivia);

        public static SyntaxTriviaList ToSyntaxTriviaList(this IEnumerable<SyntaxTrivia> sequence) =>
            SyntaxFactory.TriviaList(sequence);
    }
}
