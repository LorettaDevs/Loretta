using System.Collections.Generic;
using Loretta.CodeAnalysis.Collections;
using Loretta.CodeAnalysis.Lua.StatisticsCollector.Mathematics;

namespace Loretta.CodeAnalysis.Lua.StatisticsCollector
{
    internal record GlobalStatistics(
        Statistics ParseTimeStatistics,
        Statistics? AllocationStatistics,
        Statistics? TokenCountStatistics,
        Statistics? TokenLengthStatistics,
        GlobalFeatureStatistics FeatureStatistics,
        DiagnosticStatistics DiagnosticStatistics)
    {
        public class Builder
        {
            private readonly object _addLock = new();
            private readonly SegmentedList<double> _parseTimes;
            private readonly SegmentedList<double> _parseAllocations;
            private readonly SegmentedList<double> _tokenCounts;
            private readonly SegmentedList<double> _tokenWidths;
            private readonly DiagnosticStatistics.Builder _diagnosticStatisticsBuilder;
            private readonly GlobalFeatureStatistics.Builder _globalFeatureStatisticsBuilder;

            public Builder()
            {
                _parseTimes = new SegmentedList<double>(Pools.BigListPoolSize);
                _parseAllocations = new SegmentedList<double>(Pools.BigListPoolSize);
                _tokenCounts = new SegmentedList<double>(Pools.BigListPoolSize);
                _tokenWidths = new SegmentedList<double>(Pools.BigListPoolSize);
                _diagnosticStatisticsBuilder = new DiagnosticStatistics.Builder();
                _globalFeatureStatisticsBuilder = new GlobalFeatureStatistics.Builder();
            }

            public void AddParse(FileStatistics fileStatistics)
            {
                lock (_addLock)
                {
                    _parseTimes.Add(fileStatistics.ParseStatistics.ParseTime);
                    if (fileStatistics.ParseStatistics.BytesAllocated > 0)
                        _parseAllocations.Add(fileStatistics.ParseStatistics.BytesAllocated);
                    if (fileStatistics.TokenStatistics is not null)
                        _tokenCounts.Add(fileStatistics.TokenStatistics.TokenCount);
                    if (fileStatistics.FeatureStatistics is not null)
                        _globalFeatureStatisticsBuilder.Merge(fileStatistics.FeatureStatistics);
                }
            }

            public void AddTokenWidth(int width)
            {
                lock (_addLock)
                    _tokenWidths.Add(width);
            }

            // Diagnostics don't need locks since they are thread-safe
            public void AddDiagnostic(Diagnostic diagnostic) => _diagnosticStatisticsBuilder.Add(diagnostic);
            public void AddDiagnostics(IEnumerable<Diagnostic> diagnostics) => _diagnosticStatisticsBuilder.Add(diagnostics);

            public GlobalStatistics Summarize()
            {
                lock (_addLock)
                {
                    return new GlobalStatistics(
                        new Statistics(_parseTimes),
                        _parseAllocations.Count > 0 ? new Statistics(_parseAllocations) : null,
                        _tokenCounts.Count > 0 ? new Statistics(_tokenCounts) : null,
                        _tokenWidths.Count > 0 ? new Statistics(_tokenWidths) : null,
                        _globalFeatureStatisticsBuilder.Summarize(),
                        _diagnosticStatisticsBuilder.Summarize());
                }
            }
        }
    }
}
