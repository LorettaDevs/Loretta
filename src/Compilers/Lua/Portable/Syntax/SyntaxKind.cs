#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (still not sure of how to document this)

namespace Loretta.CodeAnalysis.Lua
{
    public enum SyntaxKind : ushort
    {
        None = 0,
        List = GreenNode.ListKind,

        // Trivia
        [Trivia]
        ShebangTrivia = 2,
        [Trivia]
        SingleLineCommentTrivia = 3,
        [Trivia]
        MultiLineCommentTrivia = 4,
        [Trivia]
        WhitespaceTrivia = 5,
        [Trivia]
        EndOfLineTrivia = 6,
        [Trivia]
        SkippedTokensTrivia = 7,
        [Trivia]
        [ExtraCategories("DocumentationCommentTrivia")]
        SingleLineDocumentationCommentTrivia = 8,
        [Trivia]
        [ExtraCategories("DocumentationCommentTrivia")]
        MultiLineDocumentationCommentTrivia = 9,

        // Textless Tokens
        [Token]
        EndOfFileToken = 10,
        /// <summary>
        /// Represents the <c>(</c> token.
        /// </summary>
        [Token(Text = "(")]
        OpenParenthesisToken = 11,
        /// <summary>
        /// Represents the <c>)</c> token.
        /// </summary>
        [Token(Text = ")")]
        CloseParenthesisToken = 12,
        /// <summary>
        /// Represents the <c>[</c> token.
        /// </summary>
        [Token(Text = "[")]
        OpenBracketToken = 13,
        /// <summary>
        /// Represents the <c>]</c> token.
        /// </summary>
        [Token(Text = "]")]
        CloseBracketToken = 14,
        /// <summary>
        /// Represents the <c>{</c> token.
        /// </summary>
        [Token(Text = "{")]
        OpenBraceToken = 15,
        /// <summary>
        /// Represents the <c>}</c> token.
        /// </summary>
        [Token(Text = "}")]
        CloseBraceToken = 16,
        /// <summary>
        /// Represents the <c>;</c> token.
        /// </summary>
        [Token(Text = ";")]
        SemicolonToken = 17,
        /// <summary>
        /// Represents the <c>:</c> token.
        /// </summary>
        [Token(Text = ":")]
        ColonToken = 18,
        /// <summary>
        /// Represents the <c>,</c> token.
        /// </summary>
        [Token(Text = ",")]
        CommaToken = 19,
        /// <summary>
        /// Represents the <c>#</c> token.
        /// </summary>
        [Token(Text = "#")]
        [UnaryOperator(precedence: 7, LengthExpression)]
        HashToken = 20,
        /// <summary>
        /// Represents the <c>+</c> token.
        /// </summary>
        [Token(Text = "+")]
        [BinaryOperator(precedence: 5, AddExpression)]
        PlusToken = 21,
        /// <summary>
        /// Represents the <c>+=</c> token.
        /// </summary>
        [Token(Text = "+=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", PlusToken)]
        [Property("CompoundAssignmentStatement", AddAssignmentStatement)]
        PlusEqualsToken = 22,
        /// <summary>
        /// Represents the <c>-</c> token.
        /// </summary>
        [Token(Text = "-")]
        [UnaryOperator(precedence: 7, UnaryMinusExpression), BinaryOperator(precedence: 5, SubtractExpression)]
        MinusToken = 23,
        /// <summary>
        /// Represents the <c>-=</c> token.
        /// </summary>
        [Token(Text = "-=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", MinusToken)]
        [Property("CompoundAssignmentStatement", SubtractAssignmentStatement)]
        MinusEqualsToken = 24,
        /// <summary>
        /// Represents the <c>*</c> token.
        /// </summary>
        [Token(Text = "*")]
        [BinaryOperator(precedence: 6, MultiplyExpression)]
        StarToken = 25,
        /// <summary>
        /// Represents the <c>*=</c> token.
        /// </summary>
        [Token(Text = "*=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", StarToken)]
        [Property("CompoundAssignmentStatement", MultiplyAssignmentStatement)]
        StartEqualsToken = 26,
        /// <summary>
        /// Represents the <c>/</c> token.
        /// </summary>
        [Token(Text = "/")]
        [BinaryOperator(precedence: 6, DivideExpression)]
        SlashToken = 27,
        /// <summary>
        /// Represents the <c>/=</c> token.
        /// </summary>
        [Token(Text = "/=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", SlashToken)]
        [Property("CompoundAssignmentStatement", DivideAssignmentStatement)]
        SlashEqualsToken = 28,
        /// <summary>
        /// Represents the <c>^</c> token.
        /// </summary>
        [Token(Text = "^")]
        [BinaryOperator(precedence: 8, ExponentiateExpression)]
        HatToken = 29,
        /// <summary>
        /// Represents the <c>^=</c> token.
        /// </summary>
        [Token(Text = "^=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", HatToken)]
        [Property("CompoundAssignmentStatement", ExponentiateAssignmentStatement)]
        HatEqualsToken = 30,
        /// <summary>
        /// Represents the <c>%</c> token.
        /// </summary>
        [Token(Text = "%")]
        [BinaryOperator(precedence: 6, ModuloExpression)]
        PercentToken = 31,
        /// <summary>
        /// Represents the <c>%=</c> token.
        /// </summary>
        [Token(Text = "%=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", PercentToken)]
        [Property("CompoundAssignmentStatement", ModuloAssignmentStatement)]
        PercentEqualsToken = 32,
        /// <summary>
        /// Represents the <c>.</c> token.
        /// </summary>
        [Token(Text = ".")]
        DotToken = 33,
        /// <summary>
        /// Represents the <c>..</c> token.
        /// </summary>
        [Token(Text = "..")]
        [BinaryOperator(precedence: 4, ConcatExpression)]
        DotDotToken = 34,
        /// <summary>
        /// Represents the <c>...</c> token.
        /// </summary>
        [Token(Text = "...")]
        DotDotDotToken = 35,
        /// <summary>
        /// Represents the <c>..=</c> token.
        /// </summary>
        [Token(Text = "..=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", DotDotToken)]
        [Property("CompoundAssignmentStatement", ConcatAssignmentStatement)]
        DotDotEqualsToken = 36,
        /// <summary>
        /// Represents the <c>=</c> token.
        /// </summary>
        [Token(Text = "=")]
        EqualsToken = 37,
        /// <summary>
        /// Represents the <c>==</c> token.
        /// </summary>
        [Token(Text = "==")]
        [BinaryOperator(precedence: 3, EqualsExpression)]
        EqualsEqualsToken = 38,
        // TODO: Add tilde token and unary operator
        /// <summary>
        /// Represents the <c>~=</c> token.
        /// </summary>
        [Token(Text = "~=")]
        [BinaryOperator(precedence: 3, NotEqualsExpression)]
        TildeEqualsToken = 39,
        /// <summary>
        /// Represents the <c>!</c> token.
        /// </summary>
        [Token(Text = "!")]
        [UnaryOperator(precedence: 7, LogicalNotExpression)]
        BangToken = 40,
        /// <summary>
        /// Represents the <c>!=</c> token.
        /// </summary>
        [Token(Text = "!=")]
        [BinaryOperator(precedence: 3, NotEqualsExpression)]
        BangEqualsToken = 41,
        /// <summary>
        /// Represents the <c>&lt;</c> token.
        /// </summary>
        [Token(Text = "<")]
        [BinaryOperator(precedence: 3, LessThanExpression)]
        LessThanToken = 42,
        /// <summary>
        /// Represents the <c>&lt;=</c> token.
        /// </summary>
        [Token(Text = "<=")]
        [BinaryOperator(precedence: 3, LessThanOrEqualExpression)]
        LessThanEqualsToken = 43,
        /// <summary>
        /// Represents the <c>&lt;&lt;</c> token. CURRENTLY UNSUPPORTED.
        /// </summary>
        [Token(Text = "<<")]
        // TODO: Add binary operator info
        LessThanLessThanToken = 44,
        /// <summary>
        /// Represents the <c>></c> token.
        /// </summary>
        [Token(Text = ">")]
        [BinaryOperator(precedence: 3, GreaterThanExpression)]
        GreaterThanToken = 45,
        /// <summary>
        /// Represents the <c>>=</c> token.
        /// </summary>
        [Token(Text = ">=")]
        [BinaryOperator(precedence: 3, GreaterThanOrEqualExpression)]
        GreaterThanEqualsToken = 46,
        /// <summary>
        /// Represents the <c>>></c> token. CURRENTLY UNSUPPORTED.
        /// </summary>
        [Token(Text = ">>")]
        // TODO: Add binary operator info
        GreaterThanGreaterThanToken = 47,
        /// <summary>
        /// Represents the <c>&amp;</c> token. CURRENTLY UNSUPPORTED.
        /// </summary>
        [Token(Text = "&")]
        // TODO: Add binary operator info
        AmpersandToken = 48,
        /// <summary>
        /// Represents the <c>&amp;&amp;</c> token.
        /// </summary>
        [Token(Text = "&&")]
        [BinaryOperator(precedence: 2, LogicalAndExpression)]
        AmpersandAmpersandToken = 49,
        /// <summary>
        /// Represents the <c>|</c> token. CURRENTLY UNSUPPORTED.
        /// </summary>
        [Token(Text = "|")]
        // TODO: Add binary operator info
        PipeToken = 50,
        /// <summary>
        /// Represents the <c>||</c> token.
        /// </summary>
        [Token(Text = "||")]
        [BinaryOperator(precedence: 1, LogicalOrExpression)]
        PipePipeToken = 51,
        /// <summary>
        /// Represents the <c>::</c> token.
        /// </summary>
        [Token(Text = "::")]
        ColonColonToken = 52,

