// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;

namespace Loretta.CodeAnalysis
{
    public abstract class SyntaxTreeOptionsProvider
    {
        /// <summary>
        /// Get whether the given tree is generated.
        /// </summary>
        public abstract GeneratedKind IsGenerated(SyntaxTree tree, CancellationToken cancellationToken);

        /// <summary>
        /// Get diagnostic severity setting for a given diagnostic identifier in a given tree.
        /// </summary>
        public abstract bool TryGetDiagnosticValue(SyntaxTree tree, string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity);

        /// <summary>
        /// Get diagnostic severity set globally for a given diagnostic identifier
        /// </summary>
        public abstract bool TryGetGlobalDiagnosticValue(string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity);
    }
}
