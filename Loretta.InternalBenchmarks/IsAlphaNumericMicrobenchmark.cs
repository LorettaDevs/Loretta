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
    public class IsAlphaNumericMicrobenchmark
    {
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsInRange ( Char start, Char value, Char end ) =>
            ( UInt32 ) ( value - start ) <= ( end - start );

        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsDecimal ( Char ch ) =>
            IsInRange ( '0', ch, '9' );

        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsAlpha ( Char ch ) =>
            IsInRange ( 'a', ( Char ) ( ch | 0b100000 ), 'z' );

        [Params ( '\\', 'g', 'G', '7' )]
        public Char Value { get; set; }

        [Benchmark ( Baseline = true )]
        public Boolean IsAlphaNumericA ( ) =>
            IsDecimal ( this.Value ) || IsAlpha ( this.Value );

        [Benchmark]
        public Boolean IsAlphaNumericB ( ) =>
            IsDecimal ( this.Value ) | IsAlpha ( this.Value );

        [Benchmark]
        public Boolean IsAlphaNumericC ( ) =>
            IsAlpha ( this.Value ) || IsDecimal ( this.Value );

        [Benchmark]
        public Boolean IsAlphaNumericD ( ) =>
            IsAlpha ( this.Value ) | IsDecimal ( this.Value );
    }
}