        // Keywords
        /// <summary>
        /// Represents the <see langword="do"/> keyword.
        /// </summary>
        [Keyword("do")]
        DoKeyword = 500,
        /// <summary>
        /// Represents the <see langword="end"/> keyword.
        /// </summary>
        [Keyword("end")]
        EndKeyword = 501,
        /// <summary>
        /// Represents the <see langword="while"/> keyword.
        /// </summary>
        [Keyword("while")]
        WhileKeyword = 502,
        /// <summary>
        /// Represents the <see langword="repeat"/> keyword.
        /// </summary>
        [Keyword("repeat")]
        RepeatKeyword = 503,
        /// <summary>
        /// Represents the <see langword="util"/> keyword.
        /// </summary>
        [Keyword("until")]
        UntilKeyword = 504,
        /// <summary>
        /// Represents the <see langword="if"/> keyword.
        /// </summary>
        [Keyword("if")]
        IfKeyword = 505,
        /// <summary>
        /// Represents the <see langword="then"/> keyword.
        /// </summary>
        [Keyword("then")]
        ThenKeyword = 506,
        /// <summary>
        /// Represents the <see langword="elseif"/> keyword.
        /// </summary>
        [Keyword("elseif")]
        ElseIfKeyword = 507,
        /// <summary>
        /// Represents the <see langword="else"/> keyword.
        /// </summary>
        [Keyword("else")]
        ElseKeyword = 508,
        /// <summary>
        /// Represents the <see langword="for"/> keyword.
        /// </summary>
        [Keyword("for")]
        ForKeyword = 509,
        /// <summary>
        /// Represents the <see langword="in"/> keyword.
        /// </summary>
        [Keyword("in")]
        InKeyword = 510,
        /// <summary>
        /// Represents the <see langword="function"/> keyword.
        /// </summary>
        [Keyword("function")]
        FunctionKeyword = 511,
        /// <summary>
        /// Represents the <see langword="local"/> keyword.
        /// </summary>
        [Keyword("local")]
        LocalKeyword = 512,
        /// <summary>
        /// Represents the <see langword="return"/> keyword.
        /// </summary>
        [Keyword("return")]
        ReturnKeyword = 513,
        /// <summary>
        /// Represents the <see langword="break"/> keyword.
        /// </summary>
        [Keyword("break")]
        BreakKeyword = 514,
        /// <summary>
        /// Represents the <see langword="goto"/> keyword.
        /// </summary>
        [Keyword("goto")]
        GotoKeyword = 515,
        /// <summary>
        /// Represents the <see langword="continue"/> keyword.
        /// </summary>
        [Keyword("continue")]
        ContinueKeyword = 516,
        /// <summary>
        /// Represents the <see langword="and"/> keyword.
        /// </summary>
        [Keyword("and")]
        [BinaryOperator(precedence: 2, LogicalAndExpression)]
        AndKeyword = 517,
        /// <summary>
        /// Represents the <see langword="or"/> keyword.
        /// </summary>
        [Keyword("or")]
        [BinaryOperator(precedence: 1, LogicalOrExpression)]
        OrKeyword = 518,
        /// <summary>
        /// Represents the <see langword="not"/> keyword.
        /// </summary>
        [Keyword("not")]
        [UnaryOperator(precedence: 7, LogicalNotExpression)]
        NotKeyword = 519,
        /// <summary>
        /// Represents the <see langword="nil"/> keyword.
        /// </summary>
        [Keyword("nil")]
        [ExtraCategories("LiteralToken")]
        [Property("ConstantValue", null)]
        NilKeyword = 520,
        /// <summary>
        /// Represents the <see langword="true"/> keyword.
        /// </summary>
        [Keyword("true")]
        [ExtraCategories("LiteralToken")]
        [Property("ConstantValue", true)]
        TrueKeyword = 521,
        /// <summary>
        /// Represents the <see langword="false"/> keyword.
        /// </summary>
        [Keyword("false")]
        [ExtraCategories("LiteralToken")]
        [Property("ConstantValue", false)]
        FalseKeyword = 522,

