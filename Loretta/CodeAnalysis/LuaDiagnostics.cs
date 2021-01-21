using System;
using GParse;

namespace Loretta.CodeAnalysis
{
    public static class LuaDiagnostics
    {
        internal static readonly DiagnosticTemplate InvalidStringEscape = DiagnosticTemplate.Error ( null, "Invalid string escape" );
        internal static readonly DiagnosticTemplate UnescapedLineBreakInString = DiagnosticTemplate.Error ( null, "Unescaped line break in string" );
        internal static readonly DiagnosticTemplate UnfinishedString = DiagnosticTemplate.Error ( null, "Unfinished string" );
        internal static readonly DiagnosticTemplate InvalidNumber = DiagnosticTemplate.Error ( null, "Invalid number" );
        internal static readonly DiagnosticTemplate NumericLiteralTooLarge = DiagnosticTemplate.Error ( null, "Numeric literal is too large" );
        internal static readonly DiagnosticTemplate UnfinishedLongComment = DiagnosticTemplate.Error ( null, "Unfinished long comment" );
        internal static readonly DiagnosticTemplate InvalidShebang = DiagnosticTemplate.Error ( null, "Shebangs are not allowed with the provided options" );
        internal static readonly DiagnosticTemplate BinaryLiteralNotSupportedInVersion = DiagnosticTemplate.Error ( null, "Binary number literals are not supported in this version of lua" );
        internal static readonly DiagnosticTemplate OctalLiteralNotSupportedInVersion = DiagnosticTemplate.Error ( null, "Octal number literals are not supported in this version of lua" );
        internal static readonly DiagnosticTemplate HexFloatLiteralNotSupportedInVersion = DiagnosticTemplate.Error ( null, "Hexadecimal floating point number literals are not supported in this version of lua" );

        /// <summary>
        /// Highlights a range retrieving the line(s) referred to by the range and inserting ^'s
        /// under the code section that the range refers to.
        /// </summary>
        /// <param name="expression">The expression containing the range to be highlighted.</param>
        /// <param name="range">The range to highlight.</param>
        /// <returns></returns>
        public static String HighlightRange ( String expression, SourceRange range )
        {
            if ( String.IsNullOrEmpty ( expression ) )
                throw new ArgumentException ( "The expression should not be null or empty.", nameof ( expression ) );

            SourceLocation start = range.Start;
            SourceLocation end = range.End;
            var lines = expression.Split ( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );

            if ( start.Line != end.Line )
            {
                var builder = new System.Text.StringBuilder ( );
                var startLine = start.Line;
                var endLine = end.Line - 1;

                for ( var i = startLine; i <= endLine; i++ )
                {
                    var line = lines[i - 1];
                    var lineLength = line.Length;

                    builder.AppendLine ( line )
                           .AppendLine ( i switch
                           {
                               _ when i == startLine => new String ( ' ', Math.Max ( start.Column - 1, 0 ) )
                                                        + new String ( '^', Math.Max ( lineLength - start.Column, 0 ) ),
                               _ when i == endLine => new String ( '^', end.Column ),
                               _ => new String ( '^', lineLength )
                           } );
                }

                return builder.ToString ( );
            }

            var len = Math.Max ( end.Byte - start.Byte, 1 );
            return String.Join (
                Environment.NewLine,
                lines[start.Line - 1],
                new String ( ' ', start.Column - 1 ) + new String ( '^', len ) );
        }
    }
}
