using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Loretta.Lexing
{
    internal static class CharUtils
    {
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsDecimal ( Char ch ) =>
            '0' <= ch && ch <= '9';

        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsHexadecimal ( Char ch ) =>
            IsDecimal ( ch ) || ( 'a' <= ch && ch <= 'f' ) || ( 'A' <= ch && ch <= 'F' );

    }
}
