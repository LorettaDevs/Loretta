# Working with Syntax Nodes
In Loretta you will be working with nodes that represent the code in different ways, a LiteralExpression node will represent any type of constant in the code such 1 or "Hello World!" and a IfStatement will represent an if statement in your code.

## Literals
You can get the value of a LiteralExpressionSyntax by doing
```cs
node.Token.Value
```

Some literals have special behaviors such as `NilLiteralExpressionSyntax` that return null or the `HashLiteralExpressionSyntax` that return a UInt value or a string through `Token.Text`

## Loops
While and repeat loops can have their conditions checked by doing `Condition` and their bodies by doing `Body`. Numerical for loops have a `InitialValue`, `FinalValue` and `StepValue` and generic for loops have a list of `Identifiers` and `Expressions`. For loops have a special identifier node called `TypedIdentifierName` which have an `Identifier` field and a `TypeBinding` field.

## Ifs
Loretta has support for both `IfStatements` and `IfExpressions`, both have a `Condition` field. `IfStatements` have a `Body` while `IfExpressions` will have a `TrueValue`. `IfStatements` and `IfExpressions` have a `ElseIfClauses` field which is a list of `ElseIfExpressionClauseSyntax`. `IfStatements` may have an `ElseClause` while `IfExpressions` will always have a `FalseValue`

## Functions
[`LocalFunctionDeclarationStatement`](xref:Loretta.CodeAnalysis.Lua.Syntax.LocalFunctionDeclarationStatement) and [`FunctionDeclarationStatement`](xref:Loretta.CodeAnalysis.Lua.Syntax.FunctionDeclarationStatement*)'s name is accessible through `Name`. The typed lua parameters are accessible through `TypeParameterList`, the parameters are accesible through `Parameters`. The typed lua return type is accesible through `TypeBinding` and the body is accesible through `Body`. 

The [`anonymous function expression`](xref:Loretta.CodeAnalysis.Lua.Syntax.AnonymousFunctionExpression*) Luau type parameters are accessible through `TypeParameterList`, the parameters are accessible through ``Parameters` and the body is accessible through `Body`.

## Parameters
[`NamedParameters`](xref:Loretta.CodeAnalysis.Lua.Syntax.NamedParameter) are similar to `TypedIdentifierNameSyntax` having an `Identifier` and a typed lua `TypeBinding`. [`VarArgParameter`](xref:Loretta.CodeAnalysis.Lua.Syntax.VarArgParameter) does not have an identifier but has a typed lua `TypeBinding`.

## Arguments
[`StringFunctionArguments`](xref:Loretta.CodeAnalysis.Lua.Syntax.StringFunctionArgument) have an `Expression` field for the string. [`TableConstructorFunctionArguments`](xref:Loretta.CodeAnalysis.Lua.Syntax.TableConstructorFunctionArgument) have a `TableConstructor` field for the table. [`ExpressionListFunctionArguments`](xref:Loretta.CodeAnalysis.Lua.Syntax.ExpressionListFunctionArgument)have an `Expressions` field for a list of arguments.