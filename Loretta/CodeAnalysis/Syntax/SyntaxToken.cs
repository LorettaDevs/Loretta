using System;
using System.Collections.Immutable;
using System.Linq;
using GParse.Math;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// A syntax token.
    /// </summary>
    public readonly struct SyntaxToken
    {
        /// <summary>
        /// Initializes a new syntax token.
        /// </summary>
        /// <param name="kind"><inheritdoc cref="Kind" path="/summary"/></param>
        /// <param name="range"><inheritdoc cref="Range" path="/summary"/></param>
        /// <param name="text"><inheritdoc cref="Text" path="/summary"/></param>
        /// <param name="value"><inheritdoc cref="Value" path="/summary"/></param>
        /// <param name="leadingTrivia"><inheritdoc cref="LeadingTrivia" path="/summary"/></param>
        /// <param name="trailingTrivia"><inheritdoc cref="TrailingTrivia" path="/summary"/></param>
        internal SyntaxToken ( SyntaxKind kind, Range<Int32> range, String? text, Object? value, ImmutableArray<SyntaxTrivia> leadingTrivia, ImmutableArray<SyntaxTrivia> trailingTrivia )
        {
            this.Kind = kind;
            this.Range = range;
            this.Text = text;
            this.Value = value;
            this.LeadingTrivia = leadingTrivia;
            this.TrailingTrivia = trailingTrivia;
        }

        /// <summary>
        /// This token's kind.
        /// </summary>
        public SyntaxKind Kind { get; }

        /// <summary>
        /// This token's position range.
        /// </summary>
        public Range<Int32> Range { get; }

        /// <summary>
        /// This token's full range including trivia.
        /// </summary>
        public Range<Int32> FullRange
        {
            get
            {
                var start = this.LeadingTrivia.Length == 0
                            ? this.Range.Start
                            : this.LeadingTrivia.First ( ).Range.Start;
                var end = this.TrailingTrivia.Length == 0
                          ? this.Range.End
                          : this.TrailingTrivia.Last ( ).Range.End;
                return new Range<Int32> ( start, end );
            }
        }

        /// <summary>
        /// This token's raw text.
        /// </summary>
        public String? Text { get; }

        /// <summary>
        /// This token's value.
        /// </summary>
        public Object? Value { get; }

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
        public Boolean IsMissing => this.Text is null;
    }
}
