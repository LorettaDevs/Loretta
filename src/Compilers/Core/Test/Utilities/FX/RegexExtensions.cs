// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Loretta.Test.Utilities
{
    public static class RegexExtensions
    {
        public static IEnumerable<Match> ToEnumerable(this MatchCollection collection)
        {
            foreach (Match m in collection)
            {
                yield return m;
            }
        }
    }
}
