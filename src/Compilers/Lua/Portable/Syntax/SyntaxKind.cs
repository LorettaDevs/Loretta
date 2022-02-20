#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (still not sure of how to document this)

using System.ComponentModel;

namespace Loretta.CodeAnalysis.Lua
{
    internal static class SyntaxKindCategory
    {
        /// <summary>
        /// Documentation comment kinds.
        /// </summary>
        public const string DocumentationCommentTrivia = nameof(DocumentationCommentTrivia);
        /// <summary>
        /// Tokens that can be used as the assignment operator in compound assignments.
        /// </summary>
        public const string CompoundAssignmentOperatorToken = nameof(CompoundAssignmentOperatorToken);
        /// <summary>
        /// Tokens that result in literal expressions.
        /// </summary>
        public const string LiteralToken = nameof(LiteralToken);
        /// <summary>
        /// Kinds that belong to <see cref="Syntax.VariableExpressionSyntax"/>.
        /// </summary>
        public const string VariableExpression = nameof(VariableExpression);
        /// <summary>
        /// Kinds that belong to <see cref="Syntax.UnaryExpressionSyntax"/>.
        /// </summary>
        public const string UnaryExpression = nameof(UnaryExpression);
        /// <summary>
        /// Kinds that belong to <see cref="Syntax.BinaryExpressionSyntax"/>.
        /// </summary>
        public const string BinaryExpression = nameof(BinaryExpression);
        /// <summary>
        /// Kinds that belong to function expressions or declarations.
        /// </summary>
        public const string FunctionExpressionOrDeclaration = nameof(FunctionExpressionOrDeclaration);
        /// <summary>
        /// Kinds that belong to compound assignment statements.
        /// </summary>
        public const string CompoundAssignmentStatement = nameof(CompoundAssignmentStatement);
    }

    internal static class SyntaxKindProperty
    {
        /// <summary>
        /// The operator token that is the operation done by the compound assignment token/expression kind that has this property.
        /// </summary>
        public const string CompoundAssignmentOperator = nameof(CompoundAssignmentOperator);
        /// <summary>
        /// The compound assignment expression that the operator token that contains this property results into.
        /// </summary>
        public const string CompoundAssignmentStatement = nameof(CompoundAssignmentStatement);
        /// <summary>
        /// The kind of the expression the literal token that has this property results into.
        /// </summary>
        public const string LiteralExpression = nameof(LiteralExpression);
        /// <summary>
        /// The value the literal that has this property results into.
        /// </summary>
        public const string ConstantValue = nameof(ConstantValue);
        /// <summary>
        /// The kind of the operator token that results in the expression that contains this property.
        /// </summary>
        public const string OperatorTokenKind = nameof(OperatorTokenKind);
    }

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
        //[Trivia]
        //[ExtraCategories(SyntaxKindCategory.DocumentationCommentTrivia)]
        //SingleLineDocumentationCommentTrivia = 8,
        //[Trivia]
        //[ExtraCategories(SyntaxKindCategory.DocumentationCommentTrivia)]
        //MultiLineDocumentationCommentTrivia = 9,

