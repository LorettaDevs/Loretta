using System;
using GParse.Collections;
using GParse.IO;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Lexing.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Loretta.Tests.Lexing.Modules
{
    [TestClass]
    public class IdentifierLexerModuleTests
    {
        private static IdentifierLexerModule GetLexerModule ( Boolean useLuaJitIdentifierRules )
        {
            var identifierLexerModule = new IdentifierLexerModule ( useLuaJitIdentifierRules ? LuaOptions.LuaJIT : LuaOptions.Lua51, new[]
                        {
                "do",
                "end"
            }, new[]
                        {
                "and",
                "or"
            } );
            identifierLexerModule.AddLiteral ( "nil", LuaTokenType.Nil, null );
            identifierLexerModule.AddLiteral ( "true", LuaTokenType.Boolean, true );
            identifierLexerModule.AddLiteral ( "false", LuaTokenType.Boolean, false );
            return identifierLexerModule;
        }

        private static readonly IdentifierLexerModule moduleWithoutLuajitRules = GetLexerModule ( false );
        private static readonly IdentifierLexerModule moduleWithLuajitRules = GetLexerModule ( true );

        [DataTestMethod]
        [DataRow ( true, false, "123" )]
        [DataRow ( true, false, " " )]
        [DataRow ( true, false, "[" )]
        [DataRow ( true, false, "]" )]
        [DataRow ( true, true, "_" )]
        [DataRow ( false, true, "🅱" )]
        [DataRow ( false, true, "\ufeff" /* ZERO WIDTH NO-BREAK SPACE */ )]
        [DataRow ( false, true, "\u206b" /* ACTIVATE SYMMETRIC SWAPPING */ )]
        [DataRow ( false, true, "\u202a" /* LEFT-TO-RIGHT EMBEDDING */ )]
        [DataRow ( false, true, "\u206a" /* INHIBIT SYMMETRIC SWAPPING */ )]
        [DataRow ( false, true, "\ufeff" /* ZERO WIDTH NO-BREAK SPACE */ )]
        [DataRow ( false, true, "\u206a" /* INHIBIT SYMMETRIC SWAPPING */ )]
        [DataRow ( false, true, "\u200e" /* LEFT-TO-RIGHT MARK */ )]
        [DataRow ( false, true, "\u200c" /* ZERO WIDTH NON-JOINER */ )]
        [DataRow ( false, true, "\u200e" /* LEFT-TO-RIGHT MARK */ )]
        public void CanConsumeWorks ( Boolean shouldWorkWithoutLuajitIdentifierRules, Boolean expected, String str )
        {
            Assert.AreEqual ( expected, moduleWithLuajitRules.CanConsumeNext ( new StringCodeReader ( str ) ) );
            if ( shouldWorkWithoutLuajitIdentifierRules )
                Assert.AreEqual ( expected, moduleWithoutLuajitRules.CanConsumeNext ( new StringCodeReader ( str ) ) );
        }

        [DataTestMethod]
        [DataRow ( true, LuaTokenType.Keyword, "do", "do", "do return end" )]
        [DataRow ( true, LuaTokenType.Keyword, "end", "end", "end" )]
        [DataRow ( true, LuaTokenType.Operator, "and", "and", "and true" )]
        [DataRow ( false, LuaTokenType.Identifier, "\ufeff\u206b\u202a\u206a\ufeff\u206a\u200e\u200c\u200e", "\ufeff\u206b\u202a\u206a\ufeff\u206a\u200e\u200c\u200e", "\ufeff\u206b\u202a\u206a\ufeff\u206a\u200e\u200c\u200e = _G" )]
        [DataRow ( true, LuaTokenType.Identifier, "_G", "_G", "_G = nil" )]
        [DataRow ( true, LuaTokenType.Identifier, "test0", "test0", "test0 = 1" )]
        [DataRow ( true, LuaTokenType.Identifier, "te0st", "te0st", "te0st = 1" )]
        [DataRow ( true, LuaTokenType.Nil, "nil", null, "nil" )]
        [DataRow ( true, LuaTokenType.Boolean, "true", true, "true and false" )]
        [DataRow ( true, LuaTokenType.Boolean, "false", false, "false and true" )]
        public void ConsumeNextWorks ( Boolean shouldWorkWithoutLuajitIdentifierRules, LuaTokenType expectedType, String expectedRaw, Object expectedValue, String input )
        {
            var diagnostics = new DiagnosticList ( );
            Token<LuaTokenType> token = moduleWithLuajitRules.ConsumeNext ( new StringCodeReader ( input ), diagnostics );

            Assert.That.DiagnosticsAreEmpty ( diagnostics, input );
            Assert.AreEqual ( expectedType, token.Type );
            Assert.AreEqual ( expectedRaw, token.Raw );
            Assert.AreEqual ( expectedValue, token.Value );

            if ( shouldWorkWithoutLuajitIdentifierRules )
            {
                diagnostics = new DiagnosticList ( );
                token = moduleWithoutLuajitRules.ConsumeNext ( new StringCodeReader ( input ), diagnostics );

                Assert.That.DiagnosticsAreEmpty ( diagnostics, input );
                Assert.AreEqual ( expectedType, token.Type );
                Assert.AreEqual ( expectedRaw, token.Raw );
                Assert.AreEqual ( expectedValue, token.Value );
            }
        }
    }
}