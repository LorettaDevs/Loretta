using System;
using System.Linq;
using GParse;
using Loretta.CodeAnalysis.Text;

namespace Loretta.Utilities
{
    /// <summary>
    /// A class with utility functions for <see cref="Diagnostic"/>s.
    /// </summary>
    public static class DiagnosticUtils
    {
        /// <summary>
        /// Highlights a <see cref="TextLocation"/>.
        /// </summary>
        /// <param name="location">The location to highlight.</param>
        /// <returns></returns>
        public static String HighlightLocation ( TextLocation location )
        {
            SourceLocation start = location.Start;
            SourceLocation end = location.End;
            var lines = location.Text.ToString ( ).Split ( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );

            if ( start.Line != end.Line )
            {
                var linesLength = lines.Skip ( start.Line - 1 )
                                       .Take ( end.Line - start.Line + 1 )
                                       .Sum ( str => str.Length );
                var builder = new System.Text.StringBuilder ( linesLength + ( end.Byte - start.Byte ) );
                var startLine = start.Line;
                var endLine = end.Line - 1;

                for ( var i = startLine; i <= endLine; i++ )
                {
                    var line = lines[i - 1];
                    var lineLength = line.Length;

                    builder.AppendLine ( line );
                    if ( i == startLine )
                        builder.AppendLine ( new String ( ' ', Math.Max ( start.Column - 1, 0 ) ) + new String ( '^', Math.Max ( lineLength - start.Column, 0 ) ) );
                    else if ( i == endLine )
                        builder.AppendLine ( new String ( '^', end.Column ) );
                    else
                        builder.AppendLine ( new String ( '^', lineLength ) );
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
