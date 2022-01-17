// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.CodeAnalysis.Syntax;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Represents a read-only list of <see cref="SyntaxTrivia"/>.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public readonly partial struct SyntaxTriviaList : IEquatable<SyntaxTriviaList>, IReadOnlyList<SyntaxTrivia>
    {
        /// <summary>
        /// An empty trivia list.
        /// </summary>
        public static SyntaxTriviaList Empty => default(SyntaxTriviaList);

        internal SyntaxTriviaList(in SyntaxToken token, GreenNode? node, int position, int index = 0)
        {
            Token = token;
            Node = node;
            Position = position;
            Index = index;
        }

        internal SyntaxTriviaList(in SyntaxToken token, GreenNode? node)
        {
            Token = token;
            Node = node;
            Position = token.Position;
            Index = 0;
        }

        /// <summary>
        /// Creates a new trivia list with the provided trivia as the only element.
        /// </summary>
        /// <param name="trivia"></param>
        public SyntaxTriviaList(SyntaxTrivia trivia)
        {
            Token = default(SyntaxToken);
            Node = trivia.UnderlyingNode;
            Position = 0;
            Index = 0;
        }

        /// <summary>
        /// Creates a list of trivia.
        /// </summary>
        /// <param name="trivias">An array of trivia.</param>
        public SyntaxTriviaList(params SyntaxTrivia[] trivias)
            : this(default, CreateNode(trivias), 0, 0)
        {
        }

        /// <summary>
        /// Creates a list of trivia.
        /// </summary>
        /// <param name="trivias">A sequence of trivia.</param>
        public SyntaxTriviaList(IEnumerable<SyntaxTrivia>? trivias)
            : this(default, SyntaxTriviaListBuilder.Create(trivias).Node, 0, 0)
        {
        }

        private static GreenNode? CreateNode(SyntaxTrivia[]? trivias)
        {
            if (trivias == null)
            {
                return null;
            }

            var builder = new SyntaxTriviaListBuilder(trivias.Length);
            builder.Add(trivias);
            return builder.ToList().Node;
        }

        internal SyntaxToken Token { get; }

        internal GreenNode? Node { get; }

        internal int Position { get; }

        internal int Index { get; }

        /// <summary>
        /// The amount of elements in this list.
        /// </summary>
        public int Count => Node == null ? 0 : (Node.IsList ? Node.SlotCount : 1);

        /// <summary>
        /// Returns the element at the provided index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SyntaxTrivia ElementAt(int index) => this[index];

        /// <summary>
        /// Gets the trivia at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the trivia to get.</param>
        /// <returns>The token at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="Count" />. </exception>
        public SyntaxTrivia this[int index]
        {
            get
            {
                if (Node != null)
                {
                    if (Node.IsList)
                    {
                        if (unchecked((uint) index < (uint) Node.SlotCount))
                        {
                            return new SyntaxTrivia(Token, Node.GetSlot(index), Position + Node.GetSlotOffset(index), Index + index);
                        }
                    }
                    else if (index == 0)
                    {
                        return new SyntaxTrivia(Token, Node, Position, Index);
                    }
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// The absolute span of the list elements in characters, including the leading and trailing trivia of the first and last elements.
        /// </summary>
        public TextSpan FullSpan
        {
            get
            {
                if (Node == null)
                {
                    return default(TextSpan);
                }

                return new TextSpan(Position, Node.FullWidth);
            }
        }

        /// <summary>
        /// The absolute span of the list elements in characters, not including the leading and trailing trivia of the first and last elements.
        /// </summary>
        public TextSpan Span
        {
            get
            {
                if (Node == null)
                {
                    return default(TextSpan);
                }

                return TextSpan.FromBounds(Position + Node.GetLeadingTriviaWidth(),
                    Position + Node.FullWidth - Node.GetTrailingTriviaWidth());
            }
        }

        /// <summary>
        /// Returns the first trivia in the list.
        /// </summary>
        /// <returns>The first trivia in the list.</returns>
        /// <exception cref="InvalidOperationException">The list is empty.</exception>        
        public SyntaxTrivia First()
        {
            if (Any())
            {
                return this[0];
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns the last trivia in the list.
        /// </summary>
        /// <returns>The last trivia in the list.</returns>
        /// <exception cref="InvalidOperationException">The list is empty.</exception>        
        public SyntaxTrivia Last()
        {
            if (Any())
            {
                return this[Count - 1];
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Does this list have any items.
        /// </summary>
        public bool Any() => Node != null;

        /// <summary>
        /// Returns a list which contains all elements of <see cref="SyntaxTriviaList"/> in reversed order.
        /// </summary>
        /// <returns><see cref="Reversed"/> which contains all elements of <see cref="SyntaxTriviaList"/> in reversed order</returns>
        public Reversed Reverse() => new Reversed(this);

        /// <summary>
        /// Returns the enumerator for this list.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator() => new Enumerator(in this);

        /// <summary>
        /// Returns the index of the provided trivia in this list.
        /// </summary>
        /// <param name="triviaInList"></param>
        /// <returns>-1 if not found.</returns>
        public int IndexOf(SyntaxTrivia triviaInList)
        {
            for (int i = 0, n = Count; i < n; i++)
            {
                var trivia = this[i];
                if (trivia == triviaInList)
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
        /// Creates a new <see cref="SyntaxTriviaList"/> with the specified trivia added to the end.
        /// </summary>
        /// <param name="trivia">The trivia to add.</param>
        public SyntaxTriviaList Add(SyntaxTrivia trivia) =>
            Insert(Count, trivia);

        /// <summary>
        /// Creates a new <see cref="SyntaxTriviaList"/> with the specified trivia added to the end.
        /// </summary>
        /// <param name="trivia">The trivia to add.</param>
        public SyntaxTriviaList AddRange(IEnumerable<SyntaxTrivia> trivia) =>
            InsertRange(Count, trivia);

        /// <summary>
        /// Creates a new <see cref="SyntaxTriviaList"/> with the specified trivia inserted at the index.
        /// </summary>
        /// <param name="index">The index in the list to insert the trivia at.</param>
        /// <param name="trivia">The trivia to insert.</param>
        public SyntaxTriviaList Insert(int index, SyntaxTrivia trivia)
        {
            if (trivia == default(SyntaxTrivia))
            {
                throw new ArgumentOutOfRangeException(nameof(trivia));
            }

            return InsertRange(index, new[] { trivia });
        }

        private static readonly ObjectPool<SyntaxTriviaListBuilder> s_builderPool =
            new(() => SyntaxTriviaListBuilder.Create());

        private static SyntaxTriviaListBuilder GetBuilder()
            => s_builderPool.Allocate();

        private static void ClearAndFreeBuilder(SyntaxTriviaListBuilder builder)
        {
            // It's possible someone might create a list with a huge amount of trivia
            // in it.  We don't want to hold onto such items forever.  So only cache
            // reasonably sized lists.  In IDE testing, around 99% of all trivia lists
            // were 16 or less elements.
            const int MaxBuilderCount = 16;
            if (builder.Count <= MaxBuilderCount)
            {
                builder.Clear();
                s_builderPool.Free(builder);
            }
        }

        /// <summary>
        /// Creates a new <see cref="SyntaxTriviaList"/> with the specified trivia inserted at the index.
        /// </summary>
        /// <param name="index">The index in the list to insert the trivia at.</param>
        /// <param name="trivia">The trivia to insert.</param>
        public SyntaxTriviaList InsertRange(int index, IEnumerable<SyntaxTrivia> trivia)
        {
            var thisCount = Count;
            if (index < 0 || index > thisCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (trivia == null)
            {
                throw new ArgumentNullException(nameof(trivia));
            }

            // Just return ourselves if we're not being asked to add anything.
            if (trivia is ICollection<SyntaxTrivia> triviaCollection && triviaCollection.Count == 0)
            {
                return this;
            }

            var builder = GetBuilder();
            try
            {
                for (int i = 0; i < index; i++)
                {
                    builder.Add(this[i]);
                }

                builder.AddRange(trivia);

                for (int i = index; i < thisCount; i++)
                {
                    builder.Add(this[i]);
                }

                return builder.Count == thisCount ? this : builder.ToList();
            }
            finally
            {
                ClearAndFreeBuilder(builder);
            }
        }

        /// <summary>
        /// Creates a new <see cref="SyntaxTriviaList"/> with the element at the specified index removed.
        /// </summary>
        /// <param name="index">The index identifying the element to remove.</param>
        public SyntaxTriviaList RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var list = this.ToList();
            list.RemoveAt(index);
            return new SyntaxTriviaList(default(SyntaxToken), GreenNode.CreateList(list, static n => n.RequiredUnderlyingNode), 0, 0);
        }

        /// <summary>
        /// Creates a new <see cref="SyntaxTriviaList"/> with the specified element removed.
        /// </summary>
        /// <param name="triviaInList">The trivia element to remove.</param>
        public SyntaxTriviaList Remove(SyntaxTrivia triviaInList)
        {
            var index = IndexOf(triviaInList);
            if (index >= 0 && index < Count)
            {
                return RemoveAt(index);
            }

            return this;
        }

        /// <summary>
        /// Creates a new <see cref="SyntaxTriviaList"/> with the specified element replaced with new trivia.
        /// </summary>
        /// <param name="triviaInList">The trivia element to replace.</param>
        /// <param name="newTrivia">The trivia to replace the element with.</param>
        public SyntaxTriviaList Replace(SyntaxTrivia triviaInList, SyntaxTrivia newTrivia)
        {
            if (newTrivia == default(SyntaxTrivia))
            {
                throw new ArgumentOutOfRangeException(nameof(newTrivia));
            }

            return ReplaceRange(triviaInList, new[] { newTrivia });
        }

        /// <summary>
        /// Creates a new <see cref="SyntaxTriviaList"/> with the specified element replaced with new trivia.
        /// </summary>
        /// <param name="triviaInList">The trivia element to replace.</param>
        /// <param name="newTrivia">The trivia to replace the element with.</param>
        public SyntaxTriviaList ReplaceRange(SyntaxTrivia triviaInList, IEnumerable<SyntaxTrivia> newTrivia)
        {
            var index = IndexOf(triviaInList);
            if (index >= 0 && index < Count)
            {
                var list = this.ToList();
                list.RemoveAt(index);
                list.InsertRange(index, newTrivia);
                return new SyntaxTriviaList(default(SyntaxToken), GreenNode.CreateList(list, static n => n.RequiredUnderlyingNode), 0, 0);
            }

            throw new ArgumentOutOfRangeException(nameof(triviaInList));
        }

        // for debugging
        private SyntaxTrivia[] Nodes => this.ToArray();

        IEnumerator<SyntaxTrivia> IEnumerable<SyntaxTrivia>.GetEnumerator()
        {
            if (Node == null)
            {
                return SpecializedCollections.EmptyEnumerator<SyntaxTrivia>();
            }

            return new EnumeratorImpl(in this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (Node == null)
            {
                return SpecializedCollections.EmptyEnumerator<SyntaxTrivia>();
            }

            return new EnumeratorImpl(in this);
        }

        /// <summary>
        /// get the green node at the specific slot
        /// </summary>
        private GreenNode? GetGreenNodeAt(int i)
        {
            LorettaDebug.Assert(Node is not null);
            return GetGreenNodeAt(Node, i);
        }

        private static GreenNode? GetGreenNodeAt(GreenNode node, int i)
        {
            LorettaDebug.Assert(node.IsList || (i == 0 && !node.IsList));
            return node.IsList ? node.GetSlot(i) : node;
        }

        /// <inheritdoc/>
        public bool Equals(SyntaxTriviaList other) =>
            Node == other.Node && Index == other.Index && Token.Equals(other.Token);

        /// <summary>
        /// Checks whether two trivia lists are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(SyntaxTriviaList left, SyntaxTriviaList right) =>
            left.Equals(right);

        /// <summary>
        /// Checks whether two trivia lists are not equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(SyntaxTriviaList left, SyntaxTriviaList right) =>
            !left.Equals(right);

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            (obj is SyntaxTriviaList list) && Equals(list);

        /// <inheritdoc/>
        public override int GetHashCode() =>
            Hash.Combine(Token.GetHashCode(), Hash.Combine(Node, Index));

        /// <summary>
        /// Copy <paramref name="count"/> number of items starting at <paramref name="offset"/> from this list into <paramref name="array"/> starting at <paramref name="arrayOffset"/>.
        /// </summary>
        internal void CopyTo(int offset, SyntaxTrivia[] array, int arrayOffset, int count)
        {
            if (offset < 0 || count < 0 || Count < offset + count)
            {
                throw new IndexOutOfRangeException();
            }

            if (count == 0)
            {
                return;
            }

            // get first one without creating any red node
            var first = this[offset];
            array[arrayOffset] = first;

            // calculate trivia position from the first ourselves from now on
            var position = first.Position;
            var current = first;

            for (int i = 1; i < count; i++)
            {
                position += current.FullWidth;
                current = new SyntaxTrivia(Token, GetGreenNodeAt(offset + i), position, Index + i);

                array[arrayOffset + i] = current;
            }
        }

        /// <inheritdoc/>
        public override string ToString() =>
            Node != null ? Node.ToString() : string.Empty;

        /// <summary>
        /// Returns the list as a string including leading and trailing trivia.
        /// </summary>
        /// <returns></returns>
        public string ToFullString() =>
            Node != null ? Node.ToFullString() : string.Empty;

        /// <summary>
        /// Creates a new trivia list.
        /// </summary>
        /// <param name="trivia"></param>
        /// <returns></returns>
        public static SyntaxTriviaList Create(SyntaxTrivia trivia) => new SyntaxTriviaList(trivia);
    }
}
