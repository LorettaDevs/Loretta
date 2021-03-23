// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Diagnostics;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal class SyntaxFirstTokenReplacer : LuaSyntaxRewriter
    {
        private readonly SyntaxToken _oldToken;
        private readonly SyntaxToken _newToken;
        private readonly int _diagnosticOffsetDelta;
        private bool _foundOldToken;

        private SyntaxFirstTokenReplacer(SyntaxToken oldToken, SyntaxToken newToken, int diagnosticOffsetDelta)
        {
            _oldToken = oldToken;
            _newToken = newToken;
            _diagnosticOffsetDelta = diagnosticOffsetDelta;
            _foundOldToken = false;
        }

        internal static TRoot Replace<TRoot>(TRoot root, SyntaxToken oldToken, SyntaxToken newToken, int diagnosticOffsetDelta)
            where TRoot : LuaSyntaxNode
        {
            var replacer = new SyntaxFirstTokenReplacer(oldToken, newToken, diagnosticOffsetDelta);
            var newRoot = (TRoot) replacer.Visit(root);
            Debug.Assert(replacer._foundOldToken);
            return newRoot;
        }

        public override LuaSyntaxNode Visit(LuaSyntaxNode node)
        {
            if (node != null)
            {
                if (!_foundOldToken)
                {
                    if (node is SyntaxToken token)
                    {
                        Debug.Assert(token == _oldToken);
                        _foundOldToken = true;
                        return _newToken; // NB: diagnostic offsets have already been updated (by SyntaxParser.AddSkippedSyntax)
                    }

                    return UpdateDiagnosticOffset(base.Visit(node), _diagnosticOffsetDelta);
                }
            }

            return node;
        }

        private static TSyntax UpdateDiagnosticOffset<TSyntax>(TSyntax node, int diagnosticOffsetDelta) where TSyntax : LuaSyntaxNode
        {
            var oldDiagnostics = node.GetDiagnostics();
            if (oldDiagnostics == null || oldDiagnostics.Length == 0)
            {
                return node;
            }

            var numDiagnostics = oldDiagnostics.Length;
            var newDiagnostics = new DiagnosticInfo[numDiagnostics];
            for (var i = 0; i < numDiagnostics; i++)
            {
                var oldDiagnostic = oldDiagnostics[i];
                newDiagnostics[i] = oldDiagnostic is not SyntaxDiagnosticInfo oldSyntaxDiagnostic ?
                    oldDiagnostic :
                    new SyntaxDiagnosticInfo(
                        oldSyntaxDiagnostic.Offset + diagnosticOffsetDelta,
                        oldSyntaxDiagnostic.Width,
                        (ErrorCode) oldSyntaxDiagnostic.Code,
                        oldSyntaxDiagnostic.Arguments);
            }
            return node.WithDiagnosticsGreen(newDiagnostics);
        }
    }
}
