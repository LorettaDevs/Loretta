using System.Collections.Generic;
using System.Linq;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// The base class for all syntax nodes.
    /// </summary>
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
                TextSpan first = this.GetChildren ( ).First ( ).Span;
                TextSpan last = this.GetChildren ( ).Last ( ).Span;
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
                TextSpan first = this.GetChildren ( ).First ( ).FullSpan;
                TextSpan last = this.GetChildren ( ).Last ( ).FullSpan;
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

        /// <summary>
        /// Retrieves all immediate children from this node.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<SyntaxNode> GetChildren ( );

        /// <summary>
        /// Returns the last token of the tree rooted by this node.
        /// </summary>
        /// <returns></returns>
        public virtual SyntaxToken GetLastToken ( )
        {
            if ( this is SyntaxToken token )
                return token;

            return this.GetChildren ( ).Last ( ).GetLastToken ( );
        }
    }
}
