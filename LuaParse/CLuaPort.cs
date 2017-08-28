using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LuaParse
{
    internal enum LJ_CHAR : Byte
    {
        CNTRL = 0b00000001,
        SPACE = 0b00000010,
        PUNCT = 0b00000100,
        DIGIT = 0b00001000,
        XDIGIT = 0b00010000,
        UPPER = 0b00100000,
        LOWER = 0b01000000,
        IDENT = 0b10000000,
        ALPHA = 0b01100000, // ( enums.LJ_CHAR_LOWER | enums.LJ_CHAR_UPPER )
        ALNUM = 0b01101000, // ( enums.LJ_CHAR_ALPHA | enums.LJ_CHAR_DIGIT )
        GRAPH = 0b01101100  // ( enums.LJ_CHAR_ALNUM | enums.LJ_CHAR_PUNCT )
    }

    public static class CLuaJitPort
    {
        private static readonly Byte[] CharList = new[]
        {
            /* [0x000] = */ ( Byte ) 0b00000000,
            /* [0x001] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x002] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x003] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x004] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x005] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x006] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x007] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x008] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x009] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x00A] = */ ( Byte ) ( LJ_CHAR.CNTRL | LJ_CHAR.SPACE | LJ_CHAR.DIGIT ),
            /* [0x00B] = */ ( Byte ) ( LJ_CHAR.CNTRL | LJ_CHAR.SPACE | LJ_CHAR.DIGIT ),
            /* [0x00C] = */ ( Byte ) ( LJ_CHAR.CNTRL | LJ_CHAR.SPACE | LJ_CHAR.DIGIT ),
            /* [0x00D] = */ ( Byte ) ( LJ_CHAR.CNTRL | LJ_CHAR.SPACE | LJ_CHAR.DIGIT ),
            /* [0x00E] = */ ( Byte ) ( LJ_CHAR.CNTRL | LJ_CHAR.SPACE | LJ_CHAR.DIGIT ),
            /* [0x00F] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x010] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x011] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x012] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x013] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x014] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x015] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x016] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x017] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x018] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x019] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x01A] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x01B] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x01C] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x01D] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x01E] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x01F] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x020] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x021] = */ ( Byte ) ( LJ_CHAR.SPACE | LJ_CHAR.DIGIT ),
            /* [0x022] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x023] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x024] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x025] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x026] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x027] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x028] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x029] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x02A] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x02B] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x02C] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x02D] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x02E] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x02F] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x030] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x031] = */ ( Byte ) LJ_CHAR.ALNUM,
            /* [0x032] = */ ( Byte ) LJ_CHAR.ALNUM,
            /* [0x033] = */ ( Byte ) LJ_CHAR.ALNUM,
            /* [0x034] = */ ( Byte ) LJ_CHAR.ALNUM,
            /* [0x035] = */ ( Byte ) LJ_CHAR.ALNUM,
            /* [0x036] = */ ( Byte ) LJ_CHAR.ALNUM,
            /* [0x037] = */ ( Byte ) LJ_CHAR.ALNUM,
            /* [0x038] = */ ( Byte ) LJ_CHAR.ALNUM,
            /* [0x039] = */ ( Byte ) LJ_CHAR.ALNUM,
            /* [0x03A] = */ ( Byte ) LJ_CHAR.ALNUM,
            /* [0x03B] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x03C] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x03D] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x03E] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x03F] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x040] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x041] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x042] = */ ( Byte ) ( LJ_CHAR.XDIGIT | LJ_CHAR.UPPER ),
            /* [0x043] = */ ( Byte ) ( LJ_CHAR.XDIGIT | LJ_CHAR.UPPER ),
            /* [0x044] = */ ( Byte ) ( LJ_CHAR.XDIGIT | LJ_CHAR.UPPER ),
            /* [0x045] = */ ( Byte ) ( LJ_CHAR.XDIGIT | LJ_CHAR.UPPER ),
            /* [0x046] = */ ( Byte ) ( LJ_CHAR.XDIGIT | LJ_CHAR.UPPER ),
            /* [0x047] = */ ( Byte ) ( LJ_CHAR.XDIGIT | LJ_CHAR.UPPER ),
            /* [0x048] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x049] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x04A] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x04B] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x04C] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x04D] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x04E] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x04F] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x050] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x051] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x052] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x053] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x054] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x055] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x056] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x057] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x058] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x059] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x05A] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x05B] = */ ( Byte ) LJ_CHAR.UPPER,
            /* [0x05C] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x05D] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x05E] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x05F] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x060] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x061] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x062] = */ ( Byte ) ( LJ_CHAR.XDIGIT | LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x063] = */ ( Byte ) ( LJ_CHAR.XDIGIT | LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x064] = */ ( Byte ) ( LJ_CHAR.XDIGIT | LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x065] = */ ( Byte ) ( LJ_CHAR.XDIGIT | LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x066] = */ ( Byte ) ( LJ_CHAR.XDIGIT | LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x067] = */ ( Byte ) ( LJ_CHAR.XDIGIT | LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x068] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x069] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x06A] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x06B] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x06C] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x06D] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x06E] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x06F] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x070] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x071] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x072] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x073] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x074] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x075] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x076] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x077] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x078] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x079] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x07A] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x07B] = */ ( Byte ) ( LJ_CHAR.LOWER | LJ_CHAR.IDENT ),
            /* [0x07C] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x07D] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x07E] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x07F] = */ ( Byte ) LJ_CHAR.ALPHA,
            /* [0x080] = */ ( Byte ) LJ_CHAR.CNTRL,
            /* [0x081] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x082] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x083] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x084] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x085] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x086] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x087] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x088] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x089] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x08A] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x08B] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x08C] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x08D] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x08E] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x08F] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x090] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x091] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x092] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x093] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x094] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x095] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x096] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x097] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x098] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x099] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x09A] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x09B] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x09C] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x09D] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x09E] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x09F] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0A0] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0A1] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0A2] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0A3] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0A4] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0A5] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0A6] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0A7] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0A8] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0A9] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0AA] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0AB] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0AC] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0AD] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0AE] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0AF] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0B0] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0B1] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0B2] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0B3] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0B4] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0B5] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0B6] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0B7] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0B8] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0B9] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0BA] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0BB] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0BC] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0BD] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0BE] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0BF] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0C0] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0C1] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0C2] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0C3] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0C4] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0C5] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0C6] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0C7] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0C8] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0C9] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0CA] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0CB] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0CC] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0CD] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0CE] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0CF] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0D0] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0D1] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0D2] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0D3] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0D4] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0D5] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0D6] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0D7] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0D8] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0D9] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0DA] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0DB] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0DC] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0DD] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0DE] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0DF] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0E0] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0E1] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0E2] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0E3] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0E4] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0E5] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0E6] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0E7] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0E8] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0E9] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0EA] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0EB] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0EC] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0ED] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0EE] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0EF] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0F0] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0F1] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0F2] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0F3] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0F4] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0F5] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0F6] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0F7] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0F8] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0F9] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0FA] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0FB] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0FC] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0FD] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0FE] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x0FF] = */ ( Byte ) LJ_CHAR.IDENT,
            /* [0x100] = */ ( Byte ) LJ_CHAR.IDENT
        };

        private static Boolean IsA ( Byte c, LJ_CHAR type ) => ( CharList[c] & ( Byte ) type ) != 0;

        #region Cntrl implementation

        private static Boolean IsCntrlByte ( Byte c ) => IsA ( c, LJ_CHAR.CNTRL );

        public static Boolean IsCntrl ( Char c )
                => Encoding.UTF8.GetBytes ( new[] { c } ).All ( IsCntrlByte );

        #endregion Cntrl implementation

        #region Space implementation

        private static Boolean IsSpaceByte ( Byte c ) => IsA ( c, LJ_CHAR.SPACE );

        public static Boolean IsSpace ( Char c )
                => Encoding.UTF8.GetBytes ( new[] { c } ).All ( IsSpaceByte );

        #endregion Space implementation

        #region Punct implementation

        private static Boolean IsPunctByte ( Byte c ) => IsA ( c, LJ_CHAR.PUNCT );

        public static Boolean IsPunct ( Char c )
                => Encoding.UTF8.GetBytes ( new[] { c } ).All ( IsPunctByte );

        #endregion Punct implementation

        #region Digit implementation

        private static Boolean IsDigitByte ( Byte c ) => IsA ( c, LJ_CHAR.DIGIT );

        public static Boolean IsDigit ( Char c )
                => Encoding.UTF8.GetBytes ( new[] { c } ).All ( IsDigitByte );

        #endregion Digit implementation

        #region XDigit implementation

        private static Boolean IsXDigitByte ( Byte c ) => IsA ( c, LJ_CHAR.XDIGIT );

        public static Boolean IsXDigit ( Char c )
                => Encoding.UTF8.GetBytes ( new[] { c } ).All ( IsXDigitByte );

        #endregion XDigit implementation

        #region Upper implementation

        private static Boolean IsUpperByte ( Byte c ) => IsA ( c, LJ_CHAR.UPPER );

        public static Boolean IsUpper ( Char c )
                => Encoding.UTF8.GetBytes ( new[] { c } ).All ( IsUpperByte );

        #endregion Upper implementation

        #region Lower implementation

        private static Boolean IsLowerByte ( Byte c ) => IsA ( c, LJ_CHAR.LOWER );

        public static Boolean IsLower ( Char c )
                => Encoding.UTF8.GetBytes ( new[] { c } ).All ( IsLowerByte );

        #endregion Lower implementation

        #region Ident implementation

        private static Boolean IsIdentByte ( Byte c ) => IsA ( c, LJ_CHAR.IDENT );

        public static Boolean IsIdent ( Char c )
                => Encoding.UTF8.GetBytes ( new[] { c } ).All ( IsIdentByte );

        #endregion Ident implementation

        #region Alpha implementation

        private static Boolean IsAlphaByte ( Byte c ) => IsA ( c, LJ_CHAR.ALPHA );

        public static Boolean IsAlpha ( Char c )
                => Encoding.UTF8.GetBytes ( new[] { c } ).All ( IsAlphaByte );

        #endregion Alpha implementation

        #region AlNum implementation

        private static Boolean IsAlNumByte ( Byte c ) => IsA ( c, LJ_CHAR.ALNUM );

        public static Boolean IsAlNum ( Char c )
                => Encoding.UTF8.GetBytes ( new[] { c } ).All ( IsAlNumByte );

        #endregion AlNum implementation

        #region Graph implementation

        private static Boolean IsGraphByte ( Byte c ) => IsA ( c, LJ_CHAR.GRAPH );

        public static Boolean IsGraph ( Char c )
                => Encoding.UTF8.GetBytes ( new[] { c } ).All ( IsGraphByte );

        #endregion Graph implementation

        [DllImport ( "ucrtbase.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "atof", CharSet = CharSet.Ansi )]
        [return: MarshalAs ( UnmanagedType.R8 )]
        public static extern Double atof ( [MarshalAs ( UnmanagedType.LPStr )] String number );
    }
}
