using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.CodeAnalysis.Lua
{
    internal partial class ScopeAndVariableManager
    {
        private class ScopeAndVariableWalker : BaseWalker
        {
            private readonly Scope _rootScope;
            private readonly IDictionary<SyntaxNode, IVariable> _variables;
            private readonly Stack<IScopeInternal> _scopeStack = new();

            public ScopeAndVariableWalker(
                Scope rootScope,
                IDictionary<SyntaxNode, IVariable> variables,
                IDictionary<SyntaxNode, IScope> scopes)
                : base(scopes)
            {
                LorettaDebug.AssertNotNull(rootScope);
                LorettaDebug.AssertNotNull(variables);
                LorettaDebug.AssertNotNull(scopes);

                _rootScope = rootScope;
                _variables = variables;
                _scopeStack.Push(rootScope);
            }

            private IScopeInternal Scope => _scopeStack.Peek();

            private IFileScopeInternal CreateFileScope(SyntaxNode node)
            {
                var scope = new FileScope(node, Scope);
                _scopes.Add(node, scope);
                _scopeStack.Push(scope);
                return scope;
            }

            private IFunctionScopeInternal CreateFunctionScope(SyntaxNode node)
            {
                var scope = new FunctionScope(node, Scope);
                _scopes.Add(node, scope);
                _scopeStack.Push(scope);
                return scope;
            }

            private IScopeInternal CreateBlockScope(SyntaxNode node)
            {
                var scope = new Scope(ScopeKind.Block, node, Scope);
                _scopes.Add(node, scope);
                _scopeStack.Push(scope);
                return scope;
            }

            private IScopeInternal PopScope(IScopeInternal scope)
            {
                var poppedScope = _scopeStack.Pop();
                LorettaDebug.Assert(poppedScope == scope);
                return poppedScope;
            }

            private IVariableInternal GetVariableOrCreateGlobal(string name)
            {
                if (!Scope.TryGetVariable(name, out var variable))
                    variable = _rootScope.CreateVariable(VariableKind.Global, name);
                return variable;
            }

            private static IVariableInternal CreateParameter(
                IFunctionScopeInternal scope,
                ParameterSyntax parameter)
            {
                var name = parameter.Kind() switch
                {
                    SyntaxKind.NamedParameter => ((NamedParameterSyntax) parameter).Name,
                    SyntaxKind.VarArgParameter => "...",
                    _ => throw new InvalidOperationException($"{parameter.Kind()} is not a known parameter kind.")
                };
                return scope.AddParameter(name, parameter);
            }
            private static IVariableInternal CreateParameter(
                IFunctionScopeInternal scope,
                string name) =>
                scope.AddParameter(name, null);

            public override void VisitCompilationUnit(CompilationUnitSyntax node)
            {
                var scope = CreateFileScope(node);
                try
                {
                    Visit(node.Statements);
                }
                finally
                {
                    PopScope(scope);
                }
            }

            public override void VisitAnonymousFunctionExpression(AnonymousFunctionExpressionSyntax node)
            {
                var scope = CreateFunctionScope(node);
                try
                {
                    foreach (var parameter in node.Parameters.Parameters)
                        _variables.Add(parameter, CreateParameter(scope, parameter));
                    Visit(node.Body);
                }
                finally
                {
                    PopScope(scope);
                }
            }

            public override void VisitVarArgExpression(VarArgExpressionSyntax node)
            {
                var variable = GetVariableOrCreateGlobal("...");
                _variables.Add(node, variable);
                variable.AddReadLocation(node);
                variable.AddReferencingScope(Scope);
                Scope.AddReferencedVariable(variable);
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (node.IsMissing || string.IsNullOrWhiteSpace(node.Name))
                    return;
                var variable = GetVariableOrCreateGlobal(node.Name);
                _variables.Add(node, variable);
                variable.AddReadLocation(node);
                variable.AddReferencingScope(Scope);
                Scope.AddReferencedVariable(variable);
            }

            public override void VisitAssignmentStatement(AssignmentStatementSyntax node)
            {
                // Visit values first so that we don't end up with circular references.
                Visit(node.EqualsValues);

                foreach (var assignee in node.Variables)
                {
                    if (assignee is IdentifierNameSyntax identifierName)
                    {
                        if (identifierName.IsMissing || string.IsNullOrWhiteSpace(identifierName.Name))
                            continue;
                        var variable = GetVariableOrCreateGlobal(identifierName.Name);
                        _variables.Add(assignee, variable);
                        variable.AddWriteLocation(node);
                        variable.AddReferencingScope(Scope);
                        Scope.AddReferencedVariable(variable);
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
                    if (identifierName.IsMissing || string.IsNullOrWhiteSpace(identifierName.Name))
                        return;
                    var variable = GetVariableOrCreateGlobal(identifierName.Name);
                    _variables.Add(identifierName, variable);
                    variable.AddWriteLocation(node);
                    variable.AddReferencingScope(Scope);
                    Scope.AddReferencedVariable(variable);
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
                    if (node.Identifier.IsMissing || string.IsNullOrWhiteSpace(node.Identifier.Name))
                        return;
                    var variable = scope.CreateVariable(VariableKind.Iteration, node.Identifier.Name, node);
                    _variables.Add(node.Identifier.IdentifierName, variable);
                    _variables.Add(node.Identifier, variable);
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
                    foreach (var typedIdentifierName in node.Identifiers)
                    {
                        var identifierName = typedIdentifierName.IdentifierName;
                        if (identifierName.IsMissing || string.IsNullOrWhiteSpace(identifierName.Name))
                            continue;
                        var variable = scope.CreateVariable(VariableKind.Iteration, identifierName.Name, node);
                        _variables.Add(typedIdentifierName, variable);
                        _variables.Add(identifierName, variable);
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
                Visit(node.EqualsValues);
                foreach (var localName in node.Names)
                {
                    var name = localName.IdentifierName;
                    if (name.IsMissing || string.IsNullOrWhiteSpace(name.Name))
                        continue;
                    var variable = Scope.CreateVariable(VariableKind.Local, name.Name, node);
                    _variables.Add(localName, variable);
                    _variables.Add(name, variable);
                    variable.AddWriteLocation(node);
                    variable.AddReferencingScope(Scope);
                    Scope.AddReferencedVariable(variable);
                }
            }

            public override void VisitLocalFunctionDeclarationStatement(LocalFunctionDeclarationStatementSyntax node)
            {
                if (!node.Name.IsMissing && !string.IsNullOrWhiteSpace(node.Name.Name))
                {
                    var variable = Scope.CreateVariable(VariableKind.Local, node.Name.Name, node);
                    _variables.Add(node.Name, variable);
                    variable.AddWriteLocation(node);
                    variable.AddReferencingScope(Scope);
                    Scope.AddReferencedVariable(variable);
                }

                var scope = CreateFunctionScope(node);
                try
                {
                    foreach (var parameter in node.Parameters.Parameters)
                        _variables.Add(parameter, CreateParameter(scope, parameter));
                    Visit(node.Body);
                }
                finally
                {
                    PopScope(scope);
                }
            }

            public override void VisitSimpleFunctionName(SimpleFunctionNameSyntax node)
            {
                var variable = GetVariableOrCreateGlobal(node.Name.Text);
                _variables.Add(node, variable);

                if (node.Parent is FunctionDeclarationStatementSyntax)
                    variable.AddWriteLocation(node);
                else
                    variable.AddReadLocation(node);
                variable.AddReferencingScope(Scope);
                Scope.AddReferencedVariable(variable);
            }

            public override void VisitFunctionDeclarationStatement(FunctionDeclarationStatementSyntax node)
            {
                Visit(node.Name);

                var scope = CreateFunctionScope(node);
                try
                {
                    if (node.Name.IsKind(SyntaxKind.MethodFunctionName))
                        CreateParameter(scope, "self");
                    foreach (var parameter in node.Parameters.Parameters)
                        _variables.Add(parameter, CreateParameter(scope, parameter));
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
