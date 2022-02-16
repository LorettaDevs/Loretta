using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.CodeAnalysis.Lua
{
    public sealed partial class Script
    {
        private sealed class RenameRewriter : LuaSyntaxRewriter
        {
            private readonly Script _script;
            private readonly IVariable _variable;
            private readonly string _newName;

            public RenameRewriter(Script script!!, IVariable variable!!, string newName)
            {
                if (string.IsNullOrEmpty(newName)) throw new ArgumentException($"'{nameof(newName)}' cannot be null or empty.", nameof(newName));
                _script = script;
                _variable = variable;
                _newName = newName;
            }

            private SyntaxToken CreateIdentifier(SyntaxToken original)
            {
                var identifier = SyntaxFactory.Identifier(
                    original.LeadingTrivia,
                    original.ContextualKind(),
                    _newName,
                    original.TrailingTrivia);
                return original.CopyAnnotationsTo(identifier);
            }

            public override SyntaxNode? VisitNamedParameter(NamedParameterSyntax node)
            {
                if (_script.GetVariable(node) == _variable)
                    return node.Update(CreateIdentifier(node.Identifier));
                return base.VisitNamedParameter(node);
            }

            public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (_script.GetVariable(node) == _variable)
                    return node.Update(CreateIdentifier(node.Identifier));
                return base.VisitIdentifierName(node);
            }

            public override SyntaxNode? VisitSimpleFunctionName(SimpleFunctionNameSyntax node)
            {
                if (_script.GetVariable(node) == _variable)
                    return node.Update(CreateIdentifier(node.Name));
                return base.VisitSimpleFunctionName(node);
            }
        }
    }
}
