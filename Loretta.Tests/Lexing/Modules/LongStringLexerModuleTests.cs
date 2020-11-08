using GParse;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Lexing.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Loretta.Tests.Lexing.Modules
{
    [TestClass]
    public class LongStringLexerModuleTests : LexerModuleTestBase<LongStringLexerModule>
    {
        [TestMethod]
        public void CanConsumeNext_ChecksProperly ( )
        {
            var module = new LongStringLexerModule ( );

            AssertCanConsumeNextReturns ( true, module, "[[" );
            AssertCanConsumeNextReturns ( true, module, "[=" );
        }

        [TestMethod]
        public void ConsumeNext_ParsesProperly ( )
        {
            var module = new LongStringLexerModule ( );
            Token<LuaTokenType> tok = TokenFactory.LongString ( "a", "[=a", SourceLocation.Zero );
            AssertConsumeNext (
                tok,
                module,
                tok.Raw,
                LuaDiagnostics.SyntaxError.ThingExpectedAfter (
                    SourceLocation.Zero.To ( new SourceLocation ( 1, 3, 2 ) ),
                    "[",
                    "=" ),
                LuaDiagnostics.SyntaxError.UnfinishedLongString ( tok.Range ) );

            tok = TokenFactory.LongString ( "aaaa", "[[aaaa]]", SourceLocation.Zero );
            AssertConsumeNext (
                tok,
                module,
                tok.Raw );

            tok = TokenFactory.LongString ( @"
aaaa
", @"[[
aaaa
]]", SourceLocation.Zero );
            AssertConsumeNext (
                tok,
                module,
                tok.Raw );

            tok = TokenFactory.LongString ( @"
    [[
        aaaa
    ]]
", @"[=[
    [[
        aaaa
    ]]
]=]", SourceLocation.Zero );
            AssertConsumeNext (
                tok,
                module,
                tok.Raw );

            tok = TokenFactory.LongString ( @"
aaaa
", @"[==[
aaaa
]=]", SourceLocation.Zero );
            AssertConsumeNext (
                tok,
                module,
                tok.Raw,
                LuaDiagnostics.SyntaxError.LongStringWithIncompatibleDelimiters ( tok.Range ) );

            tok = TokenFactory.LongString ( "aaaa", "[=[aaaa]]", SourceLocation.Zero );
            AssertConsumeNext (
                tok,
                module,
                tok.Raw,
                LuaDiagnostics.SyntaxError.LongStringWithIncompatibleDelimiters ( tok.Range ) );

            tok = TokenFactory.LongString ( @"
aaaa
aaaa
aaaa", @"[[
aaaa
aaaa
aaaa", SourceLocation.Zero );
            AssertConsumeNext (
                tok,
                module,
                tok.Raw,
                LuaDiagnostics.SyntaxError.UnfinishedLongString ( tok.Range ) );
        }
    }
}

