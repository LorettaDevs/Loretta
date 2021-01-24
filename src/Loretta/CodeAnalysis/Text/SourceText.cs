using System;
using System.Collections.Immutable;
using System.IO;
using GParse.IO;

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

        /// <summary>
        /// Loads the text from the provided path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static SourceText Load ( String path )
        {
            var text = File.ReadAllText ( path );
            return From ( text, path );
        }

        private static void AddLine (
            ImmutableArray<TextLine>.Builder result,
            SourceText sourceText,
            Int32 position,
            Int32 lineStart,
            Int32 lineBreakWidth )
        {
            var lineLength = position - lineStart;
            var lineLengthWithLineBreak = lineLength + lineBreakWidth;
            var line = new TextLine ( sourceText, lineStart, lineLength, lineLengthWithLineBreak );
            result.Add ( line );
        }

        private static ImmutableArray<TextLine> ParseLines ( SourceText sourceText, String text )
        {
            ImmutableArray<TextLine>.Builder result = ImmutableArray.CreateBuilder<TextLine> ( );

            var position = 0;
            var lineStart = 0;

            while ( position < text.Length )
            {
                switch ( text[position] )
                {
                    case '\r':
                        if ( position + 1 < text.Length && text[position + 1] == '\n' )
                        {
                            AddLine ( result, sourceText, position, lineStart, 2 );
                            position += 2;
                            lineStart = position;
                            break;
                        }
                        else
                        {
                            goto case '\n';
                        }

                    case '\n':
                        AddLine ( result, sourceText, position, lineStart, 1 );
                        position += 1;
                        lineStart = position;
                        break;

                    default:
                        position++;
                        break;
                }
            }

            if ( position >= lineStart )
                AddLine ( result, sourceText, position, lineStart, 0 );

            return result.ToImmutable ( );
        }

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
        /// The lines contained within this source text.
        /// </summary>
        public ImmutableArray<TextLine> Lines { get; }

        /// <summary>
        /// The length of this source text.
        /// </summary>
        public Int32 Length => this._text.Length;

        /// <summary>
        /// Obtains the character at the provided <paramref name="index"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Char this[Int32 index] => this._text[index];

        /// <summary>
        /// Returns the index of the <see cref="TextLine"/> that the
        /// provided <paramref name="position"/> is contained within.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Int32 GetLineIndex ( Int32 position )
        {
            var lower = 0;
            var upper = this.Lines.Length - 1;

            while ( lower <= upper )
            {
                var index = lower + ( ( upper - lower ) >> 1 );
                var start = this.Lines[index].Start;

                if ( position == start )
                    return index;

                if ( start > position )
                {
                    upper = index - 1;
                }
                else
                {
                    lower = index + 1;
                }
            }

            return lower - 1;
        }

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
