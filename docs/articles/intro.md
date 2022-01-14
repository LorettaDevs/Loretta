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
1. (Optional) Pick a [`LuaSyntaxOptions` preset](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxOptions) and then create a [`LuaParseOptions`](xref:Loretta.CodeAnalysis.Lua.LuaParseOptions) from it. If no preset is picked, [`LuaSyntaxOptions.All`](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxOptions.All) is used by default;
2. (Optional) Create a [`SourceText`](xref:Loretta.CodeAnalysis.Text.SourceText) from your code (using one of the [`SourceText.From`](xref:Loretta.CodeAnalysis.Text.SourceText.From*) overloads);
3. Call [`LuaSyntaxTree.ParseText`](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxTree.ParseText*) with your [`SourceText`](xref:Loretta.CodeAnalysis.Text.SourceText)/`string`, (optional) [`LuaParseOptions`](xref:Loretta.CodeAnalysis.Lua.LuaParseOptions), (optional) `path` and (optional) `CancellationToken`;
4. Do whatever you want with the returned [`LuaSyntaxTree`](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxTree).

#### Formatting Code
The [`NormalizeWhitespace`](xref:Loretta.CodeAnalysis.Lua.SyntaxExtensions.NormalizeWhitespace*) method replaces all whitespace and and end of line trivia by normalized (standard code style) ones.

### Accessing scope information
If you'd like to get scoping and variable information, create a new [`Script`](xref:Loretta.CodeAnalysis.Lua.Script) from your [`SyntaxTree`](xref:Loretta.CodeAnalysis.SyntaxTree)s and then do one of the following:
- Access [`Script.RootScope`](xref:Loretta.CodeAnalysis.Lua.Script.RootScope) to get the global scope;
- Call [`Script.GetScope(SyntaxNode)`](xref:Loretta.CodeAnalysis.Lua.Script.GetScope(Loretta.CodeAnalysis.SyntaxNode)) or [`Script.FindScope(SyntaxNode, ScopeKind)`](xref:Loretta.CodeAnalysis.Lua.Script.FindScope(Loretta.CodeAnalysis.SyntaxNode,Loretta.CodeAnalysis.Lua.ScopeKind)) to get an [`IScope`](xref:Loretta.CodeAnalysis.Lua.IScope);
- Call [`Script.GetVariable(SyntaxNode)`](xref:Loretta.CodeAnalysis.Lua.Script.GetVariable(Loretta.CodeAnalysis.SyntaxNode)) to get an [`IVariable`](xref:Loretta.CodeAnalysis.Lua.IVariable);
- Call [`Script.GetLabel(SyntaxNode)`](xref:Loretta.CodeAnalysis.Lua.Script.GetLabel(Loretta.CodeAnalysis.SyntaxNode)) on a [`GotoStatementSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.GotoStatementSyntax) or a [`GotoLabelStatementSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.GotoLabelStatementSyntax) to get an [`IGotoLabel`](xref:Loretta.CodeAnalysis.Lua.IGotoLabel);

#### <a name="using-variables"></a>Using Variables
There are 4 kinds of variables:
- [`VariableKind.Local`](xref:Loretta.CodeAnalysis.Lua.VariableKind.Local) a variable declared in one of the following nodes:
    - [`LocalVariableDeclarationStatementSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.LocalVariableDeclarationStatementSyntax);
    - [`LocalFunctionDeclarationStatementSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.LocalFunctionDeclarationStatementSyntax).
- [`VariableKind.Global`](xref:Loretta.CodeAnalysis.Lua.VariableKind.Global) a variable used without a previous declaration;
- [`VariableKind.Parameter`](xref:Loretta.CodeAnalysis.Lua.VariableKind.Parameter) a function [parameter](xref:Loretta.CodeAnalysis.Lua.Syntax.ParameterSyntax).
- [`VariableKind.Iteration`](xref:Loretta.CodeAnalysis.Lua.VariableKind.Iteration) a variable that is an iteration variable from a [`NumericForStatementSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.NumericForStatementSyntax) or [`GenericForStatementSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.GenericForStatementSyntax);

