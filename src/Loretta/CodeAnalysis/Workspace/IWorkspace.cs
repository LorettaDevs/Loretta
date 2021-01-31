using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.CodeAnalysis.Syntax;

namespace Loretta.CodeAnalysis.Workspace
{
    /// <summary>
    /// A workspace.
    /// </summary>
    public interface IWorkspace : IReadOnlyWorkspace
    {
        /// <summary>
        /// Inserts or updates existing syntax trees.
        /// </summary>
        /// <param name="syntaxTrees">The syntax trees to be added/updated.</param>
        /// <param name="added">The syntax trees that were added.</param>
        /// <param name="removed">The (old) syntax trees that got replaced.</param>
        void UpsertSyntaxTrees (
            IEnumerable<SyntaxTree> syntaxTrees,
            out ImmutableArray<SyntaxTree> added,
            out ImmutableArray<SyntaxTree> removed );

        /// <summary>
        /// Removes the provided syntax trees from the workspace.
        /// </summary>
        /// <param name="syntaxTrees">The syntax trees to be removed</param>
        void RemoveSyntaxTrees ( IEnumerable<SyntaxTree> syntaxTrees );
    }
}