        // Fixed-text Tokens
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
        [UnaryOperator(precedence: 12, LengthExpression)]
        HashToken = 20,
        /// <summary>
        /// Represents the <c>+</c> token.
        /// </summary>
        [Token(Text = "+")]
        [BinaryOperator(precedence: 10, AddExpression)]
        PlusToken = 21,
        /// <summary>
        /// Represents the <c>+=</c> token.
        /// </summary>
        [Token(Text = "+=")]
        [ExtraCategories(SyntaxKindCategory.CompoundAssignmentOperatorToken)]
        [Property(SyntaxKindProperty.CompoundAssignmentOperator, PlusToken)]
        [Property(SyntaxKindProperty.CompoundAssignmentStatement, AddAssignmentStatement)]
        PlusEqualsToken = 22,
        /// <summary>
        /// Represents the <c>-</c> token.
        /// </summary>
        [Token(Text = "-")]
        [UnaryOperator(precedence: 12, UnaryMinusExpression), BinaryOperator(precedence: 10, SubtractExpression)]
        MinusToken = 23,
        /// <summary>
        /// Represents the <c>-=</c> token.
        /// </summary>
        [Token(Text = "-=")]
        [ExtraCategories(SyntaxKindCategory.CompoundAssignmentOperatorToken)]
        [Property(SyntaxKindProperty.CompoundAssignmentOperator, MinusToken)]
        [Property(SyntaxKindProperty.CompoundAssignmentStatement, SubtractAssignmentStatement)]
        MinusEqualsToken = 24,
        /// <summary>
        /// Represents the <c>*</c> token.
        /// </summary>
        [Token(Text = "*")]
        [BinaryOperator(precedence: 11, MultiplyExpression)]
        StarToken = 25,
        /// <summary>
        /// Represents the <c>*=</c> token.
        /// </summary>
        [Token(Text = "*=")]
        [ExtraCategories(SyntaxKindCategory.CompoundAssignmentOperatorToken)]
        [Property(SyntaxKindProperty.CompoundAssignmentOperator, StarToken)]
        [Property(SyntaxKindProperty.CompoundAssignmentStatement, MultiplyAssignmentStatement)]
        StarEqualsToken = 26,
        /// <summary>
        /// Represents the <c>/</c> token.
        /// </summary>
        [Token(Text = "/")]
        [BinaryOperator(precedence: 11, DivideExpression)]
        SlashToken = 27,
        /// <summary>
        /// Represents the <c>/=</c> token.
        /// </summary>
        [Token(Text = "/=")]
        [ExtraCategories(SyntaxKindCategory.CompoundAssignmentOperatorToken)]
        [Property(SyntaxKindProperty.CompoundAssignmentOperator, SlashToken)]
        [Property(SyntaxKindProperty.CompoundAssignmentStatement, DivideAssignmentStatement)]
        SlashEqualsToken = 28,
        /// <summary>
        /// Represents the <c>^</c> token.
        /// </summary>
        [Token(Text = "^")]
        [BinaryOperator(precedence: 14, ExponentiateExpression)]
        HatToken = 29,
        /// <summary>
        /// Represents the <c>^=</c> token.
        /// </summary>
        [Token(Text = "^=")]
        [ExtraCategories(SyntaxKindCategory.CompoundAssignmentOperatorToken)]
        [Property(SyntaxKindProperty.CompoundAssignmentOperator, HatToken)]
        [Property(SyntaxKindProperty.CompoundAssignmentStatement, ExponentiateAssignmentStatement)]
        HatEqualsToken = 30,
        /// <summary>
        /// Represents the <c>%</c> token.
        /// </summary>
        [Token(Text = "%")]
        [BinaryOperator(precedence: 11, ModuloExpression)]
        PercentToken = 31,
        /// <summary>
        /// Represents the <c>%=</c> token.
        /// </summary>
        [Token(Text = "%=")]
        [ExtraCategories(SyntaxKindCategory.CompoundAssignmentOperatorToken)]
        [Property(SyntaxKindProperty.CompoundAssignmentOperator, PercentToken)]
        [Property(SyntaxKindProperty.CompoundAssignmentStatement, ModuloAssignmentStatement)]
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
        [BinaryOperator(precedence: 9, ConcatExpression)]
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
        [ExtraCategories(SyntaxKindCategory.CompoundAssignmentOperatorToken)]
        [Property(SyntaxKindProperty.CompoundAssignmentOperator, DotDotToken)]
        [Property(SyntaxKindProperty.CompoundAssignmentStatement, ConcatAssignmentStatement)]
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
        [UnaryOperator(precedence: 12, LogicalNotExpression)]
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
        /// Represents the <c>&lt;&lt;</c> token.
        /// </summary>
        [Token(Text = "<<")]
        [BinaryOperator(precedence: 7, LeftShiftExpression)]
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
        /// Represents the <c>>></c> token.
        /// </summary>
        [Token(Text = ">>")]
        [BinaryOperator(precedence: 7, RightShiftExpression)]
        GreaterThanGreaterThanToken = 47,
        /// <summary>
        /// Represents the <c>&amp;</c> token.
        /// </summary>
        [Token(Text = "&")]
        [BinaryOperator(precedence: 6, BitwiseAndExpression)]
        AmpersandToken = 48,
        /// <summary>
        /// Represents the <c>&amp;&amp;</c> token.
        /// </summary>
        [Token(Text = "&&")]
        [BinaryOperator(precedence: 2, LogicalAndExpression)]
        AmpersandAmpersandToken = 49,
        /// <summary>
        /// Represents the <c>|</c> token.
        /// </summary>
        [Token(Text = "|")]
        [BinaryOperator(precedence: 4, BitwiseOrExpression)]
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
        /// <summary>
        /// Represents the <c>~</c> token.
        /// </summary>
        [Token(Text = "~")]
        [UnaryOperator(precedence: 12, BitwiseNotExpression), BinaryOperator(precedence: 5, ExclusiveOrExpression)]
        TildeToken = 53,
        /// <summary>
        /// Represents the <c>?</c> token.
        /// </summary>
        [Token(Text = "?")]
        QuestionToken = 54,

