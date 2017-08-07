namespace LuaParse.Tokens
{
    public enum TokenType
    {
        LParen,     // (
        RParen,     // )
        RBracket,   // [
        LBracket,   // ]
        RCurly,     // {
        LCurly,     // }
        Colon,      // :
        Semicolon,  // ;
        Period,     // .
        Comma,      // ,
        String,     // "x", 'x', [[x]], [=[x]=]
        Number,     // 1e+5, 1.e+5, 1.5, 1E5
        Comment,    // --[[x]], --[=[x]=], --x, /*x*/, //x
        Do,         // do
        In,         // in
        If,         // if
        Nil,        // nil
        Not,        // not
        End,        // end
        For,        // for
        Else,       // else
        Then,       // then
        While,      // while
        Local,      // local
        Break,      // break
        Until,      // until
        Repeat,     // repeat
        ElseIf,     // elseif
        Return,     // return
        Function,   // function
        BinaryOp,   // +, -, /, *, ^, and, or, &&, ||, ~=, !=, ==
        UnaryOp,    // -, +, !, not
        Identifier, // Something that doesn't fits in the rest and may be a var nam
        Equal,      // =
        PossibleValue = RParen | Number | String | Identifier | Nil
    }
}
