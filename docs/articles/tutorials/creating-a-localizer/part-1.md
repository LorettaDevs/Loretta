# Creating a Localizer - Part 1
Hello, welcome! In this tutorial we'll build a function localizer from scratch using Loretta.

Our objective is to transform the following:
```lua
local zero, nine, comma = string.byte('09,', 1, 3)
-- Parses a comma separated list of numbers into numbers
local function parser(input)
    return function()
        local len = #input
        local numstart = nil

        for idx = 1, len do
            -- Read the char as a byte since it's more efficient
            -- than a plain tostring
            local ch = string.byte(input, idx)

            -- If we have a decimal char, then do nothing other
            -- than set the start position if it's not set
            if ch >= zero and ch <= nine then
                if numstart == nil then
                    numstart = idx
                end
            elseif ch == comma then
                -- Otherwise, if we have a start set, return the parsed
                -- number and set the starting position back to nil
                if numstart ~= nil then
                    return tostring(string.sub(input, numstart, idx))
                    numstart = nil
                end
            else
                error(string.format("Invalid character '%c' found in input.", string.char(ch)))
            end
        end
    end
end
```
Into the following script:
```lua
local string_byte, tostring, string_sub, error, string_format, string_char = string.byte, tostring, string.sub, error, string.format, string.char
local zero, nine, comma = string_byte('09,', 1, 3)
-- Parses a comma separated list of numbers into numbers
local function parser(input)
    return function()
        local len = #input
        local numstart = nil

        for idx = 1, len do
            -- Read the char as a byte since it's more efficient
            -- than a plain string.sub
            local ch = string_byte(input, idx)

            -- If we have a decimal char, then do nothing other
            -- than set the start position if it's not set
            if ch >= zero and ch <= nine then
                if numstart == nil then
                    numstart = idx
                end
            elseif ch == comma then
                -- Otherwise, if we have a start set, return the parsed
                -- number and set the starting position back to nil
                if numstart ~= nil then
                    return tostring(string_sub(input, numstart, idx))
                    numstart = nil
                end
            else
                error(string_format("Invalid character '%c' found in input.", string_char(ch)))
            end
        end
    end
end
```
For LuaJIT (no interpreter) this has no performance difference but for LuaJIT in interpreter mode and PUC Lua it does so there's some benefit to doing it.

Now that introductions are out of the way, let's get started!

## 1. Creating a console project
Since our program will be invoked from the command line, we'll need to create a console project.

First let's create a new console project by running `dotnet new console` in an empty directory:
```console
❯ dotnet new console
The template "Console App" was created successfully.

Processing post-creation actions...
Running 'dotnet restore' on B:\tutorials\localizer\localizer.csproj...
  Determining projects to restore...
  Restored B:\tutorials\localizer\localizer.csproj (in 65 ms).
Restore succeeded.
```

And now let's add a reference to the Loretta nuget package with `dotnet add package Loretta.CodeAnalysis.Lua`:
```console
❯ dotnet add package Loretta.CodeAnalysis.Lua
  Determining projects to restore...
info : Adding PackageReference for package 'Loretta.CodeAnalysis.Lua' into project 'B:\tutorials\localizer\localizer.csproj'.
info :   CACHE https://api.nuget.org/v3/registration5-gz-semver2/loretta.codeanalysis.lua/index.json
info : Restoring packages for B:\tutorials\localizer\localizer.csproj...
info : Package 'Loretta.CodeAnalysis.Lua' is compatible with all the specified frameworks in project 'B:\tutorials\localizer\localizer.csproj'.
info : PackageReference for package 'Loretta.CodeAnalysis.Lua' version '0.2.8' added to file 'B:\tutorials\localizer\localizer.csproj'.
info : Writing assets file to disk. Path: B:\tutorials\localizer\obj\project.assets.json
log  : Restored B:\tutorials\localizer\localizer.csproj (in 67 ms).
```

## 2. Implementing file loading
Now it is time for us to make our program load lua files and parse them. We'll start by loading the files into a
@Loretta.CodeAnalysis.Text.SourceText first by using [SourceText.From](xref:Loretta.CodeAnalysis.Text.SourceText.From*).
Also note that since we're using [new templates](https://aka.ms/new-console-template).

