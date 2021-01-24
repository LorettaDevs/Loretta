using System;
using System.Runtime.CompilerServices;
using GParse;

namespace Loretta.CodeAnalysis.Text
{
    /// <summary>
    /// Represents a location in a <see cref="SourceText"/>.
    /// </summary>
    public readonly struct TextLocation
    {
        /// <summary>
        /// Initializes a new text location.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="span"></param>
        public TextLocation ( SourceText text, TextSpan span )
        {
            this.Text = text;
            this.Span = span;
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
        /// The index of line where this location starts at.
        /// </summary>
        public Int32 StartLine => this.Text.GetLineIndex ( this.Span.Start );

        /// <summary>
        /// The index of the character on the line this location starts at.
        /// </summary>
        public Int32 StartCharacter => this.Span.Start - this.Text.Lines[this.StartLine].Start;

        /// <summary>
        /// The index of the line this location ends at.
        /// </summary>
        public Int32 EndLine => this.Text.GetLineIndex ( this.Span.End );

        /// <summary>
        /// The index of the character on the line this location ends at.
        /// </summary>
        public Int32 EndCharacter => this.Span.End - this.Text.Lines[this.EndLine].Start;
    }
}
