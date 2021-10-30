using System.Collections.Generic;
using System.Collections.Immutable;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// A script containing one or more files.
    /// </summary>
    public sealed class Script
    {
        /// <summary>
        /// An empty script with no syntax trees.
        /// </summary>
        public static Script Empty { get; } = new Script();

        private readonly ScopeAndVariableManager _scopeAndVariableManager;

        /// <summary>
        /// Initializes an empty script.
        /// </summary>
        public Script() : this(ImmutableArray<SyntaxTree>.Empty)
        {
        }

        /// <summary>
        /// Initializes a new script.
        /// </summary>
        /// <param name="syntaxTrees"></param>
        public Script(ImmutableArray<SyntaxTree> syntaxTrees)
        {
            SyntaxTrees = syntaxTrees;
            _scopeAndVariableManager = new ScopeAndVariableManager(syntaxTrees);
        }

        /// <summary>
        /// The syntax trees contained in this script.
        /// </summary>
        public ImmutableArray<SyntaxTree> SyntaxTrees { get; }

        /// <summary>
        /// The root scope of the script.
        /// </summary>
        public IScope RootScope => _scopeAndVariableManager.GetLazyState().RootScope;

        /// <summary>
        /// Get the scope for the provided node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public IScope? GetScope(SyntaxNode node)
        {
            _scopeAndVariableManager.GetLazyState().Scopes.TryGetValue(node, out var scope);
            return scope;
        }

        /// <summary>
        /// Attempts to find the outermost scope of the provided kind (or a more generic one).
        /// </summary>
        /// <param name="node">The node to search from.</param>
        /// <param name="kind">The kind to search for.</param>
        /// <returns></returns>
        /// <remarks>
        ///   The kind parameter searches for a scope of the provided kind or a more generic one as in the following list:
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <see cref="ScopeKind.Block"/> searches for: <see cref="ScopeKind.Block"/>,
        ///         <see cref="ScopeKind.Function"/>, <see cref="ScopeKind.File"/>, <see cref="ScopeKind.Global"/>.
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <see cref="ScopeKind.Function"/> searches for: <see cref="ScopeKind.Function"/>,
        ///         <see cref="ScopeKind.File"/>, <see cref="ScopeKind.Global"/>.
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <see cref="ScopeKind.File"/> searches for: <see cref="ScopeKind.File"/>, <see cref="ScopeKind.Global"/>.
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description><see cref="ScopeKind.Global"/> searches for itself.</description>
        ///     </item>
        ///   </list>
        /// </remarks>
        public IScope? FindScope(SyntaxNode node, ScopeKind kind = ScopeKind.Block)
        {
            var scopes = _scopeAndVariableManager.GetLazyState().Scopes;

            foreach (var ancestor in node.AncestorsAndSelf())
            {
                if (scopes.TryGetValue(ancestor, out var scope) && scope.Kind <= kind)
                    return scope;
            }

            return null;
        }

        /// <summary>
        /// Get the variable for the provided node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public IVariable? GetVariable(SyntaxNode node)
        {
            _scopeAndVariableManager.GetLazyState().Variables.TryGetValue(node, out var variable);
            return variable;
        }

        /// <summary>
        /// Get the goto label for the provided node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public IGotoLabel? GetLabel(SyntaxNode node)
        {
            _scopeAndVariableManager.GetLazyState().Labels.TryGetValue(node, out var label);
            return label;
        }
    }
}
