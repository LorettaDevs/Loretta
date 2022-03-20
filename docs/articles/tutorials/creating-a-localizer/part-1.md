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

And then save the initial code that was presented in the introduction (the one without all the `local`s at the top) in a file named `sample.lua`.

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

For that we'll use one of the fundamental building blocks of working with trees in Loretta:
@Loretta.CodeAnalysis.Lua.LuaSyntaxWalker.
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
##### [Program.cs](#tab/programcs-1)
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

##### [FunctionCallCollector.cs](#tab/functioncallcollectorcs-1)
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

## 5. Function call processing

Now that we have all the function calls in the script, we need to deduplicate them and group them up so we can map
each function call to a local variable.

First we need to filter the list of function calls to the ones that we can process.
For simplicity's sake, we'll only be accepting function calls on identifiers and members of identifiers (e.g.: `print`
or `math.ceil`).

For that, we'll be implementing a method to check a function call's @Loretta.CodeAnalysis.Lua.Syntax.FunctionCallExpressionSyntax.Expression
to see if it is an @Loretta.CodeAnalysis.Lua.Syntax.IdentifierNameSyntax or a @Loretta.CodeAnalysis.Lua.Syntax.MemberAccessExpressionSyntax
whose base @Loretta.CodeAnalysis.Lua.Syntax.MemberAccessExpressionSyntax.Expression is a @Loretta.CodeAnalysis.Lua.Syntax.IdentifierNameSyntax.

We can check if a node is of a certain type by using the @Loretta.CodeAnalysis.LuaExtensions.IsKind(Loretta.CodeAnalysis.SyntaxNode,Loretta.CodeAnalysis.Lua.SyntaxKind)
method:
```cs
/// <summary>
/// Returns whether the provided node can be turned into a local.
/// </summary>
/// <remarks>
/// A node can be turned into a local if it is a <see cref="IdentifierNameSyntax" />
/// or a <see cref="MemberAccessExpressionSyntax" /> with its base passing this function.
///
/// This means that <c>a</c> passes, <c>a.b</c> passes and <c>a.b.c</c> passes but
/// <c>(1 + 1).a</c> does not nor does <c>(1 + 1).a.b.c</c>.
/// </remarks>
static bool canTurnIntoLocal(SyntaxNode node)
{
    if (node.IsKind(SyntaxKind.IdentifierName))
        return true;
    else if (node.IsKind(SyntaxKind.MemberAccessExpression))
        return canTurnIntoLocal(((MemberAccessExpressionSyntax) node).Expression);
    else
        return false;
}
```

Then afterwards, we'll make a function that will convert a node into its local name:
```cs
/// <summary>
/// Turns a name that we can turn into a local and makes it into a local variable name.
/// </summary>
static IdentifierNameSyntax getLocalName(SyntaxNode node)
{
    Debug.Assert(canTurnIntoLocal(node), "Node cannot be turned into local!");

    // We use a stack because we'll be reading this in reverse order.
    var nameParts = new Stack<string>();

    while (node.IsKind(SyntaxKind.MemberAccessExpression))
    {
        var memberExpr = (MemberAccessExpressionSyntax) node;
        nameParts.Push(memberExpr.MemberName.Text);
        node = memberExpr.Expression;
    }

    nameParts.Push(((IdentifierNameSyntax) node).Name);

    return SyntaxFactory.IdentifierName(string.Join("_", nameParts));
}
```

And then finally, we'll use a bit of LINQ to glue everything together:
```cs
var groups = FunctionCallCollector.Collect(syntaxTree.GetRoot())
                                  .Where(call => canTurnIntoLocal(call.Expression))
                                  .GroupBy(call => getLocalName(call.Expression));
```

Then finally, we can print out the results of the array so we can see something in the console
for the first time!
```cs
foreach (var group in groups)
{
    Console.WriteLine($"{group.Key}:");
    foreach (var call in group)
    {
        Console.WriteLine($"    {call}");
    }
}
```

Which results in the following output:
```console
❯ dotnet run -- .\sample.lua
string_byte:
    string.byte('09,', 1, 3)
    string.byte(input, idx)
tostring:
    tostring(string.sub(input, numstart, idx))
string_sub:
    string.sub(input, numstart, idx)
error:
    error(string.format("Invalid character '%c' found in input.", string.char(ch)))
string_format:
    string.format("Invalid character '%c' found in input.", string.char(ch))
string_char:
    string.char(ch)
```

