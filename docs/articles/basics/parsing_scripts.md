# Parsing Scripts
Complexity Level: Low

## First Lines of Code
Head to your *Program.cs* tab and import the following directive.
```cs
using Loretta.CodeAnalysis.Lua;
```
and then use [`LuaSyntaxTree.ParseText`](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxTree.ParseText*) to parse the script to a [`LuaSyntaxTree`](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxTree*).

If you'd like to parse an expression or a statement, you can use the following in [`SyntaxFactory`](xref:Loretta.CodeAnalysis.Lua.SyntaxFactory*)
```cs
LuaSyntaxNode SyntaxFactory.ParseExpression(string expression)
LuaSyntaxNode SyntaxFactory.ParseStatement(string statement)
```

## Using a rewriter
[`LuaSyntaxRewriter`](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxRewriter*)s are an abstract class that allow you to modify nodes in the script. 

Below is a snippet demonstrating a [`LuaSyntaxRewriter`](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxRewriter*) replacing every true to false in the script.
```cs
internal class ChangeBooleans : LuaSyntaxRewriter 
{
        public override SyntaxNode? VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            return node.Kind() == SyntaxKind.TrueLiteralExpression ? SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression) : base.VisitLiteralExpression(node);
        }
}
```

## Using a walker
[`LuaSyntaxWalker`](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxWalker*)s are an abstract class that are similar to rewriters but cannot modify nodes. 

Below is a snippet demonstrating a [`LuaSyntaxWalker`](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxWalker*) storing every number literal in the script
```cs
internal class FixCompoundOperators : LuaSyntaxWalker
{
  public List<LuaSyntaxNode> NumericalLiteralExpressions = new();
  public override void VisitLiteralExpression(LiteralExpressionSyntax node)
  {
    if (node.Kind() == SyntaxKind.NumericalLiteralExpression)
    {
      NumericalLiteralExpressions.Add(node);
    }
  }
}
```

