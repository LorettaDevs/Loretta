using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using GParse.IO;

namespace Loretta.InternalBenchmarks
{
    [SimpleJob ( BenchmarkDotNet.Jobs.RuntimeMoniker.NetCoreApp31 )]
    [MeanTimePerItemColumn ( NumbersCount ), MeanColumn, MedianColumn]
    [HtmlExporter, MarkdownExporter, AsciiDocExporter]
    public partial class HexCharConversionBenchmark
    {
        private const Int32 NumbersCount = 1_000_000;

        private static readonly String[] NumbersToParse;

        private static readonly Vector<UInt16> Nines;
        private static readonly Vector<UInt16> Masks;
        private static readonly Vector<UInt16> Checkers;
        private static readonly Int32[] ParsedNumbers = new Int32[NumbersCount];
        private static readonly Int32 Count;

        static HexCharConversionBenchmark ( )
        {
            NumbersToParse = new String[NumbersCount];
            var r = new Random ( );
            for ( var i = NumbersCount - 1; i >= 0; i-- )
                NumbersToParse[i] = r.NextDouble ( ) > 0.5d ? $"{r.Next ( ):X}" : $"{r.Next ( ):x}";

            Count = Vector<UInt16>.Count;

            var v = new UInt16[Count];
            Array.Fill<UInt16> ( v, 9 );
            Nines = new Vector<UInt16> ( v );

            v = new UInt16[Count];
            Array.Fill<UInt16> ( v, 0b1111 );
            Masks = new Vector<UInt16> ( v );

            v = new UInt16[Count];
            Array.Fill<UInt16> ( v, 0b1000000 );
            Checkers = new Vector<UInt16> ( v );
        }

        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        private static Boolean IsHexCharBitwise ( Char ch ) =>
            ( '0' <= ch & ch <= '9' ) | ( 'a' <= ch & ch <= 'f' ) | ( 'A' <= ch & ch <= 'F' );

        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        private static Boolean IsHexCharBoolean ( Char ch ) =>
            ( '0' <= ch && ch <= '9' ) || ( 'a' <= ch && ch <= 'f' ) || ( 'A' <= ch && ch <= 'F' );

        [IterationCleanup]
        public void IterationCleanup ( )
        {
            for ( var i = NumbersCount - 1; i >= 0; i-- )
            {
                if ( ParsedNumbers[i] != Int32.Parse ( NumbersToParse[i], System.Globalization.NumberStyles.HexNumber ) )
                    throw new Exception ( $"Failed to parse all numbers properly: {i}. {NumbersToParse[i]} != {ParsedNumbers[i]:x}" );
            }

            Array.Clear ( ParsedNumbers, 0, NumbersCount );
        }

        [Benchmark]
        public void CoreFxParsingWithBitwiseCheck ( )
        {
            var buff = new StringBuilder ( );
            for ( var i = NumbersCount - 1; i >= 0; i-- )
            {
                buff.Clear ( );
                var reader = new StringCodeReader ( NumbersToParse[i] );
                ParsedNumbers[i] = Int32.Parse ( reader.ReadStringWhile ( c => IsHexCharBitwise ( c ) ), System.Globalization.NumberStyles.HexNumber );
            }
        }

        [Benchmark]
        public void CoreFxParsingWithBooleanCheck ( )
        {
            var buff = new StringBuilder ( );
            for ( var i = NumbersCount - 1; i >= 0; i-- )
            {
                buff.Clear ( );
                var reader = new StringCodeReader ( NumbersToParse[i] );
                ParsedNumbers[i] = Int32.Parse ( reader.ReadStringWhile ( c => IsHexCharBoolean ( c ) ), System.Globalization.NumberStyles.HexNumber );
            }
        }

        [Benchmark]
        public void NaiveHexParsingWithBitwiseCheck ( )
        {
            for ( var i = NumbersCount - 1; i >= 0; i-- )
            {
                Char ch;
                var accumulator = 0;
                var reader = new StringCodeReader ( NumbersToParse[i] );
                while ( reader.Position < reader.Length && IsHexCharBitwise ( ch = reader.Peek ( ).Value ) )
                {
                    reader.Advance ( 1 );
                    if ( '0' <= ch && ch <= '9' )
                        accumulator = accumulator * 16 + ch - '0';
                    else if ( 'a' <= ch && ch <= 'f' )
                        accumulator = accumulator * 16 + ch - 'a' + 10;
                    else if ( 'A' <= ch && ch <= 'F' )
                        accumulator = accumulator * 16 + ch - 'A' + 10;
                }
                ParsedNumbers[i] = accumulator;
            }
        }

