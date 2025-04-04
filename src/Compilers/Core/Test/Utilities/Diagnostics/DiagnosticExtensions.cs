// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Loretta.CodeAnalysis.Test.Utilities;

namespace Loretta.CodeAnalysis
{
    [PublicAPI]
    public static class DiagnosticExtensions
    {
        [PublicAPI]
        public static void Verify(this IEnumerable<Diagnostic> actual, params DiagnosticDescription[] expected)
            => Verify(actual, expected, errorCodeOnly: false);

        [PublicAPI]
        public static bool AreEquivalent(
            this IEnumerable<Diagnostic> actual,
            DiagnosticDescription[]      expected,
            bool                         errorCodeOnly)
        {
            if (expected == null) throw new ArgumentException("Must specify expected errors.", nameof(expected));

            var includeDefaultSeverity   = expected.Length > 0 && expected.All(static e => e.DefaultSeverity != null);
            var includeEffectiveSeverity = expected.Length > 0 && expected.All(static e => e.EffectiveSeverity != null);
            var unmatched = actual.Select(
                                      d => new DiagnosticDescription(
                                          d,
                                          errorCodeOnly,
                                          includeDefaultSeverity,
                                          includeEffectiveSeverity))
                                  .ToList();

            // Try to match each of the 'expected' errors to one of the 'actual' ones.
            // If any of the expected errors don't appear, fail test.
            foreach (var d in expected)
            {
                var index = unmatched.IndexOf(d);
                if (index > -1)
                    unmatched.RemoveAt(index);
                else
                    return false;
            }

            // If any 'extra' errors appear that were not in the 'expected' list, fail test.
            return unmatched.Count <= 0;
        }

        [PublicAPI]
        private static void Verify(IEnumerable<Diagnostic> actual, DiagnosticDescription[] expected, bool errorCodeOnly)
        {
            var diagnostics = actual.ToArray();
            if (!AreEquivalent(diagnostics, expected, errorCodeOnly))
                Assert.Fail(DiagnosticDescription.GetAssertText(expected, diagnostics));
        }
    }
}
