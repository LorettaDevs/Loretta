// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using Loretta.Cci;
using Loretta.Utilities;
using System.Diagnostics;

namespace Loretta.CodeAnalysis.Emit
{
    [DebuggerDisplay("{Name, nq}")]
    internal struct AnonymousTypeValue
    {
        public readonly string Name;
        public readonly int UniqueIndex;
        public readonly ITypeDefinition Type;

        public AnonymousTypeValue(string name, int uniqueIndex, ITypeDefinition type)
        {
            RoslynDebug.Assert(!string.IsNullOrEmpty(name));
            RoslynDebug.Assert(uniqueIndex >= 0);

            this.Name = name;
            this.UniqueIndex = uniqueIndex;
            this.Type = type;
        }
    }
}
