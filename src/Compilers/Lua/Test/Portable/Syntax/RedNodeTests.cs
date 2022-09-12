// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Loretta.CodeAnalysis.Lua.UnitTests
{
    public partial class RedNodeTests
    {
        private static SeparatedSyntaxList<T> SeparatedSyntaxList<T>(params T[] args)
            where T : SyntaxNode
        {
            if (args.Length < 1)
                return new SeparatedSyntaxList<T>();

            var builder = new CodeAnalysis.Syntax.SeparatedSyntaxListBuilder<T>(args.Length);
            foreach (var arg in args)
                builder.Add(arg);
            return builder.ToList();
        }

        private static SyntaxList<T> SyntaxList<T>(params T[] args)
            where T : SyntaxNode
        {
            if (args.Length < 1)
                return new SyntaxList<T>();

            var builder = new CodeAnalysis.Syntax.SyntaxListBuilder<T>(args.Length);
            foreach (var arg in args)
                builder.Add(arg);
            return builder.ToList();
        }

        private class TokenDeleteRewriter : LuaSyntaxRewriter
        {
            public override SyntaxToken VisitToken(SyntaxToken token) =>
                SyntaxFactory.MissingToken(token.Kind());
        }

        private class IdentityRewriter : LuaSyntaxRewriter
        {
            public override SyntaxNode DefaultVisit(SyntaxNode node) => node;
        }
    }
}
