using Loretta.CodeAnalysis.Lua.Test.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Lexical;

public abstract class LexicalTestsBase : LuaTestBase
{
    protected static IEnumerable<SyntaxToken> Lex(string text, LuaSyntaxOptions? options = null)
        => SyntaxFactory.ParseTokens(text, options: new LuaParseOptions(options ?? LuaSyntaxOptions.All));

    protected static SyntaxToken LexToken(string text, LuaSyntaxOptions? options = null)
    {
        var result = default(SyntaxToken);
        foreach (var token in Lex(text, options))
        {
            if (result.Kind() == SyntaxKind.None)
                result = token;
            else if (token.Kind() != SyntaxKind.EndOfFileToken) Assert.Fail("More than one token was lexed: " + token);
        }

        if (result.Kind() == SyntaxKind.None) Assert.Fail("No tokens were lexed");

        return result;
    }
}
