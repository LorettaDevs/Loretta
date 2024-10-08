﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis
{
    internal class SyntaxTreeComparer : IEqualityComparer<SyntaxTree>
    {
        public static readonly SyntaxTreeComparer Instance = new();

        public bool Equals(SyntaxTree? x, SyntaxTree? y)
        {
            if (x == null)
            {
                return y == null;
            }
            else if (y == null)
            {
                return false;
            }

            return string.Equals(x.FilePath, y.FilePath, StringComparison.OrdinalIgnoreCase) &&
                SourceTextComparer.Instance.Equals(x.GetText(), y.GetText());
        }

        public int GetHashCode(SyntaxTree obj) =>
            Hash.Combine(obj.FilePath.GetHashCode(), SourceTextComparer.Instance.GetHashCode(obj.GetText()));
    }
}
