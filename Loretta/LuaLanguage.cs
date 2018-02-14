using System;
using System.Collections.Generic;
using System.Text;
using GParse.Lexing.Settings;

namespace Loretta
{
    /// <summary>
    /// Allows you to change the way the parser works, adding new
    /// operators and keywords to the language also allows to
    /// modify string literals, character escaping modes and
    /// number literals. Basically creating a new language based
    /// on lua
    /// </summary>
    public class LuaLanguage
    {
        /// <summary>
        /// Name of the sub-language
        /// </summary>
        public readonly String Name;

        /// <summary>
        /// Version of the sub-language
        /// </summary>
        public readonly Version Version;

        /// <summary>
        /// Character settings
        /// </summary>
        public CharLexSettings CharSettings;

        /// <summary>
        /// String settings (char settings will be from <see cref="CharSettings" />)
        /// </summary>
        public StringLexSettings StringSettings;

        /// <summary>
        /// Unary operators
        /// </summary>
        public IDictionary<String, Int32> UnaryOperators;

        /// <summary>
        /// Binary operators
        /// </summary>
        public IDictionary<String, Double[]> BinaryOperators;

        /// <summary>
        /// Extra keywords used in non-standard lua (currently
        /// requires you to subclass the lua parser to implement
        /// your own constructs.)
        /// </summary>
        public IList<String> ExtraKeywords;

        public LuaLanguage ( String Name, Version Version )
        {
            this.Name = Name;
            this.Version = Version;
        }

        public LuaLanguage ( String Name, Version Version, LuaLanguage Base )
        {
            this.Name = Name;
            this.Version = Version;

            this.StringSettings = Base.StringSettings;
            this.CharSettings = Base.CharSettings;
            this.UnaryOperators = Base.UnaryOperators;
            this.BinaryOperators = Base.BinaryOperators;
            this.ExtraKeywords = Base.ExtraKeywords;
        }

        public static readonly LuaLanguage Lua51 = new LuaLanguage ( "Lua", new Version ( 5, 1 ) )
        {
            // Straight from http://www.lua.org/manual/5.1/manual.html#2.5.6
            UnaryOperators = new Dictionary<String, Int32>
            {
                { "not", 8 },
                { "#", 8 },
                { "-", 8 }
            },
            BinaryOperators = new Dictionary<String, Double[]>
            {
                { "+", new[] { 6d, 6 } },
                { "-", new[] { 6d, 6 } },
                { "<<", new[] { 5d, 5 } },
                { ">>", new[] { 5d, 5 } },
                { "|", new[] { 4.5, 4.5 } },
                { "&", new[] { 4.25, 4.25 } },
                { "~", new[] { 4d, 4 } },
                { "%", new[] { 7d, 7 } },
                { "/", new[] { 7d, 7 } },
                { "*", new[] { 7d, 7 } },
                { "^", new[] { 10d, 9 } },
                { "..", new[] { 5d, 4 } },
                { "==", new[] { 3d, 3 } },
                { "<", new[] { 3d, 3 } },
                { "<=", new[] { 3d, 3 } },
                { "~=", new[] { 3d, 3 } },
                { "!=", new[] { 3d, 3 } },
                { ">", new[] { 3d, 3 } },
                { ">=", new[] { 3d, 3 } },
                { "and", new[] { 2d, 2 } },
                { "or", new[] { 1d, 1 } },
            },
            CharSettings = new CharLexSettings
            {
                // Non-existent
                BinaryEscapePrefix = null,

                // Also non-existent
                OctalEscapePrefix = null,

                // \0 -> \255
                DecimalEscapePrefix = "\\",
                DecimalEscapeMaxLengh = 255,

                // \x0 -> \xFF
                HexadecimalEscapePrefix = "\\x",
                HexadecimalEscapeMaxLengh = 2
            },
            StringSettings = new StringLexSettings
            {
                // Char settings above
                NewlineEscape = "\\"
            }
        };

        public static readonly LuaLanguage GLua;

        public static readonly LuaLanguage LuaJIT = new LuaLanguage ( "LuaJIT", new Version ( 2, 0, 1 ), Lua51 );

        static LuaLanguage ( )
        {
            Lua51.CharSettings
                .RegisterEscapeConstant ( @"\0", '\0' )
                .RegisterEscapeConstant ( @"\a", '\a' )
                .RegisterEscapeConstant ( @"\b", '\b' )
                .RegisterEscapeConstant ( @"\f", '\f' )
                .RegisterEscapeConstant ( @"\n", '\n' )
                .RegisterEscapeConstant ( @"\r", '\r' )
                .RegisterEscapeConstant ( @"\t", '\t' )
                .RegisterEscapeConstant ( @"\v", '\v' )
                .RegisterEscapeConstant ( @"\'", '\'' )
                .RegisterEscapeConstant ( "\\\"", '"' )
                .RegisterEscapeConstant ( @"\\", '\\' );

            GLua = new LuaLanguage ( "GLua", new Version ( 1, 0 ), Lua51 );
        }
    }
}
