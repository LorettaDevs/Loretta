using System;
using System.Collections.Immutable;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents trivia contained in the source text.
    /// </summary>
    public sealed class SyntaxTrivia
    {
        /// <summary>
        /// An empty array of trivia.
        /// </summary>
        public static ImmutableArray<SyntaxTrivia> Empty => ImmutableArray<SyntaxTrivia>.Empty;

        internal SyntaxTrivia ( SyntaxKind kind, Int32 position, ReadOnlyMemory<Char> text )
        {
            this.Kind = kind;
            this.Position = position;
            this.Text = text;
        }

        /// <summary>
        /// This trivia's kind.
        /// </summary>
        public SyntaxKind Kind { get; }

        /// <summary>
        /// This token's position.
        /// </summary>
        public Int32 Position { get; }

        /// <summary>
        /// This trivia's position range.
        /// </summary>
        public TextSpan Span => new ( this.Position, this.Text.Length );

        /// <summary>
        /// This trivia's text.
        /// </summary>
        public ReadOnlyMemory<Char> Text { get; }
    }
}