using System;
using System.Collections.Generic;
using System.Linq;
using GParse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Loretta.Tests
{
    public static class TestUtils
    {
        public static void AssertDiagnosticsEmpty ( IReadOnlyList<Diagnostic> diagnostics, String input )
        {
            if ( diagnostics.Count > 0 )
            {
                foreach ( Diagnostic diagnostic in diagnostics )
                {
                    Logger.LogMessage ( @"{0} {1}:{2}
{3}", diagnostic.Id, diagnostic.Severity, diagnostic.Description, LuaDiagnostics.HighlightRange ( input, diagnostic.Range ) );
                }

                Assert.IsFalse ( diagnostics.Any ( diag => diag.Severity == DiagnosticSeverity.Error ) );
            }
        }
    }
}