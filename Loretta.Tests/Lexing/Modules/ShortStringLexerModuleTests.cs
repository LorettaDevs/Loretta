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
    public class ShortStringLexerModuleTests
    {
        private readonly ShortStringLexerModule lexerModule;

        public ShortStringLexerModuleTests ( )
        {
            this.lexerModule = new ShortStringLexerModule ( LuaOptions.All );
        }

        [DataTestMethod]
        [DataRow ( true, "'" )]
        [DataRow ( true, "\"" )]
        [DataRow ( false, "." )]
        [DataRow ( false, "a" )]
        [DataRow ( false, "[[x]]" )]
        public void CanConsumeNextWorks ( Boolean expected, String input ) =>
            Assert.AreEqual ( expected, this.lexerModule.CanConsumeNext ( new StringCodeReader ( input ) ) );

        [DataTestMethod]
        [DataRow ( "hello\x20world\0", @"'hello\x20world\0'" )]
        [DataRow ( "\a\b\f\n\r\t\v\\\r\n\'\"", @"""\a\b\f\n\r\t\v\\\
\'\""""" )]
        public void ConsumeNextWorks ( String expected, String input )
        {
            var diagnostics = new DiagnosticList ( );
            Token<LuaTokenType> token = this.lexerModule.ConsumeNext ( new StringCodeReader ( input ), diagnostics );

            Assert.That.DiagnosticsAreEmpty ( diagnostics, input );
            Assert.AreEqual ( LuaTokenType.String, token.Type );
            Assert.AreEqual ( input, token.Raw );
            Assert.AreEqual ( expected, token.Value );
        }

        [DataTestMethod]
        [DataRow ( @"""" )]
        [DataRow ( @"""xxxxxx
""" )]
        [DataRow ( @"'" )]
        [DataRow ( @"'xxxxxx
'" )]
        [DataRow ( @"""'" )]
        [DataRow ( @"""xxxxxx
'" )]
        public void ConsumeNextReportsUnfinishedString ( String input )
        {
            var diagnostics = new DiagnosticList ( );
            this.lexerModule.ConsumeNext ( new StringCodeReader ( input ), diagnostics );

            Diagnostic diagnostic = diagnostics.Single ( );
            Assert.AreEqual ( "LUA0006", diagnostic.Id );
            Assert.AreEqual ( DiagnosticSeverity.Error, diagnostic.Severity );
        }
    }
}