using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// Applies Lua-specific modification and filtering of <see cref="Diagnostic"/>s.
    /// </summary>
    internal static class LuaDiagnosticFilter
    {
        /// <summary>
        /// Modifies an input <see cref="Diagnostic"/> per the given options. For example, the
        /// severity may be escalated, or the <see cref="Diagnostic"/> may be filtered out entirely
        /// (by returning null).
        /// </summary>
        /// <param name="d">The input diagnostic</param>
        /// <param name="warningLevelOption">The maximum warning level to allow. Diagnostics with a higher warning level will be filtered out.</param>
        /// <param name="generalDiagnosticOption">How warning diagnostics should be reported</param>
        /// <param name="specificDiagnosticOptions">How specific diagnostics should be reported</param>
        /// <returns>A diagnostic updated to reflect the options, or null if it has been filtered out</returns>
        internal static Diagnostic? Filter(
            Diagnostic d,
            int warningLevelOption,
            ReportDiagnostic generalDiagnosticOption,
            IDictionary<string, ReportDiagnostic> specificDiagnosticOptions,
            SyntaxTreeOptionsProvider? syntaxTreeOptions,
            CancellationToken cancellationToken)
        {
            if (d == null)
            {
                return d;
            }
            else if (d.IsNotConfigurable())
            {
                if (d.IsEnabledByDefault)
                {
                    // Enabled NotConfigurable should always be reported as it is.
                    return d;
                }
                else
                {
                    // Disabled NotConfigurable should never be reported.
                    return null;
                }
            }
            else if (d.Severity == InternalDiagnosticSeverity.Void)
            {
                return null;
            }

            var reportAction = GetDiagnosticReport(d.Severity,
                d.IsEnabledByDefault,
                d.Id,
                d.WarningLevel,
                d.Location,
                warningLevelOption,
                generalDiagnosticOption,
                specificDiagnosticOptions,
                syntaxTreeOptions,
                cancellationToken,
                out var hasPragmaSuppression);

            if (hasPragmaSuppression)
            {
                d = d.WithIsSuppressed(true);
            }

            return d.WithReportDiagnostic(reportAction);
        }

        /// <summary>
        /// Take a warning and return the final disposition of the given warning,
        /// based on both command line options and pragmas. The diagnostic options
        /// have precedence in the following order:
        ///     1. Warning level
        ///     2. Command line options (/nowarn, /warnaserror)
        ///     3. Editor config options (syntax tree level)
        ///     4. Global analyzer config options (compilation level)
        ///     5. Global warning level
        ///
        /// Pragmas are considered separately. If a diagnostic would not otherwise
        /// be suppressed, but is suppressed by a pragma, <paramref name="hasPragmaSuppression"/>
        /// is true but the diagnostic is not reported as suppressed.
        /// </summary> 
        internal static ReportDiagnostic GetDiagnosticReport(
            DiagnosticSeverity severity,
            bool isEnabledByDefault,
            string id,
            int diagnosticWarningLevel,
            Location location,
            int warningLevelOption,
            ReportDiagnostic generalDiagnosticOption,
            IDictionary<string, ReportDiagnostic> specificDiagnosticOptions,
            SyntaxTreeOptionsProvider? syntaxTreeOptions,
            CancellationToken cancellationToken,
            out bool hasPragmaSuppression)
        {
            hasPragmaSuppression = false;

            Debug.Assert(location.SourceTree is null || location.SourceTree is LuaSyntaxTree);
            var tree = location.SourceTree as LuaSyntaxTree;
            var position = location.SourceSpan.Start;

            // 1. Warning level
            if (diagnosticWarningLevel > warningLevelOption)  // honor the warning level
            {
                return ReportDiagnostic.Suppress;
            }

            var isSpecified = false;
            var specifiedWarnAsErrorMinus = false;

            if (specificDiagnosticOptions.TryGetValue(id, out var report))
            {
                // 2. Command line options (/nowarn, /warnaserror)
                isSpecified = true;

                // 'ReportDiagnostic.Default' is added to SpecificDiagnosticOptions for "/warnaserror-:DiagnosticId",
                if (report == ReportDiagnostic.Default)
                {
                    specifiedWarnAsErrorMinus = true;
                }
            }

            // Apply syntax tree options, if applicable.
            if (syntaxTreeOptions != null &&
                (!isSpecified || specifiedWarnAsErrorMinus))
            {
                // 3. Editor config options (syntax tree level)
                // 4. Global analyzer config options (compilation level)
                // Do not apply config options if it is bumping a warning to an error and "/warnaserror-:DiagnosticId" was specified on the command line.
                if ((tree != null && syntaxTreeOptions.TryGetDiagnosticValue(tree, id, cancellationToken, out var reportFromSyntaxTreeOptions) ||
                    syntaxTreeOptions.TryGetGlobalDiagnosticValue(id, cancellationToken, out reportFromSyntaxTreeOptions)) &&
                    !(specifiedWarnAsErrorMinus && severity == DiagnosticSeverity.Warning && reportFromSyntaxTreeOptions == ReportDiagnostic.Error))
                {
                    isSpecified = true;
                    report = reportFromSyntaxTreeOptions;

                    // '/warnaserror' should promote warnings configured in analyzer config to error.
                    if (!specifiedWarnAsErrorMinus && report == ReportDiagnostic.Warn && generalDiagnosticOption == ReportDiagnostic.Error)
                    {
                        report = ReportDiagnostic.Error;
                    }
                }
            }

            if (!isSpecified)
            {
                report = isEnabledByDefault ? ReportDiagnostic.Default : ReportDiagnostic.Suppress;
            }

            if (report == ReportDiagnostic.Suppress)
            {
                return ReportDiagnostic.Suppress;
            }

            if (report == ReportDiagnostic.Suppress) // check options (/nowarn)
            {
                return ReportDiagnostic.Suppress;
            }

            // 5. Global options
            // Unless specific warning options are defined (/warnaserror[+|-]:<n> or /nowarn:<n>, 
            // follow the global option (/warnaserror[+|-] or /nowarn).
            if (report == ReportDiagnostic.Default)
            {
                switch (generalDiagnosticOption)
                {
                    case ReportDiagnostic.Error:
                        if (promoteToAnError())
                        {
                            return ReportDiagnostic.Error;
                        }
                        break;
                    case ReportDiagnostic.Suppress:
                        // When doing suppress-all-warnings, don't lower severity for anything other than warning and info.
                        // We shouldn't suppress hidden diagnostics here because then features that use hidden diagnostics to
                        // display a lightbulb would stop working if someone has suppress-all-warnings (/nowarn) specified in their project.
                        if (severity == DiagnosticSeverity.Warning || severity == DiagnosticSeverity.Info)
                        {
                            report = ReportDiagnostic.Suppress;
                            isSpecified = true;
                        }
                        break;
                }
            }

            return report;

            bool promoteToAnError()
            {
                Debug.Assert(report == ReportDiagnostic.Default);
                Debug.Assert(generalDiagnosticOption == ReportDiagnostic.Error);

                // If we've been asked to do warn-as-error then don't raise severity for anything below warning (info or hidden).
                return severity == DiagnosticSeverity.Warning &&
                       // In the case where /warnaserror+ is followed by /warnaserror-:<n> on the command line,
                       // do not promote the warning specified in <n> to an error.
                       !isSpecified;

            }
        }
    }
}
