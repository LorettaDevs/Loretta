# Working with Ifs

## IfStatement
[`Generic if statement`](xref:Loretta.CodeAnalysis.Lua.Syntax.IfStatement*) 

The if statements condition is accessible via `Condition`, the body is accessible through `Body` as a `StatementListSyntax`, All elseifs are accessible with ElseIfClauses and any else is accessible with ElseClause.

#### [`Creating`](https://ggg-killer.github.io/Loretta/api/Loretta.CodeAnalysis.Lua.SyntaxFactory.html#Loretta_CodeAnalysis_Lua_SyntaxFactory_IfStatement_Loretta_CodeAnalysis_Lua_Syntax_ExpressionSyntax_) 

## IfExpression
[`If expressions`](xref:Loretta.CodeAnalysis.Lua.Syntax.IfStatement*) are a ternary operator added in Luau.

The if expression condition is accessible via `Condition`, `TrueValue` is the value that gets set if the condition is truthy and `FalseValue` is the opposite. All elseifs are accessible with ElseIfClauses.

#### [`Creating`](https://ggg-killer.github.io/Loretta/api/Loretta.CodeAnalysis.Lua.SyntaxFactory.html#Loretta_CodeAnalysis_Lua_SyntaxFactory_IfExpression_Loretta_CodeAnalysis_Lua_Syntax_ExpressionSyntax_Loretta_CodeAnalysis_Lua_Syntax_ExpressionSyntax_Loretta_CodeAnalysis_Lua_Syntax_ExpressionSyntax_) 