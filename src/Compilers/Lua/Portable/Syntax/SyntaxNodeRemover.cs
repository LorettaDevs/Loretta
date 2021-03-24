// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Loretta.CodeAnalysis.Syntax;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax
{
    internal static class SyntaxNodeRemover
    {
        internal static TRoot? RemoveNodes<TRoot>(TRoot root,
                IEnumerable<SyntaxNode> nodes,
                SyntaxRemoveOptions options)
            where TRoot : SyntaxNode
        {
            if (nodes == null)
            {
                return root;
            }

            var nodeArray = nodes.ToArray();

            if (nodeArray.Length == 0)
            {
                return root;
            }

            var remover = new SyntaxRemover(nodes.ToArray(), options);
            var result = remover.Visit(root);

            var residualTrivia = remover.ResidualTrivia;

            // the result of the SyntaxRemover will be null when the root node is removed.
            if (result != null && residualTrivia.Count > 0)
            {
                result = result.WithTrailingTrivia(result.GetTrailingTrivia().Concat(residualTrivia));
            }

            return (TRoot?) result;
        }

        private class SyntaxRemover : LuaSyntaxRewriter
        {
            private readonly HashSet<SyntaxNode> _nodesToRemove;
            private readonly SyntaxRemoveOptions _options;
            private readonly TextSpan _searchSpan;
            private readonly SyntaxTriviaListBuilder _residualTrivia;

            public SyntaxRemover(
                SyntaxNode[] nodesToRemove,
                SyntaxRemoveOptions options)
                : base(nodesToRemove.Any(n => n.IsPartOfStructuredTrivia()))
            {
                _nodesToRemove = new HashSet<SyntaxNode>(nodesToRemove);
                _options = options;
                _searchSpan = ComputeTotalSpan(nodesToRemove);
                _residualTrivia = SyntaxTriviaListBuilder.Create();
            }

            private static TextSpan ComputeTotalSpan(SyntaxNode[] nodes)
            {
                var span0 = nodes[0].FullSpan;
                var start = span0.Start;
                var end = span0.End;

                for (var i = 1; i < nodes.Length; i++)
                {
                    var span = nodes[i].FullSpan;
                    start = Math.Min(start, span.Start);
                    end = Math.Max(end, span.End);
                }

                return new TextSpan(start, end - start);
            }

            internal SyntaxTriviaList ResidualTrivia
            {
                get
                {
                    if (_residualTrivia != null)
                    {
                        return _residualTrivia.ToList();
                    }
                    else
                    {
                        return default(SyntaxTriviaList);
                    }
                }
            }

            private void AddResidualTrivia(SyntaxTriviaList trivia, bool requiresNewLine = false)
            {
                if (requiresNewLine)
                {
                    AddEndOfLine(GetEndOfLine(trivia) ?? SyntaxFactory.CarriageReturnLineFeed);
                }

                _residualTrivia.Add(trivia);
            }

            private void AddEndOfLine(SyntaxTrivia? eolTrivia)
            {
                if (!eolTrivia.HasValue)
                {
                    return;
                }

                if (_residualTrivia.Count == 0 || !IsEndOfLine(_residualTrivia[_residualTrivia.Count - 1]))
                {
                    _residualTrivia.Add(eolTrivia.Value);
                }
            }

            /// <summary>
            /// Returns whether the specified <see cref="SyntaxTrivia"/> token is also the end of the line.  This will
            /// be true for <see cref="SyntaxKind.EndOfLineTrivia"/>, <see cref="SyntaxKind.SingleLineCommentTrivia"/>,
            /// and all preprocessor directives.
            /// </summary>
            private static bool IsEndOfLine(SyntaxTrivia trivia)
            {
                return trivia.Kind() == SyntaxKind.EndOfLineTrivia
                    || trivia.Kind() == SyntaxKind.SingleLineCommentTrivia
                    || trivia.IsDirective;
            }

            /// <summary>
            /// Returns the first end of line found in a <see cref="SyntaxTriviaList"/>.
            /// </summary>
            private static SyntaxTrivia? GetEndOfLine(SyntaxTriviaList list)
            {
                foreach (var trivia in list)
                {
                    if (trivia.Kind() == SyntaxKind.EndOfLineTrivia)
                    {
                        return trivia;
                    }
                }

                return null;
            }

            private bool IsForRemoval(SyntaxNode node) =>
                _nodesToRemove.Contains(node);

            private bool ShouldVisit(SyntaxNode node) =>
                node.FullSpan.IntersectsWith(_searchSpan)
                || (_residualTrivia != null && _residualTrivia.Count > 0);

            [return: NotNullIfNotNull("node")]
            public override SyntaxNode? Visit(SyntaxNode? node)
            {
                var result = node;

                if (node != null)
                {
                    if (IsForRemoval(node))
                    {
                        AddTrivia(node);
                        result = null;
                    }
                    else if (ShouldVisit(node))
                    {
                        result = base.Visit(node);
                    }
                }

                return result;
            }

            public override SyntaxToken VisitToken(SyntaxToken token)
            {
                var result = token;

                // only bother visiting trivia if we are removing a node in structured trivia
                if (VisitIntoStructuredTrivia)
                {
                    result = base.VisitToken(token);
                }

                // the next token gets the accrued trivia.
                if (result.Kind() != SyntaxKind.None && _residualTrivia != null && _residualTrivia.Count > 0)
                {
                    _residualTrivia.Add(result.LeadingTrivia);
                    result = result.WithLeadingTrivia(_residualTrivia.ToList());
                    _residualTrivia.Clear();
                }

                return result;
            }

            // deal with separated lists and removal of associated separators
            public override SeparatedSyntaxList<TNode> VisitList<TNode>(SeparatedSyntaxList<TNode> list)
            {
                var withSeps = list.GetWithSeparators();
                var removeNextSeparator = false;

                SyntaxNodeOrTokenListBuilder? alternate = null;
                for (int i = 0, n = withSeps.Count; i < n; i++)
                {
                    var item = withSeps[i];
                    SyntaxNodeOrToken visited;

                    if (item.IsToken) // separator
                    {
                        if (removeNextSeparator)
                        {
                            removeNextSeparator = false;
                            visited = default(SyntaxNodeOrToken);
                        }
                        else
                        {
                            visited = VisitListSeparator(item.AsToken());
                        }
                    }
                    else
                    {
                        var node = (TNode) item.AsNode()!;

                        if (IsForRemoval(node))
                        {
                            if (alternate == null)
                            {
                                alternate = new SyntaxNodeOrTokenListBuilder(n);
                                alternate.Add(withSeps, 0, i);
                            }

                            CommonSyntaxNodeRemover.GetSeparatorInfo(
                                withSeps, i, (int) SyntaxKind.EndOfLineTrivia,
                                out var nextTokenIsSeparator, out var nextSeparatorBelongsToNode);

                            if (!nextSeparatorBelongsToNode &&
                                alternate.Count > 0 &&
                                alternate[alternate.Count - 1].IsToken)
                            {
                                var separator = alternate[alternate.Count - 1].AsToken();
                                AddTrivia(separator, node);
                                alternate.RemoveLast();
                            }
                            else if (nextTokenIsSeparator)
                            {
                                var separator = withSeps[i + 1].AsToken();
                                AddTrivia(node, separator);
                                removeNextSeparator = true;
                            }
                            else
                            {
                                AddTrivia(node);
                            }

                            visited = default;
                        }
                        else
                        {
                            visited = VisitListElement(node);
                        }
                    }

                    if (item != visited && alternate == null)
                    {
                        alternate = new SyntaxNodeOrTokenListBuilder(n);
                        alternate.Add(withSeps, 0, i);
                    }

                    if (alternate != null && visited.Kind() != SyntaxKind.None)
                    {
                        alternate.Add(visited);
                    }
                }

                if (alternate != null)
                {
                    return alternate.ToList().AsSeparatedList<TNode>();
                }

                return list;
            }

            private void AddTrivia(SyntaxNode node)
            {
                if ((_options & SyntaxRemoveOptions.KeepLeadingTrivia) != 0)
                {
                    AddResidualTrivia(node.GetLeadingTrivia());
                }
                else if ((_options & SyntaxRemoveOptions.KeepEndOfLine) != 0)
                {
                    AddEndOfLine(GetEndOfLine(node.GetLeadingTrivia()));
                }

                if ((_options & SyntaxRemoveOptions.KeepTrailingTrivia) != 0)
                {
                    AddResidualTrivia(node.GetTrailingTrivia());
                }
                else if ((_options & SyntaxRemoveOptions.KeepEndOfLine) != 0)
                {
                    AddEndOfLine(GetEndOfLine(node.GetTrailingTrivia()));
                }

                if ((_options & SyntaxRemoveOptions.AddElasticMarker) != 0)
                {
                    AddResidualTrivia(SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));
                }
            }

            private void AddTrivia(SyntaxToken token, SyntaxNode node)
            {
                RoslynDebug.Assert(node.Parent is not null);
                if ((_options & SyntaxRemoveOptions.KeepLeadingTrivia) != 0)
                {
                    AddResidualTrivia(token.LeadingTrivia);
                    AddResidualTrivia(token.TrailingTrivia);
                    AddResidualTrivia(node.GetLeadingTrivia());
                }
                else if ((_options & SyntaxRemoveOptions.KeepEndOfLine) != 0)
                {
                    // For retrieving an EOL we don't need to check the node leading trivia as
                    // it can be always retrieved from the token trailing trivia, if one exists.
                    var eol = GetEndOfLine(token.LeadingTrivia) ??
                              GetEndOfLine(token.TrailingTrivia);
                    AddEndOfLine(eol);
                }

                if ((_options & SyntaxRemoveOptions.KeepTrailingTrivia) != 0)
                {
                    AddResidualTrivia(node.GetTrailingTrivia());
                }
                else if ((_options & SyntaxRemoveOptions.KeepEndOfLine) != 0)
                {
                    AddEndOfLine(GetEndOfLine(node.GetTrailingTrivia()));
                }

                if ((_options & SyntaxRemoveOptions.AddElasticMarker) != 0)
                {
                    AddResidualTrivia(SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));
                }
            }

            private void AddTrivia(SyntaxNode node, SyntaxToken token)
            {
                RoslynDebug.Assert(node.Parent is not null);
                if ((_options & SyntaxRemoveOptions.KeepLeadingTrivia) != 0)
                {
                    AddResidualTrivia(node.GetLeadingTrivia());
                }
                else if ((_options & SyntaxRemoveOptions.KeepEndOfLine) != 0)
                {
                    AddEndOfLine(GetEndOfLine(node.GetLeadingTrivia()));
                }

                if ((_options & SyntaxRemoveOptions.KeepTrailingTrivia) != 0)
                {
                    AddResidualTrivia(node.GetTrailingTrivia());
                    AddResidualTrivia(token.LeadingTrivia);
                    AddResidualTrivia(token.TrailingTrivia);
                }
                else if ((_options & SyntaxRemoveOptions.KeepEndOfLine) != 0)
                {
                    // For retrieving an EOL we don't need to check the token leading trivia as
                    // it can be always retrieved from the node trailing trivia, if one exists.
                    var eol = GetEndOfLine(node.GetTrailingTrivia()) ??
                              GetEndOfLine(token.TrailingTrivia);
                    AddEndOfLine(eol);
                }

                if ((_options & SyntaxRemoveOptions.AddElasticMarker) != 0)
                {
                    AddResidualTrivia(SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));
                }
            }

            private TextSpan GetRemovedSpan(TextSpan span, TextSpan fullSpan)
            {
                var removedSpan = fullSpan;

                if ((_options & SyntaxRemoveOptions.KeepLeadingTrivia) != 0)
                {
                    removedSpan = TextSpan.FromBounds(span.Start, removedSpan.End);
                }

                if ((_options & SyntaxRemoveOptions.KeepTrailingTrivia) != 0)
                {
                    removedSpan = TextSpan.FromBounds(removedSpan.Start, span.End);
                }

                return removedSpan;
            }
        }
    }
}
