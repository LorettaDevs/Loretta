using BenchmarkDotNet.Attributes;
using Loretta.InternalBenchmarks.Columns;

namespace Loretta.InternalBenchmarks.Attributes
{
    internal class MedianThroughputColumnAttribute : ColumnConfigBaseAttribute
    {
        public MedianThroughputColumnAttribute(string paramName)
            : base(ThroughputColumn.Median(paramName))
        {
        }
    }
}
