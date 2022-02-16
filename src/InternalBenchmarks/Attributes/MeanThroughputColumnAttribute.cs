using BenchmarkDotNet.Attributes;
using Loretta.InternalBenchmarks.Columns;

namespace Loretta.InternalBenchmarks.Attributes
{
    internal class MeanThroughputColumnAttribute : ColumnConfigBaseAttribute
    {
        public MeanThroughputColumnAttribute(string paramName)
            : base(ThroughputColumn.Mean(paramName))
        {
        }
    }
}
