using System.Collections.Generic;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua
{
    internal partial class ScopeAndVariableManager
    {
        private class GotoWalker : LuaSyntaxWalker
        {
            private readonly IDictionary<SyntaxNode, IScope> _scopes;
            private readonly IDictionary<SyntaxNode, IGotoLabel> _labels;

            public GotoWalker(IDictionary<SyntaxNode, IScope> scopes, IDictionary<SyntaxNode, IGotoLabel> labels)
            {
                RoslynDebug.AssertNotNull(scopes);
                RoslynDebug.AssertNotNull(labels);

                _scopes = scopes;
                _labels = labels;
            }
        }
    }
}
