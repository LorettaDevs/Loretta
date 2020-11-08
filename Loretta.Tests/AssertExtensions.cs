using System;
using System.Collections.Generic;
using System.Linq;
using GParse;
using GParse.Lexing;
using Loretta.Lexing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Loretta.Tests
{
    public static class AssertExtensions
    {
        public static void TokensAreEqual ( this Assert assert, Token<LuaTokenType> expected, Token<LuaTokenType> actual, Boolean compareTrivia = false )
        {
            _ = assert;
            Assert.AreEqual ( expected.Id, actual.Id );
            Assert.AreEqual ( expected.Raw, actual.Raw );
            Assert.AreEqual ( expected.Value, actual.Value );
            Assert.AreEqual ( expected.Type, actual.Type );
            Assert.AreEqual ( expected.Range, actual.Range );
            Assert.AreEqual ( expected.IsTrivia, actual.IsTrivia );

            if ( compareTrivia )
                CollectionAssert.AreEqual ( expected.Trivia.ToArray ( ), actual.Trivia.ToArray ( ) );
        }

        public static void DiagnosticsAreEmpty ( this Assert assert, IReadOnlyList<Diagnostic> diagnostics, String input )
        {
            _ = assert;
            if ( diagnostics.Count > 0 )
            {
                foreach ( Diagnostic diagnostic in diagnostics )
                {
                    Logger.LogMessage ( @"{0} {1}:{2}
{3}", diagnostic.Id, diagnostic.Severity, diagnostic.Description, LuaDiagnostics.HighlightRange ( input, diagnostic.Range ) );
                }

                Assert.Fail ( );
            }
        }

        public static void CollectionContainsDiagnostic (
            this Assert assert,
            IReadOnlyList<Diagnostic> diagnostics,
            String id,
            DiagnosticSeverity severity,
            String message = null,
            SourceRange? range = null )
        {
            _ = assert;
            IEnumerable<Diagnostic> diags = diagnostics.Where ( diag => diag.Id.Equals ( id, StringComparison.Ordinal ) && diag.Severity == severity );

            if ( message is not null )
                diags = diags.Where ( diag => diag.Description.Equals ( message, StringComparison.Ordinal ) );

            if ( range is not null )
                diags = diags.Where ( diag => diag.Range == range );

            Assert.IsTrue ( diags.Any ( ) );
        }

        public static void CollectionContainsDiagnostic (
            this Assert assert,
            IReadOnlyList<Diagnostic> diagnostics,
            Diagnostic diagnostic ) =>
            assert.CollectionContainsDiagnostic (
                diagnostics,
                diagnostic.Id,
                diagnostic.Severity,
                diagnostic.Description,
                diagnostic.Range );

        public static void EnumerableCountIs<T> ( this Assert assert, IEnumerable<T> enumerable, Int32 expectedCount )
        {
            _ = assert;
            Assert.AreEqual ( expectedCount, enumerable.Count ( ) );
        }
    }
}