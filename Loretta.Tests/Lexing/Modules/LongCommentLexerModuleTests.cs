using GParse;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Lexing.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Loretta.Tests.Lexing.Modules
{
    [TestClass]
    public class LongCommentLexerModuleTests : LexerModuleTestBase<LongCommentLexerModule>
    {
        [TestMethod]
        public void CanConsumeNext_ChecksProperly ( )
        {
            var module = new LongCommentLexerModule ( );

            AssertCanConsumeNextReturns ( true, module, "--[[" );
            AssertCanConsumeNextReturns ( true, module, "--[=" );
        }

        [TestMethod]
        public void ConsumeNext_ParsesProperly ( )
        {
            var module = new LongCommentLexerModule ( );

            AssertConsumeNext (
                TokenFactory.Comment ( "[==a", null, SourceLocation.Zero ),
                module,
                "--[==a" );
            AssertConsumeNext (
                TokenFactory.LongComment ( "aaaa", "--[[aaaa]]", SourceLocation.Zero ),
                module,
                "--[[aaaa]]" );
            AssertConsumeNext (
                TokenFactory.LongComment ( @"
aaaa
", @"--[[
aaaa
]]", SourceLocation.Zero ),
                module,
                @"--[[
aaaa
]]" );
            AssertConsumeNext (
                TokenFactory.LongComment ( @"
    [[
        aaaa
    ]]
", @"--[=[
    [[
        aaaa
    ]]
]=]", SourceLocation.Zero ),
                module,
                @"--[=[
    [[
        aaaa
    ]]
]=]" );
            Token<LuaTokenType> tok = TokenFactory.LongComment ( @"
aaaa
", @"--[==[
aaaa
]=]", SourceLocation.Zero );
            AssertConsumeNext (
                tok,
                module,
                @"--[==[
aaaa
]=]",
                LuaDiagnostics.SyntaxError.LongCommentWithIncompatibleDelimiters ( tok.Range ) );
            tok = TokenFactory.LongComment ( "aaaa", "--[=[aaaa]]", SourceLocation.Zero );
            AssertConsumeNext (
                tok,
                module,
                tok.Raw,
                LuaDiagnostics.SyntaxError.LongCommentWithIncompatibleDelimiters ( tok.Range ) );
            tok = TokenFactory.LongComment ( @"
aaaa
aaaa
aaaa", @"--[[
aaaa
aaaa
aaaa", SourceLocation.Zero );
            AssertConsumeNext (
                tok,
                module,
                tok.Raw,
                LuaDiagnostics.SyntaxError.UnfinishedLongComment ( tok.Range ) );
        }
    }
}