        // Tokens with Text
        [Token]
        BadToken = 1000,
        [Token]
        [ExtraCategories("LiteralToken")]
        [Property("LiteralExpression", NumericalLiteralExpression)]
        NumericLiteralToken = 1001,
        [Token]
        [ExtraCategories("LiteralToken")]
        [Property("LiteralExpression", StringLiteralExpression)]
        StringLiteralToken = 1002,
        [Token]
        IdentifierToken = 1003,

        // Parameters
        NamedParameter = 2000,
        VarArgParameter = 2001,
        ParameterList = 2002,

        // Table Fields
        IdentifierKeyedTableField = 2003,
        ExpressionKeyedTableField = 2004,
        UnkeyedTableField = 2005,

        // Function Names
        SimpleFunctionName = 2006,
        MemberFunctionName = 2007,
        MethodFunctionName = 2008,

        // Function Call Arguments
        StringFunctionArgument = 2009,
        TableConstructorFunctionArgument = 2010,
        ExpressionListFunctionArgument = 2011,

        // Primary Expressions
        AnonymousFunctionExpression = 2012,
        TableConstructorExpression = 2013,
        NumericalLiteralExpression = 2014,
        StringLiteralExpression = 2015,
        TrueLiteralExpression = 2016,
        FalseLiteralExpression = 2017,
        NilLiteralExpression = 2018,
        VarArgExpression = 2019,
        [ExtraCategories("VariableExpression")]
        IdentifierName = 2020,

