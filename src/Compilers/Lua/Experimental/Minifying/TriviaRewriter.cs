using System;
using System.Collections.Generic;
using System.Text;

namespace Loretta.CodeAnalysis.Lua.Experimental.Minifying
{
    internal class TriviaRewriter : LuaSyntaxRewriter
    {
        public static readonly TriviaRewriter Instance = new();

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            if (token.IsKind(SyntaxKind.None))
                return token;

            var nextToken = token.GetNextToken();
            if (SyntaxFacts.RequiresSeparator(token.Kind(), token.Text, nextToken.Kind(), nextToken.Text))
            {
                token = token.WithLeadingTrivia(/* none */);
                token = token.WithTrailingTrivia(SyntaxFactory.Space);
            }
            else
            {
                token = token.WithoutTrivia();
            }

            return token;
        }

    }
}
