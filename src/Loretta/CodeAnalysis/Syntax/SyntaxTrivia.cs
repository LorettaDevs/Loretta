using System;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents trivia contained in the source text.
    /// </summary>
    public readonly struct SyntaxTrivia
    {
        internal SyntaxTrivia ( SyntaxTree syntaxTree, SyntaxKind kind, Int32 position, String text )
        {
            this.SyntaxTree = syntaxTree;
            this.Kind = kind;
            this.Position = position;
            this.Text = text;
        }

        /// <summary>
        /// The tree this trivia belongs to.
        /// </summary>
        public SyntaxTree SyntaxTree { get; }

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
        public TextSpan Span => new ( this.Position, this.Text?.Length ?? 0 );

        /// <summary>
        /// This trivia's text.
        /// </summary>
        public String Text { get; }
    }
}