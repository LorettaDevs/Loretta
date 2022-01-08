# Loretta
A C# (G)Lua lexer, parser, code analysis, transformation and code generation toolkit.

![Repo Stats](https://repobeats.axiom.co/api/embed/089a9f7dae190ea8dd0fc0750abbebceea3e86dd.svg "Repobeats analytics image")

This is (another) rewrite from scratch based on Roslyn and [The Complete Syntax of Lua](https://www.lua.org/manual/5.2/manual.html#9) with a few extensions:
1. Operators introduced in Garry's Mod Lua (glua):
    - `&&` for `and`;
    - `||` for `or`;
    - `!=` for `~=`;
    - `!` for `not`;
2. Comment types introduced in Garry's Mod Lua (glua):
    - C style single line comment: `// ...`;
    - C style multi line comment: `/* */`;
3. Characters accepted as part of identifiers by LuaJIT (emojis, non-rendering characters, [or basically any byte above `127`/`0x7F`](https://github.com/LuaJIT/LuaJIT/blob/e9af1abec542e6f9851ff2368e7f196b6382a44c/src/lj_char.c#L10-L13));
4. Luau syntax (partial):
    - Roblox compound assignment: `+=`, `-=`, `*=`, `/=`, `^=`, `%=`, `..=`;
    - If expressions: `if a then b else c` and `if a then b elseif c then d else e`;
5. Lua 5.3 bitwise operators;
6. FiveM's hash string syntax (only parsing, manual node creation currently not possible);
7. Continue support. The following options are available:
    - No continue at all;
    - Roblox's `continue` which is a contextual keyword;
    - Garry's Mod's `continue` which is a full fledged keyword.

## Installing Loretta v0.2
We have two NuGet packages:
| Package      | Stable                                                                                                                                                                                                     | Latest                                                                                                                                                                                                               |
|--------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Main         | [![Loretta.CodeAnalysis.Lua](https://img.shields.io/nuget/v/Loretta.CodeAnalysis.Lua?style=for-the-badge)](https://www.nuget.org/packages/Loretta.CodeAnalysis.Lua)                                        | [![Loretta.CodeAnalysis.Lua](https://img.shields.io/nuget/vpre/Loretta.CodeAnalysis.Lua?style=for-the-badge)](https://www.nuget.org/packages/Loretta.CodeAnalysis.Lua/latest)                                        |
| Experimental | [![Loretta.CodeAnalysis.Lua.Experimental](https://img.shields.io/nuget/v/Loretta.CodeAnalysis.Lua.Experimental?style=for-the-badge)](https://www.nuget.org/packages/Loretta.CodeAnalysis.Lua.Experimental) | [![Loretta.CodeAnalysis.Lua.Experimental](https://img.shields.io/nuget/vpre/Loretta.CodeAnalysis.Lua.Experimental?style=for-the-badge)](https://www.nuget.org/packages/Loretta.CodeAnalysis.Lua.Experimental/latest) |

The experimental package currently contains the following experimental features:
- Constant Folder.

## Using Loretta v0.2

### Parsing text
1. (Optional) Pick a [`LuaSyntaxOptions` preset](src/Compilers/Lua/Portable/LuaSyntaxOptions.cs#L12-L104) and then create a `LuaParseOptions` from it. If no preset is picked, `LuaSyntaxOptions.All` is used by default;
2. (Optional) Create a `SourceText` from your code (using one of the `SourceText.From` overloads);
3. Call `LuaSyntaxTree.ParseText` with your `SourceText`/`string`, (optional) `LuaParseOptions`, (optional) `path` and (optional) `CancellationToken`;
4. Do whatever you want with the returned `LuaSyntaxTree`.

#### Formatting Code
The `NormalizeWhitespace` method replaces all whitespace and and end of line trivia by normalized (standard code style) ones.

### Accessing scope information
If you'd like to get scoping and variable information, create a new `Script` from your `SyntaxTree`s and then do one of the following:
- Access `Script.RootScope` to get the global scope;
- Call [`Script.GetScope(SyntaxNode)`](#using-scopes) or [`Script.FindScope(SyntaxNode, ScopeKind)`](#using-scopes) to get an `IScope`;
- Call [`Script.GetVariable(SyntaxNode)`](#using-variables) to get an `IVariable`;
- Call [`Script.GetLabel(SyntaxNode)`](#using-labels) on a `GotoStatementSyntax` or a `GotoLabelStatementSyntax` to get an `IGotoLabel`;

#### Using Variables
There are 4 kinds of variables:
- `VariableKind.Local` a variable declared in a `LocalVariableDeclarationStatementSyntax`;
- `VariableKind.Global` a variable used without a previous declaration;
- `VariableKind.Parameter` a function parameter;
- `VariableKind.Iteration` a variable that is an iteration variable from a `NumericForLoopSyntax` or `GenericForLoopSyntax`;

The interface for variables is `IVariable` which exposes the following information:
- `IVariable.Kind`- The `VariableKind`;
- `IVariable.Scope` - The containing scope;
- `IVariable.Name` - The variable name (might be `...` for varargs);
- `IVariable.Declaration` - The place where the variable was declared (`null` for the implcit `arg` and `...` variables available in all files and global variables);
- `IVariable.ReferencingScopes` - The scopes that have statements that **directly** reference this variable;
- `IVariable.CapturingScopes` - Scopes that capture this variable as an upvalue;
- `IVariable.ReadLocations` - Nodes that read from this variable;
- `IVariable.WriteLocations` - Nodes that write to this variable;

#### Using Scopes
There are 4 kinds of scopes:
- `ScopeKind.Global` - There is only one of these, the `Script.RootScope`. It implements [`IScope`](#IScope) and only contains globals;
- [`ScopeKind.File`](#IFileScope) - These implement [`IFileScope`](#IFileScope) and are the root scopes for files (`LuaSyntaxTree`s);
- [`ScopeKind.Function`](#IFunctionScope) - These implement [`IFunctionScope`](#IFunctionScope) and are generated for these nodes:
    - `AnonymousFunctionExpressionSyntax`;
    - `LocalFunctionDeclarationStatementSyntax`;
    - `FunctionDeclarationStatementSyntax`.
- [`ScopeKind.Block`](#IScope) - These implement only [`IScope`](#IScope) and are generated for normal blocks from these nodes:
    - `NumericForStatementSyntax`;
    - `GenericForStatementSyntax`;
    - `WhileStatementSyntax`;
    - `RepeatUntilStatementSyntax`;
    - `IfStatementSyntax`;
    - `ElseIfClauseSyntax`;
    - `ElseClauseSyntax`;
    - `DoStatementSyntax`.

##### `IScope`
`IScope`s are the most basic kind of scope and all other scopes derive from it.
The information exposed by them is:
- `IScope.Kind` - The `ScopeKind`;
- `IScope.Node` - The `SyntaxNode` that originated the scope. Will be `null` for global and file scopes;
- `IScope.Parent` - The scope's parent `IScope`. Will be `null` for the global scope;
- `IScope.DeclaredVariables` - The `IVariable`s that were declared in this scope;
- `IScope.ReferencedVariables` - The `IVariable`s that are referenced by this scope or its children;
- `IScope.GotoLabels` - The `IGotoLabel`s directly contained by this scope.

##### `IFunctionScope`
`IFunctionScope`s are scopes from function declarations.
They have everything from [`IScope`s](#IScope) and also:
- `IFunctionScope.Parameters` - The `IVariable` parameters for this function;
- `IFunctionScope.CapturedVariables` - The `IVariable`s captured as upvalues by this function.

##### `IFileScope`
`IFileScope`s are scopes for entire files (`LuaSyntaxTree`s).
They have everything from [`IScope`s](#IScope) and also:
- `IFileScope.ArgVariable` - The implicit `arg` variable available in all lua script files;
- `IFileScope.VarArgParameter` - The implicit vararg parameter available in all lua script files.

### Experimental Features
Experimental features are in the `Loretta.CodeAnalysis.Lua.Experimental` package.

Experimental here can mean a lot of things, but mainly one or more of the following:
- Untested;
- Unstable;
- Unsupported.

Each experimental project below will list its state.

#### Constant Folder
Confidence Level: High
Why is this experimental? No comprehensive testing done.

##### How to use
To use the constant folder, just add the following using to the top of your file:
```cs
using Loretta.CodeAnalysis.Lua.Experimental;
```
and then use the `ConstantFold` extension method for `SyntaxNode` to constant fold that node and its children.

###### Example
The following `Program.cs` file is a simple console program that will constant fold the code in a file passed as an argument:

```cs
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Lua.Experimental;

var path = args[1];

// Read the file into a SourceText
SourceText sourceText;
using (var stream = File.OpenRead(path))
    sourceText = SourceText.From(stream, Encoding.UTF8);

// Parse the text into a tree
var syntaxTree = LuaSyntaxTree.ParseText(sourceText, options: new LuaParseOptions(LuaSyntaxOptions.All), path: path);

// Fold the tree's root.
var root = syntaxTree.GetRoot().ConstantFold();

// Write the tree back to the file
using (var writer = File.OpenText(path))
    root.WriteTo(writer);
```

##### How Does it Work?
The constant folder basically turns operations that it knows are constant into their actual values.

A small (but not comprehensive) set examples are:
| Original Expression | Folded Expression |
|---------------------|-------------------|
| `--1`               | `1`               |
| `not "hi"`          | `false`           |
| `1 + 1`             | `2`               |
| `#"hello"           | `5`               |
| `({a = 1}).a`       | `1`               |
| `({["e" .. not "hi"] = {["e" .. not not "hi"] = (2 + 2 ^ 3 + ~2)}}).efalse["e" .. not (nil ~= nil)]` | `(7)` |