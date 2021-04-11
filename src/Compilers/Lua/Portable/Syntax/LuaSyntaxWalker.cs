// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// Represents a <see cref="LuaSyntaxVisitor"/> that descends an entire <see cref="LuaSyntaxNode"/> graph
    /// visiting each LuaSyntaxNode and its child SyntaxNodes and <see cref="SyntaxToken"/>s in depth-first order.
    /// </summary>
    public abstract class LuaSyntaxWalker : LuaSyntaxVisitor
    {
        /// <summary>
        /// The depth up to which the walker should go into.
        /// </summary>
        protected SyntaxWalkerDepth Depth { get; }

        /// <summary>
        /// Initializes the syntax walker with hte provided depth.
        /// </summary>
        /// <param name="depth"></param>
        protected LuaSyntaxWalker(SyntaxWalkerDepth depth = SyntaxWalkerDepth.Node)
        {
            Depth = depth;
        }

        private int _recursionDepth;

        /// <summary>
        /// Called when the syntax walker visits a node.
        /// </summary>
        /// <param name="node"></param>
        public override void Visit(SyntaxNode? node)
        {
            if (node != null)
            {
                _recursionDepth++;
                StackGuard.EnsureSufficientExecutionStack(_recursionDepth);

                ((LuaSyntaxNode) node).Accept(this);

                _recursionDepth--;
            }
        }

        /// <summary>
        /// Called when the walker walks into a node.
        /// </summary>
        /// <param name="node"></param>
        public override void DefaultVisit(SyntaxNode node)
        {
            var childCnt = node.ChildNodesAndTokens().Count;
            var i = 0;

            while (i < childCnt)
            {
                var child = ChildSyntaxList.ItemInternal((LuaSyntaxNode) node, i);
                i++;

                var asNode = child.AsNode();
                if (asNode != null)
                {
                    if (Depth >= SyntaxWalkerDepth.Node)
                    {
                        Visit(asNode);
                    }
                }
                else
                {
                    if (Depth >= SyntaxWalkerDepth.Token)
                    {
                        VisitToken(child.AsToken());
                    }
                }
            }
        }

        /// <summary>
        /// Called when the walker visits a token.
        /// </summary>
        /// <param name="token"></param>
        public virtual void VisitToken(SyntaxToken token)
        {
            if (Depth >= SyntaxWalkerDepth.Trivia)
            {
                VisitLeadingTrivia(token);
                VisitTrailingTrivia(token);
            }
        }

        /// <summary>
        /// Called when the walker should visit the leading trivia of a token.
        /// </summary>
        /// <param name="token"></param>
        public virtual void VisitLeadingTrivia(SyntaxToken token)
        {
            if (token.HasLeadingTrivia)
            {
                foreach (var tr in token.LeadingTrivia)
                {
                    VisitTrivia(tr);
                }
            }
        }

        /// <summary>
        /// Called when the walker should visit the trailing trivia of a token.
        /// </summary>
        /// <param name="token"></param>
        public virtual void VisitTrailingTrivia(SyntaxToken token)
        {
            if (token.HasTrailingTrivia)
            {
                foreach (var tr in token.TrailingTrivia)
                {
                    VisitTrivia(tr);
                }
            }
        }

        /// <summary>
        /// Called when the walker visits a trivia.
        /// </summary>
        /// <param name="trivia"></param>
        public virtual void VisitTrivia(SyntaxTrivia trivia)
        {
            if (Depth >= SyntaxWalkerDepth.StructuredTrivia && trivia.HasStructure)
            {
                Visit((LuaSyntaxNode) trivia.GetStructure()!);
            }
        }
    }
}
