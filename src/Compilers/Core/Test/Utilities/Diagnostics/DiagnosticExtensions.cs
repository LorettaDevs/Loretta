// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.CodeAnalysis.Test.Utilities;
using Loretta.Test.Utilities;
using Loretta.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis
{
    public static class DiagnosticExtensions
    {
        public static void Verify(this IEnumerable<Diagnostic> actual, params DiagnosticDescription[] expected) =>
            Verify(actual, expected, errorCodeOnly: false);

        public static void Verify(this IEnumerable<Diagnostic> actual, bool fallbackToErrorCodeOnlyForNonEnglish, params DiagnosticDescription[] expected) =>
            Verify(actual, expected, errorCodeOnly: fallbackToErrorCodeOnlyForNonEnglish && EnsureEnglishUICulture.PreferredOrNull != null);

        private static void Verify(IEnumerable<Diagnostic> actual, DiagnosticDescription[] expected, bool errorCodeOnly)
        {
            if (expected == null)
            {
                throw new ArgumentException("Must specify expected errors.", nameof(expected));
            }

            var includeDefaultSeverity = expected.Any() && expected.All(e => e.DefaultSeverity != null);
            var includeEffectiveSeverity = expected.Any() && expected.All(e => e.EffectiveSeverity != null);
            var unmatched = actual.Select(d => new DiagnosticDescription(d, errorCodeOnly, includeDefaultSeverity, includeEffectiveSeverity))
                                  .ToList();

            // Try to match each of the 'expected' errors to one of the 'actual' ones.
            // If any of the expected errors don't appear, fail test.
            foreach (var d in expected)
            {
                var index = unmatched.IndexOf(d);
                if (index > -1)
                {
                    unmatched.RemoveAt(index);
                }
                else
                {
                    Assert.True(false, DiagnosticDescription.GetAssertText(expected, actual));
                }
            }

            // If any 'extra' errors appear that were not in the 'expected' list, fail test.
            if (unmatched.Count > 0)
            {
                Assert.True(false, DiagnosticDescription.GetAssertText(expected, actual));
            }
        }
    }
}
