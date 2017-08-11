using System;
using System.Linq;
using System.Text;

namespace LuaParse
{
    public static class CLuaPort
    {
        private static Boolean IsAlphaByte ( Byte c )
        {
            return
                ( c >= 'a' && c <= 'z' ) ||
                ( c >= 'A' && c <= 'Z' ) ||
                ( c >= 194 && c <= 244 ) ||
                ( c == '_' );
        }

        private static Boolean IsAlNumByte ( Byte c )
        {
            return IsAlphaByte ( c ) || ( c >= '0' && c <= '9' );
        }

        public static Boolean IsAlpha ( Char c )
            => Encoding.UTF8.GetBytes ( new[] { c } ).All ( IsAlphaByte );

        public static Boolean IsAlNum ( Char c )
            => Encoding.UTF8.GetBytes ( new[] { c } ).All ( IsAlNumByte );
    }
}
