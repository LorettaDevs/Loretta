// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Syntax.InternalSyntax
{
    internal partial struct SyntaxList<TNode> : IEquatable<SyntaxList<TNode>>
        where TNode : GreenNode
    {
        private readonly GreenNode? _node;

        internal SyntaxList(GreenNode? node)
        {
            _node = node;
        }

        internal GreenNode? Node => _node;

        public int Count => _node == null ? 0 : (_node.IsList ? _node.SlotCount : 1);

        public TNode? this[int index]
        {
            get
            {
                if (_node == null)
                {
                    return null;
                }
                else if (_node.IsList)
                {
                    LorettaDebug.Assert(index >= 0);
                    LorettaDebug.Assert(index <= _node.SlotCount);

                    return (TNode?) _node.GetSlot(index);
                }
                else if (index == 0)
                {
                    return (TNode?) _node;
                }
                else
                {
                    throw ExceptionUtilities.Unreachable;
                }
            }
        }

        internal TNode GetRequiredItem(int index)
        {
            var node = this[index];
            LorettaDebug.Assert(node is object);
            return node;
        }

        internal GreenNode? ItemUntyped(int index)
        {
            LorettaDebug.Assert(_node is not null);
            var node = _node;
            if (node.IsList)
            {
                return node.GetSlot(index);
            }

            LorettaDebug.Assert(index == 0);
            return node;
        }

        public bool Any() => _node != null;

        public bool Any(int kind)
        {
            foreach (var element in this)
            {
                if (element.RawKind == kind)
                {
                    return true;
                }
            }

            return false;
        }

        internal TNode[] Nodes
        {
            get
            {
                var arr = new TNode[Count];
                for (int i = 0; i < Count; i++)
                {
                    arr[i] = GetRequiredItem(i);
                }
                return arr;
            }
        }

        public TNode? Last
        {
            get
            {
                LorettaDebug.Assert(_node is not null);
                var node = _node;
                if (node.IsList)
                {
                    return (TNode?) node.GetSlot(node.SlotCount - 1);
                }

                return (TNode?) node;
            }
        }

        public Enumerator GetEnumerator() => new(this);

        internal void CopyTo(int offset, ArrayElement<GreenNode>[] array, int arrayOffset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                array[arrayOffset + i].Value = GetRequiredItem(i + offset);
            }
        }

        public static bool operator ==(SyntaxList<TNode> left, SyntaxList<TNode> right) =>
            left._node == right._node;

        public static bool operator !=(SyntaxList<TNode> left, SyntaxList<TNode> right) =>
            left._node != right._node;

        public bool Equals(SyntaxList<TNode> other) => _node == other._node;

        public override bool Equals(object? obj) => (obj is SyntaxList<TNode> list) && Equals(list);

        public override int GetHashCode() => _node != null ? _node.GetHashCode() : 0;

        public SeparatedSyntaxList<TOther> AsSeparatedList<TOther>() where TOther : GreenNode =>
            new(this);

        public static implicit operator SyntaxList<TNode>(TNode node) => new(node);

        public static implicit operator SyntaxList<TNode>(SyntaxList<GreenNode> nodes) => new(nodes._node);

        public static implicit operator SyntaxList<GreenNode>(SyntaxList<TNode> nodes) => new(nodes.Node);
    }
}
