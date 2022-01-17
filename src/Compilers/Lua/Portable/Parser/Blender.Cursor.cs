// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal partial struct Blender
    {
        /// <summary>
        /// THe cursor represents a location in the tree that we can move around to indicate where
        /// we are in the original tree as we're incrementally parsing.  When it is at a node or
        /// token, it can either move forward to that entity's next sibling.  It can also move down
        /// to a node's first child or first token.
        ///
        /// Once the cursor hits the end of file, it's done.  Note: the cursor will skip any other
        /// zero length nodes in the tree.
        /// </summary>
        private struct Cursor
        {
            public readonly SyntaxNodeOrToken CurrentNodeOrToken;
            private readonly int _indexInParent;

            private Cursor(SyntaxNodeOrToken node, int indexInParent)
            {
                CurrentNodeOrToken = node;
                _indexInParent = indexInParent;
            }

            public static Cursor FromRoot(Lua.LuaSyntaxNode node) => new(node, indexInParent: 0);

            public bool IsFinished => CurrentNodeOrToken.Kind() is SyntaxKind.None or SyntaxKind.EndOfFileToken;

            private static bool IsNonZeroWidthOrIsEndOfFile(SyntaxNodeOrToken token) => token.Kind() == SyntaxKind.EndOfFileToken || token.FullWidth != 0;

            public Cursor MoveToNextSibling()
            {
                if (CurrentNodeOrToken.Parent != null)
                {
                    // First, look to the nodes to the right of this one in our parent's child list
                    // to get the next sibling.
                    var siblings = CurrentNodeOrToken.Parent.ChildNodesAndTokens();
                    for (int i = _indexInParent + 1, n = siblings.Count; i < n; i++)
                    {
                        var sibling = siblings[i];
                        if (IsNonZeroWidthOrIsEndOfFile(sibling))
                        {
                            return new Cursor(sibling, i);
                        }
                    }

                    // We're at the end of this sibling chain.  Walk up to the parent and see who is
                    // the next sibling of that.
                    return MoveToParent().MoveToNextSibling();
                }

                return default(Cursor);
            }

            private Cursor MoveToParent()
            {
                var parent = CurrentNodeOrToken.Parent;
                var index = IndexOfNodeInParent(parent);
                return new Cursor(parent, index);
            }

            private static int IndexOfNodeInParent(SyntaxNode node)
            {
                if (node.Parent == null)
                {
                    return 0;
                }

                var children = node.Parent.ChildNodesAndTokens();
                var index = SyntaxNodeOrToken.GetFirstChildIndexSpanningPosition(children, ((Lua.LuaSyntaxNode) node).Position);
                for (int i = index, n = children.Count; i < n; i++)
                {
                    var child = children[i];
                    if (child == node)
                    {
                        return i;
                    }
                }

                throw ExceptionUtilities.Unreachable;
            }

            public Cursor MoveToFirstChild()
            {
                LorettaDebug.Assert(CurrentNodeOrToken.IsNode);

                // Just try to get the first node directly.  This is faster than getting the list of
                // child nodes and tokens (which forces all children to be enumerated for the sake
                // of counting.  It should always be safe to index the 0th element of a node.  But
                // just to make sure that this is not a problem, we verify that the slot count of the
                // node is greater than 0.
                var node = CurrentNodeOrToken.AsNode();

                if (node.SlotCount > 0)
                {
                    var child = ChildSyntaxList.ItemInternal(node, 0);
                    if (IsNonZeroWidthOrIsEndOfFile(child))
                    {
                        return new Cursor(child, 0);
                    }
                }

                // Fallback to enumerating all children.
                var index = 0;
                foreach (var child in CurrentNodeOrToken.ChildNodesAndTokens())
                {
                    if (IsNonZeroWidthOrIsEndOfFile(child))
                    {
                        return new Cursor(child, index);
                    }

                    index++;
                }

                return new Cursor();
            }

            public Cursor MoveToFirstToken()
            {
                var cursor = this;
                if (!cursor.IsFinished)
                {
                    for (var node = cursor.CurrentNodeOrToken; node.Kind() != SyntaxKind.None && !SyntaxFacts.IsToken(node.Kind()); node = cursor.CurrentNodeOrToken)
                    {
                        cursor = cursor.MoveToFirstChild();
                    }
                }

                return cursor;
            }
        }
    }
}
