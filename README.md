# Loretta
A C# (G)Lua lexer, parser, code analysis and transformation (rather weak currently) and code generation toolkit.

This is a rewrite from scratch based on [The Complete Syntax of Lua](https://www.lua.org/manual/5.2/manual.html#9) with a few extensions:
1. Operators introduced in Garry's Mod Lua (glua):
    - `&&` for `and`;
    - `||` for `or`;
    - `!=` for `~=`;
    - `!` for `not`;
2. Comment types introduced in Garry's Mod Lua (glua):
    - C style single line comment: `// ...`;
    - C style multi line comment: `/* */`;
3. Characters accepted as part of identifiers by LuaJIT (emojis, non-rendering characters, [or any byte above `0x7F`](https://github.com/GGG-KILLER/Loretta/blob/master/Loretta/Lexing/Modules/IdentifierLexerModule.cs#L61)).