#### Our code so far
##### [Program.cs](#tab/programcs-2)
```cs
// See https://aka.ms/new-console-template for more information
using Localizer;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Text;
using System.Diagnostics;

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

var groups = FunctionCallCollector.Collect(syntaxTree.GetRoot())
                                  .Where(call => canTurnIntoLocal(call.Expression))
                                  .GroupBy(call => getLocalName(call.Expression));

foreach (var group in groups)
{
    Console.WriteLine(group.Key + ":");
    foreach (var call in group)
    {
        Console.WriteLine($"    {call}");
    }
}

return 0;

/// <summary>
/// Returns whether the provided node can be turned into a local.
/// </summary>
/// <remarks>
/// A node can be turned into a local if it is a <see cref="IdentifierNameSyntax" />
/// or a <see cref="MemberAccessExpressionSyntax" /> with its base passing this function.
///
/// This means that <c>a</c> passes, <c>a.b</c> passes and <c>a.b.c</c> passes but
/// <c>(1 + 1).a</c> does not nor does <c>(1 + 1).a.b.c</c>.
/// </remarks>
static bool canTurnIntoLocal(SyntaxNode node)
{
    if (node.IsKind(SyntaxKind.IdentifierName))
        return true;
    else if (node.IsKind(SyntaxKind.MemberAccessExpression))
        return canTurnIntoLocal(((MemberAccessExpressionSyntax) node).Expression);
    else
        return false;
}

/// <summary>
/// Turns a name that we can turn into a local and makes it into a local variable name.
/// </summary>
static string getLocalName(SyntaxNode node)
{
    Debug.Assert(canTurnIntoLocal(node), "Node cannot be turned into local!");

    // We use a stack because we'll be reading this in reverse order.
    var nameParts = new Stack<string>();

    while (node.IsKind(SyntaxKind.MemberAccessExpression))
    {
        var memberExpr = (MemberAccessExpressionSyntax) node;
        nameParts.Push(memberExpr.MemberName.Text);
        node = memberExpr.Expression;
    }

    nameParts.Push(((IdentifierNameSyntax) node).Name);

    return string.Join("_", nameParts);
}
```

##### [FunctionCallCollector.cs](#tab/functioncallcollectorcs-2)
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

## 6. Rewriting the input file

For this last step, we'll rewrite the input file to add the `local` declaration at the top of the file as
well as rewriting all function calls to their local counterparts.

For this we'll be using another fundamental building block of working with Loretta trees: @Loretta.CodeAnalysis.Lua.LuaSyntaxRewriter.
The rewriter allows you to replace certain nodes in the tree without having to modify all parents yourself
as well as making your life easier since you can handle only the nodes you're interested in.

Another important component of this will be the @Loretta.CodeAnalysis.Lua.SyntaxFactory which is the static
class that's used to create everything related to nodes including the nodes themselves but also the
@Loretta.CodeAnalysis.SyntaxList`1 and the @Loretta.CodeAnalysis.SeparatedSyntaxList`1.

With the LuaSyntaxRewriter and SyntaxFactory introduction out of the way, let's get started on writing the
code with the following `using`s as well as the namespace in a file named `Rewriter.cs`:
```cs
using System.Collections.Immutable;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Localizer;
```

Then we'll create our rewriter which will inherit from @Loretta.CodeAnalysis.Lua.LuaSyntaxRewriter and
have 2 private fields:

1. A field to store the groups we generated when mapping the function calls to their local name counterparts;
2. And another field to store the mapping of the strings to the @Loretta.CodeAnalysis.Lua.Syntax.IdentifierNameSyntax.

It'll have a private constructor as well as a public static method to provide a contained method that
enforces correct usage of our rewriter:
```cs
class Rewriter : LuaSyntaxRewriter
{
    public static SyntaxNode Rewrite(
        IEnumerable<IGrouping<string, FunctionCallExpressionSyntax>> functionCalls,
        SyntaxNode node)
    {
        var rewriter = new Rewriter(functionCalls);
        return rewriter.Visit(node);
    }

    private readonly IEnumerable<IGrouping<string, FunctionCallExpressionSyntax>> _functionCalls;
    private readonly ImmutableDictionary<string, IdentifierNameSyntax> _localNames;

    private Rewriter(IEnumerable<IGrouping<string, FunctionCallExpressionSyntax>> functionCalls)
    {
        _functionCalls = functionCalls;
        // Create deduplicated identifier name nodes since they can be safely reused.
        _localNames = functionCalls.ToImmutableDictionary(g => g.Key, g => SyntaxFactory.IdentifierName(g.Key));
    }
}
```

Then for the first step of our rewriter, we'll override @Loretta.CodeAnalysis.Lua.LuaSyntaxRewriter.VisitCompilationUnit(Loretta.CodeAnalysis.Lua.Syntax.CompilationUnitSyntax)
so that we can add the local variable declaration at the top of it.
The @Loretta.CodeAnalysis.Lua.Syntax.CompilationUnitSyntax represents a parsed file and contains only the
list of statements at the root of the file as well as the EOF token.

But first we'll make it visit every statement in the compilation unit and update the compilation unit with
the results of it:
```cs
    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
    {
        var statements = VisitList(node.Statements.Statements);

        var statementList = node.Statements.WithStatements(statements);
        return node.WithStatements(statementList);
    }
```

