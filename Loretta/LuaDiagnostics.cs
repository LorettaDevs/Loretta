using System;
using GParse;

namespace Loretta
{
    /// <summary>
    /// The class containing the factory methods for the diagnostics emmitted by the lua parser
    /// along with a method to highlight a range in the source code.
    /// </summary>
    public static partial class LuaDiagnostics
    {
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
            return String.Join ( Environment.NewLine,
                                lines[start.Line - 1],
                                new String ( ' ', start.Column - 1 ) + new String ( '^', len ) );
        }
    }
}