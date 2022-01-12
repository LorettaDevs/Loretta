namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    using Loretta.CodeAnalysis.Syntax.InternalSyntax;
    using Loretta.Utilities;

    internal abstract partial class LuaSyntaxRewriter : LuaSyntaxVisitor<LuaSyntaxNode>
    {
        protected bool VisitIntoStructuredTrivia { get; }

        public LuaSyntaxRewriter(bool visitIntoStructuredTrivia = false)
        {
            VisitIntoStructuredTrivia = visitIntoStructuredTrivia;
        }

        public override LuaSyntaxNode VisitToken(SyntaxToken token)
        {
            var leading = VisitList(token.LeadingTrivia);
            var trailing = VisitList(token.TrailingTrivia);

            if (leading != token.LeadingTrivia || trailing != token.TrailingTrivia)
            {
                if (leading != token.LeadingTrivia)
                {
                    token = token.TokenWithLeadingTrivia(leading.Node);
                }

                if (trailing != token.TrailingTrivia)
                {
                    token = token.TokenWithTrailingTrivia(trailing.Node);
                }
            }

            return token;
        }

        public SyntaxList<TNode> VisitList<TNode>(SyntaxList<TNode> list) where TNode : LuaSyntaxNode
        {
            SyntaxListBuilder? alternate = null;
            for (int i = 0, n = list.Count; i < n; i++)
            {
                var item = list[i];
                var visited = Visit(item);
                if (item != visited && alternate == null)
                {
                    alternate = new SyntaxListBuilder(n);
                    alternate.AddRange(list, 0, i);
                }

                if (alternate != null)
                {
                    LorettaDebug.Assert(visited != null && visited.Kind != SyntaxKind.None, "Cannot remove node using Syntax.InternalSyntax.SyntaxRewriter.");
                    alternate.Add(visited);
                }
            }

            if (alternate != null)
            {
                return alternate.ToList();
            }

            return list;
        }

        public SeparatedSyntaxList<TNode> VisitList<TNode>(SeparatedSyntaxList<TNode> list) where TNode : LuaSyntaxNode
        {
            // A separated list is filled with Lua nodes and Lua tokens.  Both of which
            // derive from InternalSyntax.LuaSyntaxNode.  So this cast is appropriately
            // typesafe.
            var withSeps = (SyntaxList<LuaSyntaxNode>) list.GetWithSeparators();
            var result = VisitList(withSeps);
            if (result != withSeps)
            {
                return result.AsSeparatedList<TNode>();
            }

            return list;
        }
    }
}
