using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.CodeAnalysis.Lua
{
    internal partial class ScopeAndVariableManager
    {
        private class GotoLabelWalker : BaseWalker
        {
            private readonly IDictionary<SyntaxNode, IGotoLabel> _labels;

            public GotoLabelWalker(
                IDictionary<SyntaxNode, IScope> scopes,
                IDictionary<SyntaxNode, IGotoLabel> labels)
                : base(scopes)
            {
                LorettaDebug.AssertNotNull(labels);

                _labels = labels;
            }

            public override void VisitGotoLabelStatement(GotoLabelStatementSyntax node)
            {
                var scope = FindScope(node) ?? throw new System.Exception("Scope not found for node.");
                var label = scope.CreateLabel(node.Identifier.Text, node);
                _labels[node] = label;
            }
        }
    }
}
