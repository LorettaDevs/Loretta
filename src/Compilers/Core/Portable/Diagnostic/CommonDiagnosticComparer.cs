// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis
{
    internal sealed class CommonDiagnosticComparer : IEqualityComparer<Diagnostic>
    {
        internal static readonly CommonDiagnosticComparer Instance = new();

        private CommonDiagnosticComparer()
        {
        }

        public bool Equals(Diagnostic? x, Diagnostic? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Location == y.Location && x.Id == y.Id;
        }

        public int GetHashCode(Diagnostic obj)
        {
            if (obj is null)
            {
                return 0;
            }

            return Hash.Combine(obj.Location, obj.Id.GetHashCode());
        }
    }
}
