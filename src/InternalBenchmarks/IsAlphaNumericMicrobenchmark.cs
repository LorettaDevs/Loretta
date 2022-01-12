using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace Loretta.InternalBenchmarks
{
    [SimpleJob(RunStrategy.Throughput, RuntimeMoniker.NetCoreApp31, baseline: true)]
    [SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net50)]
    [DisassemblyDiagnoser(exportHtml: true, exportDiff: true)]
    public class IsAlphaNumericMicrobenchmark
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(char start, char value, char end) => (uint) (value - start) <= (end - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDecimal(char ch) => IsInRange('0', ch, '9');

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlpha(char ch) => IsInRange('a', (char) (ch | 0b100000), 'z');

        [Params('\\', 'g', 'G', '7')]
        public char Value { get; set; }

        [Benchmark(Baseline = true)]
        public bool IsAlphaNumericA() => IsDecimal(Value) || IsAlpha(Value);

        [Benchmark]
        public bool IsAlphaNumericB() => IsDecimal(Value) | IsAlpha(Value);

        [Benchmark]
        public bool IsAlphaNumericC() => IsAlpha(Value) || IsDecimal(Value);

        [Benchmark]
        public bool IsAlphaNumericD() => IsAlpha(Value) | IsDecimal(Value);
    }
}
