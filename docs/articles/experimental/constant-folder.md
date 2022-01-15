# Constant Folder
Confidence Level: High

Why is this experimental? No comprehensive testing done.

## How to use
To use the constant folder, just add the following using to the top of your file:
```cs
using Loretta.CodeAnalysis.Lua.Experimental;
```
and then use the `ConstantFold` extension method for `SyntaxNode` to constant fold that node and its children.

### Example
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

## How Does it Work?
The constant folder basically turns operations that it knows are constant into their actual values.

A small (but not comprehensive) set examples are:

| Original Expression | Folded Expression |
|---------------------|-------------------|
| `--1`               | `1`               |
| `not "hi"`          | `false`           |
| `1 + 1`             | `2`               |
| `#"hello"`           | `5`               |
| `({a = 1}).a`       | `1`               |
| `({["e" .. not "hi"] = {["e" .. not not "hi"] = (2 + 2 ^ 3 + ~2)}}).efalse["e" .. not (nil ~= nil)]` | `(7)` |