        [Benchmark]
        public void NaiveHexParsingWithBooleanCheck ( )
        {
            for ( var i = NumbersCount - 1; i >= 0; i-- )
            {
                Char ch;
                var accumulator = 0;
                var reader = new StringCodeReader ( NumbersToParse[i] );
                while ( reader.Position < reader.Length && IsHexCharBoolean ( ch = reader.Peek ( ).Value ) )
                {
                    reader.Advance ( 1 );
                    if ( '0' <= ch && ch <= '9' )
                        accumulator = accumulator * 16 + ch - '0';
                    else if ( 'a' <= ch && ch <= 'f' )
                        accumulator = accumulator * 16 + ch - 'a' + 10;
                    else if ( 'A' <= ch && ch <= 'F' )
                        accumulator = accumulator * 16 + ch - 'A' + 10;
                }
                ParsedNumbers[i] = accumulator;
            }
        }

        private static readonly IDictionary<Char, Int32> lut = new Dictionary<Char, Int32>
        {
            ['0'] = 0,
            ['1'] = 1,
            ['2'] = 2,
            ['3'] = 3,
            ['4'] = 4,
            ['5'] = 5,
            ['6'] = 6,
            ['7'] = 7,
            ['8'] = 8,
            ['9'] = 9,
            ['a'] = 10,
            ['b'] = 11,
            ['c'] = 12,
            ['d'] = 13,
            ['e'] = 14,
            ['f'] = 15,
            ['A'] = 10,
            ['B'] = 11,
            ['C'] = 12,
            ['D'] = 13,
            ['E'] = 14,
            ['F'] = 15,
        };

        [Benchmark]
        public void LookupTableParsing ( )
        {
            for ( var i = NumbersCount - 1; i >= 0; i-- )
            {
                var accumulator = 0;
                var reader = new StringCodeReader ( NumbersToParse[i] );
                while ( reader.Position < reader.Length && lut.TryGetValue ( reader.Peek ( ).Value, out var digit ) )
                {
                    reader.Advance ( 1 );
                    accumulator = accumulator * 16 + digit;
                }
                ParsedNumbers[i] = accumulator;
            }
        }

        private static readonly Int32[] lua =
        {
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            10,
            11,
            12,
            13,
            14,
            15,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            10,
            11,
            12,
            13,
            14,
            15
        };

        [Benchmark]
        public void LookupArrayParsing ( )
        {
            for ( var i = NumbersCount - 1; i >= 0; i-- )
            {
                Char ch; Int32 v;
                var accumulator = 0;
                var reader = new StringCodeReader ( NumbersToParse[i] );
                while ( reader.Position < reader.Length && ( ch = reader.Peek ( ).Value ) < 103 && ( v = lua[ch] ) != -1 )
                {
                    reader.Advance ( 1 );
                    accumulator = accumulator * 16 + v;
                }
                ParsedNumbers[i] = accumulator;
            }
        }

        //[MethodImpl ( MethodImplOptions.AggressiveInlining )]
        private static Int32 HexCharToInt ( in Char ch )
        {
            switch ( ch )
            {
                case '0':
                    return 0;

                case '1':
                    return 1;

                case '2':
                    return 2;

                case '3':
                    return 3;

                case '4':
                    return 4;

                case '5':
                    return 5;

                case '6':
                    return 6;

                case '7':
                    return 7;

                case '8':
                    return 8;

                case '9':
                    return 9;

                case 'a':
                case 'A':
                    return 10;

                case 'b':
                case 'B':
                    return 11;

                case 'c':
                case 'C':
                    return 12;

                case 'd':
                case 'D':
                    return 13;

                case 'e':
                case 'E':
                    return 14;

                case 'f':
                case 'F':
                    return 15;

                default:
                    return -1;
            }
        }

        [Benchmark]
        public void SwitchParsing ( )
        {
            for ( var i = NumbersCount - 1; i >= 0; i-- )
            {
                Int32 v;
                var accumulator = 0;
                var reader = new StringCodeReader ( NumbersToParse[i] );
                while ( reader.Position < reader.Length && ( v = HexCharToInt ( reader.Peek ( ).Value ) ) != -1 )
                {
                    reader.Advance ( 1 );
                    accumulator = accumulator * 16 + v;
                }
                ParsedNumbers[i] = accumulator;
            }
        }

