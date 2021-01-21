using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Syntax
{
    public abstract class SyntaxNode
    {
        private protected SyntaxNode ( SyntaxTree syntaxTree )
        {
            this.SyntaxTree = syntaxTree;
        }

        /// <summary>
        /// The <see cref="Syntax.SyntaxTree"/> this node belongs to.
        /// </summary>
        public SyntaxTree SyntaxTree { get; }

        /// <summary>
        /// This node's parent.
        /// </summary>
        public SyntaxNode? Parent => this.SyntaxTree.GetParent ( this );

        /// <summary>
        /// This node's kind.
        /// </summary>
        public abstract SyntaxKind Kind { get; }

        /// <summary>
        /// The span of this node.
        /// </summary>
        public virtual TextSpan Span
        {
            get
            {
                var first = GetChildren ( ).First ( ).Span;
                var last = GetChildren ( ).Last ( ).Span;
                return TextSpan.FromBounds ( first.Start, last.End );
            }
        }

        /// <summary>
        /// The span of this node including trivia.
        /// </summary>
        public virtual TextSpan FullSpan
        {
            get
            {
                var first = GetChildren ( ).First ( ).FullSpan;
                var last = GetChildren ( ).Last ( ).FullSpan;
                return TextSpan.FromBounds ( first.Start, last.End );
            }
        }

        /// <summary>
        /// This node's location.
        /// </summary>
        public TextLocation Location => new ( this.SyntaxTree.Text, this.Span );

        /// <summary>
        /// Returns this node and its ancestors in order.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SyntaxNode> AncestorsAndSelf ( )
        {
            SyntaxNode? node = this;
            while ( node != null )
            {
                yield return node;
                node = node.Parent;
            }
        }

        /// <summary>
        /// Returns this node ancestors in order.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SyntaxNode> Ancestors ( )
        {
            SyntaxNode? node = this.Parent;
            while ( node != null )
            {
                yield return node;
                node = node.Parent;
            }
        }

        public abstract IEnumerable<SyntaxNode> GetChildren ( );

        public virtual SyntaxToken GetLastToken ( )
        {
            return GetChildren ( ).Last ( ).GetLastToken ( );
        }
    }
}
