using System;
using GParse;

namespace Loretta.CodeAnalysis.Text
{
    /// <summary>
    /// Represents a location in a <see cref="SourceText"/>.
    /// </summary>
    public readonly struct TextLocation
    {
        private readonly Lazy<SourceRange> _range;

        /// <summary>
        /// Initializes a new text location.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="span"></param>
        public TextLocation ( SourceText text, TextSpan span )
        {
            this.Text = text;
            this.Span = span;
            this._range = new Lazy<SourceRange> ( ( ) =>
                SourceRange.Calculate ( text.ToString ( ), (span.Start, span.End) ) );
        }

        /// <summary>
        /// The text this location is contained within.
        /// </summary>
        public SourceText Text { get; }

        /// <summary>
        /// This location's span.
        /// </summary>
        public TextSpan Span { get; }

        /// <summary>
        /// The name of the file this location is contained within.
        /// </summary>
        public String FileName => this.Text.FileName;

        /// <summary>
        /// The <see cref="SourceLocation"/> this location starts at.
        /// </summary>
        public SourceLocation Start => this._range.Value.Start;

        /// <summary>
        /// The <see cref="SourceLocation"/> this location ends at.
        /// </summary>
        public SourceLocation End => this._range.Value.End;
    }
}
