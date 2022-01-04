using System;
using System.Collections.Generic;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Experimental.Minifying
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
    internal partial class RenamingRewriter : LuaSyntaxRewriter
    {
        private readonly Script _script;
        private readonly Stack<IScope> _stack = new();
        private readonly RenameTable _renameTable;
        private IScope Scope => _stack.Peek();

        public RenamingRewriter(Script script, NamingStrategy namingStrategy, ISlotAllocator slotAllocator)
        {
            _script = script;
            _renameTable = new RenameTable(script, namingStrategy, slotAllocator);
        }

        public void EnterScope(IScope scope) =>
            _stack.Push(scope);

        public void ExitScope(IScope scope)
        {
            var popped = _stack.Pop();
            RoslynDebug.Assert(popped == scope);
        }

        #region Scope Pushing

        public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                EnterScope(scope);
                return base.VisitCompilationUnit(node);
            }
            finally
            {
                ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitAnonymousFunctionExpression(AnonymousFunctionExpressionSyntax node)
        {
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                EnterScope(scope);
                return base.VisitAnonymousFunctionExpression(node);
            }
            finally
            {
                ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitNumericForStatement(NumericForStatementSyntax node)
        {
            // Update the values that are outside of the inner scope first.
            var initialValue = (ExpressionSyntax) Visit(node.InitialValue);
            var finalValue = (ExpressionSyntax) Visit(node.FinalValue);
            var stepValue = node.StepValue;
            if (stepValue is not null)
                stepValue = (ExpressionSyntax) Visit(stepValue);

            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                EnterScope(scope);
                return node.Update(
                    node.ForKeyword,
                    (IdentifierNameSyntax?) Visit(node.Identifier) ?? throw new ArgumentNullException("identifier"),
                    node.EqualsToken,
                    initialValue,
                    node.FinalValueCommaToken,
                    finalValue,
                    node.StepValueCommaToken,
                    stepValue,
                    node.DoKeyword,
                    (StatementListSyntax?) Visit(node.Body) ?? throw new ArgumentNullException("body"),
                    node.EndKeyword,
                    node.SemicolonToken);
            }
            finally
            {
                ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitGenericForStatement(GenericForStatementSyntax node)
        {
            var expressions = VisitList(node.Expressions);

            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                EnterScope(scope);
                return node.Update(
                    node.ForKeyword,
                    VisitList(node.Identifiers),
                    node.InKeyword,
                    expressions,
                    node.DoKeyword,
                    (StatementListSyntax?) Visit(node.Body) ?? throw new ArgumentNullException("body"),
                    node.EndKeyword,
                    node.SemicolonToken);
            }
            finally
            {
                ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node)
        {
            var condition = (ExpressionSyntax) Visit(node.Condition) ?? throw new ArgumentNullException("condition");
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                EnterScope(scope);
                return node.Update(
                    node.WhileKeyword,
                    condition,
                    node.DoKeyword,
                    (StatementListSyntax?) Visit(node.Body) ?? throw new ArgumentNullException("body"),
                    node.EndKeyword,
                    node.SemicolonToken);
            }
            finally
            {
                ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitRepeatUntilStatement(RepeatUntilStatementSyntax node)
        {
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                EnterScope(scope);
                return base.VisitRepeatUntilStatement(node);
            }
            finally
            {
                ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitIfStatement(IfStatementSyntax node)
        {
            var condition = (ExpressionSyntax?) Visit(node.Condition) ?? throw new ArgumentNullException("condition");

            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                EnterScope(scope);
                return node.Update(
                    node.IfKeyword,
                    condition,
                    node.ThenKeyword,
                    (StatementListSyntax?) Visit(node.Body) ?? throw new ArgumentNullException("body"),
                    VisitList(node.ElseIfClauses),
                    (ElseClauseSyntax?) Visit(node.ElseClause),
                    node.EndKeyword,
                    node.SemicolonToken);
            }
            finally
            {
                ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitElseIfClause(ElseIfClauseSyntax node)
        {
            var condition = (ExpressionSyntax?) Visit(node.Condition) ?? throw new ArgumentNullException("condition");
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                EnterScope(scope);
                return node.Update(
                    node.ElseIfKeyword,
                    condition,
                    node.ThenKeyword,
                    (StatementListSyntax?) base.Visit(node.Body) ?? throw new ArgumentNullException("body"));
            }
            finally
            {
                ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitLocalVariableDeclarationStatement(LocalVariableDeclarationStatementSyntax node)
        {
            var values = VisitList(node.Values);
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                EnterScope(scope);
                return node.Update(
                    node.LocalKeyword,
                    VisitList(node.Names),
                    node.EqualsToken,
                    values,
                    node.SemicolonToken);
            }
            finally
            {
                ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitLocalFunctionDeclarationStatement(LocalFunctionDeclarationStatementSyntax node)
        {
            var name = (IdentifierNameSyntax?) Visit(node.Name) ?? throw new ArgumentNullException("name");
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                EnterScope(scope);
                return node.Update(
                    node.LocalKeyword,
                    node.FunctionKeyword,
                    name,
                    (ParameterListSyntax?) base.Visit(node.Parameters) ?? throw new ArgumentNullException("parameters"),
                    (StatementListSyntax?) base.Visit(node.Body) ?? throw new ArgumentNullException("body"),
                    node.EndKeyword,
                    node.SemicolonToken);
            }
            finally
            {
                ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitFunctionDeclarationStatement(FunctionDeclarationStatementSyntax node)
        {
            var name = (FunctionNameSyntax?) Visit(node.Name) ?? throw new ArgumentNullException("name");
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                EnterScope(scope);
                return node.Update(
                    node.FunctionKeyword,
                    name,
                    (ParameterListSyntax?) base.Visit(node.Parameters) ?? throw new ArgumentNullException("parameters"),
                    (StatementListSyntax?) base.Visit(node.Body) ?? throw new ArgumentNullException("body"),
                    node.EndKeyword,
                    node.SemicolonToken);
            }
            finally
            {
                ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitDoStatement(DoStatementSyntax node)
        {
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                EnterScope(scope);
                return base.VisitDoStatement(node);
            }
            finally
            {
                ExitScope(scope);
            }
        }

        #endregion Scope Pushing

        #region Renaming

        public override SyntaxNode? VisitNamedParameter(NamedParameterSyntax node)
        {
            var newName = _renameTable.GetNewVariableName(Scope, node);
            return newName != null && node.Name != newName
                ? node.Update(SyntaxFactory.Identifier(newName))
                : node;
        }

        public override SyntaxNode? VisitSimpleFunctionName(SimpleFunctionNameSyntax node)
        {
            var newName = _renameTable.GetNewVariableName(Scope, node);
            return newName != null && node.Name.Text != newName
                ? node.Update(SyntaxFactory.Identifier(newName))
                : node;
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            var newName = _renameTable.GetNewVariableName(Scope, node);
            return newName != null && node.Name != newName
                ? node.Update(SyntaxFactory.Identifier(newName))
                : node;
        }

        #endregion Renaming
    }
}
