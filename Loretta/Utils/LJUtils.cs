using System;
using System.Collections.Generic;

namespace Loretta.Utils
{
    public enum LJ_CHAR : Byte
    {
        NONE = 0b00000000,
        CNTRL = 0b00000001,
        SPACE = 0b00000010,
        PUNCT = 0b00000100,
        DIGIT = 0b00001000,
        XDIGIT = 0b00010000,
        UPPER = 0b00100000,
        LOWER = 0b01000000,
        IDENT = 0b10000000,
        ALPHA = LOWER | UPPER,
        ALNUM = ALPHA | DIGIT,
        GRAPH = ALNUM | PUNCT
    }

    public static class LJUtils
    {
        public static readonly LJ_CHAR[] CharList = new LJ_CHAR[] {
            #region Long ass char list
            /* '\x00'(0x 0) */ LJ_CHAR.CNTRL,
            /* '\x01'(0x 1) */ LJ_CHAR.CNTRL,
            /* '\x02'(0x 2) */ LJ_CHAR.CNTRL,
            /* '\x03'(0x 3) */ LJ_CHAR.CNTRL,
            /* '\x04'(0x 4) */ LJ_CHAR.CNTRL,
            /* '\x05'(0x 5) */ LJ_CHAR.CNTRL,
            /* '\x06'(0x 6) */ LJ_CHAR.CNTRL,
            /* '\x07'(0x 7) */ LJ_CHAR.CNTRL,
            /* '\x08'(0x 8) */ LJ_CHAR.CNTRL,
            /*   '\t'(0x 9) */ LJ_CHAR.CNTRL | LJ_CHAR.SPACE,
            /*   '\n'(0x A) */ LJ_CHAR.CNTRL | LJ_CHAR.SPACE,
            /* '\x0b'(0x B) */ LJ_CHAR.CNTRL | LJ_CHAR.SPACE,
            /* '\x0c'(0x C) */ LJ_CHAR.CNTRL | LJ_CHAR.SPACE,
            /*   '\r'(0x D) */ LJ_CHAR.CNTRL | LJ_CHAR.SPACE,
            /* '\x0e'(0x E) */ LJ_CHAR.CNTRL,
            /* '\x0f'(0x F) */ LJ_CHAR.CNTRL,
            /* '\x10'(0x10) */ LJ_CHAR.CNTRL,
            /* '\x11'(0x11) */ LJ_CHAR.CNTRL,
            /* '\x12'(0x12) */ LJ_CHAR.CNTRL,
            /* '\x13'(0x13) */ LJ_CHAR.CNTRL,
            /* '\x14'(0x14) */ LJ_CHAR.CNTRL,
            /* '\x15'(0x15) */ LJ_CHAR.CNTRL,
            /* '\x16'(0x16) */ LJ_CHAR.CNTRL,
            /* '\x17'(0x17) */ LJ_CHAR.CNTRL,
            /* '\x18'(0x18) */ LJ_CHAR.CNTRL,
            /* '\x19'(0x19) */ LJ_CHAR.CNTRL,
            /* '\x1a'(0x1A) */ LJ_CHAR.CNTRL,
            /* '\x1b'(0x1B) */ LJ_CHAR.CNTRL,
            /* '\x1c'(0x1C) */ LJ_CHAR.CNTRL,
            /* '\x1d'(0x1D) */ LJ_CHAR.CNTRL,
            /* '\x1e'(0x1E) */ LJ_CHAR.CNTRL,
            /* '\x1f'(0x1F) */ LJ_CHAR.CNTRL,
            /*    ' '(0x20) */ LJ_CHAR.SPACE,
            /*    '!'(0x21) */ LJ_CHAR.PUNCT,
            /*    '"'(0x22) */ LJ_CHAR.PUNCT,
            /*    '#'(0x23) */ LJ_CHAR.PUNCT,
            /*    '$'(0x24) */ LJ_CHAR.PUNCT,
            /*    '%'(0x25) */ LJ_CHAR.PUNCT,
            /*    '&'(0x26) */ LJ_CHAR.PUNCT,
            /*    "'"(0x27) */ LJ_CHAR.PUNCT,
            /*    '('(0x28) */ LJ_CHAR.PUNCT,
            /*    ')'(0x29) */ LJ_CHAR.PUNCT,
            /*    '*'(0x2A) */ LJ_CHAR.PUNCT,
            /*    '+'(0x2B) */ LJ_CHAR.PUNCT,
            /*    ','(0x2C) */ LJ_CHAR.PUNCT,
            /*    '-'(0x2D) */ LJ_CHAR.PUNCT,
            /*    '.'(0x2E) */ LJ_CHAR.PUNCT,
            /*    '/'(0x2F) */ LJ_CHAR.PUNCT,
            /*    '0'(0x30) */ LJ_CHAR.DIGIT | LJ_CHAR.XDIGIT | LJ_CHAR.IDENT,
            /*    '1'(0x31) */ LJ_CHAR.DIGIT | LJ_CHAR.XDIGIT | LJ_CHAR.IDENT,
            /*    '2'(0x32) */ LJ_CHAR.DIGIT | LJ_CHAR.XDIGIT | LJ_CHAR.IDENT,
            /*    '3'(0x33) */ LJ_CHAR.DIGIT | LJ_CHAR.XDIGIT | LJ_CHAR.IDENT,
            /*    '4'(0x34) */ LJ_CHAR.DIGIT | LJ_CHAR.XDIGIT | LJ_CHAR.IDENT,
            /*    '5'(0x35) */ LJ_CHAR.DIGIT | LJ_CHAR.XDIGIT | LJ_CHAR.IDENT,
            /*    '6'(0x36) */ LJ_CHAR.DIGIT | LJ_CHAR.XDIGIT | LJ_CHAR.IDENT,
            /*    '7'(0x37) */ LJ_CHAR.DIGIT | LJ_CHAR.XDIGIT | LJ_CHAR.IDENT,
            /*    '8'(0x38) */ LJ_CHAR.DIGIT | LJ_CHAR.XDIGIT | LJ_CHAR.IDENT,
            /*    '9'(0x39) */ LJ_CHAR.DIGIT | LJ_CHAR.XDIGIT | LJ_CHAR.IDENT,
            /*    ':'(0x3A) */ LJ_CHAR.PUNCT,
            /*    ';'(0x3B) */ LJ_CHAR.PUNCT,
            /*    '<'(0x3C) */ LJ_CHAR.PUNCT,
            /*    '='(0x3D) */ LJ_CHAR.PUNCT,
            /*    '>'(0x3E) */ LJ_CHAR.PUNCT,
            /*    '?'(0x3F) */ LJ_CHAR.PUNCT,
            /*    '@'(0x40) */ LJ_CHAR.PUNCT,
            /*    'A'(0x41) */ LJ_CHAR.XDIGIT | LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'B'(0x42) */ LJ_CHAR.XDIGIT | LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'C'(0x43) */ LJ_CHAR.XDIGIT | LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'D'(0x44) */ LJ_CHAR.XDIGIT | LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'E'(0x45) */ LJ_CHAR.XDIGIT | LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'F'(0x46) */ LJ_CHAR.XDIGIT | LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'G'(0x47) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'H'(0x48) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'I'(0x49) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'J'(0x4A) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'K'(0x4B) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'L'(0x4C) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'M'(0x4D) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'N'(0x4E) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'O'(0x4F) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'P'(0x50) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'Q'(0x51) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'R'(0x52) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'S'(0x53) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'T'(0x54) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'U'(0x55) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'V'(0x56) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'W'(0x57) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'X'(0x58) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'Y'(0x59) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    'Z'(0x5A) */ LJ_CHAR.UPPER | LJ_CHAR.IDENT,
            /*    '['(0x5B) */ LJ_CHAR.PUNCT,
            /*   '\\'(0x5C) */ LJ_CHAR.PUNCT,
            /*    ']'(0x5D) */ LJ_CHAR.PUNCT,
            /*    '^'(0x5E) */ LJ_CHAR.PUNCT,
            /*    '_'(0x5F) */ LJ_CHAR.PUNCT | LJ_CHAR.IDENT,
            /*    '`'(0x60) */ LJ_CHAR.PUNCT,
            /*    'a'(0x61) */ LJ_CHAR.XDIGIT | LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'b'(0x62) */ LJ_CHAR.XDIGIT | LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'c'(0x63) */ LJ_CHAR.XDIGIT | LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'd'(0x64) */ LJ_CHAR.XDIGIT | LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'e'(0x65) */ LJ_CHAR.XDIGIT | LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'f'(0x66) */ LJ_CHAR.XDIGIT | LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'g'(0x67) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'h'(0x68) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'i'(0x69) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'j'(0x6A) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'k'(0x6B) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'l'(0x6C) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'm'(0x6D) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'n'(0x6E) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'o'(0x6F) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'p'(0x70) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'q'(0x71) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'r'(0x72) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    's'(0x73) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    't'(0x74) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'u'(0x75) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'v'(0x76) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'w'(0x77) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'x'(0x78) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'y'(0x79) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    'z'(0x7A) */ LJ_CHAR.LOWER | LJ_CHAR.IDENT,
            /*    '{'(0x7B) */ LJ_CHAR.PUNCT,
            /*    '|'(0x7C) */ LJ_CHAR.PUNCT,
            /*    '}'(0x7D) */ LJ_CHAR.PUNCT,
            /*    '~'(0x7E) */ LJ_CHAR.PUNCT,
            /* '\x7f'(0x7F) */ LJ_CHAR.CNTRL,
            /* '\x80'(0x80) */ LJ_CHAR.IDENT,
            /* '\x81'(0x81) */ LJ_CHAR.IDENT,
            /* '\x82'(0x82) */ LJ_CHAR.IDENT,
            /* '\x83'(0x83) */ LJ_CHAR.IDENT,
            /* '\x84'(0x84) */ LJ_CHAR.IDENT,
            /* '\x85'(0x85) */ LJ_CHAR.IDENT,
            /* '\x86'(0x86) */ LJ_CHAR.IDENT,
            /* '\x87'(0x87) */ LJ_CHAR.IDENT,
            /* '\x88'(0x88) */ LJ_CHAR.IDENT,
            /* '\x89'(0x89) */ LJ_CHAR.IDENT,
            /* '\x8a'(0x8A) */ LJ_CHAR.IDENT,
            /* '\x8b'(0x8B) */ LJ_CHAR.IDENT,
            /* '\x8c'(0x8C) */ LJ_CHAR.IDENT,
            /* '\x8d'(0x8D) */ LJ_CHAR.IDENT,
            /* '\x8e'(0x8E) */ LJ_CHAR.IDENT,
            /* '\x8f'(0x8F) */ LJ_CHAR.IDENT,
            /* '\x90'(0x90) */ LJ_CHAR.IDENT,
            /* '\x91'(0x91) */ LJ_CHAR.IDENT,
            /* '\x92'(0x92) */ LJ_CHAR.IDENT,
            /* '\x93'(0x93) */ LJ_CHAR.IDENT,
            /* '\x94'(0x94) */ LJ_CHAR.IDENT,
            /* '\x95'(0x95) */ LJ_CHAR.IDENT,
            /* '\x96'(0x96) */ LJ_CHAR.IDENT,
            /* '\x97'(0x97) */ LJ_CHAR.IDENT,
            /* '\x98'(0x98) */ LJ_CHAR.IDENT,
            /* '\x99'(0x99) */ LJ_CHAR.IDENT,
            /* '\x9a'(0x9A) */ LJ_CHAR.IDENT,
            /* '\x9b'(0x9B) */ LJ_CHAR.IDENT,
            /* '\x9c'(0x9C) */ LJ_CHAR.IDENT,
            /* '\x9d'(0x9D) */ LJ_CHAR.IDENT,
            /* '\x9e'(0x9E) */ LJ_CHAR.IDENT,
            /* '\x9f'(0x9F) */ LJ_CHAR.IDENT,
            /* '\xa0'(0xA0) */ LJ_CHAR.IDENT,
            /*    '¡'(0xA1) */ LJ_CHAR.IDENT,
            /*    '¢'(0xA2) */ LJ_CHAR.IDENT,
            /*    '£'(0xA3) */ LJ_CHAR.IDENT,
            /*    '¤'(0xA4) */ LJ_CHAR.IDENT,
            /*    '¥'(0xA5) */ LJ_CHAR.IDENT,
            /*    '¦'(0xA6) */ LJ_CHAR.IDENT,
            /*    '§'(0xA7) */ LJ_CHAR.IDENT,
            /*    '¨'(0xA8) */ LJ_CHAR.IDENT,
            /*    '©'(0xA9) */ LJ_CHAR.IDENT,
            /*    'ª'(0xAA) */ LJ_CHAR.IDENT,
            /*    '«'(0xAB) */ LJ_CHAR.IDENT,
            /*    '¬'(0xAC) */ LJ_CHAR.IDENT,
            /* '\xad'(0xAD) */ LJ_CHAR.IDENT,
            /*    '®'(0xAE) */ LJ_CHAR.IDENT,
            /*    '¯'(0xAF) */ LJ_CHAR.IDENT,
            /*    '°'(0xB0) */ LJ_CHAR.IDENT,
            /*    '±'(0xB1) */ LJ_CHAR.IDENT,
            /*    '²'(0xB2) */ LJ_CHAR.IDENT,
            /*    '³'(0xB3) */ LJ_CHAR.IDENT,
            /*    '´'(0xB4) */ LJ_CHAR.IDENT,
            /*    'µ'(0xB5) */ LJ_CHAR.IDENT,
            /*    '¶'(0xB6) */ LJ_CHAR.IDENT,
            /*    '·'(0xB7) */ LJ_CHAR.IDENT,
            /*    '¸'(0xB8) */ LJ_CHAR.IDENT,
            /*    '¹'(0xB9) */ LJ_CHAR.IDENT,
            /*    'º'(0xBA) */ LJ_CHAR.IDENT,
            /*    '»'(0xBB) */ LJ_CHAR.IDENT,
            /*    '¼'(0xBC) */ LJ_CHAR.IDENT,
            /*    '½'(0xBD) */ LJ_CHAR.IDENT,
            /*    '¾'(0xBE) */ LJ_CHAR.IDENT,
            /*    '¿'(0xBF) */ LJ_CHAR.IDENT,
            /*    'À'(0xC0) */ LJ_CHAR.IDENT,
            /*    'Á'(0xC1) */ LJ_CHAR.IDENT,
            /*    'Â'(0xC2) */ LJ_CHAR.IDENT,
            /*    'Ã'(0xC3) */ LJ_CHAR.IDENT,
            /*    'Ä'(0xC4) */ LJ_CHAR.IDENT,
            /*    'Å'(0xC5) */ LJ_CHAR.IDENT,
            /*    'Æ'(0xC6) */ LJ_CHAR.IDENT,
            /*    'Ç'(0xC7) */ LJ_CHAR.IDENT,
            /*    'È'(0xC8) */ LJ_CHAR.IDENT,
            /*    'É'(0xC9) */ LJ_CHAR.IDENT,
            /*    'Ê'(0xCA) */ LJ_CHAR.IDENT,
            /*    'Ë'(0xCB) */ LJ_CHAR.IDENT,
            /*    'Ì'(0xCC) */ LJ_CHAR.IDENT,
            /*    'Í'(0xCD) */ LJ_CHAR.IDENT,
            /*    'Î'(0xCE) */ LJ_CHAR.IDENT,
            /*    'Ï'(0xCF) */ LJ_CHAR.IDENT,
            /*    'Ð'(0xD0) */ LJ_CHAR.IDENT,
            /*    'Ñ'(0xD1) */ LJ_CHAR.IDENT,
            /*    'Ò'(0xD2) */ LJ_CHAR.IDENT,
            /*    'Ó'(0xD3) */ LJ_CHAR.IDENT,
            /*    'Ô'(0xD4) */ LJ_CHAR.IDENT,
            /*    'Õ'(0xD5) */ LJ_CHAR.IDENT,
            /*    'Ö'(0xD6) */ LJ_CHAR.IDENT,
            /*    '×'(0xD7) */ LJ_CHAR.IDENT,
            /*    'Ø'(0xD8) */ LJ_CHAR.IDENT,
            /*    'Ù'(0xD9) */ LJ_CHAR.IDENT,
            /*    'Ú'(0xDA) */ LJ_CHAR.IDENT,
            /*    'Û'(0xDB) */ LJ_CHAR.IDENT,
            /*    'Ü'(0xDC) */ LJ_CHAR.IDENT,
            /*    'Ý'(0xDD) */ LJ_CHAR.IDENT,
            /*    'Þ'(0xDE) */ LJ_CHAR.IDENT,
            /*    'ß'(0xDF) */ LJ_CHAR.IDENT,
            /*    'à'(0xE0) */ LJ_CHAR.IDENT,
            /*    'á'(0xE1) */ LJ_CHAR.IDENT,
            /*    'â'(0xE2) */ LJ_CHAR.IDENT,
            /*    'ã'(0xE3) */ LJ_CHAR.IDENT,
            /*    'ä'(0xE4) */ LJ_CHAR.IDENT,
            /*    'å'(0xE5) */ LJ_CHAR.IDENT,
            /*    'æ'(0xE6) */ LJ_CHAR.IDENT,
            /*    'ç'(0xE7) */ LJ_CHAR.IDENT,
            /*    'è'(0xE8) */ LJ_CHAR.IDENT,
            /*    'é'(0xE9) */ LJ_CHAR.IDENT,
            /*    'ê'(0xEA) */ LJ_CHAR.IDENT,
            /*    'ë'(0xEB) */ LJ_CHAR.IDENT,
            /*    'ì'(0xEC) */ LJ_CHAR.IDENT,
            /*    'í'(0xED) */ LJ_CHAR.IDENT,
            /*    'î'(0xEE) */ LJ_CHAR.IDENT,
            /*    'ï'(0xEF) */ LJ_CHAR.IDENT,
            /*    'ð'(0xF0) */ LJ_CHAR.IDENT,
            /*    'ñ'(0xF1) */ LJ_CHAR.IDENT,
            /*    'ò'(0xF2) */ LJ_CHAR.IDENT,
            /*    'ó'(0xF3) */ LJ_CHAR.IDENT,
            /*    'ô'(0xF4) */ LJ_CHAR.IDENT,
            /*    'õ'(0xF5) */ LJ_CHAR.IDENT,
            /*    'ö'(0xF6) */ LJ_CHAR.IDENT,
            /*    '÷'(0xF7) */ LJ_CHAR.IDENT,
            /*    'ø'(0xF8) */ LJ_CHAR.IDENT,
            /*    'ù'(0xF9) */ LJ_CHAR.IDENT,
            /*    'ú'(0xFA) */ LJ_CHAR.IDENT,
            /*    'û'(0xFB) */ LJ_CHAR.IDENT,
            /*    'ü'(0xFC) */ LJ_CHAR.IDENT,
            /*    'ý'(0xFD) */ LJ_CHAR.IDENT,
            /*    'þ'(0xFE) */ LJ_CHAR.IDENT,
            /*    'ÿ'(0xFF) */ LJ_CHAR.IDENT
            #endregion Long ass char list
        };

