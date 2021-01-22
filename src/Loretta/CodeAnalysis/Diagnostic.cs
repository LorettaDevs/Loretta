using System;
using System.Collections.Generic;
using System.Text;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Represents a diagnostic emmited by the compiler, such as an error, warning, suggestion, etc.
    /// </summary>
    public class Diagnostic
    {
        /// <summary>
        /// Initializes a new diagnostic
        /// </summary>
        /// <param name="severity"><inheritdoc cref="Severity" path="/summary"/></param>
        /// <param name="id"><inheritdoc cref="Id" path="/summary"/></param>
        /// <param name="description"><inheritdoc cref="Description" path="/summary"/></param>
        /// <param name="location"><inheritdoc cref="Location" path="/summary"/></param>
        public Diagnostic ( DiagnosticSeverity severity, String id, String description, TextLocation location )
        {
            this.Id = id;
            this.Severity = severity;
            this.Description = description;
            this.Location = location;
        }

        /// <summary>
        /// The severity of the diagnostic
        /// </summary>
        public DiagnosticSeverity Severity { get; }

        /// <summary>
        /// The ID of the emitted diagnostic
        /// </summary>
        public String Id { get; }

        /// <summary>
        /// The description of this diagnostic
        /// </summary>
        public String Description { get; }

        /// <summary>
        /// The location that the diagnostic is reffering to in the code
        /// </summary>
        public TextLocation Location { get; }
    }
}