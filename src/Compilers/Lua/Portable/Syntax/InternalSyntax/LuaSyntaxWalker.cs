using System;
using System.Collections.Generic;
using System.Text;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal class LuaSyntaxWalker : LuaSyntaxVisitor
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
            if (depth == SyntaxWalkerDepth.StructuredTrivia)
                throw new NotSupportedException();
            Depth = depth;
        }

        private int _recursionDepth;

        /// <summary>
        /// Called when the syntax walker visits a node.
        /// </summary>
        /// <param name="node"></param>
        public override void Visit(LuaSyntaxNode node)
        {
            if (node != null)
            {
                _recursionDepth++;
                StackGuard.EnsureSufficientExecutionStack(_recursionDepth);

                node.Accept(this);

                _recursionDepth--;
            }
        }

        /// <summary>
        /// Called when the walker walks into a node.
        /// </summary>
        /// <param name="node"></param>
        protected override void DefaultVisit(LuaSyntaxNode node)
        {
            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.IsToken)
                {
                    if (Depth >= SyntaxWalkerDepth.Token)
                    {
                        Visit((SyntaxToken) child);
                    }
                }
                else
                {
                    if (Depth >= SyntaxWalkerDepth.Node)
                    {
                        Visit((LuaSyntaxNode) child);
                    }
                }
            }
        }

        /// <summary>
        /// Called when the walker visits a token.
        /// </summary>
        /// <param name="token"></param>
        public override void VisitToken(SyntaxToken token)
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
                    VisitTrivia((SyntaxTrivia) tr);
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
                    VisitTrivia((SyntaxTrivia) tr);
                }
            }
        }
    }
}