        /// <summary>
        /// Will list all categories a certain character holds
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static String Types ( Char c )
        {
            if ( c > 255 )
                throw new Exception ( "Character is not in the Extended ASCII range ([0, 255])." );

            LJ_CHAR[] enums = new LJ_CHAR[] { LJ_CHAR.GRAPH, LJ_CHAR.ALNUM, LJ_CHAR.ALPHA, LJ_CHAR.IDENT, LJ_CHAR.LOWER, LJ_CHAR.UPPER, LJ_CHAR.XDIGIT, LJ_CHAR.DIGIT, LJ_CHAR.PUNCT, LJ_CHAR.SPACE, LJ_CHAR.CNTRL };
            var list = new List<String> ( );
            LJ_CHAR vals = CharList[c];

            foreach ( LJ_CHAR @enum in enums )
                if ( ( vals & @enum ) > 0 )
                    list.Add ( $"LJ_CHAR.{@enum.ToString ( )}" );

            return list.Count > 0 ? String.Join ( " | ", list ) : "LJ_CHAR.NONE";
        }

        public static Boolean IsA ( Byte c, LJ_CHAR type )
        {
            if ( c > 255 )
                throw new Exception ( "Character is outside of the Extended ASCII range. ([0, 255])" );
            return ( CharList[c] & type ) == type;
        }

