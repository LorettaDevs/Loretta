// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Loretta.CodeAnalysis
{
    public partial struct SeparatedSyntaxList<TNode>
    {
        // Public struct enumerator
        // Only implements enumerator pattern as used by foreach
        // Does not implement IEnumerator. Doing so would require the struct to implement IDisposable too.
        /// <summary>
        /// The enumerator for <see cref="SeparatedSyntaxList{TNode}"/>.
        /// </summary>
        [SuppressMessage("Performance", "CA1067", Justification = "Equality not actually implemented")]
        public struct Enumerator
        {
            private readonly SeparatedSyntaxList<TNode> _list;
            private int _index;

            internal Enumerator(in SeparatedSyntaxList<TNode> list)
            {
                _list = list;
                _index = -1;
            }

            /// <inheritdoc cref="IEnumerator.MoveNext"/>
            public bool MoveNext()
            {
                int newIndex = _index + 1;
                if (newIndex < _list.Count)
                {
                    _index = newIndex;
                    return true;
                }

                return false;
            }

            /// <inheritdoc cref="IEnumerator{T}.Current"/>
            public TNode Current
            {
                get
                {
                    return _list[_index];
                }
            }

            /// <inheritdoc cref="IEnumerator.Reset"/>
            public void Reset()
            {
                _index = -1;
            }

            /// <summary>
            /// Not supported. Do not call.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            /// <exception cref="NotSupportedException">
            /// Always thrown.
            /// </exception>
            public override bool Equals(object? obj)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Not supported. Do not call.
            /// </summary>
            /// <returns></returns>
            /// <exception cref="NotSupportedException">
            /// Always thrown.
            /// </exception>
            public override int GetHashCode()
            {
                throw new NotSupportedException();
            }
        }

        // IEnumerator wrapper for Enumerator.
        private class EnumeratorImpl : IEnumerator<TNode>
        {
            private Enumerator _e;

            internal EnumeratorImpl(in SeparatedSyntaxList<TNode> list)
            {
                _e = new Enumerator(in list);
            }

            public TNode Current
            {
                get
                {
                    return _e.Current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return _e.Current;
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return _e.MoveNext();
            }

            public void Reset()
            {
                _e.Reset();
            }
        }
    }
}
