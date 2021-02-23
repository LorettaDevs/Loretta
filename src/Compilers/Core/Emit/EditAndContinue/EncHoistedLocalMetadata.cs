// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Diagnostics;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Emit
{
    internal struct EncHoistedLocalMetadata
    {
        public readonly string Name;
        public readonly Cci.ITypeReference Type;
        public readonly SynthesizedLocalKind SynthesizedKind;

        public EncHoistedLocalMetadata(string name, Cci.ITypeReference type, SynthesizedLocalKind synthesizedKind)
        {
            RoslynDebug.Assert(name != null);
            RoslynDebug.Assert(type != null);
            RoslynDebug.Assert(synthesizedKind.IsLongLived());

            this.Name = name;
            this.Type = type;
            this.SynthesizedKind = synthesizedKind;
        }
    }
}
