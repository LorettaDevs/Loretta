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
