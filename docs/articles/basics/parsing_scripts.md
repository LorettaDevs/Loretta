# Parsing Scripts

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

### Rewriting a LuaSyntaxTree
```cs
    return syntaxTree.WithRootAndOptions(rewriter.Visit(syntaxTree.GetRoot()), syntaxTree.Options));
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

### Walking a LuaSyntaxTree
```cs
    walker.Visit(syntaxTree.GetRoot())
```

## Using the syntax factory
The [`SyntaxFactory`](xref:Loretta.CodeAnalysis.Lua.SyntaxFactory*) is the API for creating nodes in Loretta, all nodes are created exclusively through the [`SyntaxFactory`](xref:Loretta.CodeAnalysis.Lua.SyntaxFactory*)

```cs
LuaSyntaxNode SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)
LuaSyntaxNode SyntaxFactory.AssignmentStatement(SeparatedList<PrefixExpressionSyntax>, SeparatedList<ExpressionSyntax>)
LuaSyntaxNode SyntaxFactory.BinaryExpression(SyntaxFacts.GetBinaryExpression(Keyword).Value, Left, Token(Keyword), Right)
```

## Using syntax list
The [`SyntaxList`](xref:Loretta.CodeAnalysis.SyntaxList*) and [`SeparatedSyntaxList`](xref:Loretta.CodeAnalysis.SeparatedSyntaxList*) and their singleton counterparts ([`SingletonList`](xref:Loretta.CodeAnalysis.SingletonList*) and [`SingletonSeparatedList`](xref:Loretta.CodeAnalysis.SingletonSeparatedList*)) have several uses in Loretta such as in tables, parameter lists, typed-luau, argument lists and assignments.

When [`SyntaxList`](xref:Loretta.CodeAnalysis.SyntaxList*) and [`SeparatedSyntaxList`](xref:Loretta.CodeAnalysis.SeparatedSyntaxList*) only have a single element you should use their singleton counterparts. You can create them in the [`SyntaxFactory`](xref:Loretta.CodeAnalysis.Lua.SyntaxFactory*) although you can use ``new`` but it is discouraged due to it being extremely inefficent.

[`SeparatedSyntaxList`](xref:Loretta.CodeAnalysis.SeparatedSyntaxList*) has an ``AddSeparator`` method which add in the token that separates the values, if ``AddSeparator`` is not used it will default to a comma token. You shouldn't use``AddSeparator`` if you have tokens beforehand, you can use the below snippet to specify both nodes and separators
```cs
SyntaxFactory.SeparatedList<TNode>(IEnumerable<TNode>, IEnumerable<SyntaxToken>)
```

## Using syntax facts
The [`SyntaxFacts`](xref:Loretta.CodeAnalysis.Lua.SyntaxFacts*) provide helper functions for Loretta allowing you to shorten code and get information about nodes.

Some examples of the helper functions.
```cs
Option<SyntaxKind> SyntaxFacts.GetUnaryExpression(SyntaxKind)
Option<SyntaxKind> SyntaxFacts.GetCompoundAssignmentOperator(SyntaxKind)
Option<SyntaxKind> SyntaxFacts.GetBinaryExpression(SyntaxKind)
string SyntaxFacts.GetText(SyntaxKind)
```
The below snippet converts compound assignments to binary expressions.
```cs
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

using static Loretta.CodeAnalysis.Lua.SyntaxFactory;
using static Loretta.CodeAnalysis.Lua.SyntaxFacts;

internal class FixCompoundOperators : LuaSyntaxRewriter
{
  public override SyntaxNode VisitCompoundAssignmentStatement(CompoundAssignmentStatementSyntax node)
  {
    var variable = (PrefixExpressionSyntax) Visit(node.Variable);
    var expression = (ExpressionSyntax) Visit(node.Expression);

    // .Value would throw an exception if the Option was None but in this case since we're sure it'll always have a value it's ok to do so
    var operatorKind = GetCompoundAssignmentOperator(node.AssignmentOperatorToken.Kind()).Value;
    var expressionKind = GetBinaryExpression(operatorKind).Value;
    var right = BinaryExpression(expressionKind, variable, Token(operatorKind), expression);
    
    return AssignmentStatement(
              SingletonSeparatedList(variable),
              SingletonSeparatedList<ExpressionSyntax>(right));
  }
}
```
