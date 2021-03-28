// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Loretta.CodeAnalysis.Syntax.InternalSyntax
{
    internal partial struct ChildSyntaxList
    {
        internal partial struct Reversed
        {
            private readonly GreenNode? _node;

            internal Reversed(GreenNode? node)
            {
                _node = node;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(_node);
            }

#if DEBUG
#pragma warning disable 618
            [Obsolete("For debugging", error: true)]
            private GreenNode[] Nodes
            {
                get
                {
                    var result = new List<GreenNode>();
                    foreach (var n in this)
                    {
                        result.Add(n);
                    }

                    return result.ToArray();
                }
            }

#pragma warning restore 618
#endif
        }
    }
}
