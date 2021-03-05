namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal abstract partial class LuaSyntaxVisitor<TResult>
    {
        public virtual TResult? Visit(LuaSyntaxNode? node)
            => node is null ? default : node.Accept(this);

        public virtual TResult? VisitToken(SyntaxToken token) => DefaultVisit(token);

        public virtual TResult? VisitTrivia(SyntaxTrivia trivia) => DefaultVisit(trivia);

        protected virtual TResult? DefaultVisit(LuaSyntaxNode node) => default;
    }

    internal abstract partial class LuaSyntaxVisitor
    {
        public virtual void Visit(LuaSyntaxNode node)
        {
            if (node is null)
                return;
            node.Accept(this);
        }

        public virtual void VisitToken(SyntaxToken token) => DefaultVisit(token);

        public virtual void VisitTrivia(SyntaxTrivia trivia) => DefaultVisit(trivia);

        protected virtual void DefaultVisit(LuaSyntaxNode node) { }
    }
}
