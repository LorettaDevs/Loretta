# Minifier
Confidence Level: Low

Why is this experimental? Almost no testing and no support.

## How to use
To use the minifier, just add the following using to the top of your file:
```cs
using Loretta.CodeAnalysis.Lua.Experimental;
```
and then use one of the [`Minify`](xref:Loretta.CodeAnalysis.Lua.Experimental.LuaExtensions.Minify*) extension methods to minify that tree.

If you'd like to use a different naming strategy or slot allocator, you'll need the following using as well:
```cs
using Loretta.CodeAnalysis.Lua.Experimental.Minifying;
```

The minifier has a few customization points (which are all optional):
1. Naming strategies: naming strategies are what's used to convert slots (look in the "How it Works" section to learn about slots) into variable names;

   There are a few builtin naming strategies in [`NamingStrategies`](xref:Loretta.CodeAnalysis.Lua.Experimental.Minifying.NamingStrategies) with the default one being [`Alphabetical`](xref:Loretta.CodeAnalysis.Lua.Experimental.Minifying.NamingStrategies.Alphabetical(System.Int32,System.Collections.Generic.IEnumerable{Loretta.CodeAnalysis.Lua.IScope})).
2. Slot allocators: slot allocators are for advanced usage so it is not recommended you write one unless if you know what you're doing.

   There are two builtin slot allocators (with the default being the [`SortedSlotAllocator`](xref:Loretta.CodeAnalysis.Lua.Experimental.Minifying.SortedSlotAllocator)):
    - [`SequentialSlotAllocator`](xref:Loretta.CodeAnalysis.Lua.Experimental.Minifying.SequentialSlotAllocator): This is the simplest possible slot allocator that does not reuse slots even
      after they are released.
      By using this, every single renamed variable will have a unique name and names will not be reused.
    - [`SortedSlotAllocator`](xref:Loretta.CodeAnalysis.Lua.Experimental.Minifying.SortedSlotAllocator): This is a slot allocator that will pick the first unused slot it has.
      By using this, variable names will be reused aggressively resulting in better compression but less readable code.

### Examples
The following `Program.cs` file is a simple console program that will minify the code in a file passed as an argument:

```cs
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Lua.Experimental;
using Loretta.CodeAnalysis.Lua.Experimental.Minifying;

var path = args[1];

// Read the file into a SourceText
SourceText sourceText;
using (var stream = File.OpenRead(path))
    sourceText = SourceText.From(stream, Encoding.UTF8);

// Parse the text into a tree
var syntaxTree = LuaSyntaxTree.ParseText(sourceText, options: new LuaParseOptions(LuaSyntaxOptions.All), path: path);

// Minify the tree.
syntaxTree = syntaxTree.Minify();
// Examples of alternative calls:
// // Numeric variable names (_0, _1, _2, etc.):
// syntaxTree = syntaxTree.Minify(NamingStrategies.Numeric);
// // No variable name reuse:
// syntaxTree = syntaxTree.Minify(NamingStrategies.Alphabetic, new SequentialSlotAllocator());

// Write the tree back to the file
using (var writer = File.OpenText(Path.ChangeExtension(path, ".min.lua")))
    syntaxTree.GetRoot().WriteTo(writer);
```

## How Does it Work?
Currently the minifier does 2 things: variable renaming and trivia removal.

### Renaming
The renamer goes through the script and does the following choices:
- When it finds a new variable, it:
    - Allocates a slot for it with the provided [`ISlotAllocator`](xref:Loretta.CodeAnalysis.Lua.Experimental.Minifying.ISlotAllocator);
    - Renames the variable with the name converted using the provided naming strategy.
- When it finds an existing variable, it:
    - Renames the variable with the name converted using the provided naming strategy;
    - Checks if this is the last use of the variable and then releases the slot if it is.

When naming a variable, the [`NamingStrategy`](xref:Loretta.CodeAnalysis.Lua.Experimental.Minifying.NamingStrategy) will generate a name that will not conflict with other names in the file.

### Trivia Removal
The trivial removal step removes all trivia (comments and whitespace) and only inserts a space where it would be necessary.

## Advanced Usage
### [`NamingStrategy`](xref:Loretta.CodeAnalysis.Lua.Experimental.Minifying.NamingStrategy)
A naming strategy is provided two things:
- The slot to convert to a name;
- The scopes the name will be added to.

The naming slot must:
- be deterministc (given the same slot and scope set it must generate the same variable name);
- not generate names that conflict with others in the provided scopes (or its parent scopes);
- generate valid identifier names.

Since none of these are validated by the minifier, you should have your own test suite for it.

### [`ISlotAllocator`](xref:Loretta.CodeAnalysis.Lua.Experimental.Minifying.ISlotAllocator)
A slot allocator has one main job: allocating slots (numbers) for variables.

The minifier will request a slot from the allocator which will then be reserved for use until it is released.
The slot allocator must not return a slot that is already allocated.

The minifier does not validate if the allocator returns an already allocated slot so this must be checked with
your own test suite.