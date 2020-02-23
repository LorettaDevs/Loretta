// Copyright (c) Stephan Tolksdorf 2008-2013
// License: Simplified BSD License. See http://www.quanttec.com/fparsec/license.html. This code was
// modified to fit this project's code style and had the preprocessor tags and comments removed for
// brevity. 32-bit floating point related code was also removed since it was not relevant to this
// project. Unsafe code was removed as well.

using System;

namespace Loretta.ThirdParty.FParsec
{
    internal static class HexFloat
    {
        // see http://www.quanttec.com/fparsec/reference/charparsers.html#members.floatToHexString
        // for more information on the supported hexadecimal floating-point format

        // The non-LOW_TRUST code in this class relies on the endianness of floating-point numbers
        // in memory being the same as the normal platform endianness, i.e. on
        // *((uint*)(&s)) and *((ulong*)(&d)) returning the correct IEEE-754 bit representation of
        // the single and double precision numbers s and d. I'm not aware of any .NET/Mono platform
        // where this is not the case. In the unlikely event anyone ever runs this code on a
        // platform where this is not the case the unit tests will detect the problem.

        private static readonly Byte[] asciiHexValuePlus1s = {
            0,  0,  0,  0,  0,  0,  0, 0, 0,  0, 0, 0, 0, 0, 0, 0,
            0,  0,  0,  0,  0,  0,  0, 0, 0,  0, 0, 0, 0, 0, 0, 0,
            0,  0,  0,  0,  0,  0,  0, 0, 0,  0, 0, 0, 0, 0, 0, 0,
            1,  2,  3,  4,  5,  6,  7, 8, 9, 10, 0, 0, 0, 0, 0, 0,
            0, 11, 12, 13, 14, 15, 16, 0, 0,  0, 0, 0, 0, 0, 0, 0,
            0,  0,  0,  0,  0,  0,  0, 0, 0,  0, 0, 0, 0, 0, 0, 0,
            0, 11, 12, 13, 14, 15, 16, 0, 0,  0, 0, 0, 0, 0, 0, 0,
            0,  0,  0,  0,  0,  0,  0, 0, 0,  0, 0, 0, 0, 0, 0, 0
        };

        public static String DoubleToHexString ( Double x )
        {
            const Int32 expBits = 11;  // bits for biased exponent
            const Int32 maxBits = 53;  // significant bits (including implicit bit)
            const Int32 maxChars = 24; // "-0x1.fffffffffffffp-1022"
            const Int32 maxBiasedExp = (1 << expBits) - 1;
            const Int32 maxExp       = 1 << (expBits - 1); // max n for which 0.5*2^n is a double
            const Int32 bias = maxExp - 1;

            const Int32 maxFractNibbles = (maxBits - 1 + 3)/4;
            const UInt64 mask  = (1UL << (maxBits - 1)) - 1; // mask for lower (maxBits - 1) bits

            var xn = unchecked((UInt64)BitConverter.DoubleToInt64Bits(x));
            var sign = (Int32)(xn >> (maxBits - 1 + expBits));
            var e = (Int32)((xn >> (maxBits - 1)) & maxBiasedExp); // the biased exponent
            var s  = xn & mask; // the significand (without the implicit bit)
            if ( e < maxBiasedExp )
            {
                if ( e == 0 && s == 0 ) return sign == 0 ? "0x0.0p0" : "-0x0.0p0";
                var str = new Char[maxChars];
                var i = 0;
                if ( sign != 0 ) str[i++] = '-';
                str[i++] = '0'; str[i++] = 'x';
                str[i++] = e > 0 ? '1' : '0';
                str[i++] = '.';
                //if ( ( maxBits - 1 ) % 4 > 0 )
                //{ // normalize fraction to multiple of 4 bits
                //    s <<= 4 - ( maxBits - 1 ) % 4;
                //}
                var lastNonNull = i;
                for ( var j = 0; j < maxFractNibbles; ++j )
                {
                    var h = unchecked((Int32) (s >> ((maxFractNibbles - 1 - j) << 2))) & 0xf;
                    if ( h != 0 ) lastNonNull = i;
                    str[i++] = "0123456789abcdef"[h];
                }
                i = lastNonNull + 1;
                str[i++] = 'p';
                if ( e >= bias )
                {
                    e -= bias;
                }
                else
                {
                    str[i++] = '-';
                    e = e > 0 ? -( e - bias ) : bias - 1;
                }
                // e holds absolute unbiased exponent
                var li = e < 10 ? 1 : (e < 100 ? 2 : (e < 1000 ? 3 : 4)); // floor(log(10, e))) + 1
                i += li;
                do
                {
                    var r = e%10; e /= 10;
                    str[--i] = ( Char ) ( 48 + r );
                } while ( e > 0 );
                i += li;
                return new String ( str, 0, i );
            }
            else
            {
                if ( s == 0 ) return sign == 0 ? "Infinity" : "-Infinity";
                else return "NaN";
            }
        }

