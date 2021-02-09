using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.Generators
{
    internal class SourceWriter : IndentedTextWriter
    {
        private readonly MemoryStream _stream;
        private readonly StreamWriter _writer;

        public SourceWriter ( String tabString = "    " )
            : base ( new StreamWriter ( new MemoryStream ( ), Encoding.UTF8 ), tabString )
        {
            this._writer = ( StreamWriter ) this.InnerWriter;
            this._stream = ( MemoryStream ) this._writer.BaseStream;
        }

        public Indenter Indenter ( String openingLine = "" ) =>
            new Indenter ( this, openingLine );

        public CurlyIndenter CurlyIndenter ( String openingLine = "", String closingExtra = "" ) =>
            new CurlyIndenter ( this, openingLine, closingExtra );

        public void WriteLineIndented ( String text )
        {
            this.Indent++;
            this.WriteLine ( text );
            this.Indent--;
        }

        public SourceText GetText ( )
        {
            this.Flush ( );
            this._writer.Flush ( );

            var pos = this._stream.Position;
            try
            {
                this._stream.Seek ( 0, SeekOrigin.Begin );
                return SourceText.From (
                    this._stream,
                    Encoding.UTF8,
                    SourceHashAlgorithm.Sha256,
                    throwIfBinaryDetected: true,
                    canBeEmbedded: true );
            }
            finally
            {
                this._stream.Seek ( pos, SeekOrigin.Begin );
            }
        }

        protected override void Dispose ( Boolean disposing )
        {
            if ( disposing )
            {
                this._writer.Dispose ( );
                this._stream.Dispose ( );
            }
            base.Dispose ( disposing );
        }
    }
}
