// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    public partial struct SyntaxTokenList
    {
        /// <summary>
        /// Reversed enumerable.
        /// </summary>
        public readonly struct Reversed : IEnumerable<SyntaxToken>, IEquatable<Reversed>
        {
            private readonly SyntaxTokenList _list;

            /// <summary>
            /// Creates a new reversed token list.
            /// </summary>
            /// <param name="list"></param>
            public Reversed(SyntaxTokenList list)
            {
                _list = list;
            }

            /// <summary>
            /// Returns the enumerator for this reversed list.
            /// </summary>
            /// <returns></returns>
            public Enumerator GetEnumerator() => new(in _list);

            IEnumerator<SyntaxToken> IEnumerable<SyntaxToken>.GetEnumerator()
            {
                if (_list.Count == 0)
                {
                    return SpecializedCollections.EmptyEnumerator<SyntaxToken>();
                }

                return new EnumeratorImpl(in _list);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                if (_list.Count == 0)
                {
                    return SpecializedCollections.EmptyEnumerator<SyntaxToken>();
                }

                return new EnumeratorImpl(in _list);
            }

            /// <inheritdoc/>
            public override bool Equals(object? obj) => obj is Reversed r && Equals(r);

            /// <inheritdoc/>
            public bool Equals(Reversed other) => _list.Equals(other._list);

            /// <inheritdoc/>
            public override int GetHashCode() => _list.GetHashCode();

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
            /// The enumerator for this reversed list.
            /// </summary>
            [StructLayout(LayoutKind.Auto)]
            public struct Enumerator
            {
                private readonly SyntaxNode? _parent;
                private readonly GreenNode? _singleNodeOrList;
                private readonly int _baseIndex;
                private readonly int _count;

                private int _index;
                private GreenNode? _current;
                private int _position;

                internal Enumerator(in SyntaxTokenList list)
                    : this()
                {
                    if (list.Any())
                    {
                        _parent = list._parent;
                        _singleNodeOrList = list.Node;
                        _baseIndex = list._index;
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

                    _index--;

                    LorettaDebug.Assert(_singleNodeOrList is not null);
                    _current = GetGreenNodeAt(_singleNodeOrList, _index);
                    LorettaDebug.Assert(_current is not null);
                    _position -= _current.FullWidth;

                    return true;
                }

                /// <inheritdoc cref="IEnumerator{T}.Current"/>
                public SyntaxToken Current
                {
                    get
                    {
                        if (_current == null)
                        {
                            throw new InvalidOperationException();
                        }

                        return new SyntaxToken(_parent, _current, _position, _baseIndex + _index);
                    }
                }

                /// <summary>
                /// Not supported. Do not use.
                /// </summary>
                /// <param name="obj"></param>
                /// <returns></returns>
                /// <exception cref="NotSupportedException">
                /// Always thrown.
                /// </exception>
                public override bool Equals(object? obj) => throw new NotSupportedException();

                /// <summary>
                /// Not supported. Do not use.
                /// </summary>
                /// <returns></returns>
                /// <exception cref="NotSupportedException">
                /// Always thrown.
                /// </exception>
                public override int GetHashCode() => throw new NotSupportedException();

                /// <summary>
                /// Not supported. Do not use.
                /// </summary>
                /// <param name="left"></param>
                /// <param name="right"></param>
                /// <returns></returns>
                /// <exception cref="NotSupportedException">
                /// Always thrown.
                /// </exception>
#pragma warning disable IDE0079 // Remove unnecessary suppression
                [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required.")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
                public static bool operator ==(Enumerator left, Enumerator right) =>
                    throw new NotSupportedException();

                /// <summary>
                /// Not supported. Do not use.
                /// </summary>
                /// <param name="left"></param>
                /// <param name="right"></param>
                /// <returns></returns>
                /// <exception cref="NotSupportedException">
                /// Always thrown.
                /// </exception>
#pragma warning disable IDE0079 // Remove unnecessary suppression
                [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required.")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
                public static bool operator !=(Enumerator left, Enumerator right) =>
                    throw new NotSupportedException();
            }

            private class EnumeratorImpl : IEnumerator<SyntaxToken>
            {
                private Enumerator _enumerator;

                // SyntaxTriviaList is a relatively big struct so is passed as ref
                internal EnumeratorImpl(in SyntaxTokenList list)
                {
                    _enumerator = new Enumerator(in list);
                }

                public SyntaxToken Current => _enumerator.Current;

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
