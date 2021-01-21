using System;
using GParse.IO;
using GParse.Math;

namespace Loretta.CodeAnalysis.Text
{
    /// <summary>
    /// Represents a source code text.
    /// </summary>
    public sealed class SourceText
    {
        /// <summary>
        /// Creates a new <see cref="SourceText"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static SourceText From ( String text, String fileName = "" ) =>
            new ( text, fileName );

        private readonly String _text;

        private SourceText ( String text, String fileName )
        {
            this._text = text;
            this.FileName = fileName;
        }

        /// <summary>
        /// The name of the file that originated this text.
        /// </summary>
        public String FileName { get; }

        /// <summary>
        /// Creates a new reader from this source text.
        /// </summary>
        /// <returns></returns>
        public ICodeReader GetReader ( ) => new StringCodeReader ( this._text );

        /// <summary>
        /// Returns the text for this source text.
        /// </summary>
        /// <returns></returns>
        public override String ToString ( ) => this._text;

        /// <summary>
        /// Returns a substring of the text for this source text.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public String ToString ( Int32 start, Int32 length ) => this._text.Substring ( start, length );

        /// <summary>
        /// Returns a substring of the text for this source text.
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public String ToString ( TextSpan span ) => this.ToString ( span.Start, span.Length );
    }
}
