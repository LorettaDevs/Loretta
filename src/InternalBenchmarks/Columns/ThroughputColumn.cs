using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;
using Tsu.Numerics;

namespace Loretta.InternalBenchmarks.Columns
{
    internal class ThroughputColumn : IColumn
    {
        public static ThroughputColumn Mean(string paramName) =>
            new("Mean", s => s.Mean, paramName);

        public static ThroughputColumn Median(string paramName) =>
            new("Median", s => s.Median, paramName);

        private readonly string _statName;
        private readonly Func<Statistics, double> _func;
        private readonly string _paramName;

        public string Id => _statName + nameof(ThroughputColumn);
        public string ColumnName => $"{_statName} Throughput";
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Statistics;
        public int PriorityInCategory => 0;
        public bool IsNumeric => true;
        public UnitType UnitType => UnitType.Dimensionless;
        public string Legend => "";

        private ThroughputColumn(string statName, Func<Statistics, double> func, string paramName)
        {
            if (string.IsNullOrWhiteSpace(statName)) throw new ArgumentException($"'{nameof(statName)}' cannot be null or whitespace.", nameof(statName));
            if (string.IsNullOrWhiteSpace(paramName)) throw new ArgumentException($"'{nameof(paramName)}' cannot be null or whitespace.", nameof(paramName));
            _statName = statName;
            _func = func ?? throw new ArgumentNullException(nameof(func));
            _paramName = paramName;
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase) => GetValue(summary, benchmarkCase, SummaryStyle.Default);

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
        {
            var file = (TestFile) benchmarkCase.Parameters[_paramName];
            var statistics = summary[benchmarkCase].ResultStatistics;

            var size = file.Contents.Length;
            var time = _func(statistics);

            var throughput = size / time / SI.Nano;
            return FileSize.Format(throughput, "{0:0.00}{1}/s");
        }

        public bool IsAvailable(Summary summary) => true;
        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
    }
}