        public static Double DoubleFromHexString ( String str )
        {
            const Int32 expBits = 11;    // bits for exponent
            const Int32 maxBits = 53;    // significant bits (including implicit bit)

            const Int32 maxExp = 1 << (expBits - 1); // max n for which 0.5*2^n is a double
            const Int32 minExp = -maxExp + 3; // min n for which 0.5*2^n is a normal double
            const Int32 minSExp = minExp - (maxBits - 1); // min n for which 0.5*2^n is a subnormal double

            const Int32 maxBits2 = maxBits + 2;
            const UInt64 mask  = (1UL << (maxBits - 1)) - 1; // mask for lower (maxBits - 1) bits

            if ( str == null ) throw new ArgumentNullException ( nameof ( str ) );
            var n = str.Length;
            if ( n == 0 ) goto InvalidFormat;

            // n*4 <= Int32.MaxValue protects against an nBits overflow, the additional -minSExp +
            // 10 margin is needed for parsing the exponent
            if ( n > ( Int32.MaxValue + minSExp - 10 ) / 4 )
                throw new OverflowException ( "The given hexadecimal string representation of a double precision floating-point number is too long." );

            var sign = 0;   // 0 == positive, 1 == negative
            UInt64 xn = 0;    // integer significand with up to maxBits + 2 bits, where the (maxBits + 2)th bit
                              // (the least significant bit) is the logical OR of the (maxBits +
                              // 2)th and all following input bits
            var nBits = -1; // number of bits in xn, not counting leading zeros
            var exp = 0;    // the base-2 exponent
            var s = str;
            var i = 0;
            // sign
            if ( s[0] == '+' )
            {
                i = 1;
            }
            else if ( s[0] == '-' )
            {
                i = 1;
                sign = 1;
            }
            // "0x" prefix
            if ( i + 1 < n && ( s[i + 1] == 'x' || s[i + 1] == 'X' ) )
            {
                if ( s[i] != '0' ) goto InvalidFormat;
                i += 2;
            }
            var pastDot = false;
            for (; ; )
            {
                if ( i == n )
                {
                    if ( !pastDot ) exp = nBits;
                    if ( nBits >= 0 ) break;
                    else goto InvalidFormat;
                }
                var c = s[i++];
                Int32 h;
                if ( c < 128 && ( h = asciiHexValuePlus1s[c] ) != 0 )
                {
                    --h;
                    if ( nBits <= 0 )
                    {
                        xn |= ( UInt32 ) h;
                        nBits = 0;
                        while ( h > 0 )
                        {
                            ++nBits;
                            h >>= 1;
                        }
                        if ( pastDot ) exp -= 4 - nBits;
                    }
                    else if ( nBits <= maxBits2 - 4 )
                    {
                        xn <<= 4;
                        xn |= ( UInt32 ) h;
                        nBits += 4;
                    }
                    else if ( nBits < maxBits2 )
                    {
                        var nRemBits = maxBits2 - nBits;
                        var nSurplusBits = 4 - nRemBits;
                        var surplusBits = h & (0xf >> nRemBits);
                        // The .NET JIT is not able to emit branch-free code for surplusBits =
                        // surplusBits != 0 ? 1 : 0; So we use this version instead:
                        surplusBits = ( 0xfffe >> surplusBits ) & 1; // = surplusBits != 0 ? 1 : 0
                        xn <<= nRemBits;
                        xn |= ( UInt32 ) ( ( h >> nSurplusBits ) | surplusBits );
                        nBits += 4;
                    }
                    else
                    {
                        xn |= ( UInt32 ) ( ( 0xfffe >> h ) & 1 ); // (0xfffe >> h) & 1 == h != 0 ? 1 : 0
                        nBits += 4;
                    }
                }
                else if ( c == '.' )
                {
                    if ( pastDot ) goto InvalidFormat;
                    pastDot = true;
                    exp = nBits >= 0 ? nBits : 0; // exponent for integer part of float
                }
                else if ( ( c | ' ' ) == 'p' && nBits >= 0 )
                {
                    if ( !pastDot ) exp = nBits;
                    var eSign = 1;
                    if ( i < n && ( s[i] == '-' || s[i] == '+' ) )
                    {
                        if ( s[i] == '-' ) eSign = -1;
                        ++i;
                    }
                    if ( i == n ) goto InvalidFormat;
                    var e = 0;
                    do
                    {
                        c = s[i++];
                        if ( ( c - ( UInt32 ) '0' ) <= 9 )
                        {
                            if ( e <= ( Int32.MaxValue - 9 ) / 10 ) e = e * 10 + ( c - '0' );
                            else e = Int32.MaxValue - 8;
                        }
                        else
                        {
                            goto InvalidFormat;
                        }
                    } while ( i < n );
                    e *= eSign;
                    // either e is exact or |e| >= int.MaxValue - 8 |exp| <= n*4 <= int.MaxValue +
                    // minSExp - 10
                    //
                    // Case 1: e and exp have the same sign Case 1.a: e is exact && |exp + e| <=
                    // int.MaxValue ==> |exp + e| is exact Case 1.b: |e| >= int.MaxValue - 8 || |exp
                    // + e| > int.MaxValue ==> |exp + e| >= int.MaxValue
                    // - 8 Case 2: e and exp have opposite signs Case 2.a: e is exact ==> |exp + e|
                    // is exact Case 2.b: |e| >= int.MaxValue - 8
                    // ==> Case e > 0: exp + e >=
                    // -(int.MaxValue + minSExp - 10) + (int.MaxValue - 8) = -minSExp + 2 > maxExp
                    // Case e < 0: exp + e <= (int.MaxValue + minSExp - 10) - (int.MaxValue - 8) =
                    // minSExp - 2
                    //
                    // hence, |exp + e| is exact || exp + e > maxExp || exp + e < minSExp - 1
                    try
                    {
                        exp = checked(exp + e);
                    }
                    catch ( OverflowException )
                    {
                        exp = e < 0 ? Int32.MinValue : Int32.MaxValue;
                    }
                    break;
                }
                else
                {
                    --i;
                    if ( nBits == -1 && i + 3 <= n )
                    {
                        if ( ( ( s[i] | ' ' ) == 'i' )
                            && ( ( s[i + 1] | ' ' ) == 'n' )
                            && ( ( s[i + 2] | ' ' ) == 'f' )
                            && ( i + 3 == n
                                || ( i + 8 == n && ( ( s[i + 3] | ' ' ) == 'i' )
                                               && ( ( s[i + 4] | ' ' ) == 'n' )
                                               && ( ( s[i + 5] | ' ' ) == 'i' )
                                               && ( ( s[i + 6] | ' ' ) == 't' )
                                               && ( ( s[i + 7] | ' ' ) == 'y' ) ) ) )
                        {
                            return sign == 0 ? Double.PositiveInfinity : Double.NegativeInfinity;
                        }
                        else if ( i + 3 == n && ( ( s[i] | ' ' ) == 'n' )
                                            && ( ( s[i + 1] | ' ' ) == 'a' )
                                            && ( ( s[i + 2] | ' ' ) == 'n' ) )
                        {
                            return Double.NaN;
                        }
                    }
                    goto InvalidFormat;
                }
            } // for
            if ( nBits == 0 ) return sign == 0 ? 0.0 : -0.0;
            if ( exp <= maxExp )
            {
                if ( exp >= minExp && nBits <= maxBits )
                {
                    // not subnormal and no rounding is required
                    if ( nBits < maxBits ) xn <<= maxBits - nBits; // normalize significand to maxBits
                    xn &= mask; // mask out lower (maxBits - 1) bits, the most significant bit is encoded in exp
                }
                else
                {
                    if ( nBits < maxBits2 ) xn <<= maxBits2 - nBits; // normalize significand to (maxBits + 2) bits
                    var isSubnormal = 0;
                    if ( exp < minExp )
                    {
                        if ( exp < minSExp - 1 ) return sign == 0 ? 0.0 : -0.0; // underflow (minSExp - 1 could still be rounded to minSExp)
                        isSubnormal = 1;
                        do
                        {
                            xn = ( xn >> 1 ) | ( xn & 1 );
                        } while ( ++exp < minExp );
                        if ( xn <= 2 ) return sign == 0 ? 0.0 : -0.0; // underflow
                    }
                    var r = unchecked((Int32)xn) & 0x7; // (lsb, bit below lsb, logical OR of all bits below the bit below lsb)
                    xn >>= 2; // truncate to maxBits
                    if ( r >= 6 || r == 3 )
                    {
                        xn++;
                        xn &= mask;
                        if ( xn == 0 )
                        { // rounded to a power of two
                            exp += 1;
                            if ( exp > maxExp ) goto Overflow;
                        }
                    }
                    else
                    {
                        xn &= mask;
                    }
                    exp -= isSubnormal;
                }
                exp -= minExp - 1; // add bias
                xn = ( ( ( UInt64 ) sign ) << ( ( maxBits - 1 ) + expBits ) ) | ( ( ( UInt64 ) exp ) << ( maxBits - 1 ) ) | xn;
                return BitConverter.Int64BitsToDouble ( unchecked(( Int64 ) xn) );
            }

            Overflow:
            var msg = n < 32 ? "The given string (\"" + str + "\") represents a value either too large or too small for a double precision floating-point number."
                        : "The given string represents a value either too large or too small for a double precision floating-point number.";
            throw new OverflowException ( msg );

            InvalidFormat:
            var errmsg = n < 32 ? "The given hexadecimal string representation of a double precision floating-point number (\"" + str + "\") is invalid."
                           : "The given hexadecimal string representation of a double precision floating-point number is invalid.";
            throw new FormatException ( errmsg );
        }
    } // class HexFloat
}