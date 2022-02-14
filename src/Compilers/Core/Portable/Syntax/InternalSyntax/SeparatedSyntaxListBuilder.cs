// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Syntax.InternalSyntax
{
    // The null-suppression uses in this type are covered under the following issue to
    // better design this type around a null _builder
    // https://github.com/dotnet/roslyn/issues/40858
    internal struct SeparatedSyntaxListBuilder<TNode> where TNode : GreenNode
    {
        private readonly SyntaxListBuilder? _builder;

        public SeparatedSyntaxListBuilder(int size)
            : this(new SyntaxListBuilder(size))
        {
        }

        public static SeparatedSyntaxListBuilder<TNode> Create() => new(8);

        internal SeparatedSyntaxListBuilder(SyntaxListBuilder builder)
        {
            _builder = builder;
        }

        public bool IsNull => _builder == null;

        public int Count => _builder!.Count;

        public GreenNode? this[int index]
        {
            get => _builder![index];

            set => _builder![index] = value;
        }

        public void Clear() => _builder!.Clear();

        public void RemoveLast() => _builder!.RemoveLast();

        public SeparatedSyntaxListBuilder<TNode> Add(TNode node)
        {
            _builder!.Add(node);
            return this;
        }

        public void AddSeparator(GreenNode separatorToken) => _builder!.Add(separatorToken);

        public void AddRange(TNode[] items, int offset, int length) => _builder!.AddRange(items, offset, length);

        public void AddRange(in SeparatedSyntaxList<TNode> nodes) => _builder!.AddRange(nodes.GetWithSeparators());

        public void AddRange(in SeparatedSyntaxList<TNode> nodes, int count)
        {
            var list = nodes.GetWithSeparators();
            _builder!.AddRange(list, Count, Math.Min(count * 2, list.Count));
        }

        public bool Any(int kind) => _builder!.Any(kind);

        public SeparatedSyntaxList<TNode> ToList()
        {
            return _builder == null
                ? default
                : new SeparatedSyntaxList<TNode>(new SyntaxList<GreenNode>(_builder.ToListNode()));
        }

        /// <summary>
        /// WARN WARN WARN: This should be used with extreme caution - the underlying builder does
        /// not give any indication that it is from a separated syntax list but the constraints
        /// (node, token, node, token, ...) should still be maintained.
        /// </summary>
        /// <remarks>
        /// In order to avoid creating a separate pool of SeparatedSyntaxListBuilders, we expose
        /// our underlying SyntaxListBuilder to SyntaxListPool.
        /// </remarks>
        internal SyntaxListBuilder? UnderlyingBuilder => _builder;

        public static implicit operator SeparatedSyntaxList<TNode>(in SeparatedSyntaxListBuilder<TNode> builder) => builder.ToList();

        public static implicit operator SyntaxListBuilder?(in SeparatedSyntaxListBuilder<TNode> builder) => builder._builder;
    }
}
