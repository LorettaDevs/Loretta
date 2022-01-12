using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The base interface for scopes.
    /// </summary>
    [InternalImplementationOnly]
    public interface IScope
    {
        /// <summary>
        /// The kind of scope.
        /// </summary>
        ScopeKind Kind { get; }

        /// <summary>
        /// The syntax node that originated this scope.
        /// Not supported for the global scope.
        /// </summary>
        SyntaxNode? Node { get; }

        /// <summary>
        /// The parent scope (if any).
        /// </summary>
        IScope? ContainingScope { get; }

        /// <summary>
        /// Contains the variables declared within the scope.
        /// As variables can be shadowed/redeclared, there may be multiple
        /// variables with the same name.
        /// </summary>
        IEnumerable<IVariable> DeclaredVariables { get; }

        /// <summary>
        /// Variables that are directly referenced by this scope.
        /// </summary>
        IEnumerable<IVariable> ReferencedVariables { get; }

        /// <summary>
        /// The goto labels contained within this scope.
        /// </summary>
        IEnumerable<IGotoLabel> GotoLabels { get; }

        /// <summary>
        /// Returns the scopes directly contained within this scope.
        /// </summary>
        IEnumerable<IScope> ContainedScopes { get; }

        /// <summary>
        /// Attempts to find a variable with the given name.
        /// </summary>
        /// <param name="name">The name of the variable to search by.</param>
        /// <param name="kind">
        /// The kind of scope up to which to search the variable in.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the providedd name is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the provided name is not a valid identifier.
        /// </exception>
        /// <remarks>
        ///   <para>The kind parameter searches for a scope of the provided kind or a more specific one as in the following list:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <see cref="ScopeKind.Block"/> searches only <see cref="ScopeKind.Block"/>s.
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <see cref="ScopeKind.Function"/> searches: <see cref="ScopeKind.Function"/>s
        ///         and <see cref="ScopeKind.Block"/>s.
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <see cref="ScopeKind.File"/> searches: <see cref="ScopeKind.File"/>,
        ///         <see cref="ScopeKind.Function"/>s and <see cref="ScopeKind.Block"/>s.
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <see cref="ScopeKind.Global"/> searches: <see cref="ScopeKind.Global"/>,
        ///         <see cref="ScopeKind.File"/>, <see cref="ScopeKind.Function"/>s
        ///         and <see cref="ScopeKind.Block"/>s.
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        IVariable? FindVariable(string name, ScopeKind kind = ScopeKind.Global);

        /// <summary>
        /// Deprecated. <inheritdoc cref="ContainingScope"/>
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use ContainingScope instead.")]
        IScope? Parent { get; }
    }

    internal interface IScopeInternal : IScope
    {
        bool TryGetVariable(string name, [NotNullWhen(true)] out IVariableInternal? variable);
        IVariableInternal GetOrCreateVariable(VariableKind kind, string name, SyntaxNode? declaration = null);
        IVariableInternal CreateVariable(VariableKind kind, string name, SyntaxNode? declaration = null);
        void AddReferencedVariable(IVariableInternal variable);

        bool TryGetLabel(string name, [NotNullWhen(true)] out IGotoLabelInternal? label);
        IGotoLabelInternal GetOrCreateLabel(string name, GotoLabelStatementSyntax? labelSyntax = null);
        IGotoLabelInternal CreateLabel(string name, GotoLabelStatementSyntax? labelSyntax = null);

        void AddChildScope(IScopeInternal scope);
    }

    internal class Scope : IScopeInternal
    {
        protected readonly IDictionary<string, IVariableInternal> _variables = new Dictionary<string, IVariableInternal>(StringOrdinalComparer.Instance);
        protected readonly ISet<IVariableInternal> _declaredVariables = new HashSet<IVariableInternal>();
        protected readonly ISet<IVariableInternal> _referencedVariables = new HashSet<IVariableInternal>();
        protected readonly IDictionary<string, IGotoLabelInternal> _labels = new Dictionary<string, IGotoLabelInternal>(StringOrdinalComparer.Instance);
        protected readonly IList<IScopeInternal> _containedScopes = new List<IScopeInternal>();

        public Scope(ScopeKind kind, SyntaxNode? node, IScopeInternal? parent)
        {
            Kind = kind;
            Node = node;
            Parent = parent;
            DeclaredVariables = SpecializedCollections.ReadOnlyEnumerable(_declaredVariables);
            ReferencedVariables = SpecializedCollections.ReadOnlyEnumerable(_referencedVariables);
            GotoLabels = SpecializedCollections.ReadOnlyEnumerable(_labels.Values);
            ContainedScopes = SpecializedCollections.ReadOnlyEnumerable(_containedScopes);
        }

        public ScopeKind Kind { get; }

        public SyntaxNode? Node { get; }

        public IScopeInternal? Parent { get; }

        IScope? IScope.ContainingScope => Parent;
        IScope? IScope.Parent => Parent;

        public IEnumerable<IVariableInternal> DeclaredVariables { get; }

        IEnumerable<IVariable> IScope.DeclaredVariables => DeclaredVariables;

        public IEnumerable<IVariableInternal> ReferencedVariables { get; }

        IEnumerable<IVariable> IScope.ReferencedVariables => ReferencedVariables;

        public IEnumerable<IGotoLabelInternal> GotoLabels { get; }

        IEnumerable<IGotoLabel> IScope.GotoLabels => GotoLabels;

        public IEnumerable<IScopeInternal> ContainedScopes { get; }

        IEnumerable<IScope> IScope.ContainedScopes => ContainedScopes;

        public IVariable? FindVariable(string name, ScopeKind kind = ScopeKind.Global)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (!StringUtils.IsIdentifier(name)) throw new ArgumentException($"'{nameof(name)}' must be a valid identifier.");
            foreach (var variable in DeclaredVariables)
            {
                if (StringOrdinalComparer.Equals(variable.Name, name))
                    return variable;
            }
            return Parent is not null && Parent.Kind >= kind ? Parent.FindVariable(name, kind) : null;
        }

        public bool TryGetVariable(string name, [NotNullWhen(true)] out IVariableInternal? variable) =>
            _variables.TryGetValue(name, out variable) || Parent?.TryGetVariable(name, out variable) is true;

        public IVariableInternal GetOrCreateVariable(VariableKind kind, string name, SyntaxNode? declaration = null)
        {
            LorettaDebug.Assert(Kind == ScopeKind.Global || kind != VariableKind.Global);
            LorettaDebug.Assert(!string.IsNullOrEmpty(name));

            if (!TryGetVariable(name, out var variable))
                variable = CreateVariable(kind, name, declaration);

            _referencedVariables.Add(variable);
            LorettaDebug.Assert(variable.Kind == kind);
            return variable;
        }

        public IVariableInternal CreateVariable(VariableKind kind, string name, SyntaxNode? declaration = null)
        {
            LorettaDebug.Assert(Kind == ScopeKind.Global || kind != VariableKind.Global);
            LorettaDebug.Assert(!string.IsNullOrEmpty(name));

            var variable = new Variable(kind, this, name, declaration);
            _variables[name] = variable;
            _declaredVariables.Add(variable);
            return variable;
        }

        public virtual void AddReferencedVariable(IVariableInternal variable)
        {
            if (_declaredVariables.Contains(variable))
                return;
            _referencedVariables.Add(variable);
            Parent?.AddReferencedVariable(variable);
        }

        public bool TryGetLabel(string name, [NotNullWhen(true)] out IGotoLabelInternal? label) =>
            _labels.TryGetValue(name, out label)
            || (Kind == ScopeKind.Block && Parent?.TryGetLabel(name, out label) is true);

        public IGotoLabelInternal GetOrCreateLabel(string name, GotoLabelStatementSyntax? labelSyntax = null)
        {
            LorettaDebug.Assert(!string.IsNullOrEmpty(name));
            LorettaDebug.AssertNotNull(labelSyntax);

            if (!TryGetLabel(name, out var label))
                label = CreateLabel(name, labelSyntax);

            return label;
        }

        public IGotoLabelInternal CreateLabel(string name, GotoLabelStatementSyntax? labelSyntax = null)
        {
            var label = new GotoLabel(name, labelSyntax);
            _labels[name] = label;
            return label;
        }

        public void AddChildScope(IScopeInternal scope)
        {
            LorettaDebug.Assert(scope.ContainingScope == this);
            _containedScopes.Add(scope);
        }
    }
}
