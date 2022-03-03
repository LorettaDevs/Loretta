# Loretta
A C# (G)Lua lexer, parser, code analysis, transformation and code generation toolkit.

[![Discord](https://img.shields.io/discord/946128539583914074?logo=discord&style=for-the-badge)](https://discord.gg/ExZxSxe7xQ)

![Repo Stats](https://repobeats.axiom.co/api/embed/089a9f7dae190ea8dd0fc0750abbebceea3e86dd.svg "Repobeats analytics image")

This is (another) rewrite from scratch based on Roslyn and [The Complete Syntax of Lua](https://www.lua.org/manual/5.4/manual.html#9) with a few extensions:
1. Operators introduced in Garry's Mod Lua (glua):
    - `&&` for `and`;
    - `||` for `or`;
    - `!=` for `~=`;
    - `!` for `not`;
2. Comment types introduced in Garry's Mod Lua (glua):
    - C style single line comment: `// ...`;
    - C style multi line comment: `/* */`;
3. Characters accepted as part of identifiers by LuaJIT (emojis, non-rendering characters, [or basically any byte above `127`/`0x7F`](https://github.com/LuaJIT/LuaJIT/blob/e9af1abec542e6f9851ff2368e7f196b6382a44c/src/lj_char.c#L10-L13));
4. Luau (Roblox Lua) syntax (partial):
    - Compound assignment: `+=`, `-=`, `*=`, `/=`, `^=`, `%=`, `..=`;
    - If expressions: `if a then b else c` and `if a then b elseif c then d else e`;
    - Typed lua syntax.
5. FiveM's hash string syntax;
6. Continue support. The following options are available:
    - No continue at all;
    - Luau's `continue` which is a contextual keyword;
    - Garry's Mod's `continue` which is a full fledged keyword.

TL;DR: This supports Lua 5.1, Lua 5.2, Lua 5.3, Lua 5.4, LuaJIT 2.0, LuaJIT 2.1, FiveM, GLua and (partially) Luau (Roblox Lua).

## Installing Loretta v0.2
We have two NuGet packages:

| Package | Stable | Latest |
|---------|--------|--------|
| Main    | [![Loretta.CodeAnalysis.Lua](https://img.shields.io/nuget/v/Loretta.CodeAnalysis.Lua?style=for-the-badge)](https://www.nuget.org/packages/Loretta.CodeAnalysis.Lua) | [![Loretta.CodeAnalysis.Lua](https://img.shields.io/nuget/vpre/Loretta.CodeAnalysis.Lua?style=for-the-badge)](https://www.nuget.org/packages/Loretta.CodeAnalysis.Lua/latest) |
| Experimental | [![Loretta.CodeAnalysis.Lua.Experimental](https://img.shields.io/nuget/v/Loretta.CodeAnalysis.Lua.Experimental?style=for-the-badge)](https://www.nuget.org/packages/Loretta.CodeAnalysis.Lua.Experimental) | [![Loretta.CodeAnalysis.Lua.Experimental](https://img.shields.io/nuget/vpre/Loretta.CodeAnalysis.Lua.Experimental?style=for-the-badge)](https://www.nuget.org/packages/Loretta.CodeAnalysis.Lua.Experimental/latest) |

## Using Loretta v0.2

The best source for getting started is the [Introduction Article](https://ggg-killer.github.io/Loretta/articles/intro.html).

### Parsing text
1. (Optional) Pick a [`LuaSyntaxOptions` preset](src/Compilers/Lua/Portable/LuaSyntaxOptions.cs#L12-L104) and then create a `LuaParseOptions` from it. If no preset is picked, `LuaSyntaxOptions.All` is used by default;
2. (Optional) Create a `SourceText` from your code (using one of the `SourceText.From` overloads);
3. Call `LuaSyntaxTree.ParseText` with your `SourceText`/`string`, (optional) `LuaParseOptions`, (optional) `path` and (optional) `CancellationToken`;
4. Do whatever you want with the returned `LuaSyntaxTree`.
