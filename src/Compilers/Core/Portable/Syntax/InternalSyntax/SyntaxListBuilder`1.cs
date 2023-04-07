// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Syntax.InternalSyntax
{
    internal readonly struct SyntaxListBuilder<TNode> where TNode : GreenNode
    {
        private readonly SyntaxListBuilder _builder;

        public SyntaxListBuilder(int size)
            : this(new SyntaxListBuilder(size))
        {
        }

        public static SyntaxListBuilder<TNode> Create() => new(8);

        internal SyntaxListBuilder(SyntaxListBuilder builder)
        {
            _builder = builder;
        }

        public bool IsNull => _builder == null;

        public int Count => _builder.Count;

        public TNode? this[int index]
        {
            get => (TNode?) _builder[index];

            set => _builder[index] = value;
        }

        public void Clear() => _builder.Clear();

        public SyntaxListBuilder<TNode> Add(TNode node)
        {
            _builder.Add(node);
            return this;
        }

        public void AddRange(TNode[] items, int offset, int length) =>
            _builder.AddRange(items, offset, length);

        public void AddRange(SyntaxList<TNode> nodes) => _builder.AddRange(nodes);

        public void AddRange(SyntaxList<TNode> nodes, int offset, int length) =>
            _builder.AddRange(nodes, offset, length);

        public bool Any(int kind) => _builder.Any(kind);

        public SyntaxList<TNode> ToList() => _builder.ToList();

        public GreenNode? ToListNode() => _builder.ToListNode();

        public static implicit operator SyntaxListBuilder(SyntaxListBuilder<TNode> builder) => builder._builder;

        public static implicit operator SyntaxList<TNode>(SyntaxListBuilder<TNode> builder)
        {
            if (builder._builder != null)
            {
                return builder.ToList();
            }

            return default;
        }

        public SyntaxList<TDerived> ToList<TDerived>() where TDerived : GreenNode =>
            new(ToListNode());
    }
}
