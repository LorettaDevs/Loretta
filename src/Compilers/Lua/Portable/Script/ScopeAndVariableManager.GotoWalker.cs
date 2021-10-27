using System.Collections.Generic;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua
{
    internal partial class ScopeAndVariableManager
    {
        private class GotoWalker : BaseWalker
        {
            private readonly IDictionary<SyntaxNode, IGotoLabel> _labels;

            public GotoWalker(
                IDictionary<SyntaxNode, IScope> scopes,
                IDictionary<SyntaxNode, IGotoLabel> labels)
                : base(scopes)
            {
                RoslynDebug.AssertNotNull(labels);

                _labels = labels;
            }

            public override void VisitGotoStatement(GotoStatementSyntax node)
            {
                var scope = FindScope(node) ?? throw new System.Exception("Scope not found for node.");
                var label = scope.GetOrCreateLabel(node.LabelName.Text);
                label.AddJump(node);
                _labels[node] = label;
            }
        }
    }
}
