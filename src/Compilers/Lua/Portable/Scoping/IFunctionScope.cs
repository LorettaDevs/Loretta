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
        IVariableInternal AddParameter(string name, SyntaxNode declaration);
    }

    internal class FunctionScope : Scope, IFunctionScopeInternal
    {
        private readonly IList<IVariable> _parameters = new List<IVariable>();

        public FunctionScope(SyntaxNode node, IScopeInternal? parent) : base(ScopeKind.Function, node, parent)
        {
            Parameters = SpecializedCollections.ReadOnlyEnumerable(_parameters);
        }

        public IEnumerable<IVariable> Parameters { get; }

        public IVariableInternal AddParameter(string name, SyntaxNode declaration)
        {
            var parameter = CreateVariable(VariableKind.Parameter, name, declaration);
            _parameters.Add(parameter);
            return parameter;
        }
    }
}
