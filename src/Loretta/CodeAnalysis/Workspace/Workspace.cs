using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Loretta.CodeAnalysis.Syntax;

namespace Loretta.CodeAnalysis.Workspace
{
    /// <summary>
    /// The implementation of a workspace.
    /// </summary>
    public class Workspace : IWorkspace
    {
        private readonly IDictionary<String, SyntaxTree> _activeTrees =
            new Dictionary<String, SyntaxTree> ( );

        /// <inheritdoc/>
        public Boolean TryGetSyntaxTree ( String path, [NotNullWhen ( true )] out SyntaxTree? syntaxTree ) =>
            this._activeTrees.TryGetValue ( path, out syntaxTree );

        /// <inheritdoc/>
        public IEnumerable<SyntaxTree> GetSyntaxTreesForDirectory ( String path ) =>
            this._activeTrees.Where ( kv => kv.Key.StartsWith ( path, StringComparison.Ordinal ) )
                             .Select ( kv => kv.Value );

        /// <inheritdoc/>
        public IImmutableDictionary<String, SyntaxTree> GetActiveSyntaxTrees ( ) =>
            this._activeTrees.ToImmutableDictionary ( );

        /// <inheritdoc/>
        public void UpsertSyntaxTrees (
            IEnumerable<SyntaxTree> syntaxTrees,
            out ImmutableArray<SyntaxTree> added,
            out ImmutableArray<SyntaxTree> removed )
        {
            if ( syntaxTrees.Any ( newTree => String.IsNullOrWhiteSpace ( newTree.Text.FileName ) ) )
                throw new InvalidOperationException ( "All syntax trees' source texts must have a file name." );
            ImmutableArray<SyntaxTree>.Builder addedTrees = ImmutableArray.CreateBuilder<SyntaxTree> ( );
            ImmutableArray<SyntaxTree>.Builder removedTrees = ImmutableArray.CreateBuilder<SyntaxTree> ( );

            foreach ( SyntaxTree newTree in syntaxTrees )
            {
                var path = newTree.Text.FileName;
                if ( this._activeTrees.TryGetValue ( path, out SyntaxTree oldTree ) )
                {
                    if ( newTree == oldTree )
                        continue;

                    removedTrees.Add ( oldTree );
                }

                addedTrees.Add ( newTree );
                this._activeTrees[path] = newTree;
            }

            added = addedTrees.ToImmutable ( );
            removed = removedTrees.ToImmutable ( );
        }

        /// <inheritdoc/>
        public void RemoveSyntaxTrees ( IEnumerable<SyntaxTree> syntaxTrees )
        {
            foreach ( SyntaxTree tree in syntaxTrees )
            {
                if ( this._activeTrees.TryGetValue ( tree.Text.FileName, out SyntaxTree activeTree ) && activeTree == tree )
                {
                    this._activeTrees.Remove ( tree.Text.FileName );
                }
            }
        }
    }
}
