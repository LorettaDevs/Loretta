using System.Collections.Generic;
using System.Threading;

namespace Loretta.CodeAnalysis.Lua.StatisticsCollector
{
    record DiagnosticStatistics(
        int ErrorCount,
        int WarningCount,
        int InformationCount,
        int TotalCount)
    {
        public class Builder
        {
            int _errorCount, _warningCount, _informationCount;

            public void Add(Diagnostic diagnostic)
            {
                switch (diagnostic.Severity)
                {
                    case DiagnosticSeverity.Info:
                        Interlocked.Increment(ref _informationCount);
                        break;
                    case DiagnosticSeverity.Warning:
                        Interlocked.Increment(ref _warningCount);
                        break;
                    case DiagnosticSeverity.Error:
                        Interlocked.Increment(ref _errorCount);
                        break;
                }
            }

            public void Add(IEnumerable<Diagnostic> diagnostics)
            {
                foreach (var diagnostic in diagnostics) Add(diagnostic);
            }

            public DiagnosticStatistics Summarize() =>
                new(_errorCount, _warningCount, _informationCount, _errorCount + _warningCount + _informationCount);
        }

        public static DiagnosticStatistics Collect(IEnumerable<Diagnostic> diagnostics)
        {
            var builder = new Builder();
            builder.Add(diagnostics);
            return builder.Summarize();
        }
    }
}
