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

