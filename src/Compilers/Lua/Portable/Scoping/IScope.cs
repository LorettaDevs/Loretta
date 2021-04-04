using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Loretta.CodeAnalysis.Lua.Syntax;
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
        SyntaxNode? Node { get; }

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
        bool TryGetVariable(string name, [NotNullWhen(true)] out IVariableInternal? variable);
        IVariableInternal GetOrCreateVariable(VariableKind kind, string name, SyntaxNode? declaration = null);
        IVariableInternal CreateVariable(VariableKind kind, string name, SyntaxNode? declaration = null);
        void AddCapturedVariable(IVariableInternal variable);

        bool TryGetLabel(string name, [NotNullWhen(true)] out IGotoLabelInternal? label);
        IGotoLabelInternal GetOrCreateLabel(string name, GotoLabelStatementSyntax label);
        IGotoLabelInternal CreateLabel(string name, GotoLabelStatementSyntax label);
    }

    internal class Scope : IScopeInternal
    {
        private readonly IDictionary<string, IVariableInternal> _variables = new Dictionary<string, IVariableInternal>(StringComparer.Ordinal);
        private readonly ISet<IVariableInternal> _declaredVariables = new HashSet<IVariableInternal>();
        private readonly ISet<IVariableInternal> _capturedVariables = new HashSet<IVariableInternal>();
        private readonly IDictionary<string, IGotoLabelInternal> _labels = new Dictionary<string, IGotoLabelInternal>(StringComparer.Ordinal);

        public Scope(ScopeKind kind, SyntaxNode? node, IScopeInternal? parent)
        {
            Kind = kind;
            Node = node;
            Parent = parent;
            DeclaredVariables = SpecializedCollections.ReadOnlyEnumerable(_declaredVariables);
            CapturedVariables = SpecializedCollections.ReadOnlyEnumerable(_capturedVariables);
            GotoLabels = SpecializedCollections.ReadOnlyEnumerable(_labels.Values);
        }

        public ScopeKind Kind { get; }

        public SyntaxNode? Node { get; }

        public IScopeInternal? Parent { get; }

        IScope? IScope.Parent => Parent;

        public IEnumerable<IVariableInternal> DeclaredVariables { get; }

        IEnumerable<IVariable> IScope.DeclaredVariables => DeclaredVariables;

        public IEnumerable<IVariableInternal> CapturedVariables { get; }

        IEnumerable<IVariable> IScope.CapturedVariables => CapturedVariables;

        public IEnumerable<IGotoLabelInternal> GotoLabels { get; }

        IEnumerable<IGotoLabel> IScope.GotoLabels => GotoLabels;

        public bool TryGetVariable(string name, [NotNullWhen(true)] out IVariableInternal? variable) =>
            _variables.TryGetValue(name, out variable) || Parent?.TryGetVariable(name, out variable) is true;

        public IVariableInternal GetOrCreateVariable(VariableKind kind, string name, SyntaxNode? declaration = null)
        {
            RoslynDebug.Assert(Kind == ScopeKind.Global || kind != VariableKind.Global);
            RoslynDebug.Assert(!string.IsNullOrEmpty(name));

            if (!TryGetVariable(name, out var variable))
                variable = CreateVariable(kind, name, declaration);

            _capturedVariables.Add(variable);
            RoslynDebug.Assert(variable.Kind == kind);
            return variable;
        }

        public IVariableInternal CreateVariable(VariableKind kind, string name, SyntaxNode? declaration = null)
        {
            RoslynDebug.Assert(Kind == ScopeKind.Global || kind != VariableKind.Global);
            RoslynDebug.Assert(!string.IsNullOrEmpty(name));

            var variable = new Variable(kind, this, name, declaration);
            _variables[name] = variable;
            _declaredVariables.Add(variable);
            return variable;
        }

        public void AddCapturedVariable(IVariableInternal variable)
        {
            if (_declaredVariables.Contains(variable))
                return;
            if (Kind == ScopeKind.Function)
            {
                _capturedVariables.Add(variable);
                variable.AddCapturingScope(this);
            }
            variable.AddReferencingScope(this);
            Parent?.AddCapturedVariable(variable);
        }

        public bool TryGetLabel(string name, [NotNullWhen(true)] out IGotoLabelInternal? label) =>
            _labels.TryGetValue(name, out label)
            || (Kind == ScopeKind.Block && Parent?.TryGetLabel(name, out label) is true);

        public IGotoLabelInternal GetOrCreateLabel(string name, GotoLabelStatementSyntax labelSyntax)
        {
            RoslynDebug.Assert(!string.IsNullOrEmpty(name));
            RoslynDebug.AssertNotNull(labelSyntax);

            if (!TryGetLabel(name, out var label))
                label = CreateLabel(name, labelSyntax);

            return label;
        }

        public IGotoLabelInternal CreateLabel(string name, GotoLabelStatementSyntax labelSyntax)
        {
            var label = new GotoLabel(name, labelSyntax);
            _labels[name] = label;
            return label;
        }
    }
}
