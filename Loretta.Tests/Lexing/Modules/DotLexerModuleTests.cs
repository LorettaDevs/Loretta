using System;
using System.Collections.Generic;
using System.Text;
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
    public class DotLexerModuleTests
    {
        private readonly DotLexerModule lexerModule;

        public DotLexerModuleTests ( )
        {
            this.lexerModule = new DotLexerModule ( );
        }

        [DataTestMethod]
        [DataRow ( true, "." )]
        [DataRow ( true, ".e" )]
        [DataRow ( false, ".1" )]
        [DataRow ( false, "1" )]
        public void CanConsumeNextWorks ( Boolean expected, String input ) =>
            Assert.AreEqual ( expected, this.lexerModule.CanConsumeNext ( new StringCodeReader ( input ) ) );

        [TestMethod]
        public void ConsumeNextReturnsExpectedToken ( )
        {
            var token = new Token<LuaTokenType> ( ".", ".", ".", LuaTokenType.Dot, new SourceRange ( new SourceLocation ( 1, 1, 0 ), new SourceLocation ( 1, 2, 1 ) ) );
            var diagnostics = new DiagnosticList ( );

            Assert.AreEqual ( token, this.lexerModule.ConsumeNext ( new StringCodeReader ( "." ), diagnostics ) );
            TestUtils.AssertDiagnosticsEmpty ( diagnostics, "." );
        }

        [TestMethod]
        public void ConsumeNextThrows ( )
        {
            var diagnostics = new DiagnosticList ( );
            Assert.ThrowsException<InvalidOperationException> ( ( ) => this.lexerModule.ConsumeNext ( new StringCodeReader ( ".2" ), diagnostics ) );
        }
    }
}
