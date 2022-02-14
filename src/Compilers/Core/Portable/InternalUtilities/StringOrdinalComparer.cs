// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.Utilities
{
    /// <summary>
    /// Compares string based upon their ordinal equality.
    /// We use this comparer for string identifiers because it does exactly what we need and nothing more
    /// The StringComparer.Ordinal as implemented by StringComparer is more complex to support 
    /// case sensitive and insensitive compares depending on flags.
    /// It also defers to the default string hash function that might not be the best for our scenarios.
    /// </summary>
    internal sealed class StringOrdinalComparer : IEqualityComparer<string>
    {
        public static readonly StringOrdinalComparer Instance = new();

        private StringOrdinalComparer()
        {
        }

        bool IEqualityComparer<string>.Equals(string? a, string? b) => Equals(a, b);

        public static bool Equals(string? a, string? b) =>
            // this is fast enough
            string.Equals(a, b);

        int IEqualityComparer<string>.GetHashCode(string s) =>
            // PERF: the default string hashcode is not always good or fast and cannot be changed for compat reasons.
            // We, however, can use anything we want in our dictionaries. 
            // Our typical scenario is a relatively short string (identifier)
            // FNV performs pretty well in such cases
            Hash.GetFNVHashCode(s);
    }
}
