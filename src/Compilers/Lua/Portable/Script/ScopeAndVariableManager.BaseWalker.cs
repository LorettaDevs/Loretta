using System.Collections.Generic;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua
{
    internal partial class ScopeAndVariableManager
    {
        private class BaseWalker : LuaSyntaxWalker
        {
            protected readonly IDictionary<SyntaxNode, IScope> _scopes;

            protected BaseWalker(
                IDictionary<SyntaxNode, IScope> scopes,
                SyntaxWalkerDepth depth = SyntaxWalkerDepth.Node)
                : base(depth)
            {
                LorettaDebug.AssertNotNull(scopes);

                _scopes = scopes;
            }

            protected IScopeInternal? FindScope(SyntaxNode leaf)
            {
                foreach (var node in leaf.AncestorsAndSelf())
                {
                    if (_scopes.TryGetValue(node, out var scope))
                        return (IScopeInternal) scope;
                }

                return null;
            }
        }
    }
}
