using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.CodeAnalysis.Text;
using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// A syntax token.
    /// </summary>
    public sealed class SyntaxToken : SyntaxNode
    {
        /// <summary>
        /// Initializes a new syntax token.
        /// </summary>
        /// <param name="kind"><inheritdoc cref="Kind" path="/summary"/></param>
        /// <param name="position"><inheritdoc cref="Position" path="/summary"/></param>
        /// <param name="text"><inheritdoc cref="Text" path="/summary"/></param>
        /// <param name="value"><inheritdoc cref="Value" path="/summary"/></param>
        /// <param name="leadingTrivia"><inheritdoc cref="LeadingTrivia" path="/summary"/></param>
        /// <param name="trailingTrivia"><inheritdoc cref="TrailingTrivia" path="/summary"/></param>
        internal SyntaxToken (
            SyntaxKind kind,
            Int32 position,
            Option<ReadOnlyMemory<Char>> text,
            Option<Object?> value,
            ImmutableArray<SyntaxTrivia> leadingTrivia,
            ImmutableArray<SyntaxTrivia> trailingTrivia )
        {
            this.Kind = kind;
            this.Position = position;
            this.Text = text.UnwrapOr ( ReadOnlyMemory<Char>.Empty );
            this.Value = value;
            this.LeadingTrivia = leadingTrivia;
            this.TrailingTrivia = trailingTrivia;
            this.IsMissing = text.IsNone;
        }

        /// <summary>
        /// This token's kind.
        /// </summary>
        public override SyntaxKind Kind { get; }

        /// <summary>
        /// This token's position.
        /// </summary>
        public Int32 Position { get; }

        /// <summary>
        /// This token's raw text.
        /// </summary>
        public ReadOnlyMemory<Char> Text { get; }

        /// <summary>
        /// This token's value.
        /// </summary>
        public Option<Object?> Value { get; }

        /// <summary>
        /// This token's position range.
        /// </summary>
        public override TextSpan Span => new ( this.Position, this.Text.Length );

        /// <summary>
        /// This token's full range including trivia.
        /// </summary>
        public override TextSpan FullSpan
        {
            get
            {
                var start = this.LeadingTrivia.Length == 0
                            ? this.Span.Start
                            : this.LeadingTrivia.First ( ).Span.Start;
                var end = this.TrailingTrivia.Length == 0
                          ? this.Span.End
                          : this.TrailingTrivia.Last ( ).Span.End;
                return TextSpan.FromBounds ( start, end );
            }
        }

        /// <summary>
        /// This token's leading trivia.
        /// </summary>
        public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }

        /// <summary>
        /// This token's trailing trivia.
        /// </summary>
        public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }

        /// <summary>
        /// Whether this token is missing (inserted by the parser and doesn't appear in source).
        /// </summary>
        public Boolean IsMissing { get; }

        /// <inheritdoc/>
        public override void Accept ( SyntaxVisitor syntaxVisitor ) =>
            syntaxVisitor.VisitToken ( this );

        /// <inheritdoc/>
        public override TReturn? Accept<TReturn> ( SyntaxVisitor<TReturn> syntaxVisitor ) where TReturn : default =>
            syntaxVisitor.VisitToken ( this );

        /// <inheritdoc/>
        public override IEnumerable<SyntaxNode> GetChildren ( ) => Enumerable.Empty<SyntaxNode> ( );

        /// <summary>
        /// Generates a new token with the provided leading trivia.
        /// </summary>
        /// <param name="leadingTrivia"></param>
        /// <returns></returns>
        public SyntaxToken WithLeadingTrivia ( ImmutableArray<SyntaxTrivia> leadingTrivia ) =>
            new SyntaxToken (
                this.Kind,
                this.Position,
                this.IsMissing ? default : this.Text,
                this.Value,
                leadingTrivia,
                this.TrailingTrivia );

        /// <summary>
        /// Generates a new token but with the provided trailing trivia.
        /// </summary>
        /// <param name="trailingTrivia"></param>
        /// <returns></returns>
        public SyntaxToken WithTrailingTrivia ( ImmutableArray<SyntaxTrivia> trailingTrivia ) =>
            new SyntaxToken (
                this.Kind,
                this.Position,
                this.IsMissing ? default : this.Text,
                this.Value,
                this.LeadingTrivia,
                trailingTrivia );
    }
}
