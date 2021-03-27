// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    internal static class ReportDiagnosticExtensions
    {
        public static string ToAnalyzerConfigString(this ReportDiagnostic reportDiagnostic)
        {
            return reportDiagnostic switch
            {
                ReportDiagnostic.Error => "error",
                ReportDiagnostic.Warn => "warning",
                ReportDiagnostic.Info => "suggestion",
                ReportDiagnostic.Hidden => "silent",
                ReportDiagnostic.Suppress => "none",
                _ => throw ExceptionUtilities.Unreachable,
            };
        }
    }
}
