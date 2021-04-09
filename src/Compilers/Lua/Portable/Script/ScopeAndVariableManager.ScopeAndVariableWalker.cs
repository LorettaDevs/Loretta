using System;
using System.Collections.Generic;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua
{
    internal partial class ScopeAndVariableManager
    {
        private class ScopeAndVariableWalker : LuaSyntaxWalker
        {
            private readonly Scope _rootScope;
            private readonly FileScope _fileScope;
            private readonly IDictionary<SyntaxNode, IVariable> _variables;
            private readonly IDictionary<SyntaxNode, IScope> _scopes;
            private readonly Stack<IScopeInternal> _scopeStack = new Stack<IScopeInternal>();

            public ScopeAndVariableWalker(
                Scope rootScope,
                FileScope fileScope,
                IDictionary<SyntaxNode, IVariable> variables,
                IDictionary<SyntaxNode, IScope> scopes)
                : base(SyntaxWalkerDepth.Node)
            {
                RoslynDebug.AssertNotNull(rootScope);
                RoslynDebug.AssertNotNull(fileScope);
                RoslynDebug.AssertNotNull(variables);
                RoslynDebug.AssertNotNull(scopes);

                _rootScope = rootScope;
                _fileScope = fileScope;
                _variables = variables;
                _scopes = scopes;
                _scopeStack.Push(rootScope);
                _scopeStack.Push(fileScope);
            }

            private IScopeInternal Scope => _scopeStack.Peek();
            private IFunctionScopeInternal CreateFunctionScope(SyntaxNode node)
            {
                var scope = new FunctionScope(node, _scopeStack.Peek());
                _scopes[node] = scope;
                _scopeStack.Push(scope);
                return scope;
            }
            private IScopeInternal CreateBlockScope(SyntaxNode node)
            {
                var scope = new Scope(ScopeKind.Block, node, _scopeStack.Peek());
                _scopes[node] = scope;
                _scopeStack.Push(scope);
                return scope;
            }
            private IScopeInternal PopScope(IScopeInternal scope)
            {
                var poppedScope = _scopeStack.Pop();
                RoslynDebug.Assert((object) poppedScope == scope);
                return poppedScope;
            }
            private IVariableInternal GetVariableOrCreateGlobal(string name)
            {
                if (!Scope.TryGetVariable(name, out var variable))
                    variable = _rootScope.CreateVariable(VariableKind.Global, name);
                return variable;
            }
            private static IVariableInternal CreateParameter(IFunctionScopeInternal scope, ParameterSyntax parameter)
            {
                var name = parameter.Kind() switch
                {
                    SyntaxKind.NamedParameter => ((NamedParameterSyntax) parameter).Name,
                    SyntaxKind.VarArgParameter => "...",
                    _ => throw new InvalidOperationException($"{parameter.Kind()} is not a known parameter kind.")
                };
                return scope.AddParameter(name, parameter);
            }

            public override void VisitAnonymousFunctionExpression(AnonymousFunctionExpressionSyntax node)
            {
                var scope = CreateFunctionScope(node);
                try
                {
                    foreach (var parameter in node.Parameters.Parameters)
                        CreateParameter(scope, parameter);
                    Visit(node.Body);
                }
                finally
                {
                    PopScope(scope);
                }
            }

            public override void VisitVarArgExpression(VarArgExpressionSyntax node)
            {
                if (!Scope.TryGetVariable("...", out var variable))
                    variable = _fileScope.VarArgParameter; // This is redundant.
                _variables[node] = variable;
                variable.AddReadLocation(node);
                variable.AddReferencingScope(Scope);
                Scope.AddCapturedVariable(variable);
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                var variable = GetVariableOrCreateGlobal(node.Name);
                _variables[node] = variable;
                variable.AddReadLocation(node);
                variable.AddReferencingScope(Scope);
                Scope.AddCapturedVariable(variable);
            }

            public override void VisitAssignmentStatement(AssignmentStatementSyntax node)
            {
                // Visit values first so that we don't end up with circular references.
                foreach (var value in node.Values)
                    Visit(value);

                foreach (var assignee in node.Variables)
                {
                    if (assignee is IdentifierNameSyntax identifierName)
                    {
                        var variable = GetVariableOrCreateGlobal(identifierName.Name);
                        _variables[assignee] = variable;
                        variable.AddWriteLocation(node);
                        variable.AddReferencingScope(Scope);
                        Scope.AddCapturedVariable(variable);
                    }
                    else
                    {
                        Visit(assignee);
                    }
                }
            }

            public override void VisitCompoundAssignmentStatement(CompoundAssignmentStatementSyntax node)
            {
                // Visit values first so that we don't end up with circular references.
                Visit(node.Expression);

                if (node.Variable is IdentifierNameSyntax identifierName)
                {
                    var variable = GetVariableOrCreateGlobal(identifierName.Name);
                    _variables[identifierName] = variable;
                    variable.AddWriteLocation(node);
                    variable.AddReferencingScope(Scope);
                    Scope.AddCapturedVariable(variable);
                }
                else
                {
                    Visit(node.Variable);
                }
            }

            public override void VisitNumericForStatement(NumericForStatementSyntax node)
            {
                Visit(node.InitialValue);
                Visit(node.FinalValue);
                if (node.StepValue is not null)
                    Visit(node.StepValue);

                var scope = CreateBlockScope(node);
                try
                {
                    var variable = scope.CreateVariable(VariableKind.Iteration, node.Identifier.Name, node);
                    _variables[node.Identifier] = variable;
                    Visit(node.Body);
                }
                finally
                {
                    PopScope(scope);
                }
            }

            public override void VisitGenericForStatement(GenericForStatementSyntax node)
            {
                foreach (var expression in node.Expressions)
                    Visit(expression);
                var scope = CreateBlockScope(node);
                try
                {
                    foreach (var identifierName in node.Identifiers)
                    {
                        var variable = scope.CreateVariable(VariableKind.Iteration, identifierName.Name, node);
                        _variables[identifierName] = variable;
                    }
                    Visit(node.Body);
                }
                finally
                {
                    PopScope(scope);
                }
            }

            public override void VisitWhileStatement(WhileStatementSyntax node)
            {
                Visit(node.Condition);
                var scope = CreateBlockScope(node);
                try
                {
                    Visit(node.Body);
                }
                finally
                {
                    PopScope(scope);
                }
            }

            public override void VisitRepeatUntilStatement(RepeatUntilStatementSyntax node)
            {
                var scope = CreateBlockScope(node);
                try
                {
                    Visit(node.Body);
                    Visit(node.Condition);
                }
                finally
                {
                    PopScope(scope);
                }
            }

            public override void VisitIfStatement(IfStatementSyntax node)
            {
                Visit(node.Condition);
                var scope = CreateBlockScope(node);
                try
                {
                    Visit(node.Body);
                }
                finally
                {
                    PopScope(scope);
                }

                foreach (var elseIfClause in node.ElseIfClauses)
                {
                    Visit(elseIfClause.Condition);
                    scope = CreateBlockScope(elseIfClause);
                    try
                    {
                        Visit(elseIfClause.Body);
                    }
                    finally
                    {
                        PopScope(scope);
                    }
                }

                if (node.ElseClause is not null)
                {
                    scope = CreateBlockScope(node.ElseClause);
                    try
                    {
                        Visit(node.ElseClause.ElseBody);
                    }
                    finally
                    {
                        PopScope(scope);
                    }
                }
            }

            public override void VisitLocalVariableDeclarationStatement(LocalVariableDeclarationStatementSyntax node)
            {
                foreach (var values in node.Values)
                    Visit(values);
                foreach (var name in node.Names)
                {
                    var variable = Scope.CreateVariable(VariableKind.Local, name.Name, node);
                    _variables[name] = variable;
                    variable.AddWriteLocation(node);
                    variable.AddReferencingScope(Scope);
                    Scope.AddCapturedVariable(variable);
                }
            }

            public override void VisitLocalFunctionDeclarationStatement(LocalFunctionDeclarationStatementSyntax node)
            {
                var variable = Scope.CreateVariable(VariableKind.Local, node.Name.Name, node);
                _variables[node.Name] = variable;
                variable.AddWriteLocation(node);
                variable.AddReferencingScope(Scope);
                Scope.AddCapturedVariable(variable);

                var scope = CreateFunctionScope(node);
                try
                {
                    foreach (var parameter in node.Parameters.Parameters)
                        CreateParameter(scope, parameter);
                    Visit(node.Body);
                }
                finally
                {
                    PopScope(scope);
                }
            }

            public override void VisitFunctionDeclarationStatement(FunctionDeclarationStatementSyntax node)
            {
                Visit(node.Name);
                var scope = CreateFunctionScope(node);
                try
                {
                    foreach (var parameter in node.Parameters.Parameters)
                        CreateParameter(scope, parameter);
                    Visit(node.Body);
                }
                finally
                {
                    PopScope(scope);
                }
            }

            public override void VisitDoStatement(DoStatementSyntax node)
            {
                var scope = CreateBlockScope(node);
                try
                {
                    Visit(node.Body);
                }
                finally
                {
                    PopScope(scope);
                }
            }
        }
    }
}
