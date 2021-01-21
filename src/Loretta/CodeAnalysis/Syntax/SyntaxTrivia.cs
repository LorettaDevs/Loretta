using System;
using GParse.Math;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents trivia contained in the source text.
    /// </summary>
    public readonly struct SyntaxTrivia
    {
        internal SyntaxTrivia ( SyntaxKind kind, Range<Int32> range, String text )
        {
            this.Kind = kind;
            this.Range = range;
            this.Text = text;
        }

        /// <summary>
        /// This trivia's kind.
        /// </summary>
        public SyntaxKind Kind { get; }

        /// <summary>
        /// This trivia's position range.
        /// </summary>
        public Range<Int32> Range { get; }

        /// <summary>
        /// This trivia's text.
        /// </summary>
        public String Text { get; }
    }
}