        public static Boolean IsA ( Char c, LJ_CHAR type )
        {
            if ( c > 255 )
                throw new Exception ( "Character is outside of the Extended ASCII range. ([0, 255])" );
            return ( CharList[c] & type ) == type;
        }

        #region Cntrl implementation

        public static Boolean IsCntrl ( Byte c ) => IsA ( c, LJ_CHAR.CNTRL );

        public static Boolean IsCntrl ( Char c ) => IsA ( c, LJ_CHAR.CNTRL );

        #endregion Cntrl implementation

        #region Space implementation

        public static Boolean IsSpace ( Byte c ) => IsA ( c, LJ_CHAR.SPACE );

        public static Boolean IsSpace ( Char c ) => IsA ( c, LJ_CHAR.SPACE );

        #endregion Space implementation

        #region Punct implementation

        public static Boolean IsPunct ( Byte c ) => IsA ( c, LJ_CHAR.PUNCT );

        public static Boolean IsPunct ( Char c ) => IsA ( c, LJ_CHAR.PUNCT );

        #endregion Punct implementation

        #region Digit implementation

        public static Boolean IsDigit ( Byte c ) => IsA ( c, LJ_CHAR.DIGIT );

        public static Boolean IsDigit ( Char c ) => IsA ( c, LJ_CHAR.DIGIT );

