using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace Loretta.InternalBenchmarks
{
    [SimpleJob(RunStrategy.Throughput, RuntimeMoniker.NetCoreApp31, baseline: true)]
    [SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net50)]
    [DisassemblyDiagnoser(exportHtml: true, exportDiff: true)]
    public class IsAlphaCheckMicrobenchmark

    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(char start, char value, char end)
            => (uint) (value - start) <= (end - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlphaAA(char ch)
            => IsInRange('a', ch, 'z') || IsInRange('A', ch, 'Z');

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlphaAB(char ch)
            => IsInRange('a', ch, 'z') | IsInRange('A', ch, 'Z');

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlphaB(char ch)
            => IsInRange('a', (char) (ch | 0b100000), 'z'); // Fastest on My Machine™

        [Params('0', 'g', 'G')]
        public char Value { get; set; }

        [Benchmark(Baseline = true)]
        public bool IsAlphaAA() => IsAlphaAA(Value);

        [Benchmark]
        public bool IsAlphaAB() => IsAlphaAB(Value);

        [Benchmark]
        public bool IsAlphaB() => IsAlphaB(Value);
    }
}