        // Big gap 53-500 (insert new fixed-text tokens here)

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
        [UnaryOperator(precedence: 12, LogicalNotExpression)]
        NotKeyword = 519,
        /// <summary>
        /// Represents the <see langword="nil"/> keyword.
        /// </summary>
        [Keyword("nil")]
        [ExtraCategories(SyntaxKindCategory.LiteralToken)]
        [Property(SyntaxKindProperty.LiteralExpression, NilLiteralExpression)]
        NilKeyword = 520,
        /// <summary>
        /// Represents the <see langword="true"/> keyword.
        /// </summary>
        [Keyword("true")]
        [ExtraCategories(SyntaxKindCategory.LiteralToken)]
        [Property(SyntaxKindProperty.LiteralExpression, TrueLiteralExpression)]
        TrueKeyword = 521,
        /// <summary>
        /// Represents the <see langword="false"/> keyword.
        /// </summary>
        [Keyword("false")]
        [ExtraCategories(SyntaxKindCategory.LiteralToken)]
        [Property(SyntaxKindProperty.LiteralExpression, FalseLiteralExpression)]
        FalseKeyword = 522,
        /// <summary>
        /// Represents the <see langword="type"/> keyword.
        /// </summary>
        [Keyword("type")]
        TypeKeyword = 523,
        /// <summary>
        /// Represents the <see langword="export"/> keyword.
        /// </summary>
        [Keyword("export")]
        ExportKeyword = 524,

        // Big gap 522-1000 (insert new keywords here)

        // Tokens with Text
        [Token]
        BadToken = 1000,
        [Token]
        [ExtraCategories(SyntaxKindCategory.LiteralToken)]
        [Property(SyntaxKindProperty.LiteralExpression, NumericalLiteralExpression)]
        NumericLiteralToken = 1001,
        [Token]
        [ExtraCategories(SyntaxKindCategory.LiteralToken)]
        [Property(SyntaxKindProperty.LiteralExpression, StringLiteralExpression)]
        StringLiteralToken = 1002,
        [Token]
        IdentifierToken = 1003,
        [Token]
        [ExtraCategories(SyntaxKindCategory.LiteralToken)]
        [Property(SyntaxKindProperty.LiteralExpression, HashStringLiteralExpression)]
        HashStringLiteralToken = 1004,

        // Big gap 1005-2000 (insert new tokens with text here)

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

        EqualsValuesClause = 2083,
        VariableAttribute = 2084,
        LocalDeclarationName = 2085,

        // Primary Expressions
        [ExtraCategories(SyntaxKindCategory.FunctionExpressionOrDeclaration)]
        AnonymousFunctionExpression = 2012,
        TableConstructorExpression = 2013,
        NumericalLiteralExpression = 2014,
        StringLiteralExpression = 2015,
        TrueLiteralExpression = 2016,
        FalseLiteralExpression = 2017,
        NilLiteralExpression = 2018,
        VarArgExpression = 2019,
        [ExtraCategories(SyntaxKindCategory.VariableExpression)]
        IdentifierName = 2020,
        IfExpression = 2080,
        ElseIfExpressionClause = 2081,
        HashStringLiteralExpression = 2082,

        // Unary Expressions
        [ExtraCategories(SyntaxKindCategory.UnaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, MinusToken)]
        UnaryMinusExpression = 2021,
        [ExtraCategories(SyntaxKindCategory.UnaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, NotKeyword)]
        LogicalNotExpression = 2022,
        [ExtraCategories(SyntaxKindCategory.UnaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, HashToken)]
        LengthExpression = 2023,
        [ExtraCategories(SyntaxKindCategory.UnaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, TildeToken)]
        BitwiseNotExpression = 2074,

