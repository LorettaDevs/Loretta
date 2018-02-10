using System;
using System.Collections.Generic;
using GParse.Lexing.Settings;

namespace Loretta.Common
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
        public IDictionary<String, Int32> BinaryOperators;

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
                { "not", 0 },
                { "#", 0 },
                { "-", 0 }
            },
            BinaryOperators = new Dictionary<String, Int32>
            {
                { "or", 0 },
                { "and", 1 },
                { "<", 2 },
                { ">", 2 },
                { "<=", 2 },
                { ">=", 2 },
                { "~=", 2 },
                { "==", 2 },
                { "..", 3 },
                { "+", 4 },
                { "-", 4 },
                { "*", 5 },
                { "/", 5 },
                { "%", 5 }
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
        }
    }
}