        [Benchmark]
        public void BitFuckeryParsingWithBitwiseCheck ( )
        {
            for ( var i = NumbersCount - 1; i >= 0; i-- )
            {
                Char ch;
                var accumulator = 0;
                var reader = new StringCodeReader ( NumbersToParse[i] );
                while ( reader.Position < reader.Length && IsHexCharBitwise ( ch = reader.Peek ( ).Value ) )
                {
                    reader.Advance ( 1 );
                    accumulator = ( accumulator << 4 ) + ( ch & 0b1111 ) + ( ( ch & 0b1000000 ) * 9 ) / 0b1000000;
                }
                ParsedNumbers[i] = accumulator;
            }
        }

        [Benchmark]
        public void BitFuckeryParsingWithBooleanCheck ( )
        {
            for ( var i = NumbersCount - 1; i >= 0; i-- )
            {
                Char ch;
                var accumulator = 0;
                var reader = new StringCodeReader ( NumbersToParse[i] );
                while ( reader.Position < reader.Length && IsHexCharBoolean ( ch = reader.Peek ( ).Value ) )
                {
                    reader.Advance ( 1 );
                    accumulator = ( accumulator << 4 ) + ( ch & 0b1111 ) + ( ( ( ch & 0b1000000 ) >> 6 ) * 9 );
                }
                ParsedNumbers[i] = accumulator;
            }
        }

        [Benchmark]
        public void BranchingBitFuckeryParsingWithBitwiseCheck ( )
        {
            for ( var i = NumbersCount - 1; i >= 0; i-- )
            {
                Char ch;
                var accumulator = 0;
                var reader = new StringCodeReader ( NumbersToParse[i] );
                while ( reader.Position < reader.Length && IsHexCharBitwise ( ch = reader.Peek ( ).Value ) )
                {
                    reader.Advance ( 1 );
                    accumulator = ( accumulator << 4 ) + ( ch & 0b1111 );
                    if ( ( ch & 0b1000000 ) != 0 )
                        accumulator += 9;
                }
                ParsedNumbers[i] = accumulator;
            }
        }

        [Benchmark]
        public void BranchingBitFuckeryParsingWithBooleanCheck ( )
        {
            for ( var i = NumbersCount - 1; i >= 0; i-- )
            {
                Char ch;
                var accumulator = 0;
                var reader = new StringCodeReader ( NumbersToParse[i] );
                while ( reader.Position < reader.Length && IsHexCharBoolean ( ch = reader.Peek ( ).Value ) )
                {
                    reader.Advance ( 1 );
                    accumulator = ( accumulator << 4 ) + ( ch & 0b1111 );
                    if ( ( ch & 0b1000000 ) != 0 )
                        accumulator += 9;
                }
                ParsedNumbers[i] = accumulator;
            }
        }

        [Benchmark]
        public void VectorizedBitFuckeryParsingWithBitwiseCheck ( )
        {
            for ( var i = NumbersCount - 1; i >= 0; i-- )
            {
                var accumulator = 0;
                var reader = new StringCodeReader ( NumbersToParse[i] );

                var arr = new UInt16[Count];
                var idx = 0;
                for ( var ch = reader.Peek ( ); idx < Count && !( ch is null ) && IsHexCharBitwise ( ch.Value ); ch = reader.Peek ( ) )
                {
                    reader.Advance ( 1 );
                    arr[idx++] = ch.Value;
                }

                var vec = new Vector<UInt16> ( arr );
                vec = ( vec & Masks ) + ( ( vec & Checkers ) * Nines / Checkers );
                for ( var j = 0; j < idx; j++ )
                    accumulator = ( accumulator * 16 ) + vec[j];
                ParsedNumbers[i] = accumulator;
            }
        }

        [Benchmark]
        public void VectorizedBitFuckeryParsingWithBooleanCheck ( )
        {
            for ( var i = NumbersCount - 1; i >= 0; i-- )
            {
                var accumulator = 0;
                var reader = new StringCodeReader ( NumbersToParse[i] );

                var arr = new UInt16[Count];
                var idx = 0;
                for ( var ch = reader.Peek ( ); idx < Count && !( ch is null ) && IsHexCharBoolean ( ch.Value ); ch = reader.Peek ( ) )
                {
                    reader.Advance ( 1 );
                    arr[idx++] = ch.Value;
                }

                var vec = new Vector<UInt16> ( arr );
                vec = ( vec & Masks ) + ( ( vec & Checkers ) * Nines / Checkers );
                for ( var j = 0; j < idx; j++ )
                    accumulator = ( accumulator * 16 ) + vec[j];
                ParsedNumbers[i] = accumulator;
            }
        }
    }
}