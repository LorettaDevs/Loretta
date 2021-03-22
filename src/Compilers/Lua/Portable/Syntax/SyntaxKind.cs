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

        // Tokens
        [Token]
        BadToken = 10,
        [Token]
        EndOfFileToken = 11,
        [Token]
        [ExtraCategories("LiteralToken")]
        [Property("LiteralExpression", NumericalLiteralExpression)]
        NumericLiteralToken = 12,
        [Token]
        [ExtraCategories("LiteralToken")]
        [Property("LiteralExpression", StringLiteralExpression)]
        StringLiteralToken = 13,
        [Token]
        IdentifierToken = 14,
        /// <summary>
        /// Represents the <c>(</c> token.
        /// </summary>
        [Token(Text = "(")]
        OpenParenthesisToken = 15,
        /// <summary>
        /// Represents the <c>)</c> token.
        /// </summary>
        [Token(Text = ")")]
        CloseParenthesisToken = 16,
        /// <summary>
        /// Represents the <c>[</c> token.
        /// </summary>
        [Token(Text = "[")]
        OpenBracketToken = 17,
        /// <summary>
        /// Represents the <c>]</c> token.
        /// </summary>
        [Token(Text = "]")]
        CloseBracketToken = 18,
        /// <summary>
        /// Represents the <c>{</c> token.
        /// </summary>
        [Token(Text = "{")]
        OpenBraceToken = 19,
        /// <summary>
        /// Represents the <c>}</c> token.
        /// </summary>
        [Token(Text = "}")]
        CloseBraceToken = 20,
        /// <summary>
        /// Represents the <c>;</c> token.
        /// </summary>
        [Token(Text = ";")]
        SemicolonToken = 21,
        /// <summary>
        /// Represents the <c>:</c> token.
        /// </summary>
        [Token(Text = ":")]
        ColonToken = 22,
        /// <summary>
        /// Represents the <c>,</c> token.
        /// </summary>
        [Token(Text = ",")]
        CommaToken = 23,
        /// <summary>
        /// Represents the <c>#</c> token.
        /// </summary>
        [Token(Text = "#")]
        [UnaryOperator(precedence: 7, LengthExpression)]
        HashToken = 24,
        /// <summary>
        /// Represents the <c>+</c> token.
        /// </summary>
        [Token(Text = "+")]
        [BinaryOperator(precedence: 5, AddExpression)]
        PlusToken = 25,
        /// <summary>
        /// Represents the <c>+=</c> token.
        /// </summary>
        [Token(Text = "+=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", PlusToken)]
        [Property("CompoundAssignmentStatement", AddAssignmentStatement)]
        PlusEqualsToken = 26,
        /// <summary>
        /// Represents the <c>-</c> token.
        /// </summary>
        [Token(Text = "-")]
        [UnaryOperator(precedence: 7, UnaryMinusExpression), BinaryOperator(precedence: 5, SubtractExpression)]
        MinusToken = 27,
        /// <summary>
        /// Represents the <c>-=</c> token.
        /// </summary>
        [Token(Text = "-=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", MinusToken)]
        [Property("CompoundAssignmentStatement", SubtractAssignmentStatement)]
        MinusEqualsToken = 28,
        /// <summary>
        /// Represents the <c>*</c> token.
        /// </summary>
        [Token(Text = "*")]
        [BinaryOperator(precedence: 6, MultiplyExpression)]
        StarToken = 29,
        /// <summary>
        /// Represents the <c>*=</c> token.
        /// </summary>
        [Token(Text = "*=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", StarToken)]
        [Property("CompoundAssignmentStatement", MultiplyAssignmentStatement)]
        StartEqualsToken = 30,
        /// <summary>
        /// Represents the <c>/</c> token.
        /// </summary>
        [Token(Text = "/")]
        [BinaryOperator(precedence: 6, DivideExpression)]
        SlashToken = 31,
        /// <summary>
        /// Represents the <c>/=</c> token.
        /// </summary>
        [Token(Text = "/=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", SlashToken)]
        [Property("CompoundAssignmentStatement", DivideAssignmentStatement)]
        SlashEqualsToken = 32,
        /// <summary>
        /// Represents the <c>^</c> token.
        /// </summary>
        [Token(Text = "^")]
        [BinaryOperator(precedence: 8, ExponentiateExpression)]
        HatToken = 33,
        /// <summary>
        /// Represents the <c>^=</c> token.
        /// </summary>
        [Token(Text = "^=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", HatToken)]
        [Property("CompoundAssignmentStatement", ExponentiateAssignmentStatement)]
        HatEqualsToken = 34,
        /// <summary>
        /// Represents the <c>%</c> token.
        /// </summary>
        [Token(Text = "%")]
        [BinaryOperator(precedence: 6, ModuloExpression)]
        PercentToken = 35,
        /// <summary>
        /// Represents the <c>%=</c> token.
        /// </summary>
        [Token(Text = "%=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", PercentToken)]
        [Property("CompoundAssignmentStatement", ModuloAssignmentStatement)]
        PercentEqualsToken = 36,
        /// <summary>
        /// Represents the <c>.</c> token.
        /// </summary>
        [Token(Text = ".")]
        DotToken = 37,
        /// <summary>
        /// Represents the <c>..</c> token.
        /// </summary>
        [Token(Text = "..")]
        [BinaryOperator(precedence: 4, ConcatExpression)]
        DotDotToken = 38,
        /// <summary>
        /// Represents the <c>...</c> token.
        /// </summary>
        [Token(Text = "...")]
        DotDotDotToken = 39,
        /// <summary>
        /// Represents the <c>..=</c> token.
        /// </summary>
        [Token(Text = "..=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", DotDotToken)]
        [Property("CompoundAssignmentStatement", ConcatAssignmentStatement)]
        DotDotEqualsToken = 40,
        /// <summary>
        /// Represents the <c>=</c> token.
        /// </summary>
        [Token(Text = "=")]
        EqualsToken = 41,
        /// <summary>
        /// Represents the <c>==</c> token.
        /// </summary>
        [Token(Text = "==")]
        [BinaryOperator(precedence: 3, EqualsExpression)]
        EqualsEqualsToken = 42,
        // TODO: Add tilde token and unary operator
        /// <summary>
        /// Represents the <c>~=</c> token.
        /// </summary>
        [Token(Text = "~=")]
        [BinaryOperator(precedence: 3, NotEqualsExpression)]
        TildeEqualsToken = 43,
        /// <summary>
        /// Represents the <c>!</c> token.
        /// </summary>
        [Token(Text = "!")]
        [UnaryOperator(precedence: 7, LogicalNotExpression)]
        BangToken = 44,
        /// <summary>
        /// Represents the <c>!=</c> token.
        /// </summary>
        [Token(Text = "!=")]
        [BinaryOperator(precedence: 3, NotEqualsExpression)]
        BangEqualsToken = 45,
        /// <summary>
        /// Represents the <c>&lt;</c> token.
        /// </summary>
        [Token(Text = "<")]
        [BinaryOperator(precedence: 3, LessThanExpression)]
        LessThanToken = 46,
        /// <summary>
        /// Represents the <c>&lt;=</c> token.
        /// </summary>
        [Token(Text = "<=")]
        [BinaryOperator(precedence: 3, LessThanOrEqualExpression)]
        LessThanEqualsToken = 47,
        /// <summary>
        /// Represents the <c>&lt;&lt;</c> token. CURRENTLY UNSUPPORTED.
        /// </summary>
        [Token(Text = "<<")]
        // TODO: Add binary operator info
        LessThanLessThanToken = 48,
        /// <summary>
        /// Represents the <c>></c> token.
        /// </summary>
        [Token(Text = ">")]
        [BinaryOperator(precedence: 3, GreaterThanExpression)]
        GreaterThanToken = 49,
        /// <summary>
        /// Represents the <c>>=</c> token.
        /// </summary>
        [Token(Text = ">=")]
        [BinaryOperator(precedence: 3, GreaterThanOrEqualExpression)]
        GreaterThanEqualsToken = 50,
        /// <summary>
        /// Represents the <c>>></c> token. CURRENTLY UNSUPPORTED.
        /// </summary>
        [Token(Text = ">>")]
        // TODO: Add binary operator info
        GreaterThanGreaterThanToken = 51,
        /// <summary>
        /// Represents the <c>&amp;</c> token. CURRENTLY UNSUPPORTED.
        /// </summary>
        [Token(Text = "&")]
        // TODO: Add binary operator info
        AmpersandToken = 52,
        /// <summary>
        /// Represents the <c>&amp;&amp;</c> token.
        /// </summary>
        [Token(Text = "&&")]
        [BinaryOperator(precedence: 2, LogicalAndExpression)]
        AmpersandAmpersandToken = 53,
        /// <summary>
        /// Represents the <c>|</c> token. CURRENTLY UNSUPPORTED.
        /// </summary>
        [Token(Text = "|")]
        // TODO: Add binary operator info
        PipeToken = 54,
        /// <summary>
        /// Represents the <c>||</c> token.
        /// </summary>
        [Token(Text = "||")]
        [BinaryOperator(precedence: 1, LogicalOrExpression)]
        PipePipeToken = 55,
        /// <summary>
        /// Represents the <c>::</c> token.
        /// </summary>
        [Token(Text = "::")]
        ColonColonToken = 56,

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

        // Parameters
        NamedParameter = 1000,
        VarArgParameter = 1001,
        ParameterList = 1002,

        // Table Fields
        IdentifierKeyedTableField = 1003,
        ExpressionKeyedTableField = 1004,
        UnkeyedTableField = 1005,

        // Function Names
        SimpleFunctionName = 1006,
        MemberFunctionName = 1007,
        MethodFunctionName = 1008,

        // Function Call Arguments
        StringFunctionArgument = 1009,
        TableConstructorFunctionArgument = 1010,
        ExpressionListFunctionArgument = 1011,

        // Primary Expressions
        AnonymousFunctionExpression = 1012,
        TableConstructorExpression = 1013,
        NumericalLiteralExpression = 1014,
        StringLiteralExpression = 1015,
        TrueLiteralExpression = 1016,
        FalseLiteralExpression = 1017,
        NilLiteralExpression = 1018,
        VarArgExpression = 1019,
        [ExtraCategories("VariableExpression")]
        NameExpression = 1020,

        // Unary Expressions
        [ExtraCategories("UnaryExpression")]
        [Property("UnaryExpressionOperatorTokenKind", MinusToken)]
        UnaryMinusExpression = 1021,
        [ExtraCategories("UnaryExpression")]
        [Property("UnaryExpressionOperatorTokenKind", NotKeyword)]
        LogicalNotExpression = 1022,
        [ExtraCategories("UnaryExpression")]
        [Property("UnaryExpressionOperatorTokenKind", HashToken)]
        LengthExpression = 1023,

        // Binary Expressions
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", PlusToken)]
        AddExpression = 1024,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", MinusToken)]
        SubtractExpression = 1025,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", StarToken)]
        MultiplyExpression = 1026,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", SlashToken)]
        DivideExpression = 1027,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", PercentToken)]
        ModuloExpression = 1028,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", DotDotToken)]
        ConcatExpression = 1029,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", EqualsEqualsToken)]
        EqualsExpression = 1030,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", TildeEqualsToken)]
        NotEqualsExpression = 1031,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", LessThanToken)]
        LessThanExpression = 1032,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", LessThanEqualsToken)]
        LessThanOrEqualExpression = 1033,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", GreaterThanToken)]
        GreaterThanExpression = 1034,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", GreaterThanEqualsToken)]
        GreaterThanOrEqualExpression = 1035,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", AndKeyword)]
        LogicalAndExpression = 1036,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", OrKeyword)]
        LogicalOrExpression = 1037,
        [ExtraCategories("BinaryExpression")]
        [Property("BinaryExpressionOperatorTokenKind", HatToken)]
        ExponentiateExpression = 1038,

        // Expressions
        BadExpression = 1039,
        ParenthesizedExpression = 1040,
        FunctionCallExpression = 1041,
        [ExtraCategories("VariableExpression")]
        MemberAccessExpression = 1042,
        [ExtraCategories("VariableExpression")]
        ElementAccessExpression = 1043,
        MethodCallExpression = 1044,

        // Assignment Statements
        [Property("AssignmentStatementOperatorTokenKind", EqualsToken)]
        AssignmentStatement = 1045,
        [ExtraCategories("CompoundAssignmentStatement")]
        [Property("AssignmentStatementOperatorTokenKind", PlusEqualsToken)]
        AddAssignmentStatement = 1046,
        [ExtraCategories("CompoundAssignmentStatement")]
        [Property("AssignmentStatementOperatorTokenKind", MinusEqualsToken)]
        SubtractAssignmentStatement = 1047,
        [ExtraCategories("CompoundAssignmentStatement")]
        [Property("AssignmentStatementOperatorTokenKind", StartEqualsToken)]
        MultiplyAssignmentStatement = 1048,
        [ExtraCategories("CompoundAssignmentStatement")]
        [Property("AssignmentStatementOperatorTokenKind", SlashEqualsToken)]
        DivideAssignmentStatement = 1049,
        [ExtraCategories("CompoundAssignmentStatement")]
        [Property("AssignmentStatementOperatorTokenKind", PercentEqualsToken)]
        ModuloAssignmentStatement = 1050,
        [ExtraCategories("CompoundAssignmentStatement")]
        [Property("AssignmentStatementOperatorTokenKind", DotDotEqualsToken)]
        ConcatAssignmentStatement = 1051,
        [ExtraCategories("CompoundAssignmentStatement")]
        [Property("AssignmentStatementOperatorTokenKind", HatEqualsToken)]
        ExponentiateAssignmentStatement = 1052,

        // Control Flow Statements
        NumericForStatement = 1053,
        GenericForStatement = 1054,
        WhileStatement = 1055,
        RepeatUntilStatement = 1056,
        IfStatement = 1057,
        ElseIfClause = 1058,
        ElseClause = 1059,

        // Jump Statements
        GotoStatement = 1060,
        BreakStatement = 1061,
        ReturnStatement = 1062,
        ContinueStatement = 1063,

        // Statements
        BadStatement = 1064,
        LocalVariableDeclarationStatement = 1065,
        LocalFunctionDeclarationStatement = 1066,
        FunctionDeclarationStatement = 1067,
        DoStatement = 1068,
        GotoLabelStatement = 1069,
        ExpressionStatement = 1070,
        CompilationUnit = 1071,
        StatementList = 1072
    }
}
