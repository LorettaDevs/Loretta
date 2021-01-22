using System;
using System.Collections.Generic;
using System.Text;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Represents the severity of the <see cref="Diagnostic"/> emmited by a parser or lexer
    /// </summary>
    public enum DiagnosticSeverity
    {
        /// <summary>
        /// The diagnostic is hidden from the user
        /// </summary>
        Hidden,
        /// <summary>
        /// The diagnostic represents information about a section of the code
        /// </summary>
        Info,
        /// <summary>
        /// The diagnostic represents a warning
        /// </summary>
        Warning,
        /// <summary>
        /// The diagnostic represents an error.
        /// </summary>
        Error,
    }
}
