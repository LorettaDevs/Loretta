using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.CodeAnalysis.Collections;
using Perfolizer.Mathematics.OutlierDetection;
using Perfolizer.Mathematics.QuantileEstimators;

namespace Loretta.CodeAnalysis.Lua.StatisticsCollector.Mathematics
{
    // Based on: https://github.com/dotnet/BenchmarkDotNet/tree/63e28c100a42a6492d09a0b93a8a4c141061bb0d/src/BenchmarkDotNet/Mathematics
    public class Statistics
    {
        public double Min { get; }
        public double LowerFence { get; }
        public double Q1 { get; }
        public double Median { get; }
        public double Mean { get; }
        public double Q3 { get; }
        public double UpperFence { get; }
        public double Max { get; }
        public double P0 { get; }
        public double P25 { get; }
        public double P50 { get; }
        public double P67 { get; }
        public double P80 { get; }
        public double P85 { get; }
        public double P90 { get; }
        public double P95 { get; }
        public double P99 { get; }
        public double P100 { get; }

        public Statistics(IEnumerable<double> values)
        {
            var sortedValues = new SegmentedList<double>(values.Where(d => !double.IsNaN(d)));
            sortedValues.Sort();
            if (sortedValues.Count < 1)
                return;

            var quartiles = Quartiles.FromSorted(sortedValues);
            Min = quartiles.Min;
            Q1 = quartiles.Q1;
            Median = quartiles.Median;
            Q3 = quartiles.Q3;
            Max = quartiles.Max;

            Mean = sortedValues.Average();

            var tukey = TukeyOutlierDetector.FromQuartiles(quartiles);
            LowerFence = tukey.LowerFence;
            UpperFence = tukey.UpperFence;

            P0 = SimpleQuantileEstimator.Instance.GetQuantileFromSorted(sortedValues, 0.0);
            P25 = SimpleQuantileEstimator.Instance.GetQuantileFromSorted(sortedValues, 0.25);
            P50 = SimpleQuantileEstimator.Instance.GetQuantileFromSorted(sortedValues, 0.50);
            P67 = SimpleQuantileEstimator.Instance.GetQuantileFromSorted(sortedValues, 0.67);
            P80 = SimpleQuantileEstimator.Instance.GetQuantileFromSorted(sortedValues, 0.80);
            P85 = SimpleQuantileEstimator.Instance.GetQuantileFromSorted(sortedValues, 0.85);
            P90 = SimpleQuantileEstimator.Instance.GetQuantileFromSorted(sortedValues, 0.90);
            P95 = SimpleQuantileEstimator.Instance.GetQuantileFromSorted(sortedValues, 0.95);
            P99 = SimpleQuantileEstimator.Instance.GetQuantileFromSorted(sortedValues, 0.99);
            P100 = SimpleQuantileEstimator.Instance.GetQuantileFromSorted(sortedValues, 1.00);
        }
    }
}