The interface for variables is [`IVariable`](xref:Loretta.CodeAnalysis.Lua.IVariable) which exposes the following information:
- [`IVariable.Kind`](xref:Loretta.CodeAnalysis.Lua.IVariable.Kind)- The [`VariableKind`](xref:Loretta.CodeAnalysis.Lua.VariableKind);
- [`IVariable.ContainingScope`](xref:Loretta.CodeAnalysis.Lua.IVariable.ContainingScope) - The containing scope;
- [`IVariable.Name`](xref:Loretta.CodeAnalysis.Lua.IVariable.Name) - The variable name (might be `...` for varargs);
- [`IVariable.Declaration`](xref:Loretta.CodeAnalysis.Lua.IVariable.Declaration) - The place where the variable was declared (`null` for the implcit `arg` and `...` variables available in all files and global variables);
- [`IVariable.ReferencingScopes`](xref:Loretta.CodeAnalysis.Lua.IVariable.ReferencingScopes) - The scopes that have statements that **directly** reference this variable;
- [`IVariable.CapturingScopes`](xref:Loretta.CodeAnalysis.Lua.IVariable.CapturingScopes) - Scopes that capture this variable as an upvalue;
- [`IVariable.ReadLocations`](xref:Loretta.CodeAnalysis.Lua.IVariable.ReadLocations) - Nodes that read from this variable;
- [`IVariable.WriteLocations`](xref:Loretta.CodeAnalysis.Lua.IVariable.WriteLocations) - Nodes that write to this variable;

#### <a name="using-scopes"></a>Using Scopes
There are 4 kinds of scopes:
- [`ScopeKind.Global`](xref:Loretta.CodeAnalysis.Lua.ScopeKind.Global) - There is only one of these, the [`Script.RootScope`](xref:Loretta.CodeAnalysis.Lua.Script.RootScope). It implements [`IScope`](xref:Loretta.CodeAnalysis.Lua.IScope) and only contains globals;
- [`ScopeKind.File`](xref:Loretta.CodeAnalysis.Lua.ScopeKind.File) - These implement [`IFileScope`](xref:Loretta.CodeAnalysis.Lua.IFileScope) and are the scope for [`CompilationUnitSyntax`es](xref:Loretta.CodeAnalysis.Lua.Syntax.CompilationUnitSyntax) which are a file's root node;
- [`ScopeKind.Function`](xref:Loretta.CodeAnalysis.Lua.ScopeKind.Function) - These implement [`IFunctionScope`](xref:Loretta.CodeAnalysis.Lua.IFunctionScope) and are generated for these nodes:
    - [`AnonymousFunctionExpressionSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.AnonymousFunctionExpressionSyntax);
    - [`LocalFunctionDeclarationStatementSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.LocalFunctionDeclarationStatementSyntax);
    - [`FunctionDeclarationStatementSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.FunctionDeclarationStatementSyntax).
- [`ScopeKind.Block`](xref:Loretta.CodeAnalysis.Lua.ScopeKind.Block) - These implement only [`IScope`](xref:Loretta.CodeAnalysis.Lua.IScope) and are generated for normal blocks from these nodes:
    - [`NumericForStatementSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.NumericForStatementSyntax);
    - [`GenericForStatementSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.GenericForStatementSyntax);
    - [`WhileStatementSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.WhileStatementSyntax);
    - [`RepeatUntilStatementSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.RepeatUntilStatementSyntax);
    - [`IfStatementSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.IfStatementSyntax);
    - [`ElseIfClauseSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.ElseIfClauseSyntax);
    - [`ElseClauseSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.ElseClauseSyntax);
    - [`DoStatementSyntax`](xref:Loretta.CodeAnalysis.Lua.Syntax.DoStatementSyntax).