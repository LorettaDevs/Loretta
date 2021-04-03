using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The base interface for scopes.
    /// </summary>
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
        SyntaxReference Node { get; }

        /// <summary>
        /// The parent scope (if any).
        /// </summary>
        IScope? Parent { get; }

        /// <summary>
        /// Contains the variables declared within the scope.
        /// As variables can be shadowed/redeclared, there may be multiple
        /// variables with the same name.
        /// </summary>
        IEnumerable<IVariable> DeclaredVariables { get; }

        /// <summary>
        /// Contains the variables that are captured by this scope.
        /// Variables captured by the scope are variables that weren't declared
        /// on the scope but are used in it.
        /// </summary>
        IEnumerable<IVariable> CapturedVariables { get; }

        /// <summary>
        /// The goto labels contained within this scope.
        /// </summary>
        IEnumerable<IGotoLabel> GotoLabels { get; }
    }

    internal interface IScopeInternal : IScope
    {
        bool TryGetVariable(string name, [NotNullWhen(true)] out IVariable? variable);
        IVariable GetOrCreateVariable(VariableKind kind, string name, SyntaxReference? declaration = null);
        IVariable CreateVariable(VariableKind kind, string name, SyntaxReference? declaration = null);
    }

    internal class Scope : IScopeInternal
    {
        private readonly IDictionary<string, IVariable> _variables = new Dictionary<string, IVariable>(StringComparer.Ordinal);
        private readonly IList<IVariable> _declaredVariables = new List<IVariable>();
        private readonly IList<IVariable> _capturedVariables = new List<IVariable>();
        private readonly IDictionary<string, IGotoLabel> _labels = new Dictionary<string, IGotoLabel>(StringComparer.Ordinal);

        public Scope(ScopeKind kind, SyntaxReference node, IScopeInternal? parent)
        {
            Kind = kind;
            Node = node ?? throw new ArgumentNullException(nameof(node));
            Parent = parent;
            DeclaredVariables = SpecializedCollections.ReadOnlyEnumerable(_declaredVariables);
            CapturedVariables = SpecializedCollections.ReadOnlyEnumerable(_capturedVariables);
            GotoLabels = SpecializedCollections.ReadOnlyEnumerable(_labels.Values);
        }

        public ScopeKind Kind { get; }

        public SyntaxReference Node { get; }

        public IScopeInternal? Parent { get; }

        IScope? IScope.Parent => Parent;

        public IEnumerable<IVariable> DeclaredVariables { get; }

        public IEnumerable<IVariable> CapturedVariables { get; }

        public IEnumerable<IGotoLabel> GotoLabels { get; }

        public bool TryGetVariable(string name, [NotNullWhen(true)] out IVariable? variable) =>
            _variables.TryGetValue(name, out variable)
            || Parent?.TryGetVariable(name, out variable) is true;

        public IVariable GetOrCreateVariable(VariableKind kind, string name, SyntaxReference? declaration = null)
        {
            RoslynDebug.Assert(Kind == ScopeKind.Global || kind != VariableKind.GlobalVariable);
            RoslynDebug.Assert(!string.IsNullOrEmpty(name));

            if (!TryGetVariable(name, out var variable))
                variable = CreateVariable(kind, name, declaration);

            RoslynDebug.Assert(variable.Kind == kind);
            return variable;
        }

        public IVariable CreateVariable(VariableKind kind, string name, SyntaxReference? declaration = null)
        {
            RoslynDebug.Assert(Kind == ScopeKind.Global || kind != VariableKind.GlobalVariable);
            RoslynDebug.Assert(!string.IsNullOrEmpty(name));

            var variable = new Variable(kind, this, name, declaration);
            _variables[name] = variable;
            _declaredVariables.Add(variable);
            return variable;
        }
    }
}
