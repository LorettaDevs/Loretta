using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.Utilities;
using Tsu;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// A script containing one or more files.
    /// </summary>
    public sealed partial class Script
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
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="syntaxTrees"/> is a default array.
        /// </exception>
        public Script(ImmutableArray<SyntaxTree> syntaxTrees)
        {
            if (syntaxTrees.IsDefault)
                throw new ArgumentException("Provided syntax trees array must not be a default one.", nameof(syntaxTrees));

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
        ///   <para>
        ///     If the tree that contains the provided node does not have a <see cref="Syntax.CompilationUnitSyntax"/>, statements on the file
        ///     root <b>will not have a scope</b>.
        ///   </para>
        ///   <para>The kind parameter searches for a scope of the provided kind or a more generic one as in the following list:</para>
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

        /// <summary>
        /// Attempts to rename the provided variable with the new provided name.
        /// </summary>
        /// <param name="variable">The variable to rename.</param>
        /// <param name="newName">The new variable's name.</param>
        /// <returns>
        /// An Ok(Script) if the rename was successful or
        /// an Err(IEnumerable&lt;RenameError&gt;) if there were errors
        /// while renaming the variable.
        /// </returns>
        public Result<Script, IEnumerable<RenameError>> RenameVariable(IVariable variable, string newName)
        {
            if (newName is null) throw new ArgumentNullException(nameof(newName));

            var errors = ArrayBuilder<RenameError>.GetInstance();
            var trees = new HashSet<SyntaxTree>();
            foreach (var location in variable.ReadLocations)
            {
                handleLocation(location);
            }
            foreach (var location in variable.WriteLocations)
            {
                handleLocation(location);
            }
            if (variable.Declaration is not null)
                handleLocation(variable.Declaration);

            if (newName.Any(static c => c >= 0x7F))
            {
                foreach (var tree in trees)
                {
                    if (!((LuaParseOptions) tree.Options).SyntaxOptions.UseLuaJitIdentifierRules)
                        errors.Add(new IdentifierNameNotSupportedError(tree));
                }
            }

            if (errors.Any())
                return Result.Err<Script, IEnumerable<RenameError>>(errors.ToImmutableAndFree());

            var visitor = new RenameRewriter(this, variable, newName);
            var finalTrees = SyntaxTrees;
            foreach (var tree in trees)
            {
                var root = tree.GetRoot();
                var rewrittenTree = tree.WithRootAndOptions(visitor.Visit(root), tree.Options);
                finalTrees = finalTrees.Replace(tree, rewrittenTree);
            }
            return Result.Ok<Script, IEnumerable<RenameError>>(new Script(finalTrees));

            void handleLocation(SyntaxNode location)
            {
                trees.Add(location.SyntaxTree);
                if (FindScope(location)?.FindVariable(newName) is not null)
                {
                    errors.Add(new VariableConflictError(variable));
                }
            }
        }
    }
}
