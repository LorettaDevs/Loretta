// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Globalization;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Abstracts the ability to classify and load messages for error codes. Allows the error
    /// infrastructure to be reused between C# and VB.
    /// </summary>
    internal abstract class CommonMessageProvider
    {
        /// <summary>
        /// Caches the return values for <see cref="GetIdForErrorCode(int)"/>.
        /// </summary>
        private static readonly ConcurrentDictionary<(string prefix, int code), string> s_errorIdCache = new ConcurrentDictionary<(string prefix, int code), string>();

        /// <summary>
        /// Given an error code, get the severity (warning or error) of the code.
        /// </summary>
        public abstract DiagnosticSeverity GetSeverity(int code);

        /// <summary>
        /// Load the message for the given error code. If the message contains
        /// "fill-in" placeholders, those should be expressed in standard string.Format notation
        /// and be in the string.
        /// </summary>
        public abstract string LoadMessage(int code, CultureInfo? language);

        /// <summary>
        /// Get an optional localizable title for the given diagnostic code.
        /// </summary>
        public abstract LocalizableString GetTitle(int code);

        /// <summary>
        /// Get an optional localizable description for the given diagnostic code.
        /// </summary>
        public abstract LocalizableString GetDescription(int code);

        /// <summary>
        /// Get a localizable message format string for the given diagnostic code.
        /// </summary>
        public abstract LocalizableString GetMessageFormat(int code);

        /// <summary>
        /// Get an optional help link for the given diagnostic code.
        /// </summary>
        public abstract string GetHelpLink(int code);

        /// <summary>
        /// Get the diagnostic category for the given diagnostic code.
        /// Default category is <see cref="Diagnostic.CompilerDiagnosticCategory"/>.
        /// </summary>
        public abstract string GetCategory(int code);

        /// <summary>
        /// Get the text prefix (e.g., "CS" for C#) used on error messages.
        /// </summary>
        public abstract string CodePrefix { get; }

        /// <summary>
        /// Get the warning level for warnings (e.g., 1 or greater for C#). VB does not have warning
        /// levels and always uses 1. Errors should return 0.
        /// </summary>
        public abstract int GetWarningLevel(int code);

        /// <summary>
        /// Type that defines error codes. For testing purposes only.
        /// </summary>
        public abstract Type ErrorCodeType { get; }

        /// <summary>
        /// Create a simple language specific diagnostic for given error code.
        /// </summary>
        public Diagnostic CreateDiagnostic(int code, Location location) => CreateDiagnostic(code, location, Array.Empty<object>());

        /// <summary>
        /// Create a simple language specific diagnostic with no location for given info.
        /// </summary>
        public abstract Diagnostic CreateDiagnostic(DiagnosticInfo info);

        /// <summary>
        /// Create a simple language specific diagnostic for given error code.
        /// </summary>
        public abstract Diagnostic CreateDiagnostic(int code, Location location, params object[] args);

        /// <summary>
        /// Given a message identifier (e.g., CS0219), severity, warning as error and a culture, 
        /// get the entire prefix (e.g., "error CS0219: Warning as Error:" for C# or "error BC42024:" for VB) used on error messages.
        /// </summary>
        public abstract string GetMessagePrefix(string id, DiagnosticSeverity severity, bool isWarningAsError, CultureInfo? culture);

        /// <summary>
        /// Given an error code (like 1234) return the identifier (CS1234 or BC1234).
        /// </summary>
        [PerformanceSensitive(
            "https://github.com/dotnet/roslyn/issues/31964",
            AllowCaptures = false,
            Constraint = "Frequently called by error list filtering; avoid allocations")]
        public string GetIdForErrorCode(int errorCode)
            => s_errorIdCache.GetOrAdd((CodePrefix, errorCode), key => key.prefix + key.code.ToString("0000"));

        /// <summary>
        /// Produces the filtering action for the diagnostic based on the options passed in.
        /// </summary>
        /// <returns>
        /// A new <see cref="DiagnosticInfo"/> with new effective severity based on the options or null if the
        /// diagnostic has been suppressed.
        /// </returns>
        public abstract ReportDiagnostic GetDiagnosticReport(DiagnosticInfo diagnosticInfo, CompilationOptions options);

        /// <summary>
        /// Filter a <see cref="DiagnosticInfo"/> based on the compilation options so that /nowarn and /warnaserror etc. take effect.options
        /// </summary>
        /// <returns>A <see cref="DiagnosticInfo"/> with effective severity based on option or null if suppressed.</returns>
        public DiagnosticInfo? FilterDiagnosticInfo(DiagnosticInfo diagnosticInfo, CompilationOptions options)
        {
            var report = this.GetDiagnosticReport(diagnosticInfo, options);
            switch (report)
            {
                case ReportDiagnostic.Error:
                    return diagnosticInfo.GetInstanceWithSeverity(DiagnosticSeverity.Error);
                case ReportDiagnostic.Warn:
                    return diagnosticInfo.GetInstanceWithSeverity(DiagnosticSeverity.Warning);
                case ReportDiagnostic.Info:
                    return diagnosticInfo.GetInstanceWithSeverity(DiagnosticSeverity.Info);
                case ReportDiagnostic.Hidden:
                    return diagnosticInfo.GetInstanceWithSeverity(DiagnosticSeverity.Hidden);
                case ReportDiagnostic.Suppress:
                    return null;
                default:
                    return diagnosticInfo;
            }
        }

        public abstract int ERR_BadDocumentationMode { get; }
    }
}
