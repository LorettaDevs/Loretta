using System;

namespace Loretta.CodeAnalysis.Text
{
    /// <summary>
    /// A line of text in a <see cref="SourceText"/>.
    /// </summary>
    public readonly struct TextLine
    {
        /// <summary>
        /// Initializes a new text line.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="lengthWithLineBreak"></param>
        internal TextLine ( SourceText text, Int32 start, Int32 length, Int32 lengthWithLineBreak )
        {
            this.Text = text;
            this.Start = start;
            this.Length = length;
            this.LengthWithLineBreak = lengthWithLineBreak;
        }

        /// <summary>
        /// The <see cref="SourceText"/> this line belongs to.
        /// </summary>
        public SourceText Text { get; }

        /// <summary>
        /// The line's start.
        /// </summary>
        public Int32 Start { get; }

        /// <summary>
        /// The line's length.
        /// </summary>
        public Int32 Length { get; }

        /// <summary>
        /// The line's end.
        /// </summary>
        public Int32 End => this.Start + this.Length;

        /// <summary>
        /// The line's end including the line break.
        /// </summary>
        public Int32 LengthWithLineBreak { get; }

        /// <summary>
        /// This line's span.
        /// </summary>
        public TextSpan Span => new TextSpan ( this.Start, this.Length );

        /// <summary>
        /// This line's span with the line break.
        /// </summary>
        public TextSpan SpanWithLineBreak => new TextSpan ( this.Start, this.LengthWithLineBreak );

        /// <summary>
        /// Retrieves line's contents.
        /// </summary>
        /// <returns></returns>
        public override String ToString ( ) => this.Text.ToString ( this.Span );
    }
}