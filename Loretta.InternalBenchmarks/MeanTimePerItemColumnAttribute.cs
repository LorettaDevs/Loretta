using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Mathematics;

namespace Loretta.InternalBenchmarks
{
    public class MeanTimePerItemColumnAttribute : ColumnConfigBaseAttribute
    {
        private static StatisticColumn GetColumn ( Int32 count ) =>
            ( StatisticColumn ) typeof ( StatisticColumn )
                .GetConstructors ( BindingFlags.NonPublic | BindingFlags.Instance )[0]
                // (string columnName, string legend, Func<Statistics, double> calc, Priority priority, UnitType type = UnitType.Time, IStatisticColumn parentColumn = null)
                .Invoke ( new Object[]
                {
                    "Mean Time/Item",
                    "Mean time spent on each item",
                    new Func<Statistics, Double> ( ( Statistics s ) => s.Mean / count ),
                    0,
                    UnitType.Time,
                    null
                } );

        public MeanTimePerItemColumnAttribute ( Int32 count ) : base ( GetColumn ( count ) )
        {
        }
    }
}