Now we need to create the @Loretta.CodeAnalysis.Lua.Syntax.LocalVariableDeclarationStatementSyntax node:
```cs
    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
    {
        var statements = VisitList(node.Statements.Statements);

        // Create the list of names as well the values
        var names = _functionCalls.Select(g => SyntaxFactory.LocalDeclarationName(_localNames[g.Key]));
        var values = _functionCalls.Select(g => g.First().Expression);

        // And then we create the local variable declaration node:
        var localDeclaration = SyntaxFactory.LocalVariableDeclarationStatement(
            SyntaxFactory.SeparatedList(names),
            SyntaxFactory.SeparatedList<ExpressionSyntax>(values));

        // Then we normalize the whitespace in the node so it doesn't look ugly:
        localDeclaration = localDeclaration.NormalizeWhitespace();
        // And we need to add a line break to the last value so that it doesn't show up
        // in the same line as the next statement in the file.
        localDeclaration = localDeclaration.WithTrailingTrivia(
            localDeclaration.GetTrailingTrivia().Add(SyntaxFactory.EndOfLine(Environment.NewLine)));

        // And finally we insert it at the start of the list.
        // Note that we're reassigning the statements list to the result of the call
        // since lists are immutable and any operations on them will return the modified
        // list.
        statements = statements.Insert(0, localDeclaration);

        var statementList = node.Statements.WithStatements(statements);
        return node.WithStatements(statementList);
    }
```

> [!NOTE]
> A lot is being done in the code above so take a while to read it through carefully.

#### Introduction to Trivia
In the code above, we used @Loretta.CodeAnalysis.SyntaxNodeExtensions.WithTrailingTrivia``1(``0,Loretta.CodeAnalysis.SyntaxTriviaList) as well as
@Loretta.CodeAnalysis.SyntaxNodeOrToken.GetTrailingTrivia to manipulate the trivia of the
local variable declaration node we created. But what is trivia?

In Loretta (as in Roslyn), we call extraneous syntax that doesn't necessarily impact parsing
such as whitespaces, line breaks, comments and shebangs (the `#!/bin/bash` you see at the
start of some linux scripts) trivia and they are stored as part of the token preceding or
following them:
- Leading trivia is all trivia located since the first line break after the previous token.
- Trailing trivia is all trivia after a token up to (and including) the first line break.

***

Now we can go back to `Program.cs` to replace the loop where we print the groups with the
following:
```cs
var root = Rewriter.Rewrite(groups, syntaxTree.GetRoot());
root.WriteTo(Console.Out);
```

Which prints out the rewritten node to the console, resulting in the following output:
```lua
local string_byte, tostring, string_sub, error, string_format, string_char = string.byte, tostring, string.sub, error, string.format, string.char
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

Lastly, we now need to rewrite the function calls to use the locals instead of the globals by
overriding @Loretta.CodeAnalysis.Lua.LuaSyntaxRewriter.VisitFunctionCallExpression(Loretta.CodeAnalysis.Lua.Syntax.FunctionCallExpressionSyntax):
```cs
    public override SyntaxNode? VisitFunctionCallExpression(FunctionCallExpressionSyntax node)
    {
        foreach (var group in _functionCalls)
        {
            // Skip to the next group if the current one doesn't have the node
            if (!group.Contains(node))
                continue;

            // Get the IdentifierNameSyntax we created earlier.
            var nameNode = _localNames[group.Key];

            // Import the trivia from old expression
            nameNode = nameNode.WithTriviaFrom(node.Expression);

            // Update the function argument(s)
            var argument = (FunctionArgumentSyntax) Visit(node.Argument)!;

            // And finally, we return the function call with the
            // updated expression.
            return node.Update(nameNode, argument);
        }
        return base.VisitFunctionCallExpression(node);
    }
```

And now when we run the program again, we get the following output in the console:
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
            -- than a plain tostring
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

#### Final code
##### [Program.cs](#tab/programcs-3)
```cs
// See https://aka.ms/new-console-template for more information
using Localizer;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Text;
using System.Diagnostics;

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

var groups = FunctionCallCollector.Collect(syntaxTree.GetRoot())
                                  .Where(call => canTurnIntoLocal(call.Expression))
                                  .GroupBy(call => getLocalName(call.Expression));

foreach (var group in groups)
{
    Console.WriteLine(group.Key + ":");
    foreach (var call in group)
    {
        Console.WriteLine($"    {call}");
    }
}

return 0;

