using System.Collections.Generic;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua
{
    internal partial class ScopeAndVariableManager
    {
        private class GotoLabelWalker : LuaSyntaxWalker
        {
            private readonly IDictionary<SyntaxNode, IScope> _scopes;
            private readonly IDictionary<SyntaxNode, IGotoLabel> _labels;

            public GotoLabelWalker(IDictionary<SyntaxNode, IScope> scopes, IDictionary<SyntaxNode, IGotoLabel> labels)
                : base(SyntaxWalkerDepth.Node)
            {
                RoslynDebug.AssertNotNull(scopes);
                RoslynDebug.AssertNotNull(labels);

                _scopes = scopes;
                _labels = labels;
            }

            public override void VisitGotoLabelStatement(GotoLabelStatementSyntax node)
            {
                var scope = (IScopeInternal) _scopes[node.Parent!];
                var label = scope.CreateLabel(node.Identifier.Text, node);
                _labels[node] = label;
            }
        }
    }
}
