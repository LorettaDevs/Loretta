# Working with Literals

## [StringLiteralExpression](xref:Loretta.CodeAnalysis.Lua.Syntax.StringLiteralExpression*)
The `string literal expression` is a [`LiteralExpressionSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.LiteralExpression*)

## Getting the string
```cs
    (string) node.Token.Value
```

## [NumericalLiteralExpression](xref:Loretta.CodeAnalysis.Lua.Syntax.NumericalLiteralExpression*)
The `numerical literal expression` is a [`LiteralExpressionSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.LiteralExpression*)

## Getting the number
```cs
    (double) node.Token.Value
```

## [`TrueLiteralExpression`](xref:Loretta.CodeAnalysis.Lua.Syntax.TrueLiteralExpression*) and [`FalseLiteralExpression`](xref:Loretta.CodeAnalysis.Lua.Syntax.FalseLiteralExpression*)
The `boolean literals` are a [`LiteralExpressionSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.LiteralExpression*)

## Getting the boolean
```cs
    (bool) node.Token.Value
```

## [`NilLiteralExpression`](xref:Loretta.CodeAnalysis.Lua.Syntax.NilLiteralExpression*)
The `nil literals` are a [`LiteralExpressionSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.LiteralExpression*)

## Getting the boolean
```cs
    node.Token.Value -> null
```

## [HashStringLiteralExpression](xref:Loretta.CodeAnalysis.Lua.Syntax.HashStringLiteralExpression*)
The `numerical literal expression` is a [`LiteralExpressionSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.LiteralExpression*)

## Getting the hash literal
```cs
    (uint) node.Token.Value
    (string) node.Token.Text
```

## Creating

To create literal nodes, you can use [one of the factory methods](xref:xref:Loretta.CodeAnalysis.Lua.SyntaxFactory.Literal*) for it.