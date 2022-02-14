// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Syntax.InternalSyntax
{
    internal partial struct ChildSyntaxList
    {
        private readonly GreenNode? _node;
        private int _count;

        internal ChildSyntaxList(GreenNode node)
        {
            _node = node;
            _count = -1;
        }

        public int Count
        {
            get
            {
                if (_count == -1)
                {
                    _count = CountNodes();
                }

                return _count;
            }
        }

        private int CountNodes()
        {
            int n = 0;
            var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                n++;
            }

            return n;
        }

#if DEBUG
        [Obsolete("For debugging only", true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "For debugging only")]
        private GreenNode[] Nodes
        {
            get
            {
                var result = new GreenNode[Count];
                var i = 0;

                foreach (var n in this)
                {
                    result[i++] = n;
                }

                return result;
            }
        }
#endif

        public Enumerator GetEnumerator() => new(_node);

        public Reversed Reverse() => new(_node);
    }
}
