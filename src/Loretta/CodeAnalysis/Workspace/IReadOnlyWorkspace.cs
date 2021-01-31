using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Loretta.CodeAnalysis.Syntax;

namespace Loretta.CodeAnalysis.Workspace
{
    /// <summary>
    /// A readonly workspace.
    /// </summary>
    public interface IReadOnlyWorkspace
    {
        /// <summary>
        /// Attempts to fetch a syntax tree for a given path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="syntaxTree">The resulting syntax tree.</param>
        /// <returns></returns>
        Boolean TryGetSyntaxTree ( String path, [NotNullWhen ( true )] out SyntaxTree? syntaxTree );

        /// <summary>
        /// Obtains all syntax trees in a directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        IEnumerable<SyntaxTree> GetSyntaxTreesForDirectory ( String path );

        /// <summary>
        /// Obtains all active syntax trees.
        /// </summary>
        /// <returns></returns>
        IImmutableDictionary<String, SyntaxTree> GetActiveSyntaxTrees ( );
    }
}
