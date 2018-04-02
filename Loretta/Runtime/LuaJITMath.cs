using System;
using System.Runtime.InteropServices;

namespace Loretta.Runtime
{
    // luajit's math.random and math.randomseed
    public class LuaJITMath
    {
        #region LuaJIT internals

        public class RandomState
        {
            public UInt64[] gen;
            public Boolean valid;
        }

        [StructLayout ( LayoutKind.Explicit )]
        public struct U64double
        {
            [FieldOffset(0)] public UInt64 u64;
            [FieldOffset(0)] public Double d;
        }

        #endregion LuaJIT internals

        private readonly RandomState rs = new RandomState
        {
            gen = new UInt64[4],
            valid = false
        };

        public void RandomSeed ( Double seed )
        {
            this.RandomInit ( seed );
        }

        public Double Random ( )
        {
            U64double u; u.u64 = 0; u.d = 0;
            if ( !this.rs.valid )
                this.RandomInit ( 0.0 );
            u.u64 = this.RandomStep ( );
            return u.d - 1.0;
        }

        public Double Random ( Double max )
        {
            U64double u; u.u64 = 0; u.d = 0;
            Double d;
            if ( !this.rs.valid )
                this.RandomInit ( 0.0 );
            u.u64 = this.RandomStep ( );
            d = u.d - 1.0;
            return Math.Floor ( d * max ) + 1.0; /* d is an int in range [1, r1] */
        }

        public Int32 Random ( Int32 max )
        {
            U64double u; u.u64 = 0; u.d = 0;
            Double d;
            if ( !this.rs.valid )
                this.RandomInit ( 0.0 );
            u.u64 = this.RandomStep ( );
            d = u.d - 1.0;
            return ( Int32 ) Math.Floor ( d * max ) + 1; /* d is an int in range [1, r1] */
        }

        public Double Random ( Double min, Double max )
        {
            U64double u; u.u64 = 0; u.d = 0;
            Double d;
            if ( !this.rs.valid )
                this.RandomInit ( 0.0 );
            u.u64 = this.RandomStep ( );
            d = u.d - 1.0;
            return Math.Floor ( d * ( max - min + 1.0 ) ) + min; /* d is an int in range [r1, r2] */
        }

        public Int32 Random ( Int32 min, Int32 max )
        {
            U64double u; u.u64 = 0; u.d = 0;
            Double d;
            if ( !this.rs.valid )
                this.RandomInit ( 0.0 );
            u.u64 = this.RandomStep ( );
            d = u.d - 1.0;
            return ( Int32 ) Math.Floor ( d * ( max - min + 1.0 ) ) + min; /* d is an int in range [r1, r2] */
        }

        private UInt64 RandomStep ( )
        {
            UInt64 z, r = 0;
            z = this.rs.gen[0];
            z = ( ( ( z << 31 ) ^ z ) >> 45 ) ^ ( ( z & 0xFFFFFFFFFFFFFFFE ) << 18 );
            r ^= z;
            this.rs.gen[0] = z;

            z = this.rs.gen[1];
            z = ( ( ( z << 19 ) ^ z ) >> 30 ) ^ ( ( z & 0xFFFFFFFFFFFFFFC0 ) << 28 );
            r ^= z;
            this.rs.gen[1] = z;

            z = this.rs.gen[2];
            z = ( ( ( z << 24 ) ^ z ) >> 48 ) ^ ( ( z & 0xFFFFFFFFFFFFFE00 ) << 7 );
            r ^= z;
            this.rs.gen[2] = z;

            z = this.rs.gen[3];
            z = ( ( ( z << 21 ) ^ z ) >> 39 ) ^ ( ( z & 0xFFFFFFFFFFFE0000 ) << 8 );
            r ^= z;
            this.rs.gen[3] = z;

            return ( r & 0xFFFFFFFFFFFFF ) | 0x3FF0000000000000;
        }

        private void RandomInit ( Double d )
        {
            UInt32 r = 0x11090601;

            for ( var i = 0; i < 4; i++ )
            {
                U64double u; u.u64 = 0;
                var m = 1U << ( Int32 ) ( r & 255 );
                r >>= 8;
                u.d = d = d * 3.14159265358979323846 + 2.7182818284590452354;
                if ( u.u64 < m ) u.u64 += m;  /* Ensure k[i] MSB of gen[i] are non-zero. */
                this.rs.gen[i] = u.u64;
            }
            this.rs.valid = true;

            for ( var i = 0; i < 10; i++ )
                this.RandomStep ( );
        }
    }
}
