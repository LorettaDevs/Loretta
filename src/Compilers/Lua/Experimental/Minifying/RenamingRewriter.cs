using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Experimental.Minifying
{
    internal partial class RenamingRewriter : LuaSyntaxRewriter
    {
        private readonly Script _script;
        private readonly RenameTable _renameTable;

        public RenamingRewriter(Script script, NamingStrategy namingStrategy, ISlotAllocator slotAllocator)
        {
            _script = script;
            _renameTable = new RenameTable(script, namingStrategy, slotAllocator);
        }

        #region Order Fixing - Fixes the order of operations so renaming happens properly

        public override SyntaxNode? VisitLocalVariableDeclarationStatement(LocalVariableDeclarationStatementSyntax node)
        {
            // This needs to happen first.
            var equalsValues = (EqualsValuesClauseSyntax?) Visit(node.EqualsValues);
            return node.Update(
                node.LocalKeyword,
                VisitList(node.Names),
                equalsValues,
                node.SemicolonToken);
        }

        public override SyntaxNode? VisitAssignmentStatement(AssignmentStatementSyntax node)
        {
            // This needs to happen first.
            var equalsValues = (EqualsValuesClauseSyntax) (Visit(node.EqualsValues) ?? throw ExceptionUtilities.Unreachable);
            return node.Update(
                VisitList(node.Variables),
                equalsValues,
                node.SemicolonToken);
        }

        #endregion Order Fixing

        #region Renaming

        public override SyntaxNode? VisitNamedParameter(NamedParameterSyntax node)
        {
            var newName = _renameTable.GetNewVariableName(node);
            return newName != null && node.Name != newName
                ? node.Update(SyntaxFactory.Identifier(newName))
                : node;
        }

        public override SyntaxNode? VisitSimpleFunctionName(SimpleFunctionNameSyntax node)
        {
            var newName = _renameTable.GetNewVariableName(node);
            return newName != null && node.Name.Text != newName
                ? node.Update(SyntaxFactory.Identifier(newName))
                : node;
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            var newName = _renameTable.GetNewVariableName(node);
            return newName != null && node.Name != newName
                ? node.Update(SyntaxFactory.Identifier(newName))
                : node;
        }

        #endregion Renaming
    }
}