First we'll add the using for [Loretta.CodeAnalysis.Text](xref:Loretta.CodeAnalysis.Text) so that we can access
@Loretta.CodeAnalysis.Text.SourceText and then we add validation for the provided file path:
```cs
if (args.Length < 1)
{
    Console.WriteLine("No file path provided!");
    return 1;
}
if (!File.Exists(args[0]))
{
    Console.WriteLine("The specified file does not exist!");
    return 1;
}
```

Then we actually load the file into a @Loretta.CodeAnalysis.Text.SourceText:
```cs
SourceText text;
using (var stream = File.OpenRead(args[0]))
    text = SourceText.From(stream);
```

The responsibility @Loretta.CodeAnalysis.Text.SourceText has is to store the code in memory in a format that won't
make it end up in the LOH (Large Object Heap) as well as allowing us to obtain specific characters from it and/or
substrings of the code as well as splitting the code into multiple [TextLines](xref:Loretta.CodeAnalysis.Text.TextLine).

We can also use the SourceText to map a @Loretta.CodeAnalysis.Text.TextSpan to a @Loretta.CodeAnalysis.Text.LinePositionSpan
which can then be used for error reporting.

@Loretta.CodeAnalysis.Text.SourceText is also important for obtaining the checksum of a file as well as calculating the
changes between two versions of a file with [SourceText.GetTextChanges](xref:Loretta.CodeAnalysis.Text.SourceText.GetTextChanges*)
or simply applying a set of changes to a file with [SourceText.WithChanges](xref:Loretta.CodeAnalysis.Text.SourceText.WithChanges*)

#### Our code up to this point
```cs
// See https://aka.ms/new-console-template for more information
using Loretta.CodeAnalysis.Text;

if (args.Length < 1)
{
    Console.WriteLine("No file path provided!");
    return 1;
}
if (!File.Exists(args[0]))
{
    Console.WriteLine("The specified file does not exist!");
    return 1;
}

SourceText text;
using (var stream = File.OpenRead(args[0]))
    text = SourceText.From(stream);
return 0;
```

## 3. Parsing code into a tree
Now it is time for us to parse the file we just loaded into a tree so we can manipulate it.

First we'll need to add another using for @Loretta.CodeAnalysis.Lua so we can access @Loretta.CodeAnalysis.Lua.LuaSyntaxTree
so we can call its @Loretta.CodeAnalysis.Lua.LuaSyntaxTree.ParseText* method. Then we'll need to pick a preset for the files
we'll be loading.

