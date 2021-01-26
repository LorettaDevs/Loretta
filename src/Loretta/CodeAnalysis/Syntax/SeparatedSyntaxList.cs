using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// A list with nodes separated by tokens.
    /// </summary>
    public abstract class SeparatedSyntaxList
    {
        private protected SeparatedSyntaxList ( )
        {
        }

        /// <summary>
        /// Returns all nodes from this list including the separators.
        /// </summary>
        /// <returns></returns>
        public abstract ImmutableArray<SyntaxNode> GetWithSeparators ( );
    }

    /// <summary>
    /// A list with nodes separated by tokens.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SeparatedSyntaxList<T> : SeparatedSyntaxList, IReadOnlyList<T>
        where T : SyntaxNode
    {
        private readonly ImmutableArray<SyntaxNode> _nodesAndSeparators;

        internal SeparatedSyntaxList ( ImmutableArray<SyntaxNode> nodesAndSeparators )
        {
            this._nodesAndSeparators = nodesAndSeparators;
        }

        /// <summary>
        /// The amount of non-separator nodes in this list.
        /// </summary>
        public Int32 Count => ( this._nodesAndSeparators.Length + 1 ) >> 1;

        /// <summary>
        /// Checks whether this separated syntax list has a trailing separator.
        /// </summary>
        // If we have an even number of elements, then we have a trailing separator, otherwise, we don't.
        public Boolean HasTrailingSeparator => ( this._nodesAndSeparators.Length & 1 ) == 0;

        /// <summary>
        /// Retrieves the nth node from this list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[Int32 index] => ( T ) this._nodesAndSeparators[index << 1];

        /// <summary>
        /// Retrieves the nth separator from this list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SyntaxToken GetSeparator ( Int32 index )
        {

            if ( index < 0 || index >= ( this.HasTrailingSeparator ? this.Count : this.Count - 1 ) )
                throw new ArgumentOutOfRangeException ( nameof ( index ) );

            return ( SyntaxToken ) this._nodesAndSeparators[( index << 1 ) | 1];
        }

        /// <inheritdoc/>
        public override ImmutableArray<SyntaxNode> GetWithSeparators ( ) => this._nodesAndSeparators;

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator ( )
        {
            for ( var idx = 0; idx < this.Count; idx++ )
                yield return this[idx];
        }

        IEnumerator IEnumerable.GetEnumerator ( ) =>
            this.GetEnumerator ( );
    }
}
