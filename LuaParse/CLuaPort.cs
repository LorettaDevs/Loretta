using System;
using System.Linq;

namespace LuaParse
{
    public static class CLuaPort
    {
        #region ctype.h

        private const Int32
            _UPPER = 0x01,      // uppercase letter
            _LOWER = 0x02,      // lowercase letter
            _DIGIT = 0x04,      // digit[0-9]
            _SPACE = 0x08,      // tab, carriage return, newline, vertical tab, or form feed
            _PUNCT = 0x10,      // punctuation character
            _CONTROL = 0x20,    // control character
            _BLANK = 0x40,      // space char (tab is handled separately)
            _HEX = 0x80,        // hexadecimal digit
            _LEADBYTE = 0x8000,
            _ALPHA = ( 0x0100 | _UPPER | _LOWER );

        private static readonly Char[] FalsePositives = new[] { '(', ')', '.', '\r', '\n', '"', '\''};

        public static Boolean IsAlpha ( Char c ) => !FalsePositives.Contains ( c ) && !Char.IsDigit ( c ) && ( Char.IsLetter ( c ) || ( c & _ALPHA ) != 0 );

        public static Boolean IsAlNum ( Char c ) => !FalsePositives.Contains ( c ) && ( Char.IsLetterOrDigit ( c ) || ( c & ( _ALPHA | _DIGIT ) ) != 0 );

        #endregion ctype.h
    }
}
