// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.Syntax
{
    internal static class SyntaxReplacer
    {
        internal static SyntaxNode Replace<TNode>(
            SyntaxNode root,
            IEnumerable<TNode>? nodes = null,
            Func<TNode, TNode, SyntaxNode>? computeReplacementNode = null,
            IEnumerable<SyntaxToken>? tokens = null,
            Func<SyntaxToken, SyntaxToken, SyntaxToken>? computeReplacementToken = null,
            IEnumerable<SyntaxTrivia>? trivia = null,
            Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia>? computeReplacementTrivia = null)
            where TNode : SyntaxNode
        {
            var replacer = new Replacer<TNode>(
                nodes, computeReplacementNode,
                tokens, computeReplacementToken,
                trivia, computeReplacementTrivia);

            return replacer.HasWork ? replacer.Visit(root) : root;
        }

        internal static SyntaxToken Replace(
            SyntaxToken root,
            IEnumerable<SyntaxNode>? nodes = null,
            Func<SyntaxNode, SyntaxNode, SyntaxNode>? computeReplacementNode = null,
            IEnumerable<SyntaxToken>? tokens = null,
            Func<SyntaxToken, SyntaxToken, SyntaxToken>? computeReplacementToken = null,
            IEnumerable<SyntaxTrivia>? trivia = null,
            Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia>? computeReplacementTrivia = null)
        {
            var replacer = new Replacer<SyntaxNode>(
                nodes, computeReplacementNode,
                tokens, computeReplacementToken,
                trivia, computeReplacementTrivia);

            return replacer.HasWork ? replacer.VisitToken(root) : root;
        }

        private sealed class Replacer<TNode> : LuaSyntaxRewriter where TNode : SyntaxNode
        {
            private readonly Func<TNode, TNode, SyntaxNode>? _computeReplacementNode;
            private readonly Func<SyntaxToken, SyntaxToken, SyntaxToken>? _computeReplacementToken;
            private readonly Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia>? _computeReplacementTrivia;

            private readonly HashSet<SyntaxNode> _nodeSet;
            private readonly HashSet<SyntaxToken> _tokenSet;
            private readonly HashSet<SyntaxTrivia> _triviaSet;
            private readonly HashSet<TextSpan> _spanSet;

            private readonly TextSpan _totalSpan;
            private readonly bool     _shouldVisitTrivia;

            public Replacer(
                IEnumerable<TNode>? nodes,
                Func<TNode, TNode, SyntaxNode>? computeReplacementNode,
                IEnumerable<SyntaxToken>? tokens,
                Func<SyntaxToken, SyntaxToken, SyntaxToken>? computeReplacementToken,
                IEnumerable<SyntaxTrivia>? trivia,
                Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia>? computeReplacementTrivia)
            {
                _computeReplacementNode = computeReplacementNode;
                _computeReplacementToken = computeReplacementToken;
                _computeReplacementTrivia = computeReplacementTrivia;

                _nodeSet   = nodes != null ? [..nodes] : s_noNodes;
                _tokenSet  = tokens != null ? [..tokens] : s_noTokens;
                _triviaSet = trivia != null ? [..trivia] : s_noTrivia;

                _spanSet =
                [
                    .. _nodeSet.Select(static n => n.FullSpan).Concat(
                        _tokenSet.Select(static t => t.FullSpan).Concat(_triviaSet.Select(static t => t.FullSpan))),
                ];

                _totalSpan = ComputeTotalSpan(_spanSet);

                VisitIntoStructuredTrivia =
                    _nodeSet.Any(static n => n.IsPartOfStructuredTrivia()) ||
                    _tokenSet.Any(static t => t.IsPartOfStructuredTrivia()) ||
                    _triviaSet.Any(static t => t.IsPartOfStructuredTrivia());

                _shouldVisitTrivia = _triviaSet.Count > 0 || VisitIntoStructuredTrivia;
            }

            private static readonly HashSet<SyntaxNode>   s_noNodes  = [];
            private static readonly HashSet<SyntaxToken>  s_noTokens = [];
            private static readonly HashSet<SyntaxTrivia> s_noTrivia = [];

            public override bool VisitIntoStructuredTrivia { get; }

            public bool HasWork => _nodeSet.Count + _tokenSet.Count + _triviaSet.Count > 0;

            private static TextSpan ComputeTotalSpan(IEnumerable<TextSpan> spans)
            {
                var first = true;
                var start = 0;
                var end = 0;

                foreach (var span in spans)
                {
                    if (first)
                    {
                        start = span.Start;
                        end = span.End;
                        first = false;
                    }
                    else
                    {
                        start = Math.Min(start, span.Start);
                        end = Math.Max(end, span.End);
                    }
                }

                return new TextSpan(start, end - start);
            }

            private bool ShouldVisit(TextSpan span)
            {
                // first do quick check against total span
                return span.IntersectsWith(_totalSpan)
                       // if the node is outside the total span of the nodes to be replaced
                       // then we won't find any nodes to replace below it.
                       && _spanSet.Any(span.IntersectsWith);
            }

            [return: NotNullIfNotNull(nameof(node))]
            public override SyntaxNode? Visit(SyntaxNode? node)
            {
                if (node == null) return null;
                
                var rewritten = node;
                
                if (ShouldVisit(node.FullSpan)) rewritten = base.Visit(node);

                if (_nodeSet.Contains(node) && _computeReplacementNode != null)
                    rewritten = _computeReplacementNode((TNode) node, (TNode) rewritten);

                return rewritten;
            }

            public override SyntaxToken VisitToken(SyntaxToken token)
            {
                var rewritten = token;

                if (_shouldVisitTrivia && ShouldVisit(token.FullSpan))
                {
                    rewritten = base.VisitToken(token);
                }

                if (_tokenSet.Contains(token) && _computeReplacementToken != null)
                {
                    rewritten = _computeReplacementToken(token, rewritten);
                }

                return rewritten;
            }

            public override SyntaxTrivia VisitListElement(SyntaxTrivia trivia)
            {
                var rewritten = trivia;

                if (VisitIntoStructuredTrivia && trivia.HasStructure && ShouldVisit(trivia.FullSpan))
                {
                    rewritten = VisitTrivia(trivia);
                }

                if (_triviaSet.Contains(trivia) && _computeReplacementTrivia != null)
                {
                    rewritten = _computeReplacementTrivia(trivia, rewritten);
                }

                return rewritten;
            }
        }

        internal static SyntaxNode ReplaceNodeInList(SyntaxNode root, SyntaxNode originalNode, IEnumerable<SyntaxNode> newNodes) =>
            new NodeListEditor(originalNode, newNodes, ListEditKind.Replace).Visit(root);

        internal static SyntaxNode InsertNodeInList(SyntaxNode root, SyntaxNode nodeInList, IEnumerable<SyntaxNode> nodesToInsert, bool insertBefore) =>
            new NodeListEditor(nodeInList, nodesToInsert, insertBefore ? ListEditKind.InsertBefore : ListEditKind.InsertAfter).Visit(root);

        public static SyntaxNode ReplaceTokenInList(SyntaxNode root, SyntaxToken tokenInList, IEnumerable<SyntaxToken> newTokens) =>
            new TokenListEditor(tokenInList, newTokens, ListEditKind.Replace).Visit(root);

        public static SyntaxNode InsertTokenInList(SyntaxNode root, SyntaxToken tokenInList, IEnumerable<SyntaxToken> newTokens, bool insertBefore) =>
            new TokenListEditor(tokenInList, newTokens, insertBefore ? ListEditKind.InsertBefore : ListEditKind.InsertAfter).Visit(root);

        public static SyntaxNode ReplaceTriviaInList(SyntaxNode root, SyntaxTrivia triviaInList, IEnumerable<SyntaxTrivia> newTrivia) =>
            new TriviaListEditor(triviaInList, newTrivia, ListEditKind.Replace).Visit(root);

        public static SyntaxNode InsertTriviaInList(SyntaxNode root, SyntaxTrivia triviaInList, IEnumerable<SyntaxTrivia> newTrivia, bool insertBefore) =>
            new TriviaListEditor(triviaInList, newTrivia, insertBefore ? ListEditKind.InsertBefore : ListEditKind.InsertAfter).Visit(root);

        public static SyntaxToken ReplaceTriviaInList(SyntaxToken root, SyntaxTrivia triviaInList, IEnumerable<SyntaxTrivia> newTrivia) =>
            new TriviaListEditor(triviaInList, newTrivia, ListEditKind.Replace).VisitToken(root);

        public static SyntaxToken InsertTriviaInList(SyntaxToken root, SyntaxTrivia triviaInList, IEnumerable<SyntaxTrivia> newTrivia, bool insertBefore) =>
            new TriviaListEditor(triviaInList, newTrivia, insertBefore ? ListEditKind.InsertBefore : ListEditKind.InsertAfter).VisitToken(root);

        private enum ListEditKind
        {
            InsertBefore,
            InsertAfter,
            Replace
        }

        private static InvalidOperationException GetItemNotListElementException() => new(CodeAnalysisResources.MissingListItem);

        private abstract class BaseListEditor(
            TextSpan     elementSpan,
            ListEditKind editKind,
            bool         visitTrivia,
            bool         visitIntoStructuredTrivia
        ) : LuaSyntaxRewriter
        {
            private readonly   bool         _visitTrivia = visitTrivia || visitIntoStructuredTrivia;
            protected readonly ListEditKind EditKind     = editKind;

            public override bool VisitIntoStructuredTrivia => visitIntoStructuredTrivia;

            private bool ShouldVisit(TextSpan span)
                =>
                    // node's full span intersects with at least one node to be replaced
                    // so we need to visit node's children to find it.
                    span.IntersectsWith(elementSpan);

            [return: NotNullIfNotNull(nameof(node))]
            public override SyntaxNode? Visit(SyntaxNode? node)
            {
                var rewritten = node;

                if (node != null)
                {
                    if (ShouldVisit(node.FullSpan))
                    {
                        rewritten = base.Visit(node);
                    }
                }

                return rewritten;
            }

            public override SyntaxToken VisitToken(SyntaxToken token)
            {
                var rewritten = token;

                if (_visitTrivia && ShouldVisit(token.FullSpan))
                {
                    rewritten = base.VisitToken(token);
                }

                return rewritten;
            }

            public override SyntaxTrivia VisitListElement(SyntaxTrivia trivia)
            {
                var rewritten = trivia;

                if (VisitIntoStructuredTrivia && trivia.HasStructure && ShouldVisit(trivia.FullSpan))
                {
                    rewritten = VisitTrivia(trivia);
                }

                return rewritten;
            }
        }

        private sealed class NodeListEditor(
            SyntaxNode              originalNode,
            IEnumerable<SyntaxNode> replacementNodes,
            ListEditKind            editKind
        ) : BaseListEditor(originalNode.Span, editKind, false, originalNode.IsPartOfStructuredTrivia())
        {
            [return: NotNullIfNotNull(nameof(node))]
            public override SyntaxNode? Visit(SyntaxNode? node)
            {
                if (node == originalNode)
                {
                    throw GetItemNotListElementException();
                }

                return base.Visit(node);
            }

            public override SeparatedSyntaxList<TNode> VisitList<TNode>(SeparatedSyntaxList<TNode> list)
            {
                if (originalNode is not TNode node) return base.VisitList(list);
                
                var index = list.IndexOf(node);
                if (index < 0 || index >= list.Count) return base.VisitList(list);

                return EditKind switch
                {
                    ListEditKind.Replace      => list.ReplaceRange(node, replacementNodes.Cast<TNode>()),
                    ListEditKind.InsertAfter  => list.InsertRange(index + 1, replacementNodes.Cast<TNode>()),
                    ListEditKind.InsertBefore => list.InsertRange(index, replacementNodes.Cast<TNode>()),
                    _                         => base.VisitList(list),
                };
            }

            public override SyntaxList<TNode> VisitList<TNode>(SyntaxList<TNode> list)
            {
                if (originalNode is not TNode node) return base.VisitList(list);
                
                var index = list.IndexOf(node);
                if (index < 0 || index >= list.Count) return base.VisitList(list);
                
                return EditKind switch
                {
                    ListEditKind.Replace      => list.ReplaceRange(node, replacementNodes.Cast<TNode>()),
                    ListEditKind.InsertAfter  => list.InsertRange(index + 1, replacementNodes.Cast<TNode>()),
                    ListEditKind.InsertBefore => list.InsertRange(index, replacementNodes.Cast<TNode>()),
                    _                         => base.VisitList(list),
                };
            }
        }

        private sealed class TokenListEditor(
            SyntaxToken              originalToken,
            IEnumerable<SyntaxToken> newTokens,
            ListEditKind             editKind
        ) : BaseListEditor(originalToken.Span, editKind, false, originalToken.IsPartOfStructuredTrivia())
        {
            public override SyntaxToken VisitToken(SyntaxToken token)
            {
                if (token == originalToken)
                {
                    throw GetItemNotListElementException();
                }

                return base.VisitToken(token);
            }

            public override SyntaxTokenList VisitList(SyntaxTokenList list)
            {
                var index = list.IndexOf(originalToken);
                if (index < 0 || index >= list.Count) return base.VisitList(list);

                return EditKind switch
                {
                    ListEditKind.Replace      => list.ReplaceRange(originalToken, newTokens),
                    ListEditKind.InsertAfter  => list.InsertRange(index + 1, newTokens),
                    ListEditKind.InsertBefore => list.InsertRange(index, newTokens),
                    _                         => base.VisitList(list),
                };
            }
        }

        private sealed class TriviaListEditor(
            SyntaxTrivia              originalTrivia,
            IEnumerable<SyntaxTrivia> newTrivia,
            ListEditKind              editKind
        ) : BaseListEditor(originalTrivia.Span, editKind, true, originalTrivia.IsPartOfStructuredTrivia())
        {
            public override SyntaxTriviaList VisitList(SyntaxTriviaList list)
            {
                var index = list.IndexOf(originalTrivia);
                if (index < 0 || index >= list.Count) return base.VisitList(list);

                return EditKind switch
                {
                    ListEditKind.Replace      => list.ReplaceRange(originalTrivia, newTrivia),
                    ListEditKind.InsertAfter  => list.InsertRange(index + 1, newTrivia),
                    ListEditKind.InsertBefore => list.InsertRange(index, newTrivia),
                    _                         => base.VisitList(list),
                };
            }
        }
    }
}
