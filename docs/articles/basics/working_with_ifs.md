# Working with Ifs

## [IfStatement](xref:Loretta.CodeAnalysis.Lua.Syntax.IfStatement*)
The [`if statements`](xref:Loretta.CodeAnalysis.Lua.Syntax.IfStatement*) condition is accessible via `Condition`, the body is accessible through `Body` as a `StatementListSyntax`, All elseifs are accessible with ElseIfClauses and any else is accessible with ElseClause.

To create if statement nodes, you can use [one of the factory methods](xref:xref:Loretta.CodeAnalysis.Lua.SyntaxFactory.IfStatement*) for it.

## [IfExpression](xref:Loretta.CodeAnalysis.Lua.Syntax.IfExpression*)
[`If expressions`](xref:Loretta.CodeAnalysis.Lua.Syntax.IfExpression*) are a ternary operator added in Luau.

The if expression condition is accessible via `Condition`, `TrueValue` is the value that gets set if the condition is truthy and `FalseValue` is the opposite. All elseifs are accessible with ElseIfClauses.

To create if expression nodes, you can use [one of the factory methods](xref:xref:Loretta.CodeAnalysis.Lua.SyntaxFactory.IfExpression*) for it.