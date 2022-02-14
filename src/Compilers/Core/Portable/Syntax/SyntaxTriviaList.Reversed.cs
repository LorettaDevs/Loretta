// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Runtime.InteropServices;

namespace Loretta.CodeAnalysis
{
    public partial struct SyntaxTriviaList
    {
        /// <summary>
        /// Reversed enumerable.
        /// </summary>
        public readonly struct Reversed : IEnumerable<SyntaxTrivia>, IEquatable<Reversed>
        {
            private readonly SyntaxTriviaList _list;

            /// <summary>
            /// Creates a new reversed trivia list.
            /// </summary>
            /// <param name="list"></param>
            public Reversed(SyntaxTriviaList list)
            {
                _list = list;
            }

            /// <summary>
            /// Returns the enumerator for this reversed trivia list.
            /// </summary>
            /// <returns></returns>
            public Enumerator GetEnumerator() => new(in _list);

            IEnumerator<SyntaxTrivia> IEnumerable<SyntaxTrivia>.GetEnumerator()
            {
                if (_list.Count == 0)
                {
                    return SpecializedCollections.EmptyEnumerator<SyntaxTrivia>();
                }

                return new ReversedEnumeratorImpl(in _list);
            }

            IEnumerator
                IEnumerable.GetEnumerator()
            {
                if (_list.Count == 0)
                {
                    return SpecializedCollections.EmptyEnumerator<SyntaxTrivia>();
                }

                return new ReversedEnumeratorImpl(in _list);
            }

            /// <inheritdoc/>
            public override int GetHashCode() => _list.GetHashCode();

            /// <inheritdoc/>
            public override bool Equals(object? obj) =>
                obj is Reversed reversed && Equals(reversed);

            /// <inheritdoc/>
            public bool Equals(Reversed other) => _list.Equals(other._list);

            /// <summary>
            /// Checks whether two reversed lists are equal.
            /// </summary>
            /// <param name="left"></param>
            /// <param name="right"></param>
            /// <returns></returns>
            public static bool operator ==(Reversed left, Reversed right) =>
                left.Equals(right);

            /// <summary>
            /// Checks whether two reversed lists are equal.
            /// </summary>
            /// <param name="left"></param>
            /// <param name="right"></param>
            /// <returns></returns>
            public static bool operator !=(Reversed left, Reversed right) =>
                !(left == right);

            /// <summary>
            /// The enumerator for reversed trivia lists.
            /// </summary>
            [StructLayout(LayoutKind.Auto)]
            public struct Enumerator
            {
                private readonly SyntaxToken _token;
                private readonly GreenNode? _singleNodeOrList;
                private readonly int _baseIndex;
                private readonly int _count;

                private int _index;
                private GreenNode? _current;
                private int _position;

                internal Enumerator(in SyntaxTriviaList list)
                    : this()
                {
                    if (list.Node is not null)
                    {
                        _token = list.Token;
                        _singleNodeOrList = list.Node;
                        _baseIndex = list.Index;
                        _count = list.Count;

                        _index = _count;
                        _current = null;

                        var last = list.Last();
                        _position = last.Position + last.FullWidth;
                    }
                }

                /// <inheritdoc cref="IEnumerator.MoveNext"/>
                public bool MoveNext()
                {
                    if (_count == 0 || _index <= 0)
                    {
                        _current = null;
                        return false;
                    }

                    LorettaDebug.Assert(_singleNodeOrList is not null);
                    _index--;

                    _current = GetGreenNodeAt(_singleNodeOrList, _index);
                    LorettaDebug.Assert(_current is not null);
                    _position -= _current.FullWidth;

                    return true;
                }

                /// <inheritdoc cref="IEnumerator{T}.Current"/>
                public SyntaxTrivia Current
                {
                    get
                    {
                        if (_current == null)
                        {
                            throw new InvalidOperationException();
                        }

                        return new SyntaxTrivia(_token, _current, _position, _baseIndex + _index);
                    }
                }
            }

            private class ReversedEnumeratorImpl : IEnumerator<SyntaxTrivia>
            {
                private Enumerator _enumerator;

                // SyntaxTriviaList is a relatively big struct so is passed as ref
                internal ReversedEnumeratorImpl(in SyntaxTriviaList list)
                {
                    _enumerator = new Enumerator(in list);
                }

                public SyntaxTrivia Current => _enumerator.Current;

                object IEnumerator.Current => _enumerator.Current;

                public bool MoveNext() => _enumerator.MoveNext();

                public void Reset() => throw new NotSupportedException();

                public void Dispose()
                {
                }
            }
        }
    }
}
