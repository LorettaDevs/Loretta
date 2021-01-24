using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Syntax;
using Loretta.CodeAnalysis.Text;

namespace Loretta.IO
{
    /// <summary>
    /// Extension methods for <see cref="TextWriter"/>
    /// </summary>
    public static class TextWriterExtensions
    {
        private static Boolean IsConsole ( this TextWriter writer )
        {
            if ( writer == Console.Out )
                return !Console.IsOutputRedirected;

            if ( writer == Console.Error )
                return !Console.IsErrorRedirected && !Console.IsOutputRedirected; // Color codes are always output to Console.Out

            if ( writer is IndentedTextWriter iw && iw.InnerWriter.IsConsole ( ) )
                return true;

            return false;
        }

        private static void SetForeground ( this TextWriter writer, ConsoleColor color )
        {
            if ( writer.IsConsole ( ) )
                Console.ForegroundColor = color;
        }

        private static void ResetColor ( this TextWriter writer )
        {
            if ( writer.IsConsole ( ) )
                Console.ResetColor ( );
        }

        /// <summary>
        /// Writes a keyword's text to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="kind"></param>
        public static void WriteKeyword ( this TextWriter writer, SyntaxKind kind )
        {
            var text = SyntaxFacts.GetText ( kind );
            Debug.Assert ( kind.IsKeyword ( ) && text != null );

            writer.WriteKeyword ( text );
        }

        /// <summary>
        /// Writes a text with a keyword's color to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="text"></param>
        public static void WriteKeyword ( this TextWriter writer, String text )
        {
            writer.SetForeground ( ConsoleColor.Blue );
            writer.Write ( text );
            writer.ResetColor ( );
        }

        /// <summary>
        /// Writes a text with an identifier's color to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="text"></param>
        public static void WriteIdentifier ( this TextWriter writer, String text )
        {
            writer.SetForeground ( ConsoleColor.DarkYellow );
            writer.Write ( text );
            writer.ResetColor ( );
        }

        /// <summary>
        /// Writes a text with a number's color to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="text"></param>
        public static void WriteNumber ( this TextWriter writer, String text )
        {
            writer.SetForeground ( ConsoleColor.Cyan );
            writer.Write ( text );
            writer.ResetColor ( );
        }

        /// <summary>
        /// Writes a text with a number's color to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="text"></param>
        public static void WriteString ( this TextWriter writer, String text )
        {
            writer.SetForeground ( ConsoleColor.Magenta );
            writer.Write ( text );
            writer.ResetColor ( );
        }

        /// <summary>
        /// Writes a single space with a punctuation's color to the output.
        /// </summary>
        /// <param name="writer"></param>
        public static void WriteSpace ( this TextWriter writer ) => writer.WritePunctuation ( " " );

        /// <summary>
        /// Writes the text of a punctuation to the output.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="kind"></param>
        public static void WritePunctuation ( this TextWriter writer, SyntaxKind kind )
        {
            var text = SyntaxFacts.GetText ( kind );
            Debug.Assert ( text != null );

            writer.WritePunctuation ( text );
        }

        /// <summary>
        /// Writes a text with a punctuation's color to the output.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="text"></param>
        public static void WritePunctuation ( this TextWriter writer, String text )
        {
            writer.SetForeground ( ConsoleColor.DarkGray );
            writer.Write ( text );
            writer.ResetColor ( );
        }

        /// <summary>
        /// Writes the list of diagnostics to the <see cref="TextWriter"/> output.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="diagnostics"></param>
        public static void WriteDiagnostics ( this TextWriter writer, IEnumerable<Diagnostic> diagnostics )
        {
            foreach ( Diagnostic? diagnostic in diagnostics.Where ( d => d.Location.Text == null ) )
            {
                ConsoleColor messageColor = diagnostic.Severity == DiagnosticSeverity.Warning
                                            ? ConsoleColor.DarkYellow
                                            : ConsoleColor.DarkRed;
                writer.SetForeground ( messageColor );
                writer.WriteLine ( $"{diagnostic.Id} - {diagnostic.Description}" );
                writer.ResetColor ( );
            }

            foreach ( Diagnostic? diagnostic in diagnostics.Where ( d => d.Location.Text != null )
                                                  .OrderBy ( d => d.Location.FileName )
                                                  .ThenBy ( d => d.Location.Span.Start )
                                                  .ThenBy ( d => d.Location.Span.Length ) )
            {
                SourceText? text = diagnostic.Location.Text;
                var fileName = diagnostic.Location.FileName;
                var startLine = diagnostic.Location.StartLine + 1;
                var startCharacter = diagnostic.Location.StartCharacter + 1;
                var endLine = diagnostic.Location.EndLine + 1;
                var endCharacter = diagnostic.Location.EndCharacter + 1;

                TextSpan span = diagnostic.Location.Span;
                var lineIndex = text.GetLineIndex ( span.Start );
                TextLine line = text.Lines[lineIndex];

                writer.WriteLine ( );

                ConsoleColor messageColor = diagnostic.Severity == DiagnosticSeverity.Warning
                                            ? ConsoleColor.DarkYellow
                                            : ConsoleColor.DarkRed;
                writer.SetForeground ( messageColor );
                writer.Write ( $"{fileName}({startLine},{startCharacter},{endLine},{endCharacter}): " );
                writer.WriteLine ( diagnostic );
                writer.ResetColor ( );

                var prefixSpan = TextSpan.FromBounds ( line.Start, span.Start );
                var suffixSpan = TextSpan.FromBounds ( span.End, line.End );

                var prefix = text.ToString ( prefixSpan );
                var error = text.ToString ( span );
                var suffix = text.ToString ( suffixSpan );

                writer.Write ( "    " );
                writer.Write ( prefix );

                writer.SetForeground ( ConsoleColor.DarkRed );
                writer.Write ( error );
                writer.ResetColor ( );

                writer.Write ( suffix );

                writer.WriteLine ( );
            }

            writer.WriteLine ( );
        }
    }
}
