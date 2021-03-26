// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// this is a basic do-nothing implementation of a syntax reference
    /// </summary>
    internal class SimpleSyntaxReference : SyntaxReference
    {
        private readonly SyntaxNode _node;

        internal SimpleSyntaxReference(SyntaxNode node)
        {
            _node = node;
        }

        public override SyntaxTree SyntaxTree => _node.SyntaxTree;

        public override TextSpan Span => _node.Span;

        public override SyntaxNode GetSyntax(CancellationToken cancellationToken) => _node;
    }
}
