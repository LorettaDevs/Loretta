// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Represents a list of nodes separated by one token.
    /// May have a trailing node.
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public readonly partial struct SeparatedSyntaxList<TNode> : IEquatable<SeparatedSyntaxList<TNode>>, IReadOnlyList<TNode> where TNode : SyntaxNode
    {
        private readonly SyntaxNodeOrTokenList _list;
        private readonly int _count;
        private readonly int _separatorCount;

        internal SeparatedSyntaxList(SyntaxNodeOrTokenList list)
            : this()
        {
            Validate(list);

            // calculating counts is very cheap when list interleaves nodes and tokens
            // so lets just do it here.

            int allCount = list.Count;
            _count = (allCount + 1) >> 1;
            _separatorCount = allCount >> 1;

            _list = list;
        }

        [Conditional("DEBUG")]
        private static void Validate(SyntaxNodeOrTokenList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if ((i & 1) == 0)
                {
                    LorettaDebug.Assert(item.IsNode, "Node missing in separated list.");
                }
                else
                {
                    LorettaDebug.Assert(item.IsToken, "Separator token missing in separated list.");
                }
            }
        }

        internal SeparatedSyntaxList(SyntaxNode node, int index)
            : this(new SyntaxNodeOrTokenList(node, index))
        {
        }

        internal SyntaxNode? Node => _list.Node;

        /// <summary>
        /// The amount of nodes contained in this list.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// The amount of separators contained in this list.
        /// </summary>
        public int SeparatorCount => _separatorCount;

        /// <summary>
        /// Obtains a node from this list at the provided index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public TNode this[int index]
        {
            get
            {
                var node = _list.Node;
                if (node != null)
                {
                    if (!node.IsList)
                    {
                        if (index == 0)
                        {
                            return (TNode) node;
                        }
                    }
                    else
                    {
                        if (unchecked((uint) index < (uint) _count))
                        {
                            return (TNode) node.GetRequiredNodeSlot(index << 1);
                        }
                    }
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Gets the separator at the given index in this list.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public SyntaxToken GetSeparator(int index)
        {
            var node = _list.Node;
            if (node != null)
            {
                LorettaDebug.Assert(node.IsList, "separated list cannot be a singleton separator");
                if (unchecked((uint) index < (uint) _separatorCount))
                {
                    index = (index << 1) + 1;
                    var green = node.Green.GetRequiredSlot(index);
                    LorettaDebug.Assert(green.IsToken);
                    return new SyntaxToken(node.Parent, green, node.GetChildPosition(index), _list.index + index);
                }
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// Returns the sequence of just the separator tokens.
        /// </summary>
        public IEnumerable<SyntaxToken> GetSeparators() =>
            _list.Where(n => n.IsToken).Select(n => n.AsToken());

        /// <summary>
        /// The absolute span of the list elements in characters, including the leading and trailing trivia of the first and last elements.
        /// </summary>
        public TextSpan FullSpan => _list.FullSpan;

        /// <summary>
        /// The absolute span of the list elements in characters, not including the leading and trailing trivia of the first and last elements.
        /// </summary>
        public TextSpan Span => _list.Span;

        /// <summary>
        /// Returns the string representation of the nodes in this list including separators but not including 
        /// the first node's leading trivia and the last node or token's trailing trivia.
        /// </summary>
        /// <returns>
        /// The string representation of the nodes in this list including separators but not including 
        /// the first node's leading trivia and the last node or token's trailing trivia.
        /// </returns>
        public override string ToString() => _list.ToString();

        /// <summary>
        /// Returns the full string representation of the nodes in this list including separators, 
        /// the first node's leading trivia, and the last node or token's trailing trivia.
        /// </summary>
        /// <returns>
        /// The full string representation of the nodes in this list including separators including separators,
        /// the first node's leading trivia, and the last node or token's trailing trivia.
        /// </returns>
        public string ToFullString() => _list.ToFullString();

        /// <summary>
        /// Returns the first node in this list.
        /// </summary>
        /// <returns></returns>
        public TNode First() => this[0];

        /// <summary>
        /// Returns the first node in this list if any, otherwise returns
        /// the default value for the type of node.
        /// </summary>
        /// <returns></returns>
        public TNode? FirstOrDefault()
        {
            if (Any())
            {
                return this[0];
            }

            return null;
        }

        /// <summary>
        /// Returns the last element in this list.
        /// </summary>
        /// <returns></returns>
        public TNode Last() => this[Count - 1];

        /// <summary>
        /// Returns the last element in this list if any, otherwise returns
        /// the default value for the node type.
        /// </summary>
        /// <returns></returns>
        public TNode? LastOrDefault()
        {
            if (Any())
            {
                return this[Count - 1];
            }

            return null;
        }

        /// <summary>
        /// Returns whether this list contains the provided node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool Contains(TNode node) => IndexOf(node) >= 0;

        /// <summary>
        /// Returns the index of the provided node in this list.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>-1 if the node was not found.</returns>
        public int IndexOf(TNode node)
        {
            for (int i = 0, n = Count; i < n; i++)
            {
                if (Equals(this[i], node))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the index of the first node that passes the provided predicate in this list.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>-1 if not found.</returns>
        public int IndexOf(Func<TNode, bool> predicate)
        {
            for (int i = 0, n = Count; i < n; i++)
            {
                if (predicate(this[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        internal int IndexOf(int rawKind)
        {
            for (int i = 0, n = Count; i < n; i++)
            {
                if (this[i].RawKind == rawKind)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the index of the last node that is equal to the provided one.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>-1 if not found.</returns>
        public int LastIndexOf(TNode node)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (Equals(this[i], node))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the index of the last node that passes the provided predicate in this list.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>-1 if not found.</returns>
        public int LastIndexOf(Func<TNode, bool> predicate)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (predicate(this[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns whether this list contains any elements.
        /// </summary>
        /// <returns></returns>
        public bool Any() => _list.Any();

        /// <summary>
        /// Returns whether this list contains any elements that pass the provided predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal bool Any(Func<TNode, bool> predicate)
        {
            for (int i = 0; i < Count; i++)
            {
                if (predicate(this[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the entire list including the separators.
        /// </summary>
        /// <returns></returns>
        public SyntaxNodeOrTokenList GetWithSeparators() => _list;

        /// <summary>
        /// Checks whether a list is equal to another.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(SeparatedSyntaxList<TNode> left, SeparatedSyntaxList<TNode> right) =>
            left.Equals(right);

        /// <summary>
        /// Checks whether two lists are not equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(SeparatedSyntaxList<TNode> left, SeparatedSyntaxList<TNode> right) =>
            !left.Equals(right);

        /// <inheritdoc/>
        public bool Equals(SeparatedSyntaxList<TNode> other) => _list == other._list;

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            (obj is SeparatedSyntaxList<TNode> list) && Equals(list);

        /// <inheritdoc/>
        public override int GetHashCode() => _list.GetHashCode();

        /// <summary>
        /// Creates a new list with the specified node added to the end.
        /// </summary>
        /// <param name="node">The node to add.</param>
        public SeparatedSyntaxList<TNode> Add(TNode node) => Insert(Count, node);

        /// <summary>
        /// Creates a new list with the specified nodes added to the end.
        /// </summary>
        /// <param name="nodes">The nodes to add.</param>
        public SeparatedSyntaxList<TNode> AddRange(IEnumerable<TNode> nodes) =>
            InsertRange(Count, nodes);

        /// <summary>
        /// Creates a new list with the specified node inserted at the index.
        /// </summary>
        /// <param name="index">The index to insert at.</param>
        /// <param name="node">The node to insert.</param>
        public SeparatedSyntaxList<TNode> Insert(int index, TNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return InsertRange(index, new[] { node });
        }

        /// <summary>
        /// Creates a new list with the specified nodes inserted at the index.
        /// </summary>
        /// <param name="index">The index to insert at.</param>
        /// <param name="nodes">The nodes to insert.</param>
        public SeparatedSyntaxList<TNode> InsertRange(int index, IEnumerable<TNode> nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var nodesWithSeps = GetWithSeparators();
            int insertionIndex = index < Count ? nodesWithSeps.IndexOf(this[index]) : nodesWithSeps.Count;

            // determine how to deal with separators (commas)
            if (insertionIndex > 0 && insertionIndex < nodesWithSeps.Count)
            {
                var previous = nodesWithSeps[insertionIndex - 1];
                if (previous.IsToken && !KeepSeparatorWithPreviousNode(previous.AsToken()))
                {
                    // pull back so item in inserted before separator
                    insertionIndex--;
                }
            }

            var nodesToInsertWithSeparators = new List<SyntaxNodeOrToken>();
            foreach (var item in nodes)
            {
                if (item != null)
                {
                    // if item before insertion point is a node, add a separator
                    if (nodesToInsertWithSeparators.Count > 0 || (insertionIndex > 0 && nodesWithSeps[insertionIndex - 1].IsNode))
                    {
                        nodesToInsertWithSeparators.Add(item.Green.CreateSeparator<TNode>(item));
                    }

                    nodesToInsertWithSeparators.Add(item);
                }
            }

            // if item after last inserted node is a node, add separator
            if (insertionIndex < nodesWithSeps.Count && nodesWithSeps[insertionIndex] is { IsNode: true } nodeOrToken)
            {
                var node = nodesWithSeps[insertionIndex].AsNode();
                LorettaDebug.Assert(node is not null);
                nodesToInsertWithSeparators.Add(node.Green.CreateSeparator<TNode>(node)); // separator
            }

            return new SeparatedSyntaxList<TNode>(nodesWithSeps.InsertRange(insertionIndex, nodesToInsertWithSeparators));
        }

        private static bool KeepSeparatorWithPreviousNode(in SyntaxToken separator)
        {
            // if the trivia after the separator contains an explicit end of line or a single line comment
            // then it should stay associated with previous node
            foreach (var tr in separator.TrailingTrivia)
            {
                LorettaDebug.Assert(tr.UnderlyingNode is not null);
                if (tr.UnderlyingNode.IsTriviaWithEndOfLine())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a new list with the element at the specified index removed.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        public SeparatedSyntaxList<TNode> RemoveAt(int index)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return Remove(this[index]);
        }

        /// <summary>
        /// Creates a new list with specified element removed.
        /// </summary>
        /// <param name="node">The element to remove.</param>
        public SeparatedSyntaxList<TNode> Remove(TNode node)
        {
            var nodesWithSeps = GetWithSeparators();
            int index = nodesWithSeps.IndexOf(node);

            if (index >= 0 && index <= nodesWithSeps.Count)
            {
                nodesWithSeps = nodesWithSeps.RemoveAt(index);

                // remove separator too
                if (index < nodesWithSeps.Count && nodesWithSeps[index].IsToken)
                {
                    nodesWithSeps = nodesWithSeps.RemoveAt(index);
                }
                else if (index > 0 && nodesWithSeps[index - 1].IsToken)
                {
                    nodesWithSeps = nodesWithSeps.RemoveAt(index - 1);
                }

                return new SeparatedSyntaxList<TNode>(nodesWithSeps);
            }

            return this;
        }

        /// <summary>
        /// Creates a new list with the specified element replaced by the new node.
        /// </summary>
        /// <param name="nodeInList">The element to replace.</param>
        /// <param name="newNode">The new node.</param>
        public SeparatedSyntaxList<TNode> Replace(TNode nodeInList, TNode newNode)
        {
            if (newNode == null)
            {
                throw new ArgumentNullException(nameof(newNode));
            }

            var index = IndexOf(nodeInList);
            if (index >= 0 && index < Count)
            {
                return new SeparatedSyntaxList<TNode>(GetWithSeparators().Replace(nodeInList, newNode));
            }

            throw new ArgumentOutOfRangeException(nameof(nodeInList));
        }

        /// <summary>
        /// Creates a new list with the specified element replaced by the new nodes.
        /// </summary>
        /// <param name="nodeInList">The element to replace.</param>
        /// <param name="newNodes">The new nodes.</param>
        public SeparatedSyntaxList<TNode> ReplaceRange(TNode nodeInList, IEnumerable<TNode> newNodes)
        {
            if (newNodes == null)
            {
                throw new ArgumentNullException(nameof(newNodes));
            }

            var index = IndexOf(nodeInList);
            if (index >= 0 && index < Count)
            {
                var newNodeList = newNodes.ToList();
                if (newNodeList.Count == 0)
                {
                    return Remove(nodeInList);
                }

                var listWithFirstReplaced = Replace(nodeInList, newNodeList[0]);

                if (newNodeList.Count > 1)
                {
                    newNodeList.RemoveAt(0);
                    return listWithFirstReplaced.InsertRange(index + 1, newNodeList);
                }

                return listWithFirstReplaced;
            }

            throw new ArgumentOutOfRangeException(nameof(nodeInList));
        }

        /// <summary>
        /// Creates a new list with the specified separator token replaced with the new separator.
        /// </summary>
        /// <param name="separatorToken">The separator token to be replaced.</param>
        /// <param name="newSeparator">The new separator token.</param>
        public SeparatedSyntaxList<TNode> ReplaceSeparator(SyntaxToken separatorToken, SyntaxToken newSeparator)
        {
            var nodesWithSeps = GetWithSeparators();
            var index = nodesWithSeps.IndexOf(separatorToken);
            if (index < 0)
            {
                throw new ArgumentException("separatorToken");
            }

            if (newSeparator.RawKind != nodesWithSeps[index].RawKind ||
                newSeparator.Language != nodesWithSeps[index].Language)
            {
                throw new ArgumentException("newSeparator");
            }

            return new SeparatedSyntaxList<TNode>(nodesWithSeps.Replace(separatorToken, newSeparator));
        }

#if DEBUG
        [Obsolete("For debugging only", true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "For debugging only")]
        private TNode[] Nodes => this.ToArray();

        [Obsolete("For debugging only", true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "For debugging only")]
        private SyntaxNodeOrToken[] NodesWithSeparators => _list.ToArray();
#endif

        /// <summary>
        /// Returns the enumerator for this list.
        /// </summary>
        /// <returns></returns>
#pragma warning disable RS0041 // uses oblivious reference types
        public Enumerator GetEnumerator() => new(this);

        IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator()
        {
            if (Any())
            {
                return new EnumeratorImpl(this);
            }

            return SpecializedCollections.EmptyEnumerator<TNode>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (Any())
            {
                return new EnumeratorImpl(this);
            }

            return SpecializedCollections.EmptyEnumerator<TNode>();
        }

        /// <summary>
        /// Converts a typed node list into an opaquely typed node list.
        /// </summary>
        /// <param name="nodes"></param>
        public static implicit operator SeparatedSyntaxList<SyntaxNode>(SeparatedSyntaxList<TNode> nodes)
        {
            return new SeparatedSyntaxList<SyntaxNode>(nodes._list);
        }

        /// <summary>
        /// Converts a list of opaquely typed nodes into a list of typed nodes.
        /// </summary>
        /// <param name="nodes"></param>
        public static implicit operator SeparatedSyntaxList<TNode>(SeparatedSyntaxList<SyntaxNode> nodes)
        {
            return new SeparatedSyntaxList<TNode>(nodes._list);
        }
    }
}
