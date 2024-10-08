﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Syntax
{
    internal class SyntaxTriviaListBuilder
    {
        private SyntaxTrivia[] _nodes;
        private int _count;

        public SyntaxTriviaListBuilder(int size)
        {
            _nodes = new SyntaxTrivia[size];
        }

        public static SyntaxTriviaListBuilder Create() => new(4);

        public static SyntaxTriviaList Create(IEnumerable<SyntaxTrivia>? trivia)
        {
            if (trivia == null)
            {
                return new SyntaxTriviaList();
            }

            var builder = Create();
            builder.AddRange(trivia);
            return builder.ToList();
        }

        public int Count => _count;

        public void Clear() => _count = 0;

        public SyntaxTrivia this[int index]
        {
            get
            {
                if (index < 0 || index > _count)
                {
                    throw new IndexOutOfRangeException();
                }

                return _nodes[index];
            }
        }

        public void AddRange(IEnumerable<SyntaxTrivia>? items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    Add(item);
                }
            }
        }

        public SyntaxTriviaListBuilder Add(SyntaxTrivia item)
        {
            if (_count >= _nodes.Length)
            {
                Grow(_count == 0 ? 8 : _nodes.Length * 2);
            }

            _nodes[_count++] = item;
            return this;
        }

        public void Add(SyntaxTrivia[] items) => Add(items, 0, items.Length);

        public void Add(SyntaxTrivia[] items, int offset, int length)
        {
            if (_count + length > _nodes.Length)
            {
                Grow(_count + length);
            }

            Array.Copy(items, offset, _nodes, _count, length);
            _count += length;
        }

        public void Add(in SyntaxTriviaList list) => Add(list, 0, list.Count);

        public void Add(in SyntaxTriviaList list, int offset, int length)
        {
            if (_count + length > _nodes.Length)
            {
                Grow(_count + length);
            }

            list.CopyTo(offset, _nodes, _count, length);
            _count += length;
        }

        private void Grow(int size)
        {
            var tmp = new SyntaxTrivia[size];
            Array.Copy(_nodes, tmp, _nodes.Length);
            _nodes = tmp;
        }

        public static implicit operator SyntaxTriviaList(SyntaxTriviaListBuilder builder) => builder.ToList();

        public SyntaxTriviaList ToList()
        {
            if (_count > 0)
            {
                switch (_count)
                {
                    case 1:
                        return new SyntaxTriviaList(default, _nodes[0].UnderlyingNode, position: 0, index: 0);
                    case 2:
                        return new SyntaxTriviaList(default,
                            InternalSyntax.SyntaxList.List(
                            _nodes[0].UnderlyingNode!,
                            _nodes[1].UnderlyingNode!), position: 0, index: 0);
                    case 3:
                        return new SyntaxTriviaList(default,
                            InternalSyntax.SyntaxList.List(
                                _nodes[0].UnderlyingNode!,
                                _nodes[1].UnderlyingNode!,
                                _nodes[2].UnderlyingNode!),
                            position: 0, index: 0);
                    default:
                    {
                        var tmp = new ArrayElement<GreenNode>[_count];
                        for (int i = 0; i < _count; i++)
                        {
                            tmp[i].Value = _nodes[i].UnderlyingNode!;
                        }

                        return new SyntaxTriviaList(default,
                            InternalSyntax.SyntaxList.List(tmp), position: 0, index: 0);
                    }
                }
            }
            else
            {
                return default;
            }
        }
    }
}