        // Binary Expressions
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, PlusToken)]
        AddExpression = 2024,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, MinusToken)]
        SubtractExpression = 2025,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, StarToken)]
        MultiplyExpression = 2026,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, SlashToken)]
        DivideExpression = 2027,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, PercentToken)]
        ModuloExpression = 2028,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, DotDotToken)]
        ConcatExpression = 2029,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, EqualsEqualsToken)]
        EqualsExpression = 2030,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, TildeEqualsToken)]
        NotEqualsExpression = 2031,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, LessThanToken)]
        LessThanExpression = 2032,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, LessThanEqualsToken)]
        LessThanOrEqualExpression = 2033,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, GreaterThanToken)]
        GreaterThanExpression = 2034,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, GreaterThanEqualsToken)]
        GreaterThanOrEqualExpression = 2035,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, AndKeyword)]
        LogicalAndExpression = 2036,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, OrKeyword)]
        LogicalOrExpression = 2037,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, HatToken)]
        ExponentiateExpression = 2038,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, PipeToken)]
        BitwiseOrExpression = 2075,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, AmpersandToken)]
        BitwiseAndExpression = 2076,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, GreaterThanGreaterThanToken)]
        RightShiftExpression = 2077,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, LessThanLessThanToken)]
        LeftShiftExpression = 2078,
        [ExtraCategories(SyntaxKindCategory.BinaryExpression)]
        [Property(SyntaxKindProperty.OperatorTokenKind, TildeToken)]
        ExclusiveOrExpression = 2079,

        // Expressions
        ParenthesizedExpression = 2040,
        FunctionCallExpression = 2041,
        [ExtraCategories(SyntaxKindCategory.VariableExpression)]
        MemberAccessExpression = 2042,
        [ExtraCategories(SyntaxKindCategory.VariableExpression)]
        ElementAccessExpression = 2043,
        MethodCallExpression = 2044,

        // Assignment Statements
        [Property(SyntaxKindProperty.OperatorTokenKind, EqualsToken)]
        AssignmentStatement = 2045,
        [ExtraCategories(SyntaxKindCategory.CompoundAssignmentStatement)]
        [Property(SyntaxKindProperty.OperatorTokenKind, PlusEqualsToken)]
        AddAssignmentStatement = 2046,
        [ExtraCategories(SyntaxKindCategory.CompoundAssignmentStatement)]
        [Property(SyntaxKindProperty.OperatorTokenKind, MinusEqualsToken)]
        SubtractAssignmentStatement = 2047,
        [ExtraCategories(SyntaxKindCategory.CompoundAssignmentStatement)]
        [Property(SyntaxKindProperty.OperatorTokenKind, StarEqualsToken)]
        MultiplyAssignmentStatement = 2048,
        [ExtraCategories(SyntaxKindCategory.CompoundAssignmentStatement)]
        [Property(SyntaxKindProperty.OperatorTokenKind, SlashEqualsToken)]
        DivideAssignmentStatement = 2049,
        [ExtraCategories(SyntaxKindCategory.CompoundAssignmentStatement)]
        [Property(SyntaxKindProperty.OperatorTokenKind, PercentEqualsToken)]
        ModuloAssignmentStatement = 2050,
        [ExtraCategories(SyntaxKindCategory.CompoundAssignmentStatement)]
        [Property(SyntaxKindProperty.OperatorTokenKind, DotDotEqualsToken)]
        ConcatAssignmentStatement = 2051,
        [ExtraCategories(SyntaxKindCategory.CompoundAssignmentStatement)]
        [Property(SyntaxKindProperty.OperatorTokenKind, HatEqualsToken)]
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
        LocalVariableDeclarationStatement = 2065,
        [ExtraCategories(SyntaxKindCategory.FunctionExpressionOrDeclaration)]
        LocalFunctionDeclarationStatement = 2066,
        [ExtraCategories(SyntaxKindCategory.FunctionExpressionOrDeclaration)]
        FunctionDeclarationStatement = 2067,
        DoStatement = 2068,
        GotoLabelStatement = 2069,
        ExpressionStatement = 2070,
        StatementList = 2072,
        EmptyStatement = 2073,

        // Small gap 2074-2085

        // Types
        TypeBinding = 2086,
        NullableType = 2087,
        TableType = 2088,
        TupleType = 2089,
        VarargType = 2090,
        TypeCast = 2091,
        TypeUnion = 2092,
        TypeIntersection = 2093,
        Generic = 2094,
        Type = 2095,
        TableElementType = 2096,
        TableTypeProperty = 2097,
        TableTypeIndexer = 2098,
        UnkeyedTableType = 2099,
        TypeParameterListSyntax = 2100,
        TypeArgumentListSyntax = 2101,

        // Big gap 2105-3001 (insert new nodes here)
        

        // Other types of nodes
        CompilationUnit = 3001,

        // BACKWARDS COMPAT:
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use StarEqualsToken (without a 't' before Equals) instead", true)]
        StartEqualsToken = StarEqualsToken,
    }
}
