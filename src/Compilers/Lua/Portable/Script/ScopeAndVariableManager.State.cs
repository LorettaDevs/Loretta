using System;
using System.Collections.Immutable;

namespace Loretta.CodeAnalysis.Lua
{
    internal partial class ScopeAndVariableManager
    {
        public class State
        {
            public State(
                IScope rootScope,
                IImmutableDictionary<SyntaxNode, IVariable> variables,
                IImmutableDictionary<SyntaxNode, IScope> scopes,
                IImmutableDictionary<SyntaxNode, IGotoLabel> labels)
            {
                RootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
                Variables = variables ?? throw new ArgumentNullException(nameof(variables));
                Scopes = scopes ?? throw new ArgumentNullException(nameof(scopes));
                Labels = labels ?? throw new ArgumentNullException(nameof(labels));
            }

            public IScope RootScope { get; }
            public IImmutableDictionary<SyntaxNode, IVariable> Variables { get; }
            public IImmutableDictionary<SyntaxNode, IScope> Scopes { get; }
            public IImmutableDictionary<SyntaxNode, IGotoLabel> Labels { get; }
        }
    }
}
