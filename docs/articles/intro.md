---
uid: articles/intro.md
---
# Getting started with Loretta
## Installing Loretta v0.2
We have two NuGet packages:

| Package | Stable | Latest |
|---------|--------|--------|
| Main    | [![Loretta.CodeAnalysis.Lua](https://img.shields.io/nuget/v/Loretta.CodeAnalysis.Lua?style=for-the-badge)](https://www.nuget.org/packages/Loretta.CodeAnalysis.Lua) | [![Loretta.CodeAnalysis.Lua](https://img.shields.io/nuget/vpre/Loretta.CodeAnalysis.Lua?style=for-the-badge)](https://www.nuget.org/packages/Loretta.CodeAnalysis.Lua/latest) |
| Experimental | [![Loretta.CodeAnalysis.Lua.Experimental](https://img.shields.io/nuget/v/Loretta.CodeAnalysis.Lua.Experimental?style=for-the-badge)](https://www.nuget.org/packages/Loretta.CodeAnalysis.Lua.Experimental) | [![Loretta.CodeAnalysis.Lua.Experimental](https://img.shields.io/nuget/vpre/Loretta.CodeAnalysis.Lua.Experimental?style=for-the-badge)](https://www.nuget.org/packages/Loretta.CodeAnalysis.Lua.Experimental/latest) |

## Using Loretta v0.2

### Parsing text
1. (Optional) Pick a [`LuaSyntaxOptions` preset](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxOptions) and then create a `LuaParseOptions` from it. If no preset is picked, `LuaSyntaxOptions.All` is used by default;
2. (Optional) Create a `SourceText` from your code (using one of the [`SourceText.From`](xref:Loretta.CodeAnalysis.Text.SourceText.From*) overloads);
3. Call [`LuaSyntaxTree.ParseText`](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxTree.ParseText*) with your `SourceText`/`string`, (optional) `LuaParseOptions`, (optional) `path` and (optional) `CancellationToken`;
4. Do whatever you want with the returned `LuaSyntaxTree`.

#### Formatting Code
The `NormalizeWhitespace` method replaces all whitespace and and end of line trivia by normalized (standard code style) ones.

### Accessing scope information
If you'd like to get scoping and variable information, create a new `Script` from your `SyntaxTree`s and then do one of the following:
- Access `Script.RootScope` to get the global scope;
- Call [`Script.GetScope(SyntaxNode)`](xref:articles/intro.md#using-scopes) or [`Script.FindScope(SyntaxNode, ScopeKind)`](xref:articles/intro.md#using-scopes) to get an `IScope`;
- Call [`Script.GetVariable(SyntaxNode)`](xref:articles/intro.md#using-variables) to get an `IVariable`;
- Call `Script.GetLabel(SyntaxNode)` on a `GotoStatementSyntax` or a `GotoLabelStatementSyntax` to get an `IGotoLabel`;

#### <a name="using-variables"></a>Using Variables
There are 4 kinds of variables:
- `VariableKind.Local` a variable declared in a `LocalVariableDeclarationStatementSyntax`;
- `VariableKind.Global` a variable used without a previous declaration;
- `VariableKind.Parameter` a function parameter;
- `VariableKind.Iteration` a variable that is an iteration variable from a `NumericForLoopSyntax` or `GenericForLoopSyntax`;

The interface for variables is [`IVariable`](xref:Loretta.CodeAnalysis.Lua.IVariable) which exposes the following information:
- `IVariable.Kind`- The `VariableKind`;
- `IVariable.Scope` - The containing scope;
- `IVariable.Name` - The variable name (might be `...` for varargs);
- `IVariable.Declaration` - The place where the variable was declared (`null` for the implcit `arg` and `...` variables available in all files and global variables);
- `IVariable.ReferencingScopes` - The scopes that have statements that **directly** reference this variable;
- `IVariable.CapturingScopes` - Scopes that capture this variable as an upvalue;
- `IVariable.ReadLocations` - Nodes that read from this variable;
- `IVariable.WriteLocations` - Nodes that write to this variable;

#### <a name="using-scopes"></a>Using Scopes
There are 4 kinds of scopes:
- `ScopeKind.Global` - There is only one of these, the `Script.RootScope`. It implements [`IScope`](xref:Loretta.CodeAnalysis.Lua.IScope) and only contains globals;
- `ScopeKind.File` - These implement [`IFileScope`](xref:Loretta.CodeAnalysis.Lua.IFileScope) and are the root scopes for [`CompilationUnitSyntax`es](xref:Loretta.CodeAnalysis.Lua.Syntax.CompilationUnitSyntax);
- `ScopeKind.Function` - These implement [`IFunctionScope`](xref:Loretta.CodeAnalysis.Lua.IFunctionScope) and are generated for these nodes:
    - `AnonymousFunctionExpressionSyntax`;
    - `LocalFunctionDeclarationStatementSyntax`;
    - `FunctionDeclarationStatementSyntax`.
- `ScopeKind.Block` - These implement only [`IScope`](xref:Loretta.CodeAnalysis.Lua.IScope) and are generated for normal blocks from these nodes:
    - `NumericForStatementSyntax`;
    - `GenericForStatementSyntax`;
    - `WhileStatementSyntax`;
    - `RepeatUntilStatementSyntax`;
    - `IfStatementSyntax`;
    - `ElseIfClauseSyntax`;
    - `ElseClauseSyntax`;
    - `DoStatementSyntax`.