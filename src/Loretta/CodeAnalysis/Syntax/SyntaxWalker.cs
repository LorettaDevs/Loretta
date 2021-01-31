namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// The base class for algorithms that walk over nodes.
    /// </summary>
    public abstract class SyntaxWalker : SyntaxVisitor
    {
        /// <inheritdoc/>
        public override void VisitToken ( SyntaxToken token )
        {
            foreach ( SyntaxTrivia leading in token.LeadingTrivia )
                this.VisitTrivia ( leading );
            foreach ( SyntaxTrivia trailing in token.TrailingTrivia )
                this.VisitTrivia ( trailing );
        }

        /// <summary>
        /// Visits a <see cref="SyntaxTrivia"/>.
        /// </summary>
        /// <param name="trivia"></param>
        public virtual void VisitTrivia ( SyntaxTrivia trivia )
        {
        }
    }
}
