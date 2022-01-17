// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Syntax.InternalSyntax
{
    internal struct SeparatedSyntaxList<TNode> : IEquatable<SeparatedSyntaxList<TNode>> where TNode : GreenNode
    {
        private readonly SyntaxList<GreenNode> _list;

        internal SeparatedSyntaxList(SyntaxList<GreenNode> list)
        {
            Validate(list);
            _list = list;
        }

        [Conditional("DEBUG")]
        private static void Validate(SyntaxList<GreenNode> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var item = list.GetRequiredItem(i);
                if ((i & 1) == 0)
                {
                    LorettaDebug.Assert(!item.IsToken, "even elements of a separated list must be nodes");
                }
                else
                {
                    LorettaDebug.Assert(item.IsToken, "odd elements of a separated list must be tokens");
                }
            }
        }

        internal GreenNode? Node => _list.Node;

        public int Count => (_list.Count + 1) >> 1;

        public int SeparatorCount => _list.Count >> 1;

        public TNode? this[int index] => (TNode?) _list[index << 1];

        /// <summary>
        /// Gets the separator at the given index in this list.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public GreenNode? GetSeparator(int index) => _list[(index << 1) + 1];

        public SyntaxList<GreenNode> GetWithSeparators() => _list;

        public static bool operator ==(in SeparatedSyntaxList<TNode> left, in SeparatedSyntaxList<TNode> right) =>
            left.Equals(right);

        public static bool operator !=(in SeparatedSyntaxList<TNode> left, in SeparatedSyntaxList<TNode> right) =>
            !left.Equals(right);

        public bool Equals(SeparatedSyntaxList<TNode> other) => _list == other._list;

        public override bool Equals(object? obj) => (obj is SeparatedSyntaxList<TNode> list) && Equals(list);

        public override int GetHashCode() => _list.GetHashCode();

        public static implicit operator SeparatedSyntaxList<GreenNode>(SeparatedSyntaxList<TNode> list) =>
            new(list.GetWithSeparators());

#if DEBUG
        [Obsolete("For debugging only", true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "For debugging only")]
        private TNode[] Nodes
        {
            get
            {
                int count = Count;
                TNode[] array = new TNode[count];
                for (int i = 0; i < count; i++)
                {
                    array[i] = this[i]!;
                }
                return array;
            }
        }
#endif
    }
}
