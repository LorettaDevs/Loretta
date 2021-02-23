// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;
using System.Diagnostics;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Represents the value of #r reference along with its source location.
    /// </summary>
    internal readonly struct ReferenceDirective
    {
        public readonly string? File;
        public readonly Location? Location;

        public ReferenceDirective(string file, Location location)
        {
            RoslynDebug.Assert(file != null);
            RoslynDebug.Assert(location != null);

            File = file;
            Location = location;
        }
    }
}
