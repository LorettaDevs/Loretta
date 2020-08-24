using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace Loretta.InternalBenchmarks
{
    [SimpleJob ( RunStrategy.Throughput, RuntimeMoniker.NetCoreApp31, baseline: true )]
    [SimpleJob ( RunStrategy.Throughput, RuntimeMoniker.NetCoreApp50 )]
    [DisassemblyDiagnoser ( exportHtml: true, exportDiff: true )]
    public class IsAlphaCheckMicrobenchmark

    {
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsInRange ( Char start, Char value, Char end ) =>
            ( UInt32 ) ( value - start ) <= ( end - start );

        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsAlphaAA ( Char ch ) =>
            IsInRange ( 'a', ch, 'z' ) || IsInRange ( 'A', ch, 'Z' );

        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsAlphaAB ( Char ch ) =>
            IsInRange ( 'a', ch, 'z' ) | IsInRange ( 'A', ch, 'Z' );

        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsAlphaB ( Char ch ) =>
            IsInRange ( 'a', ( Char ) ( ch | 0b100000 ), 'z' ); // Fastest on My Machine™

        [Params ( '0', 'g', 'G' )]
        public Char Value { get; set; }

        [Benchmark ( Baseline = true )]
        public Boolean IsAlphaAA ( ) =>
            IsAlphaAA ( this.Value );

        [Benchmark]
        public Boolean IsAlphaAB ( ) =>
            IsAlphaAB ( this.Value );

        [Benchmark]
        public Boolean IsAlphaB ( ) =>
            IsAlphaB ( this.Value );
    }
}