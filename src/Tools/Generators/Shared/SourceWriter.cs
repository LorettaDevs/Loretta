using System.CodeDom.Compiler;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.Generators
{
    internal class SourceWriter : IndentedTextWriter
    {
        private readonly MemoryStream _stream;
        private readonly StreamWriter _writer;

        public SourceWriter(string tabString = "    ")
            : base(new StreamWriter(new MemoryStream(), Encoding.UTF8), tabString)
        {
            _writer = (StreamWriter) InnerWriter;
            _stream = (MemoryStream) _writer.BaseStream;
        }

        public Indenter Indenter(string openingLine = "") =>
            new(this, openingLine);

        public CurlyIndenter CurlyIndenter(string openingLine = "", string closingExtra = "") =>
            new(this, openingLine, closingExtra);

        public void WriteLineIndented(string text)
        {
            Indent++;
            WriteLine(text);
            Indent--;
        }

        public SourceText GetText()
        {
            Flush();
            _writer.Flush();

            var pos = _stream.Position;
            try
            {
                _stream.Seek(0, SeekOrigin.Begin);
                return SourceText.From(
                    _stream,
                    Encoding.UTF8,
                    SourceHashAlgorithm.Sha256,
                    throwIfBinaryDetected: true,
                    canBeEmbedded: true);
            }
            finally
            {
                _stream.Seek(pos, SeekOrigin.Begin);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writer.Dispose();
                _stream.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
