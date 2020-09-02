using System;
using GParse.Lexing;

namespace Loretta.Lexing
{
    /// <summary>
    /// The type of the lua token.
    /// </summary>
    public enum LuaTokenType
    {
        /// <summary>
        /// End of file.
        /// </summary>
        // EOF is the first so that it is used as the default value
        EOF,

        #region Trivia

        /// <summary>
        /// A single line comment (--...).
        /// </summary>
        Comment,

        /// <summary>
        /// A long comment (--[[...]]).
        /// </summary>
        LongComment,

        /// <summary>
        /// Any whitespace.
        /// </summary>
        Whitespace,

        /// <summary>
        /// A shebang (#!...)
        /// </summary>
        Shebang,

        #endregion Trivia

        #region Literals

        /// <summary>
        /// A vararg (...).
        /// </summary>
        VarArg,

        /// <summary>
        /// A quoted string.
        /// </summary>
        String,

        /// <summary>
        /// A long/literal string literal.
        /// </summary>
        LongString,

        /// <summary>
        /// A number literal.
        /// </summary>
        Number,

        /// <summary>
        /// A boolean literal.
        /// </summary>
        Boolean,

        /// <summary>
        /// A nil literal.
        /// </summary>
        Nil,

        #endregion Literals

        #region Punctuation

        /// <summary>
        /// A left parenthesis '('.
        /// </summary>
        LParen,

        /// <summary>
        /// A right parenthesis ')'.
        /// </summary>
        RParen,

        /// <summary>
        /// A left bracket '['.
        /// </summary>
        LBracket,

        /// <summary>
        /// A right bracket ']'.
        /// </summary>
        RBracket,

        /// <summary>
        /// A left curly bracket '{'.
        /// </summary>
        LCurly,

        /// <summary>
        /// A right curly bracket '}'.
        /// </summary>
        RCurly,

        /// <summary>
        /// A semicolon ';'.
        /// </summary>
        Semicolon,

        /// <summary>
        /// A colon ':'.
        /// </summary>
        Colon,

        /// <summary>
        /// A dot '.'.
        /// </summary>
        Dot,

        /// <summary>
        /// A comma ','.
        /// </summary>
        Comma,

        #endregion Punctuation

        #region Others

        /// <summary>
        /// A keyword.
        /// </summary>
        Keyword,

        /// <summary>
        /// An identifier.
        /// </summary>
        Identifier,

        /// <summary>
        /// An operator.
        /// </summary>
        Operator,

        /// <summary>
        /// A goto label delimiter '::'.
        /// </summary>
        GotoLabelDelimiter,

        #endregion Others
    }

    /// <summary>
    /// Extension methods for <see cref="LuaTokenType" />.
    /// </summary>
    public static class LuaTokenTypeExtensions
    {
        /// <summary>
        /// Checks whether a tokens <see cref="Token{LuaTokenType}.Raw" /> value can be used in an
        /// string message.
        /// </summary>
        /// <param name="tokenType">The token type.</param>
        /// <returns>Whether the raw value of the token can be used in a string.</returns>
        public static Boolean CanUseRawInError ( this LuaTokenType tokenType ) =>
            tokenType == LuaTokenType.Identifier
            || tokenType == LuaTokenType.Keyword
            || tokenType == LuaTokenType.Number
            || tokenType == LuaTokenType.Operator;
    }
}