        // Unary Expressions
        [ExtraCategories("UnaryExpression")]
        [Property("UnaryExpressionOperatorTokenKind", MinusToken)]
        UnaryMinusExpression = 2021,
        [ExtraCategories("UnaryExpression")]
        [Property("UnaryExpressionOperatorTokenKind", NotKeyword)]
        LogicalNotExpression = 2022,
        [ExtraCategories("UnaryExpression")]
        [Property("UnaryExpressionOperatorTokenKind", HashToken)]
        LengthExpression = 2023,

        // Binary Expressions
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", PlusToken)]
        AddExpression = 2024,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", MinusToken)]
        SubtractExpression = 2025,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", StarToken)]
        MultiplyExpression = 2026,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", SlashToken)]
        DivideExpression = 2027,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", PercentToken)]
        ModuloExpression = 2028,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", DotDotToken)]
        ConcatExpression = 2029,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", EqualsEqualsToken)]
        EqualsExpression = 2030,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", TildeEqualsToken)]
        NotEqualsExpression = 2031,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", LessThanToken)]
        LessThanExpression = 2032,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", LessThanEqualsToken)]
        LessThanOrEqualExpression = 2033,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", GreaterThanToken)]
        GreaterThanExpression = 2034,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", GreaterThanEqualsToken)]
        GreaterThanOrEqualExpression = 2035,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", AndKeyword)]
        LogicalAndExpression = 2036,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", OrKeyword)]
        LogicalOrExpression = 2037,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", HatToken)]
        ExponentiateExpression = 2038,

