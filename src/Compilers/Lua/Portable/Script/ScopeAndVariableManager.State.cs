namespace Loretta.CodeAnalysis.Lua
{
    internal partial class ScopeAndVariableManager
    {
        public class State
        {
            public State(
                IScope rootScope!!,
                IImmutableDictionary<SyntaxNode, IVariable> variables!!,
                IImmutableDictionary<SyntaxNode, IScope> scopes!!,
                IImmutableDictionary<SyntaxNode, IGotoLabel> labels!!)
            {
                RootScope = rootScope;
                Variables = variables;
                Scopes = scopes;
                Labels = labels;
            }

            public IScope RootScope { get; }
            public IImmutableDictionary<SyntaxNode, IVariable> Variables { get; }
            public IImmutableDictionary<SyntaxNode, IScope> Scopes { get; }
            public IImmutableDictionary<SyntaxNode, IGotoLabel> Labels { get; }
        }
    }
}
