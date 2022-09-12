// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Loretta.CodeAnalysis.Syntax.InternalSyntax;
using Xunit;
using InternalSyntax = Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax;

namespace Loretta.CodeAnalysis.Lua.UnitTests
{
    public partial class GreenNodeTests
    {
        private static CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<T> SeparatedSyntaxList<T>(params T[] args)
            where T : GreenNode
        {
            if (args.Length < 1)
                return new CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<T>();

            var builder = new SeparatedSyntaxListBuilder<T>(args.Length);
            foreach (var arg in args)
                builder.Add(arg);
            return builder.ToList();
        }

        private static CodeAnalysis.Syntax.InternalSyntax.SyntaxList<T> SyntaxList<T>(params T[] args)
            where T : GreenNode
        {
            if (args.Length < 1)
                return new CodeAnalysis.Syntax.InternalSyntax.SyntaxList<T>();

            var builder = new SyntaxListBuilder<T>(args.Length);
            foreach (var arg in args)
                builder.Add(arg);
            return builder.ToList();
        }

        private static void AttachAndCheckDiagnostics(InternalSyntax.LuaSyntaxNode node)
        {
            var nodeWithDiags = node.SetDiagnostics(new DiagnosticInfo[] { new LuaDiagnosticInfo(ErrorCode.ERR_CannotBeAssignedTo) });
            var diags = nodeWithDiags.GetDiagnostics();

            Assert.NotEqual(node, nodeWithDiags);
            var diag = Assert.Single(diags);
            Assert.Equal(ErrorCode.ERR_CannotBeAssignedTo, (ErrorCode) diag.Code);
        }

        private class TokenDeleteRewriter : InternalSyntax.LuaSyntaxRewriter
        {
            public override InternalSyntax.LuaSyntaxNode VisitToken(InternalSyntax.SyntaxToken token) =>
                InternalSyntax.SyntaxFactory.MissingToken(token.Kind);
        }

        private class IdentityRewriter : InternalSyntax.LuaSyntaxRewriter
        {
            protected override InternalSyntax.LuaSyntaxNode DefaultVisit(InternalSyntax.LuaSyntaxNode node) => node;
        }
    }
}
