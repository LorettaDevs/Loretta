namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// The base class for syntax visitors.
    /// </summary>
    public abstract partial class SyntaxVisitor
    {
        /// <summary>
        /// Visits the provided node.
        /// </summary>
        /// <param name="node">The node to be visited.</param>
        public virtual void Visit ( SyntaxNode? node )
        {
            if ( node is not null )
                node.Accept ( this );
        }

        /// <summary>
        /// Visits a <see cref="SyntaxToken"/>.
        /// </summary>
        /// <param name="token">The token being visited.</param>
        public virtual void VisitToken ( SyntaxToken token ) => this.DefaultVisit ( token );

        /// <summary>
        /// The default visit method.
        /// All node visitor methods fall back to this method by default.
        /// </summary>
        /// <param name="node">The node being visited.</param>
        public virtual void DefaultVisit ( SyntaxNode node )
        {
        }
    }

    /// <summary>
    /// The base class for syntax visitors with a return value.
    /// </summary>
    /// <typeparam name="TReturn"></typeparam>
    public abstract partial class SyntaxVisitor<TReturn>
    {
        /// <summary>
        /// Visits the provided node.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns></returns>
        public virtual TReturn? Visit ( SyntaxNode? node )
        {
            if ( node is not null )
                return node.Accept ( this );
            return default;
        }

        /// <summary>
        /// Visits a <see cref="SyntaxToken"/>.
        /// </summary>
        /// <param name="token">The token being visited.</param>
        /// <returns></returns>
        public virtual TReturn? VisitToken ( SyntaxToken token ) => this.DefaultVisit ( token );

        /// <summary>
        /// The default visit method.
        /// All node visitor methods fall back to this method.
        /// </summary>
        /// <param name="node">The node being visited.</param>
        /// <returns></returns>
        public virtual TReturn? DefaultVisit ( SyntaxNode node ) => default;
    }
}
