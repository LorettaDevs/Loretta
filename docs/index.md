# Loretta
A C# (G)Lua lexer, parser, code analysis, transformation and code generation toolkit. For getting started, see the [tutorial](tutorials/creating-a-localizer/part-1.md).

[![Discord](https://img.shields.io/discord/946128539583914074?logo=discord&style=for-the-badge)](https://discord.gg/ExZxSxe7xQ)
[![Loretta.CodeAnalysis.Lua](https://img.shields.io/nuget/v/Loretta.CodeAnalysis.Lua?label=Stable&logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/Loretta.CodeAnalysis.Lua)
[![Loretta.CodeAnalysis.Lua](https://img.shields.io/nuget/vpre/Loretta.CodeAnalysis.Lua?label=Preview&logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/Loretta.CodeAnalysis.Lua/latest)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/GGG-KILLER/Loretta/build-and-test.yml?branch=main&logo=github&style=for-the-badge)](https://github.com/GGG-KILLER/Loretta/actions/workflows/build-and-test.yml)

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
5. FiveM's hash string syntax (only parsing, manual node creation currently not possible);
6. Continue support. The following options are available:
    - No continue at all;
    - Roblox's `continue` which is a contextual keyword;
    - Garry's Mod's `continue` which is a full fledged keyword.

TL;DR: This supports Lua 5.1, Lua 5.2, Lua 5.3, Lua 5.4, LuaJIT 2.0, LuaJIT 2.1, FiveM, GLua and (partially) Luau (Roblox Lua).