        #endregion Digit implementation

        #region XDigit implementation

        public static Boolean IsXDigit ( Byte c ) => IsA ( c, LJ_CHAR.XDIGIT );

        public static Boolean IsXDigit ( Char c ) => IsA ( c, LJ_CHAR.XDIGIT );

        #endregion XDigit implementation

        #region Upper implementation

        public static Boolean IsUpper ( Byte c ) => IsA ( c, LJ_CHAR.UPPER );

        public static Boolean IsUpper ( Char c ) => IsA ( c, LJ_CHAR.UPPER );

        #endregion Upper implementation

        #region Lower implementation

        public static Boolean IsLower ( Byte c ) => IsA ( c, LJ_CHAR.LOWER );

        public static Boolean IsLower ( Char c ) => IsA ( c, LJ_CHAR.LOWER );

        #endregion Lower implementation

        #region Ident implementation

        public static Boolean IsIdent ( Byte c ) => IsA ( c, LJ_CHAR.IDENT );

        public static Boolean IsIdent ( Char c ) => IsA ( c, LJ_CHAR.IDENT );

        #endregion Ident implementation

        #region Alpha implementation

        public static Boolean IsAlpha ( Byte c ) => IsA ( c, LJ_CHAR.ALPHA );

        public static Boolean IsAlpha ( Char c ) => IsA ( c, LJ_CHAR.ALPHA );

        #endregion Alpha implementation

        #region AlNum implementation

        public static Boolean IsAlNum ( Byte c ) => IsA ( c, LJ_CHAR.ALNUM );

        public static Boolean IsAlNum ( Char c ) => IsA ( c, LJ_CHAR.ALNUM );

        #endregion AlNum implementation

        #region Graph implementation

        public static Boolean IsGraph ( Byte c ) => IsA ( c, LJ_CHAR.GRAPH );

        public static Boolean IsGraph ( Char c ) => IsA ( c, LJ_CHAR.GRAPH );

        #endregion Graph implementation
    }
}
