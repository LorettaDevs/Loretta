// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using Loretta.Utilities;
using AnalyzerOptions = System.Collections.Immutable.ImmutableDictionary<string, string>;
using TreeOptions = System.Collections.Immutable.ImmutableDictionary<string, Loretta.CodeAnalysis.ReportDiagnostic>;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Holds results from <see cref="AnalyzerConfigSet.GetOptionsForSourcePath(string)"/>.
    /// </summary>
    public readonly struct AnalyzerConfigOptionsResult
    {
        /// <summary>
        /// Options that customize diagnostic severity as reported by the compiler.
        /// </summary>
        public TreeOptions TreeOptions { get; }

        /// <summary>
        /// Options that do not have any special compiler behavior and are passed to analyzers as-is.
        /// </summary>
        public AnalyzerOptions AnalyzerOptions { get; }

        /// <summary>
        /// Any produced diagnostics while applying analyzer configuration.
        /// </summary>
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        internal AnalyzerConfigOptionsResult(
            TreeOptions treeOptions,
            AnalyzerOptions analyzerOptions,
            ImmutableArray<Diagnostic> diagnostics)
        {
            RoslynDebug.Assert(treeOptions != null);
            RoslynDebug.Assert(analyzerOptions != null);

            TreeOptions = treeOptions;
            AnalyzerOptions = analyzerOptions;
            Diagnostics = diagnostics;
        }
    }
}
