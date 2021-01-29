using System;
using System.Collections;
using System.Collections.Generic;
using Loretta.CodeAnalysis.Syntax;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// A list of diagnostics.
    /// </summary>
    public class DiagnosticBag : IReadOnlyList<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics;

        /// <inheritdoc />
        public Int32 Count =>
            this._diagnostics.Count;

        /// <inheritdoc />
        public Diagnostic this[Int32 index] =>
            this._diagnostics[index];

        /// <summary>
        /// Initializes this <see cref="DiagnosticBag" />
        /// </summary>
        public DiagnosticBag ( )
        {
            this._diagnostics = new List<Diagnostic> ( );
        }

        /// <summary>
        /// Reports a diagnostic
        /// </summary>
        /// <param name="diagnostic"></param>
        public void Report ( Diagnostic diagnostic ) =>
            this._diagnostics.Add ( diagnostic );

        /// <summary>
        /// Adds multiple diagnostics to this list.
        /// </summary>
        /// <param name="diagnostics">The diagnostics to be added.</param>
        public void AddRange ( IEnumerable<Diagnostic> diagnostics ) =>
            this._diagnostics.AddRange ( diagnostics );

        /// <summary>
        /// Adds multiple diagnostics to this list.
        /// </summary>
        /// <param name="diagnostics">The diagnostics to be added.</param>
        public void AddRange ( params Diagnostic[] diagnostics ) =>
            this._diagnostics.AddRange ( diagnostics );

        internal void ReportInvalidStringEscape ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0001", "Invalid string escape", location ) );

        internal void ReportUnescapedLineBreakInString ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0002", "Unescaped line break in string", location ) );

        internal void ReportUnfinishedString ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0003", "Unfinished string", location ) );

        internal void ReportInvalidNumber ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0004", "Invalid number", location ) );

        internal void ReportNumericLiteralTooLarge ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0005", "Numeric literal is too large", location ) );

        internal void ReportUnfinishedLongComment ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0006", "Unfinished long comment", location ) );

        internal void ReportShebangNotSupportedInVersion ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0007", "Shebangs are not supported in this lua version", location ) );

        internal void ReportBinaryLiteralNotSupportedInVersion ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0008", "Binary number literals are not supported in this lua version", location ) );

        internal void ReportOctalLiteralNotSupportedInVersion ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0009", "Octal number literals are not supported in this lua version", location ) );

        internal void ReportHexFloatLiteralNotSupportedInVersion ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0010", "Hexadecimal floating point number literals are not supported in this lua version", location ) );

        internal void ReportUnderscoreInNumberLiteralNotSupportedInVersion ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0011", "Underscores in number literals are not supported in this lua version", location ) );

        internal void ReportCCommentsNotSupportedInVersion ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0012", "C comments are not supported in this lua version", location ) );

        internal void ReportLuajitIdentifierRulesNotSupportedInVersion ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0013", "Characters with value above 0x7F are not supported in this lua version", location ) );

        internal void ReportBadCharacter ( TextLocation location, Char character ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0014", $"Bad character input: '{LoCharUtils.ToReadableString ( character )}'", location ) );

        internal void ReportUnexpectedToken ( TextLocation location, SyntaxKind actual, SyntaxKind expected ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0015", $"Unexpected token {actual}, expected {expected}", location ) );

        internal void ReportHexStringEscapesNotSupportedInVersion ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0016", "Hexadecimal string escapes are not supported in this lua version", location ) );

        internal void ReportAmbiguousFunctionCallOrNewStatement ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0017", "Syntax ambiguous between a function call and a new statement", location ) );

        internal void ReportNonFunctionCallBeingUsedAsStatement ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0018", "Function calls are the only expressions that can be used as statements", location ) );

        internal void ReportCannotBeAssignedTo ( TextLocation location ) =>
            this.Report ( new Diagnostic ( DiagnosticSeverity.Error, "LUA0019", "This expression cannot be assigned to", location ) );

        /// <inheritdoc/>
        public IEnumerator<Diagnostic> GetEnumerator ( ) =>
            this._diagnostics.GetEnumerator ( );

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator ( ) =>
            this._diagnostics.GetEnumerator ( );
    }
}