The "preset" we'll be choosing is a set of options in the @Loretta.CodeAnalysis.Lua.LuaSyntaxOptions class which defines which
errors the parser will be generating as well as which constructs the parser will accept or not (such as integers, C comments
and C boolean operators for Garry's Mod Lua, Typed Lua for Luau/Roblox Lua and others).

The presets that are currently available are the following:
- [LuaSyntaxOptions.Lua51](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxOptions.Lua51): The preset for Lua 5.1
- [LuaSyntaxOptions.Lua52](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxOptions.Lua52): The preset for Lua 5.2
- [LuaSyntaxOptions.Lua53](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxOptions.Lua53): The preset for Lua 5.3
- [LuaSyntaxOptions.Lua54](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxOptions.Lua54): The preset for Lua 5.4
- [LuaSyntaxOptions.LuaJIT20](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxOptions.LuaJIT20): The preset for LuaJIT 2.0
- [LuaSyntaxOptions.LuaJIT21](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxOptions.LuaJIT21): The preset for LuaJIT 2.1
- [LuaSyntaxOptions.FiveM](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxOptions.FiveM): The preset for FiveM's flavor of Lua 5.3
- [LuaSyntaxOptions.GMod](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxOptions.GMod): The preset for Garry's Mod's flavor of LuaJIT 2.0

And then we have the following 2 that are meant to accept the largest amount of syntax possible.
These presets exist mostly for cases when the file's lua version is not known and as such we try to accept the largest
amount of syntax without erroring (and as such are **not recommented for general usage**):
- [LuaSyntaxOptions.All](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxOptions.All): The preset for accepting the most Lua without integers
- [LuaSyntaxOptions.AllWithIntegers](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxOptions.AllWithIntegers): The preset for accepting the most Lua with integers

  The side effect of accepting integers is that this preset will not accept C comment syntax.

In this snippet the Lua51 preset will be used but you should choose the one that applies best to your use case.
```cs
var parseOptions = new LuaParseOptions(LuaSyntaxOptions.Lua51);
var syntaxTree = LuaSyntaxTree.ParseText(text, parseOptions, args[0]);
```

Here we do 2 things: we define a @Loretta.CodeAnalysis.Lua.LuaParseOptions using the
[LuaSyntaxOptions.Lua51](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxOptions.Lua51) preset and then call
[LuaSyntaxTree.ParseText](xref:Loretta.CodeAnalysis.Lua.LuaSyntaxTree.ParseText*) with the text we loaded earlier as well
as the parse options and the file name through (`args[0]`).

Now, we need to check that the parsed code contains no errors. We'll do that by using
[SyntaxTree.GetDiagnostics](xref:Loretta.CodeAnalysis.SyntaxTree.GetDiagnostics*) and checking that the list of diagnostics
has no errors:

```cs
var hasErrors = false;
foreach (var diagnostic in syntaxTree.GetDiagnostics().OrderByDescending(diag => diag.Severity))
{
    Console.WriteLine(diagnostic.ToString());
    hasErrors |= diagnostic.Severity == DiagnosticSeverity.Error;
}
if (hasErrors)
{
    Console.WriteLine("File has errors! Exiting...");
    return 2;
}
```

In Loretta (as in Roslyn), errors, warnings and infos are called [diagnostics](xref:Loretta.CodeAnalysis.Diagnostic).
A diagnostic contains important information about an error such as:
- [Diagnostic.Id](xref:Loretta.CodeAnalysis.Diagnostic.Id): The diagnostic's ID.
  As an example, `LUA0001` is the diagnostic ID for an invalid string escape.
- [Diagnostic.Location](xref:Loretta.CodeAnalysis.Diagnostic.Location): The location the diagnostic was reported at. This
  is important for being able to point to the user where an error or warning is in their text editor or to output it to the
  command line.
- [Diagnostic.Severity](xref:Loretta.CodeAnalysis.Diagnostic.Severity): The diagnostic's severity (whether it's an error,
  warning, info or suggestion). The value is a member of the @Loretta.CodeAnalysis.DiagnosticSeverity enum.
- [Diagnostic.Descriptor](xref:Loretta.CodeAnalysis.Diagnostic.Descriptor): This is the instance of the diagnostic's definition
  which we call a @Loretta.CodeAnalysis.DiagnosticDescriptor.

For diagnostics, you can think of the @Loretta.CodeAnalysis.DiagnosticDescriptor as a class' definition and the
@Loretta.CodeAnalysis.Diagnostic as the class' instance.

#### Our code up to this point
```cs
// See https://aka.ms/new-console-template for more information
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Text;
using Loretta.CodeAnalysis.Lua;

if (args.Length < 1)
{
    Console.WriteLine("No file path provided!");
    return 1;
}
if (!File.Exists(args[0]))
{
    Console.WriteLine("The specified file does not exist!");
    return 1;
}

SourceText text;
using (var stream = File.OpenRead(args[0]))
    text = SourceText.From(stream);

var parseOptions = new LuaParseOptions(LuaSyntaxOptions.All);
var syntaxTree = LuaSyntaxTree.ParseText(text, parseOptions, args[0]);

var hasErrors = false;
foreach (var diagnostic in syntaxTree.GetDiagnostics().OrderByDescending(diag => diag.Severity))
{
    Console.WriteLine(diagnostic.ToString());
    hasErrors |= diagnostic.Severity == DiagnosticSeverity.Error;
}
if (hasErrors)
{
    Console.WriteLine("File has errors! Exiting...");
    return 2;
}
return 0;
```

## 4. Collecting function calls
Now that we have the parsed tree from the file and have confirmed it does not have any errors, it is time for us
to start extracting the function calls from the tree so that we can create local variables for them.

To that we'll use one of the fundamental building blocks of working with trees in Loretta:
the @Loretta.CodeAnalysis.Lua.LuaSyntaxWalker.
The walker allows us to go through every node of the tree recursively and only act upon the nodes we're interested
in, which in our case is the @Loretta.CodeAnalysis.Lua.Syntax.FunctionCallExpressionSyntax.

Since we'll be implementing a new class for the walker we'll create a new file called `FunctionCallCollector.cs`
which will start out with 3 `using`s for namespaces which we'll need as well as our namespace:
```cs
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Localizer;
```

