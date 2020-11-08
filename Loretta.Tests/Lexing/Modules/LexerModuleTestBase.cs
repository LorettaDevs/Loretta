using System;
using GParse;
using GParse.Collections;
using GParse.IO;
using GParse.Lexing;
using GParse.Lexing.Modules;
using Loretta.Lexing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Loretta.Tests.Lexing.Modules
{
    public abstract class LexerModuleTestBase<T>
        where T : ILexerModule<LuaTokenType>
    {
        protected static Token<LuaTokenType> Token ( String id, String raw, Object? value, LuaTokenType type, SourceRange range ) =>
            new Token<LuaTokenType> ( id, raw, value, type, range );

        protected static void AssertCanConsumeNextReturns ( Boolean expected, T module, String input )
        {
            // Setup
            var reader = new StringCodeReader ( input );

            // Act
            var actual = module.CanConsumeNext ( reader );

            // Check
            Assert.AreEqual ( expected, actual );
        }

        protected static void AssertConsumeNext (
            Token<LuaTokenType> expectedToken,
            T module,
            String input,
            params Diagnostic[] expectedDiagnostics )
        {
            // Setup
            var actualDiagnostics = new DiagnosticList ( );
            var reader = new StringCodeReader ( input );

            // Act
            Token<LuaTokenType> actualToken = module.ConsumeNext ( reader, actualDiagnostics );

            // Check
            Assert.That.TokensAreEqual ( expectedToken, actualToken, false );
            foreach ( Diagnostic expectedDiagnostic in expectedDiagnostics )
            {
                Assert.That.CollectionContainsDiagnostic ( actualDiagnostics, expectedDiagnostic );
            }
            Assert.That.EnumerableCountIs ( actualDiagnostics, expectedDiagnostics.Length );
        }
    }
}
