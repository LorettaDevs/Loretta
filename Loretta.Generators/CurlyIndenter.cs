using System;
using System.CodeDom.Compiler;

namespace Loretta.Generators
{
    /// <summary>
    /// Takes care of opening and closing curly braces for code generation
    /// </summary>
    internal readonly struct CurlyIndenter : IDisposable
    {
        private readonly IndentedTextWriter _indentedTextWriter;

        /// <summary>
        /// Default constructor that maked a tidies creation of the line before the opening curly
        /// </summary>
        /// <param name="indentedTextWriter">The writer to use</param>
        /// <param name="openingLine">any line to write before the curly</param>
        public CurlyIndenter ( IndentedTextWriter indentedTextWriter, String openingLine = "" )
        {
            this._indentedTextWriter = indentedTextWriter;
            if ( !String.IsNullOrWhiteSpace ( openingLine ) )
                indentedTextWriter.WriteLine ( openingLine );
            indentedTextWriter.WriteLine ( "{" );
            indentedTextWriter.Indent++;
        }

        /// <summary>
        /// When the variable goes out of scope the closing brace is injected and indentation reduced.
        /// </summary>
        public void Dispose ( )
        {
            this._indentedTextWriter.Indent--;
            this._indentedTextWriter.WriteLine ( "}" );
        }
    }
}