        // Expressions
        BadExpression = 2039,
        ParenthesizedExpression = 2040,
        FunctionCallExpression = 2041,
        [ExtraCategories("VariableExpression")]
        MemberAccessExpression = 2042,
        [ExtraCategories("VariableExpression")]
        ElementAccessExpression = 2043,
        MethodCallExpression = 2044,

        // Assignment Statements
        [Property("AssignmentStatementOperatorTokenKind", EqualsToken)]
        AssignmentStatement = 2045,
        [ExtraCategories("CompoundAssignmentStatement")]
        [Property("AssignmentStatementOperatorTokenKind", PlusEqualsToken)]
        AddAssignmentStatement = 2046,
        [ExtraCategories("CompoundAssignmentStatement")]
        [Property("AssignmentStatementOperatorTokenKind", MinusEqualsToken)]
        SubtractAssignmentStatement = 2047,
        [ExtraCategories("CompoundAssignmentStatement")]
        [Property("AssignmentStatementOperatorTokenKind", StartEqualsToken)]
        MultiplyAssignmentStatement = 2048,
        [ExtraCategories("CompoundAssignmentStatement")]
        [Property("AssignmentStatementOperatorTokenKind", SlashEqualsToken)]
        DivideAssignmentStatement = 2049,
        [ExtraCategories("CompoundAssignmentStatement")]
        [Property("AssignmentStatementOperatorTokenKind", PercentEqualsToken)]
        ModuloAssignmentStatement = 2050,
        [ExtraCategories("CompoundAssignmentStatement")]
        [Property("AssignmentStatementOperatorTokenKind", DotDotEqualsToken)]
        ConcatAssignmentStatement = 2051,
        [ExtraCategories("CompoundAssignmentStatement")]
        [Property("AssignmentStatementOperatorTokenKind", HatEqualsToken)]
        ExponentiateAssignmentStatement = 2052,

        // Control Flow Statements
        NumericForStatement = 2053,
        GenericForStatement = 2054,
        WhileStatement = 2055,
        RepeatUntilStatement = 2056,
        IfStatement = 2057,
        ElseIfClause = 2058,
        ElseClause = 2059,

        // Jump Statements
        GotoStatement = 2060,
        BreakStatement = 2061,
        ReturnStatement = 2062,
        ContinueStatement = 2063,

        // Statements
        BadStatement = 2064,
        LocalVariableDeclarationStatement = 2065,
        LocalFunctionDeclarationStatement = 2066,
        FunctionDeclarationStatement = 2067,
        DoStatement = 2068,
        GotoLabelStatement = 2069,
        ExpressionStatement = 2070,
        CompilationUnit = 2071,
        StatementList = 2072
    }
}
