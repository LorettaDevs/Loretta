// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Diagnostics;
using Loretta.CodeAnalysis.CodeGen;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Emit
{
    [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
    internal struct EncHoistedLocalInfo : IEquatable<EncHoistedLocalInfo>
    {
        public readonly LocalSlotDebugInfo SlotInfo;
        public readonly Cci.ITypeReference Type;

        public EncHoistedLocalInfo(bool ignored)
        {
            SlotInfo = new LocalSlotDebugInfo(SynthesizedLocalKind.EmitterTemp, LocalDebugId.None);
            Type = null;
        }

        public EncHoistedLocalInfo(LocalSlotDebugInfo slotInfo, Cci.ITypeReference type)
        {
            RoslynDebug.Assert(type != null);
            this.SlotInfo = slotInfo;
            this.Type = type;
        }

        public bool IsUnused
        {
            get { return this.Type == null; }
        }

        public bool Equals(EncHoistedLocalInfo other)
        {
            RoslynDebug.Assert(this.Type != null);
            RoslynDebug.Assert(other.Type != null);

            return this.SlotInfo.Equals(other.SlotInfo) &&
                   Cci.SymbolEquivalentEqualityComparer.Instance.Equals(this.Type, other.Type);
        }

        public override bool Equals(object obj)
        {
            return obj is EncHoistedLocalInfo && Equals((EncHoistedLocalInfo)obj);
        }

        public override int GetHashCode()
        {
            return Hash.Combine(Cci.SymbolEquivalentEqualityComparer.Instance.GetHashCode(this.Type), this.SlotInfo.GetHashCode());
        }

        private string GetDebuggerDisplay()
        {
            if (this.IsUnused)
            {
                return "[invalid]";
            }

            return string.Format("[Id={0}, SynthesizedKind={1}, Type={2}]",
                this.SlotInfo.Id,
                this.SlotInfo.SynthesizedKind,
                this.Type);
        }
    }
}
