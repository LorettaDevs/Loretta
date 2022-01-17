// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Syntax.InternalSyntax
{
    internal class SyntaxListPool
    {
        private ArrayElement<SyntaxListBuilder?>[] _freeList = new ArrayElement<SyntaxListBuilder?>[10];
        private int _freeIndex;

#if DEBUG
        private readonly List<SyntaxListBuilder> _allocated = new();
#endif

        internal SyntaxListPool()
        {
        }

        internal SyntaxListBuilder Allocate()
        {
            SyntaxListBuilder item;
            if (_freeIndex > 0)
            {
                _freeIndex--;
                item = _freeList[_freeIndex].Value!;
                _freeList[_freeIndex].Value = null;
            }
            else
            {
                item = new SyntaxListBuilder(10);
            }

#if DEBUG
            LorettaDebug.Assert(!_allocated.Contains(item));
            _allocated.Add(item);
#endif
            return item;
        }

        internal SyntaxListBuilder<TNode> Allocate<TNode>() where TNode : GreenNode
        {
            return new SyntaxListBuilder<TNode>(Allocate());
        }

        internal SeparatedSyntaxListBuilder<TNode> AllocateSeparated<TNode>() where TNode : GreenNode
        {
            return new SeparatedSyntaxListBuilder<TNode>(Allocate());
        }

        internal void Free<TNode>(in SeparatedSyntaxListBuilder<TNode> item) where TNode : GreenNode
        {
            LorettaDebug.Assert(item.UnderlyingBuilder is not null);
            Free(item.UnderlyingBuilder);
        }

        internal void Free(SyntaxListBuilder item)
        {
            item.Clear();
            if (_freeIndex >= _freeList.Length)
            {
                Grow();
            }
#if DEBUG
            LorettaDebug.Assert(_allocated.Contains(item));

            _allocated.Remove(item);
#endif
            _freeList[_freeIndex].Value = item;
            _freeIndex++;
        }

        private void Grow()
        {
            var tmp = new ArrayElement<SyntaxListBuilder?>[_freeList.Length * 2];
            Array.Copy(_freeList, tmp, _freeList.Length);
            _freeList = tmp;
        }

        public SyntaxList<TNode> ToListAndFree<TNode>(SyntaxListBuilder<TNode> item)
            where TNode : GreenNode
        {
            var list = item.ToList();
            Free(item);
            return list;
        }

        public SeparatedSyntaxList<TNode> ToListAndFree<TNode>(in SeparatedSyntaxListBuilder<TNode> item)
            where TNode : GreenNode
        {
            var list = item.ToList();
            Free(item);
            return list;
        }
    }
}
