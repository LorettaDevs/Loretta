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
3. Characters accepted as part of identifiers by LuaJIT (emojis, non-rendering characters, [or basically any byte above `0x7F`](https://github.com/GGG-KILLER/Loretta/blob/e9b0bef6b3959c3365c311e80c68391dc6ac66e0/Loretta/Utilities/CharUtils.cs#L65-L87));
4. Roblox compound assignment: `+=`, `-=`, `*=`, `/=`, `^=`, `%=`, `..=`.

## Using Loretta
1. Pick a [preset](https://github.com/GGG-KILLER/Loretta/blob/e9b0bef6b3959c3365c311e80c68391dc6ac66e0/Loretta/LuaOptions.cs#L32-L119);
2. Initialize a new `LuaLexerBuilder` and a `LuaParserBuilder` by passing the selected preset to its constructor. These should be used for the lifetime of your application;
3. Then to parse code:
    1. Obtain an `IProgress<Diagnostic>` to be able to receive generated diagnostics (GParse provides a simple one through `GParse.Collections.DiagnosticList` which implements both `IProgress<Diagnostic>` and `IReadOnlyList<Diagnostic>`);
    2. Create a lexer by calling `LuaLexerBuilder.CreateLexer(String, IProgress<Diagnostic>)` with the code to be parsed and the created `IProgress<Diagnostic>`;
    3. Create a token reader by passing the built lexer to `TokenReader<LuaTokenType>`'s constructor;
    4. Create a parser by calling `LuaParserBuilder.CreateLexer(ITokenReader<LuaTokenType>, IProgress<Diagnostic>)` with the created token reader and `IProgress<Diagnostic>`;
    5. Call the `LuaParser.Parse()` method to parse the full code (may throw `FatalParsingException`s depending on the code's content);
    6. Then with the obtained tree you can:
        1. Transform code with a tree folder (Loretta comes with a very simple `ConstantFolder` as an example);
        2. Process the tree with a tree walker (Loretta comes with a `FormattedLuaCodeSerializer` to transform the tree back into code and as an example).

### Code sample:

```cs
public static class CodeFormatter
{
    private static readonly LuaLexerBuilder _lexerBuilder = new LuaLexerBuilder ( LuaOptions.GMod );
    private static readonly LuaParserBuilder _parserBuilder = new LuaParserBuilder ( LuaOptions.GMod );
    
    public static (IEnumerable<Diagnostic> diagnostics, String? formatted) Format ( String code )
    {
        var diagnostics = new DiagnosticList ( );
        var lexer = this._lexerBuilder.CreateLexer ( code, diagnostics );
        var tokenReader = new TokenReader<LuaTokenType> ( );
        var parser = this._parserBuilder.CreateParser ( lexer, diagnostics );
        var tree = parser.Parse ( );
        if ( diagnostics.Any ( diagnostic => diagnostic.Severity == DiagnosticSeverity.Error ) )
        {
            return (diagnostics, null);
        }
        
        return (diagnostics, FormattedLuaCodeSerializer.Format ( LuaOptions.GMod, tree ));
    }
}
```
