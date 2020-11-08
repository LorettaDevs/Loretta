using System;
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
        private readonly DotLexerModule lexerModule = new DotLexerModule ( );

        [DataTestMethod]
        [DataRow ( true, "." )]
        [DataRow ( true, ".." )]
        [DataRow ( true, "..." )]
        [DataRow ( true, ".e" )]
        [DataRow ( true, "..e" )]
        [DataRow ( true, "...e" )]
        [DataRow ( false, ".1" )]
        [DataRow ( true, "..1" )]
        [DataRow ( true, "...1" )]
        public void CanConsumeNextWorks ( Boolean expected, String input ) =>
            Assert.AreEqual ( expected, this.lexerModule.CanConsumeNext ( new StringCodeReader ( input ) ) );

        [TestMethod]
        public void ConsumeNextReturnsExpectedToken ( )
        {
            var token = new Token<LuaTokenType> (
                ".",
                ".",
                ".",
                LuaTokenType.Dot,
                new SourceRange (
                    new SourceLocation ( 1, 1, 0 ),
                    new SourceLocation ( 1, 2, 1 ) ) );
            var token2 = new Token<LuaTokenType> (
                "..",
                "..",
                "..",
                LuaTokenType.Operator,
                new SourceRange (
                    new SourceLocation ( 1, 1, 0 ),
                    new SourceLocation ( 1, 3, 2 ) ) );
            var token3 = new Token<LuaTokenType> (
                "...",
                "...",
                "...",
                LuaTokenType.VarArg,
                new SourceRange (
                    new SourceLocation ( 1, 1, 0 ),
                    new SourceLocation ( 1, 4, 3 ) ) );

            var diagnostics = new DiagnosticList ( );
            Assert.That.TokensAreEqual ( token, this.lexerModule.ConsumeNext ( new StringCodeReader ( "." ), diagnostics ) );
            Assert.That.DiagnosticsAreEmpty ( diagnostics, "." );

            diagnostics = new DiagnosticList ( );
            Assert.That.TokensAreEqual ( token2, this.lexerModule.ConsumeNext ( new StringCodeReader ( ".." ), diagnostics ) );
            Assert.That.DiagnosticsAreEmpty ( diagnostics, ".." );

            diagnostics = new DiagnosticList ( );
            Assert.That.TokensAreEqual ( token3, this.lexerModule.ConsumeNext ( new StringCodeReader ( "..." ), diagnostics ) );
            Assert.That.DiagnosticsAreEmpty ( diagnostics, "..." );
        }
    }
}