Then we need to actually create our class and make it inherit from @Loretta.CodeAnalysis.Lua.LuaSyntaxWalker as well
as add a proper constructor for it passing [SyntaxWalkerDepth.Node](xref:Loretta.CodeAnalysis.SyntaxWalkerDepth.Node)
to the @Loretta.CodeAnalysis.Lua.LuaSyntaxWalker constructor as we are not interested in anything below nodes for this
walker:
```cs
internal class FunctionCallCollector : LuaSyntaxWalker
{
    private FunctionCallCollector() : base(SyntaxWalkerDepth.Node)
    {
    }
}
```

The constructor is private because we'll be exposing the functionality of this class as a public static method and it being
a @Loretta.CodeAnalysis.Lua.LuaSyntaxWalker will be an internal implementation detail of the class.

But you might've noticed we're missing something. That's right! We're missing a list so we can store the function calls we'll
be collecting!

For that we'll be using an `ImmutableArray<FunctionCallExpressionSyntax>.Builder` so that later we can return an
`ImmutableArray<FunctionCallExpressionSyntax>`:
```cs
internal class FunctionCallCollector : LuaSyntaxWalker
{
    private readonly ImmutableArray<FunctionCallExpressionSyntax>.Builder _functionCalls;

    private FunctionCallCollector() : base(SyntaxWalkerDepth.Node)
    {
        _functionCalls = ImmutableArray.CreateBuilder<FunctionCallExpressionSyntax>();
    }
}
```

Now, we have to actually add the function calls to the list. We'll do that by overriding the
@Loretta.CodeAnalysis.Lua.LuaSyntaxWalker.VisitFunctionCallExpression method so that we can do something whenever it finds a
function call.
It's also important to keep in mind we'll have to call the `base` method otherwise other function calls that might be contained inside the
one we're visiting will not be visited.
```cs
    public override void VisitFunctionCallExpression(FunctionCallExpressionSyntax node)
    {
        _functionCalls.Add(node);
        base.VisitFunctionCallExpression(node);
    }
```

And lastly, let's add our public static method at the top of our class so that we can actually use this walker:
```cs
    public static ImmutableArray<FunctionCallExpressionSyntax> Collect(SyntaxNode node)
    {
        var collector = new FunctionCallCollector();
        collector.Visit(node);
        return collector._functionCalls.ToImmutable();
    }
```

Now that we have our function call collector done, it's time for us to actually use it back in `Program.cs`:

#### Our code so far
##### [Program.cs](#tab/programcs-4)
```cs
// See https://aka.ms/new-console-template for more information
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Text;
using Loretta.CodeAnalysis.Lua;

if (args.Length < 1)
{
    Console.WriteLine("No file path provided!");
    return 1;
}
if (!File.Exists(args[0]))
{
    Console.WriteLine("The specified file does not exist!");
    return 1;
}

SourceText text;
using (var stream = File.OpenRead(args[0]))
    text = SourceText.From(stream);

var parseOptions = new LuaParseOptions(LuaSyntaxOptions.All);
var syntaxTree = LuaSyntaxTree.ParseText(text, parseOptions, args[0]);

var hasErrors = false;
foreach (var diagnostic in syntaxTree.GetDiagnostics().OrderByDescending(diag => diag.Severity))
{
    Console.WriteLine(diagnostic.ToString());
    hasErrors |= diagnostic.Severity == DiagnosticSeverity.Error;
}
if (hasErrors)
{
    Console.WriteLine("File has errors! Exiting...");
    return 2;
}
return 0;
```

##### [FunctionCallCollector.cs](#tab/functioncallcollectorcs-4)
```cs
using System.Collections.Immutable;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Localizer;

internal class FunctionCallCollector : LuaSyntaxWalker
{
    public static ImmutableArray<FunctionCallExpressionSyntax> Collect(SyntaxNode node)
    {
        var collector = new FunctionCallCollector();
        collector.Visit(node);
        return collector._functionCalls.ToImmutable();
    }

    private readonly ImmutableArray<FunctionCallExpressionSyntax>.Builder _functionCalls;

    private FunctionCallCollector() : base(SyntaxWalkerDepth.Node)
    {
        _functionCalls = ImmutableArray.CreateBuilder<FunctionCallExpressionSyntax>();
    }

    public override void VisitFunctionCallExpression(FunctionCallExpressionSyntax node)
    {
        _functionCalls.Add(node);
        base.VisitFunctionCallExpression(node);
    }
}
```

***