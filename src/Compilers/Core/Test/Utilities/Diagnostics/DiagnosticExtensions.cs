// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using Loretta.CodeAnalysis.Test.Utilities;
using Loretta.Test.Utilities;
using Loretta.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis
{
    public static class DiagnosticExtensions
    {
        /// <summary>
        /// This is obsolete. Use Verify instead.
        /// </summary>
        public static void VerifyErrorCodes(this IEnumerable<Diagnostic> actual, params DiagnosticDescription[] expected) =>
            Verify(actual, expected, errorCodeOnly: true);

        public static void VerifyErrorCodes(this ImmutableArray<Diagnostic> actual, params DiagnosticDescription[] expected) =>
            VerifyErrorCodes((IEnumerable<Diagnostic>) actual, expected);

        internal static void Verify(this DiagnosticBag actual, params DiagnosticDescription[] expected) =>
            Verify(actual.AsEnumerable(), expected, errorCodeOnly: false);

        public static void Verify(this IEnumerable<Diagnostic> actual, params DiagnosticDescription[] expected) =>
            Verify(actual, expected, errorCodeOnly: false);

        public static void Verify(this IEnumerable<Diagnostic> actual, bool fallbackToErrorCodeOnlyForNonEnglish, params DiagnosticDescription[] expected) =>
            Verify(actual, expected, errorCodeOnly: fallbackToErrorCodeOnlyForNonEnglish && EnsureEnglishUICulture.PreferredOrNull != null);

        public static void VerifyWithFallbackToErrorCodeOnlyForNonEnglish(this IEnumerable<Diagnostic> actual, params DiagnosticDescription[] expected) =>
            Verify(actual, true, expected);

        public static void Verify(this ImmutableArray<Diagnostic> actual, params DiagnosticDescription[] expected) =>
            Verify((IEnumerable<Diagnostic>) actual, expected);

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

        public static string Concat(this string[] str) => string.Concat(str);

        public static string Inspect(this Diagnostic e) =>
            e.Location.IsInSource ? $"{e.Severity} {e.Id}: {e.GetMessage(CultureInfo.CurrentCulture)}" : "no location: ";

        public static string ToString(this Diagnostic d, IFormatProvider formatProvider)
        {
            IFormattable formattable = d;
            return formattable.ToString(null, formatProvider);
        }
    }
}
