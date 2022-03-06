# Working with Syntax Nodes
In Loretta you will be working with nodes that represent the code in different ways, a LiteralExpression node will represent any type of constant in the code such 1 or "Hello World!" and a IfStatement will represent an if statement in your code.

## Literals
You can get the value of a LiteralExpressionSyntax by doing
```cs
node.Token.Value
```

Some literals such as NilLiteralExpressionSyntax