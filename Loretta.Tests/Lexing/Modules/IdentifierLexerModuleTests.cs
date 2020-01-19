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
        private readonly IdentifierLexerModule lexerModule;

        public IdentifierLexerModuleTests ( )
        {
            this.lexerModule = new IdentifierLexerModule ( new[]
            {
                "do",
                "end"
            }, new[]
            {
                "and",
                "or"
            } );
        }

        [DataTestMethod]
        [DataRow ( false, "123" )]
        [DataRow ( false, " " )]
        [DataRow ( false, "[" )]
        [DataRow ( false, "]" )]
        [DataRow ( true, "_" )]
        [DataRow ( true, "🅱" )]
        [DataRow ( true, "\ufeff" /* ZERO WIDTH NO-BREAK SPACE */ )]
        [DataRow ( true, "\u206b" /* ACTIVATE SYMMETRIC SWAPPING */ )]
        [DataRow ( true, "\u202a" /* LEFT-TO-RIGHT EMBEDDING */ )]
        [DataRow ( true, "\u206a" /* INHIBIT SYMMETRIC SWAPPING */ )]
        [DataRow ( true, "\ufeff" /* ZERO WIDTH NO-BREAK SPACE */ )]
        [DataRow ( true, "\u206a" /* INHIBIT SYMMETRIC SWAPPING */ )]
        [DataRow ( true, "\u200e" /* LEFT-TO-RIGHT MARK */ )]
        [DataRow ( true, "\u200c" /* ZERO WIDTH NON-JOINER */ )]
        [DataRow ( true, "\u200e" /* LEFT-TO-RIGHT MARK */ )]
        public void CanConsumeWorks ( Boolean expected, String str ) =>
            Assert.AreEqual ( expected, this.lexerModule.CanConsumeNext ( new StringCodeReader ( str ) ) );

        [DataTestMethod]
        [DataRow ( LuaTokenType.Keyword, "do", "do", "do return end" )]
        [DataRow ( LuaTokenType.Keyword, "end", "end", "end" )]
        [DataRow ( LuaTokenType.Operator, "and", "and", "and true" )]
        [DataRow ( LuaTokenType.Identifier, "\ufeff\u206b\u202a\u206a\ufeff\u206a\u200e\u200c\u200e", "\ufeff\u206b\u202a\u206a\ufeff\u206a\u200e\u200c\u200e", "\ufeff\u206b\u202a\u206a\ufeff\u206a\u200e\u200c\u200e = _G" )]
        [DataRow ( LuaTokenType.Identifier, "_G", "_G", "_G = nil" )]
        [DataRow ( LuaTokenType.Nil, "nil", null, "nil" )]
        [DataRow ( LuaTokenType.Boolean, "true", true, "true and false" )]
        [DataRow ( LuaTokenType.Boolean, "false", false, "false and true" )]
        public void ConsumeNextWorks ( LuaTokenType expectedType, String expectedRaw, Object expectedValue, String input )
        {
            var diagnostics = new DiagnosticList ( );
            Token<LuaTokenType> token = this.lexerModule.ConsumeNext ( new StringCodeReader ( input ), diagnostics );

            TestUtils.AssertDiagnosticsEmpty ( diagnostics, input );
            Assert.AreEqual ( expectedType, token.Type );
            Assert.AreEqual ( expectedRaw, token.Raw );
            Assert.AreEqual ( expectedValue, token.Value );
        }
    }
}