/// <summary>
/// Returns whether the provided node can be turned into a local.
/// </summary>
/// <remarks>
/// A node can be turned into a local if it is a <see cref="IdentifierNameSyntax" />
/// or a <see cref="MemberAccessExpressionSyntax" /> with its base passing this function.
///
/// This means that <c>a</c> passes, <c>a.b</c> passes and <c>a.b.c</c> passes but
/// <c>(1 + 1).a</c> does not nor does <c>(1 + 1).a.b.c</c>.
/// </remarks>
static bool canTurnIntoLocal(SyntaxNode node)
{
    if (node.IsKind(SyntaxKind.IdentifierName))
        return true;
    else if (node.IsKind(SyntaxKind.MemberAccessExpression))
        return canTurnIntoLocal(((MemberAccessExpressionSyntax) node).Expression);
    else
        return false;
}

/// <summary>
/// Turns a name that we can turn into a local and makes it into a local variable name.
/// </summary>
static string getLocalName(SyntaxNode node)
{
    Debug.Assert(canTurnIntoLocal(node), "Node cannot be turned into local!");

    // We use a stack because we'll be reading this in reverse order.
    var nameParts = new Stack<string>();

    while (node.IsKind(SyntaxKind.MemberAccessExpression))
    {
        var memberExpr = (MemberAccessExpressionSyntax) node;
        nameParts.Push(memberExpr.MemberName.Text);
        node = memberExpr.Expression;
    }

    nameParts.Push(((IdentifierNameSyntax) node).Name);

    return string.Join("_", nameParts);
}
```

##### [FunctionCallCollector.cs](#tab/functioncallcollectorcs-3)
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

##### [Rewriter.cs](#tab/rewritercs-3)
```cs
using System.Collections.Immutable;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Localizer;

class Rewriter : LuaSyntaxRewriter
{
    public static SyntaxNode Rewrite(
        IEnumerable<IGrouping<string, FunctionCallExpressionSyntax>> functionCalls,
        SyntaxNode node)
    {
        var rewriter = new Rewriter(functionCalls);
        return rewriter.Visit(node);
    }

    private readonly IEnumerable<IGrouping<string, FunctionCallExpressionSyntax>> _functionCalls;
    private readonly ImmutableDictionary<string, IdentifierNameSyntax> _localNames;

    private Rewriter(IEnumerable<IGrouping<string, FunctionCallExpressionSyntax>> functionCalls)
    {
        _functionCalls = functionCalls;
        // Create deduplicated identifier name nodes since they can be safely reused.
        _localNames = functionCalls.ToImmutableDictionary(g => g.Key, g => SyntaxFactory.IdentifierName(g.Key));
    }

    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
    {
        var statements = VisitList(node.Statements.Statements);

        // Create the list of names as well the values
        var names = _functionCalls.Select(g => SyntaxFactory.LocalDeclarationName(_localNames[g.Key]));
        var values = _functionCalls.Select(g => g.First().Expression);

        // And then we create the local variable declaration node:
        var localDeclaration = SyntaxFactory.LocalVariableDeclarationStatement(
            SyntaxFactory.SeparatedList(names),
            SyntaxFactory.SeparatedList<ExpressionSyntax>(values));

        // Then we normalize the whitespace in the node so it doesn't look ugly:
        localDeclaration = localDeclaration.NormalizeWhitespace();
        // And we need to add a line break to the last value so that it doesn't show up
        // in the same line as the next statement in the file.
        localDeclaration = localDeclaration.WithTrailingTrivia(
            localDeclaration.GetTrailingTrivia().Add(SyntaxFactory.EndOfLine(Environment.NewLine)));

        // And finally we insert it at the start of the list.
        // Note that we're reassigning the statements list to the result of the call
        // since lists are immutable and any operations on them will return the modified
        // list.
        statements = statements.Insert(0, localDeclaration);

        var statementList = node.Statements.WithStatements(statements);
        return node.WithStatements(statementList);
    }

    public override SyntaxNode? VisitFunctionCallExpression(FunctionCallExpressionSyntax node)
    {
        foreach (var group in _functionCalls)
        {
            // Skip to the next group if the current one doesn't have the node
            if (!group.Contains(node))
                continue;

            // Get the IdentifierNameSyntax we created earlier.
            var nameNode = _localNames[group.Key];

            // Import the trivia from old expression
            nameNode = nameNode.WithTriviaFrom(node.Expression);

            // Update the function argument(s)
            var argument = (FunctionArgumentSyntax) Visit(node.Argument)!;

            // And finally, we return the function call with the
            // updated expression.
            return node.Update(nameNode, argument);
        }
        return base.VisitFunctionCallExpression(node);
    }
}
```

***

## 7. What's next?
So we've built a localizer! But it is not perfect.

In part 2 we'll handle checking whether or not the variable the function is being called
(or the variable that contains the field being called) is a global so that we don't create any
locals for the following case:
```lua
local function something(func)
    return func(42)
end

local t = {
    sq = function(x)
        return x * x
    end
}

local k = something(function(x)
    return t.sq(x)
end)
```