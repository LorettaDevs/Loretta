namespace Loretta.Lexing
{
    public enum LuaTokenType
    {
        // EOF is the first so that it is used as the default value
        EOF,

        #region Trivia

        Comment,
        LongComment,
        Whitespace,
        Shebang,

        #endregion Trivia
        #region Literals

        VarArg,
        String,
        LongString,
        Number,
        Boolean,
        Nil,

        #endregion Literals
        #region Punctuation

        LParen,
        RParen,
        LBracket,
        RBracket,
        LCurly,
        RCurly,
        Semicolon,
        Colon,
        Dot,
        Comma,

        #endregion Punctuation
        #region Others

        Keyword,
        Identifier,
        Operator,
        GotoLabelDelimiter,

        #endregion Others
    }
}
