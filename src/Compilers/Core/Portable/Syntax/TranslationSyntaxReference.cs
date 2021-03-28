// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Threading;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// This is a SyntaxReference implementation that lazily translates the result (SyntaxNode) of the
    /// original syntax reference to another one.
    /// </summary>
    internal abstract class TranslationSyntaxReference : SyntaxReference
    {
        private readonly SyntaxReference _reference;

        protected TranslationSyntaxReference(SyntaxReference reference)
        {
            _reference = reference;
        }

        public sealed override TextSpan Span
        {
            get { return _reference.Span; }
        }

        public sealed override SyntaxTree SyntaxTree
        {
            get { return _reference.SyntaxTree; }
        }

        public sealed override SyntaxNode GetSyntax(CancellationToken cancellationToken = default(CancellationToken))
        {
            var node = Translate(_reference, cancellationToken);
            RoslynDebug.Assert(node.SyntaxTree == _reference.SyntaxTree);
            return node;
        }

        protected abstract SyntaxNode Translate(SyntaxReference reference, CancellationToken cancellationToken);
    }
}
