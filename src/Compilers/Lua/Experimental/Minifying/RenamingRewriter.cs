using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Loretta.Utilities;
using System.Threading;
using System.Linq;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.CodeAnalysis.Lua.Experimental.Minifying
{
    internal partial class RenamingRewriter : LuaSyntaxRewriter
    {
        private readonly Script _script;
        private readonly StateStack _stateStack;
        private readonly RenameTable _renameTable;

        public RenamingRewriter(Script script, NamingStrategy namingStrategy)
        {
            _script = script;
            _stateStack = new StateStack();
            _renameTable = new RenameTable(script, _stateStack, namingStrategy);
        }

        #region Scope Pushing

        public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                _stateStack.EnterScope(scope);
                return base.VisitCompilationUnit(node);
            }
            finally
            {
                _stateStack.ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitAnonymousFunctionExpression(AnonymousFunctionExpressionSyntax node)
        {
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                _stateStack.EnterScope(scope);
                return base.VisitAnonymousFunctionExpression(node);
            }
            finally
            {
                _stateStack.ExitScope(scope);
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
                _stateStack.EnterScope(scope);
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
                _stateStack.ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitGenericForStatement(GenericForStatementSyntax node)
        {
            var expressions = VisitList(node.Expressions);

            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                _stateStack.EnterScope(scope);
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
                _stateStack.ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node)
        {
            var condition = (ExpressionSyntax) Visit(node.Condition) ?? throw new ArgumentNullException("condition");
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                _stateStack.EnterScope(scope);
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
                _stateStack.ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitRepeatUntilStatement(RepeatUntilStatementSyntax node)
        {
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                _stateStack.EnterScope(scope);
                return base.VisitRepeatUntilStatement(node);
            }
            finally
            {
                _stateStack.ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitIfStatement(IfStatementSyntax node)
        {
            var condition = (ExpressionSyntax?) Visit(node.Condition) ?? throw new ArgumentNullException("condition");

            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                _stateStack.EnterScope(scope);
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
                _stateStack.ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitElseIfClause(ElseIfClauseSyntax node)
        {
            var condition = (ExpressionSyntax?) Visit(node.Condition) ?? throw new ArgumentNullException("condition");
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                _stateStack.EnterScope(scope);
                return node.Update(
                    node.ElseIfKeyword,
                    condition,
                    node.ThenKeyword,
                    (StatementListSyntax?) base.Visit(node.Body) ?? throw new ArgumentNullException("body"));
            }
            finally
            {
                _stateStack.ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitLocalVariableDeclarationStatement(LocalVariableDeclarationStatementSyntax node)
        {
            var values = VisitList(node.Values);
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                _stateStack.EnterScope(scope);
                return node.Update(
                    node.LocalKeyword,
                    VisitList(node.Names),
                    node.EqualsToken,
                    values,
                    node.SemicolonToken);
            }
            finally
            {
                _stateStack.ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitLocalFunctionDeclarationStatement(LocalFunctionDeclarationStatementSyntax node)
        {
            var name = (IdentifierNameSyntax?) Visit(node.Name) ?? throw new ArgumentNullException("name");
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                _stateStack.EnterScope(scope);
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
                _stateStack.ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitFunctionDeclarationStatement(FunctionDeclarationStatementSyntax node)
        {
            var name = (FunctionNameSyntax?) Visit(node.Name) ?? throw new ArgumentNullException("name");
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                _stateStack.EnterScope(scope);
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
                _stateStack.ExitScope(scope);
            }
        }

        public override SyntaxNode? VisitDoStatement(DoStatementSyntax node)
        {
            var scope = _script.GetScope(node);
            RoslynDebug.AssertNotNull(scope);
            try
            {
                _stateStack.EnterScope(scope);
                return base.VisitDoStatement(node);
            }
            finally
            {
                _stateStack.ExitScope(scope);
            }
        }

        #endregion Scope Pushing

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
            return newName != null && node.Name != newName
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
