using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// The Lua syntax rewriter.
    /// </summary>
    public abstract class SyntaxRewriter : SyntaxVisitor<SyntaxNode?>
    {
        /// <inheritdoc/>
        [return: NotNullIfNotNull ( "node" )]
        public override SyntaxNode? Visit ( SyntaxNode? node ) => base.Visit ( node );

        /// <summary>
        /// Rewrites a token.
        /// Returns the input token if unchanged.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override SyntaxNode VisitToken ( SyntaxToken token )
        {
            ImmutableArray<SyntaxTrivia> leadingTrivia = token.LeadingTrivia;
            if ( !leadingTrivia.IsEmpty )
            {
                ImmutableArray<SyntaxTrivia> visitedLeading = this.VisitList ( leadingTrivia );

                if ( leadingTrivia != visitedLeading )
                    token = token.WithLeadingTrivia ( visitedLeading );
            }

            ImmutableArray<SyntaxTrivia> trailingTrivia = token.TrailingTrivia;
            if ( !trailingTrivia.IsEmpty )
            {
                ImmutableArray<SyntaxTrivia> visitedTrailing = this.VisitList ( trailingTrivia );

                if ( trailingTrivia != visitedTrailing )
                    token = token.WithTrailingTrivia ( visitedTrailing );
            }

            return token;
        }

        /// <summary>
        /// Visits a <see cref="SyntaxTrivia"/>.
        /// </summary>
        /// <param name="trivia"></param>
        /// <returns></returns>
        public virtual SyntaxTrivia VisitTrivia ( SyntaxTrivia trivia ) => trivia;

        /// <summary>
        /// Visits a list of trivia tokens.
        /// Returns the input list if unchanged.
        /// </summary>
        /// <param name="triviaList"></param>
        /// <returns></returns>
        public virtual ImmutableArray<SyntaxTrivia> VisitList ( ImmutableArray<SyntaxTrivia> triviaList )
        {
            ImmutableArray<SyntaxTrivia>.Builder? newTriviaList = null;

            for ( var idx = triviaList.Length - 1; idx >= 0; idx-- )
            {
                SyntaxTrivia trivia = triviaList[idx];
                SyntaxTrivia visited = this.VisitListElement ( trivia );

                if ( trivia != visited )
                {
                    newTriviaList ??= triviaList.ToBuilder ( );
                    if ( visited.Kind != SyntaxKind.None )
                        newTriviaList[idx] = visited;
                    else
                        newTriviaList.RemoveAt ( idx );
                }
            }

            if ( newTriviaList is not null )
                return newTriviaList.ToImmutable ( );
            return triviaList;
        }

        /// <summary>
        /// Visits a <see cref="SyntaxTrivia"/> list element.
        /// </summary>
        /// <param name="trivia"></param>
        /// <returns></returns>
        public virtual SyntaxTrivia VisitListElement ( SyntaxTrivia trivia ) =>
            this.VisitTrivia ( trivia );

        /// <summary>
        /// Rewrites a list of tokens.
        /// Returns the input list if unchanged.
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public virtual ImmutableArray<TNode> VisitList<TNode> ( ImmutableArray<TNode> nodes ) where TNode : SyntaxNode
        {
            ImmutableArray<TNode>.Builder? newNodes = null;

            for ( var idx = nodes.Length - 1; idx >= 0; idx-- )
            {
                TNode node = nodes[idx];
                TNode? visited = this.VisitListElement ( node );

                if ( node != visited )
                {
                    newNodes ??= nodes.ToBuilder ( );
                    if ( visited is { Kind: not SyntaxKind.None } )
                        newNodes[idx] = visited;
                    else
                        newNodes.RemoveAt ( idx );
                }
            }

            if ( newNodes is not null )
                return newNodes.ToImmutable ( );
            return nodes;
        }

        /// <summary>
        /// Visits an <see cref="ImmutableArray{T}"/> or <see cref="SeparatedSyntaxList{T}"/> element.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual TNode? VisitListElement<TNode> ( TNode? node ) where TNode : SyntaxNode =>
            ( TNode? ) this.Visit ( node );

        /// <summary>
        /// Rewrites a <see cref="SeparatedSyntaxList{T}"/>.
        /// Returns the input list if unchanged.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public virtual SeparatedSyntaxList<TNode> VisitList<TNode> ( SeparatedSyntaxList<TNode> list ) where TNode : SyntaxNode
        {
            ImmutableArray<SyntaxNode>.Builder? newList = null;
            var count = list.Count;
            var sepCount = list.SeparatorCount;

            for ( var idx = 0; idx < count; idx++ )
            {
                TNode? node = list[idx];
                TNode? visitedNode = this.VisitListElement ( node );

                if ( idx < sepCount )
                {
                    SyntaxToken sep = list.GetSeparator ( idx );
                    SyntaxToken visitedSep = this.VisitListSeparator ( sep );

                    if ( node != visitedNode || sep != visitedSep )
                    {
                        newList ??= list.GetWithSeparators ( ).ToBuilder ( );

                        if ( visitedNode is null or { Kind: SyntaxKind.None } )
                            throw new InvalidOperationException ( "Node expected but got null." );
                        if ( visitedSep.Kind is SyntaxKind.None )
                            throw new InvalidOperationException ( "Separator expected but got SyntaxKind.None." );

                        newList[idx << 1] = visitedNode;
                        newList[( idx << 1 ) | 1] = visitedSep;
                    }
                }
                else
                {
                    if ( node != visitedNode )
                    {
                        newList ??= ImmutableArray.CreateBuilder<SyntaxNode> ( idx );

                        if ( visitedNode is null or { Kind: SyntaxKind.None } )
                            throw new InvalidOperationException ( "Node expected but got null." );

                        newList[idx << 1] = visitedNode;
                    }
                }
            }

            if ( newList is not null )
                return new SeparatedSyntaxList<TNode> ( newList.MoveToImmutable ( ) );
            return list;
        }

        /// <summary>
        /// Visits a <see cref="SeparatedSyntaxList{T}"/>'s separator.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual SyntaxToken VisitListSeparator ( SyntaxToken token ) =>
            ( SyntaxToken ) this.VisitToken ( token );
    }
}
