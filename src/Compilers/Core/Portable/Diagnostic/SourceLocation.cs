// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// A program location in source code.
    /// </summary>
    internal sealed class SourceLocation : Location, IEquatable<SourceLocation?>
    {
        private readonly SyntaxTree _syntaxTree;
        private readonly TextSpan _span;

        public SourceLocation(SyntaxTree syntaxTree, TextSpan span)
        {
            _syntaxTree = syntaxTree;
            _span = span;
        }

        public SourceLocation(SyntaxNode node)
            : this(node.SyntaxTree, node.Span)
        {
        }

        public SourceLocation(in SyntaxToken token)
            : this(token.SyntaxTree!, token.Span)
        {
        }

        public SourceLocation(in SyntaxNodeOrToken nodeOrToken)
            : this(nodeOrToken.SyntaxTree!, nodeOrToken.Span)
        {
            LorettaDebug.Assert(nodeOrToken.SyntaxTree is not null);
        }

        public SourceLocation(in SyntaxTrivia trivia)
            : this(trivia.SyntaxTree!, trivia.Span)
        {
            LorettaDebug.Assert(trivia.SyntaxTree is not null);
        }

        public SourceLocation(SyntaxReference syntaxRef)
            : this(syntaxRef.SyntaxTree, syntaxRef.Span)
        {
            // If we're using a syntaxref, we don't have a node in hand, so we couldn't get equality
            // on syntax node, so associatedNodeOpt shouldn't be set. We never use this constructor
            // when binding executable code anywhere, so it has no use.
        }

        public override LocationKind Kind => LocationKind.SourceFile;

        public override TextSpan SourceSpan => _span;

        public override SyntaxTree SourceTree => _syntaxTree;

        public override FileLinePositionSpan GetLineSpan()
        {
            // If there's no syntax tree (e.g. because we're binding speculatively),
            // then just return an invalid span.
            if (_syntaxTree == null)
            {
                FileLinePositionSpan result = default;
                LorettaDebug.Assert(!result.IsValid);
                return result;
            }

            return _syntaxTree.GetLineSpan(_span);
        }

        public bool Equals(SourceLocation? other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other != null && other._syntaxTree == _syntaxTree && other._span == _span;
        }

        public override bool Equals(object? obj) => Equals(obj as SourceLocation);

        public override int GetHashCode() => Hash.Combine(_syntaxTree, _span.GetHashCode());

#pragma warning disable IDE0079 // Remove unnecessary suppression
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1845:Use span-based 'string.Concat'", Justification = "This is only used for the debugger.")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
        protected override string GetDebuggerDisplay() => base.GetDebuggerDisplay() + "\"" + _syntaxTree.ToString().Substring(_span.Start, _span.Length) + "\"";
    }
}
