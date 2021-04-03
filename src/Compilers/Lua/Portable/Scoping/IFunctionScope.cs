using System.Collections.Generic;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// A function's scope.
    /// </summary>
    public interface IFunctionScope : IScope
    {
        /// <summary>
        /// The parameters
        /// </summary>
        IEnumerable<IVariable> Parameters { get; }
    }

    internal interface IFunctionScopeInternal : IScopeInternal, IFunctionScope
    {
        void AddParameter(string name, SyntaxReference declaration);
    }

    internal class FunctionScope : Scope, IFunctionScopeInternal
    {
        private readonly IList<IVariable> _parameters = new List<IVariable>();

        public FunctionScope(ScopeKind kind, SyntaxReference node, IScopeInternal? parent) : base(kind, node, parent)
        {
            Parameters = SpecializedCollections.ReadOnlyEnumerable(_parameters);
        }

        public IEnumerable<IVariable> Parameters { get; }

        public void AddParameter(string name, SyntaxReference declaration) =>
            _parameters.Add(CreateVariable(VariableKind.Parameter, name, declaration));
    }
}
