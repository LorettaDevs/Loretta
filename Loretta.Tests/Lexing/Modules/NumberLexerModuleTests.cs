using System;
using System.Linq;
using GParse;
using GParse.Collections;
using GParse.IO;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Lexing.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Loretta.Tests.Lexing.Modules
{
    [TestClass]
    public class NumberLexerModuleTests
    {
        private readonly NumberLexerModule lexerModule;

        public NumberLexerModuleTests ( )
        {
            this.lexerModule = new NumberLexerModule ( LuaOptions.All );
        }

        [DataTestMethod]
        [DataRow ( false, "a" )]
        [DataRow ( false, "." )]
        [DataRow ( true, "1" )]
        [DataRow ( true, ".1" )]
        public void CanConsumeNextWorks ( Boolean expected, String input ) =>
            Assert.AreEqual ( expected, this.lexerModule.CanConsumeNext ( new StringCodeReader ( input ) ) );

        [DataTestMethod]
        // Decimal
        [DataRow ( 1, "1" )]
        [DataRow ( .1, ".1" )]
        [DataRow ( 1.1, "1.1" )]
        [DataRow ( 1e10, "1e10" )]
        [DataRow ( 1e+10, "1e+10" )]
        [DataRow ( 1e-10, "1e-10" )]
        [DataRow ( 1.1e20, "1.1e20" )]
        [DataRow ( 1.1e+20, "1.1e+20" )]
        [DataRow ( 1.1e-20, "1.1e-20" )]
        // Hexadecimal
        [DataRow ( 2, "0x2" )]
        [DataRow ( .125, "0x.2" )]
        [DataRow ( 2.125, "0x2.2" )]
        [DataRow ( 8, "0x2p2" )]
        [DataRow ( 8, "0x2p+2" )]
        [DataRow ( .5, "0x2p-2" )]
        [DataRow ( 8.5, "0x2.2p2" )]
        [DataRow ( 8.5, "0x2.2p+2" )]
        [DataRow ( 0.53125, "0x2.2p-2" )]
        // Octal
        [DataRow ( 7, "0o7" )]
        [DataRow ( 16, "0o20" )]
        // Binary
        [DataRow ( 0b1, "0b1" )]
        [DataRow ( 0b10, "0b10" )]
        public void ConsumeNextParsesProperly ( Double value, String input )
        {
            var diagnostics = new DiagnosticList ( );
            Token<LuaTokenType> token = this.lexerModule.ConsumeNext ( new StringCodeReader ( input ), diagnostics );

            Assert.That.DiagnosticsAreEmpty ( diagnostics, input );
            Assert.AreEqual ( LuaTokenType.Number, token.Type );
            Assert.AreEqual ( input, token.Raw );
            Assert.AreEqual ( value, token.Value );
        }

        /*
         * LUA0004 -> Invalid number
         * LUA0005 -> Number too large
         */

        [DataTestMethod]
        [DataRow ( "LUA0004", "." )]
        [DataRow ( "LUA0004", ".1e" )]
        [DataRow ( "LUA0004", "0x." )]
        [DataRow ( "LUA0004", "0x.1p" )]
        [DataRow ( "LUA0004", "0o" )]
        [DataRow ( "LUA0004", "0b" )]
        // Overflow case for normal double does not exist. Double.Parse doesn't seems to throw on overflow.
        [DataRow ( "LUA0005", "0x7fffffffffffffffp2000" )]
        [DataRow ( "LUA0005", "0o7777777777777777777777" )]
        [DataRow ( "LUA0005", "0b1111111111111111111111111111111111111111111111111111111111111111" )]
        public void ConsumeNextFailsProperly ( String expectedDiagnosticId, String input )
        {
            var diagnostics = new DiagnosticList ( );
            Token<LuaTokenType> token = this.lexerModule.ConsumeNext ( new StringCodeReader ( input ), diagnostics );

            Assert.That.CollectionContainsDiagnostic ( diagnostics, expectedDiagnosticId, DiagnosticSeverity.Error );
            Assert.That.EnumerableCountIs ( diagnostics, 1 );

            Assert.AreEqual ( LuaTokenType.Number, token.Type );
            StringAssert.StartsWith ( input, token.Raw );
            Assert.AreEqual ( .0, token.Value );
        }
    }
}