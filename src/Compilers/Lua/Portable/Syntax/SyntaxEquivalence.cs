// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax
{
    using Green = InternalSyntax;

    internal static class SyntaxEquivalence
    {
        internal static bool AreEquivalent(SyntaxTree? before, SyntaxTree? after, Func<SyntaxKind, bool>? ignoreChildNode, bool topLevel)
        {
            if (before == after)
            {
                return true;
            }

            if (before == null || after == null)
            {
                return false;
            }

            return AreEquivalent(before.GetRoot(), after.GetRoot(), ignoreChildNode, topLevel);
        }

        public static bool AreEquivalent(SyntaxNode? before, SyntaxNode? after, Func<SyntaxKind, bool>? ignoreChildNode, bool topLevel)
        {
            RoslynDebug.Assert(!topLevel || ignoreChildNode == null);

            if (before == null || after == null)
            {
                return before == after;
            }

            return AreEquivalentRecursive(before.Green, after.Green, ignoreChildNode, topLevel: topLevel);
        }

        public static bool AreEquivalent(SyntaxTokenList before, SyntaxTokenList after) =>
            AreEquivalentRecursive(before.Node, after.Node, ignoreChildNode: null, topLevel: false);

        public static bool AreEquivalent(SyntaxToken before, SyntaxToken after) =>
            before.RawKind == after.RawKind
            && (before.Node == null || AreTokensEquivalent(before.Node, after.Node));

        private static bool AreTokensEquivalent(GreenNode? before, GreenNode? after)
        {
            if (before is null || after is null)
            {
                return before is null && after is null;
            }

            // NOTE(cyrusn): Do we want to drill into trivia?  Can documentation ever affect the
            // global meaning of symbols?  This can happen in java with things like the "@obsolete"
            // clause in doc comment.  However, i don't know if anything like that exists in C#.

            // NOTE(cyrusn): I don't believe we need to examine skipped text.  It isn't relevant from
            // the perspective of global symbolic information.
            RoslynDebug.Assert(before.RawKind == after.RawKind);

            if (before.IsMissing != after.IsMissing)
            {
                return false;
            }

            // These are the tokens that don't have fixed text.
            return (SyntaxKind) before.RawKind switch
            {
                SyntaxKind.IdentifierToken => ((Green.SyntaxToken) before).ValueText != ((Green.SyntaxToken) after).ValueText,
                SyntaxKind.NumericLiteralToken
                or SyntaxKind.StringLiteralToken => ((Green.SyntaxToken) before).Text != ((Green.SyntaxToken) after).Text,
                _ => true,
            };
        }

        private static bool AreEquivalentRecursive(GreenNode? before, GreenNode? after, Func<SyntaxKind, bool>? ignoreChildNode, bool topLevel)
        {
            if (before == after)
            {
                return true;
            }

            if (before == null || after == null)
            {
                return false;
            }

            if (before.RawKind != after.RawKind)
            {
                return false;
            }

            if (before.IsToken)
            {
                RoslynDebug.Assert(after.IsToken);
                return AreTokensEquivalent(before, after);
            }

            if (topLevel)
            {
                // Once we get down to the body level we don't need to go any further and we can
                // consider these trees equivalent.
                switch ((SyntaxKind) before.RawKind)
                {
                    case SyntaxKind.StatementList:
                        return true;
                }
            }

            if (ignoreChildNode != null)
            {
                var e1 = before.ChildNodesAndTokens().GetEnumerator();
                var e2 = after.ChildNodesAndTokens().GetEnumerator();
                while (true)
                {
                    GreenNode? child1 = null;
                    GreenNode? child2 = null;

                    // skip ignored children:
                    while (e1.MoveNext())
                    {
                        var c = e1.Current;
                        if (c != null && (c.IsToken || !ignoreChildNode((SyntaxKind) c.RawKind)))
                        {
                            child1 = c;
                            break;
                        }
                    }

                    while (e2.MoveNext())
                    {
                        var c = e2.Current;
                        if (c != null && (c.IsToken || !ignoreChildNode((SyntaxKind) c.RawKind)))
                        {
                            child2 = c;
                            break;
                        }
                    }

                    if (child1 == null || child2 == null)
                    {
                        // false if some children remained
                        return child1 == child2;
                    }

                    if (!AreEquivalentRecursive(child1, child2, ignoreChildNode, topLevel))
                    {
                        return false;
                    }
                }
            }
            else
            {
                // simple comparison - not ignoring children

                var slotCount = before.SlotCount;
                if (slotCount != after.SlotCount)
                {
                    return false;
                }

                for (var i = 0; i < slotCount; i++)
                {
                    var child1 = before.GetSlot(i);
                    var child2 = after.GetSlot(i);

                    if (!AreEquivalentRecursive(child1, child2, ignoreChildNode, topLevel))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
