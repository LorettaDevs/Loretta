# Working with Syntax Nodes
In Loretta you will be working with nodes that represent the code in different ways, a LiteralExpression node will represent any type of constant in the code such 1 or "Hello World!" and a IfStatement will represent an if statement in your code.

## Literals
You can get the value of a LiteralExpressionSyntax by doing
```cs
node.Token.Value
```

Some literals such as NilLiteralExpressionSyntax








--------------------------------------------------------------------------
# Working with Functions

## [LocalFunctionDeclarationStatement](xref:Loretta.CodeAnalysis.Lua.Syntax.LocalFunctionDeclarationStatement*) and [FunctionDeclarationStatement](xref:Loretta.CodeAnalysis.Lua.Syntax.FunctionDeclarationStatement*)
The `function declaration statements` name is accessible through `Name`, Luau type parameters are accessible through `TypeParameterList`, the parameters are accessible through ``Parameters`, the typed lua return type is accessible through `TypeBinding` and the body is accessible through `Body`.

To create local function declaration statement nodes, you can use [one of the factory methods](xref:xref:Loretta.CodeAnalysis.Lua.SyntaxFactory.LocalFunctionDeclarationStatement*) for it.

## [AnonymousFunctionExpression](xref:Loretta.CodeAnalysis.Lua.Syntax.AnonymousFunctionExpression*)
The [`anonymous function expression`](xref:Loretta.CodeAnalysis.Lua.Syntax.AnonymousFunctionExpression*) Luau type parameters are accessible through `TypeParameterList`, the parameters are accessible through ``Parameters` and the body is accessible through `Body`.

To create function declaration statement nodes, you can use [one of the factory methods](xref:xref:Loretta.CodeAnalysis.Lua.SyntaxFactory.AnonymousFunctionExpression*) for it.

# Working with Parameters

## NamedParameter

## VarArgParameter

# Working with Ifs

## [IfStatement](xref:Loretta.CodeAnalysis.Lua.Syntax.IfStatement*)
The [`if statements`](xref:Loretta.CodeAnalysis.Lua.Syntax.IfStatement*) condition is accessible via `Condition`, the body is accessible through `Body` as a `StatementListSyntax`, All elseifs are accessible with ElseIfClauses and any else is accessible with ElseClause.

To create if statement nodes, you can use [one of the factory methods](xref:xref:Loretta.CodeAnalysis.Lua.SyntaxFactory.IfStatement*) for it.

## [IfExpression](xref:Loretta.CodeAnalysis.Lua.Syntax.IfExpression*)
[`If expressions`](xref:Loretta.CodeAnalysis.Lua.Syntax.IfExpression*) are a ternary operator added in Luau.

The if expression condition is accessible via `Condition`, `TrueValue` is the value that gets set if the condition is truthy and `FalseValue` is the opposite. All elseifs are accessible with ElseIfClauses.

To create if expression nodes, you can use [one of the factory methods](xref:xref:Loretta.CodeAnalysis.Lua.SyntaxFactory.IfExpression*) for it.

# Working with Loops

## [WhileStatement](xref:Loretta.CodeAnalysis.Lua.Syntax.WhileStatement*)
The [`while statement`](xref:Loretta.CodeAnalysis.Lua.Syntax.WhileStatement*) condition is accessible via `Condition` and the body is accessible through `Body` 

To create while statement nodes, you can use [one of the factory methods](xref:xref:Loretta.CodeAnalysis.Lua.SyntaxFactory.WhileStatement*) for it.

## [RepeatUntilStatement](xref:Loretta.CodeAnalysis.Lua.Syntax.RepeatUntilStatement*)
The [`repeat statement`](xref:Loretta.CodeAnalysis.Lua.Syntax.RepeatUntilStatement*) body is accesible via `Body` and the condition is accessible through `Condition`

To create repeat until statement nodes, you can use [one of the factory methods](xref:xref:Loretta.CodeAnalysis.Lua.SyntaxFactory.RepeatUntilStatement*) for it.

## [NumericForStatement](xref:Loretta.CodeAnalysis.Lua.Syntax.NumericForStatement*)
The [`numeric for statement`](xref:Loretta.CodeAnalysis.Lua.Syntax.NumericForStatement*)'s variable is accessible via `Identifier` as a `TypedIdentifierNameSyntax`, the initial value, final value and step value are accessible through `InitialValue`, `FinalValue`, `StepValue`. The body is accessible through `Body`

To create numeric for statement nodes, you can use [one of the factory methods](xref:xref:Loretta.CodeAnalysis.Lua.SyntaxFactory.NumericForStatement*) for it.

## [GenericForStatement](xref:Loretta.CodeAnalysis.Lua.Syntax.GenericForStatement*)
The [`generic for statement`](xref:Loretta.CodeAnalysis.Lua.Syntax.GenericForStatement*)'s have a list of variables accessible through `Identifiers` with `TypedIdentifierNameSyntax`, the expressions after the in keyword are accessible via a list of `Expressions`. The body is accessible through `Body`

To create numeric for statement nodes, you can use [one of the factory methods](xref:xref:Loretta.CodeAnalysis.Lua.SyntaxFactory.NumericForStatement*) for it.

## [TypedIdentifierName](xref:Loretta.CodeAnalysis.Lua.Syntax.TypedIdentifierName*)
The [`typed identifier name`](xref:Loretta.CodeAnalysis.Lua.Syntax.GenericForStatement*) are a node that contain an `IdentifierNameSyntax` and a typed-lua `TypeBindingSyntax`

To create typed identifier name nodes, you can use [one of the factory methods](xref:xref:Loretta.CodeAnalysis.Lua.SyntaxFactory.TypedIdentifierName*) for it.

