#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (still not sure of how to document this)

namespace Loretta.CodeAnalysis.Lua
{
    public enum SyntaxKind : ushort
    {
        None,
        List,

        // Trivia
        [Trivia]
        ShebangTrivia,
        [Trivia]
        SingleLineCommentTrivia,
        [Trivia]
        MultiLineCommentTrivia,
        [Trivia]
        WhitespaceTrivia,
        [Trivia]
        EndOfLineTrivia,
        [Trivia]
        SkippedTokensTrivia,
        [Trivia]
        [ExtraCategories("DocumentationCommentTrivia")]
        SingleLineDocumentationCommentTrivia,
        [Trivia]
        [ExtraCategories("DocumentationCommentTrivia")]
        MultiLineDocumentationCommentTrivia,

        // Tokens
        [Token]
        BadToken,
        [Token]
        EndOfFileToken,
        [Token]
        [ExtraCategories("LiteralToken")]
        [Property("LiteralExpression", NumericalLiteralExpression)]
        NumericLiteralToken,
        [Token]
        [ExtraCategories("LiteralToken")]
        [Property("LiteralExpression", StringLiteralExpression)]
        StringLiteralToken,
        [Token]
        IdentifierToken,
        [Token(Text = "(")]
        OpenParenthesisToken,
        [Token(Text = ")")]
        CloseParenthesisToken,
        [Token(Text = "[")]
        OpenBracketToken,
        [Token(Text = "]")]
        CloseBracketToken,
        [Token(Text = "{")]
        OpenBraceToken,
        [Token(Text = "}")]
        CloseBraceToken,
        [Token(Text = ";")]
        SemicolonToken,
        [Token(Text = ":")]
        ColonToken,
        [Token(Text = ",")]
        CommaToken,
        [Token(Text = "#")]
        [UnaryOperator(precedence: 7, LengthExpression)]
        HashToken,
        [Token(Text = "+")]
        [BinaryOperator(precedence: 5, AddExpression)]
        PlusToken,
        [Token(Text = "+=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", PlusToken)]
        [Property("CompoundAssignmentStatement", AddAssignmentStatement)]
        PlusEqualsToken,
        [Token(Text = "-")]
        [UnaryOperator(precedence: 7, UnaryMinusExpression), BinaryOperator(precedence: 5, SubtractExpression)]
        MinusToken,
        [Token(Text = "-=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", MinusToken)]
        [Property("CompoundAssignmentStatement", SubtractAssignmentStatement)]
        MinusEqualsToken,
        [Token(Text = "*")]
        [BinaryOperator(precedence: 6, MultiplyExpression)]
        StarToken,
        [Token(Text = "*=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", StarToken)]
        [Property("CompoundAssignmentStatement", MultiplyAssignmentStatement)]
        StartEqualsToken,
        [Token(Text = "/")]
        [BinaryOperator(precedence: 6, DivideExpression)]
        SlashToken,
        [Token(Text = "/=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", SlashToken)]
        [Property("CompoundAssignmentStatement", DivideAssignmentStatement)]
        SlashEqualsToken,
        [Token(Text = "^")]
        [BinaryOperator(precedence: 8, ExponentiateExpression)]
        HatToken,
        [Token(Text = "^=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", HatToken)]
        [Property("CompoundAssignmentStatement", ExponentiateAssignmentStatement)]
        HatEqualsToken,
        [Token(Text = "%")]
        [BinaryOperator(precedence: 6, ModuloExpression)]
        PercentToken,
        [Token(Text = "%=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", PercentToken)]
        [Property("CompoundAssignmentStatement", ModuloAssignmentStatement)]
        PercentEqualsToken,
        [Token(Text = ".")]
        DotToken,
        [Token(Text = "..")]
        [BinaryOperator(precedence: 4, ConcatExpression)]
        DotDotToken,
        [Token(Text = "...")]
        DotDotDotToken,
        [Token(Text = "..=")]
        [ExtraCategories("CompoundAssignmentOperatorToken")]
        [Property("CompoundAssignmentOperator", DotDotToken)]
        [Property("CompoundAssignmentStatement", ConcatAssignmentStatement)]
        DotDotEqualsToken,
        [Token(Text = "=")]
        EqualsToken,
        [Token(Text = "==")]
        [BinaryOperator(precedence: 3, EqualsExpression)]
        EqualsEqualsToken,
        // TODO: Add tilde token and unary operator
        [Token(Text = "~=")]
        [BinaryOperator(precedence: 3, NotEqualsExpression)]
        TildeEqualsToken,
        [Token(Text = "!")]
        [UnaryOperator(precedence: 7, LogicalNotExpression)]
        BangToken,
        [Token(Text = "!=")]
        [BinaryOperator(precedence: 3, NotEqualsExpression)]
        BangEqualsToken,
        [Token(Text = "<")]
        [BinaryOperator(precedence: 3, LessThanExpression)]
        LessThanToken,
        [Token(Text = "<=")]
        [BinaryOperator(precedence: 3, LessThanOrEqualExpression)]
        LessThanEqualsToken,
        [Token(Text = "<<")]
        // TODO: Add binary operator info
        LessThanLessThanToken,
        [Token(Text = ">")]
        [BinaryOperator(precedence: 3, GreaterThanExpression)]
        GreaterThanToken,
        [Token(Text = ">=")]
        [BinaryOperator(precedence: 3, GreaterThanOrEqualExpression)]
        GreaterThanEqualsToken,
        [Token(Text = ">>")]
        // TODO: Add binary operator info
        GreaterThanGreaterThanToken,
        [Token(Text = "&")]
        // TODO: Add binary operator info
        AmpersandToken,
        [Token(Text = "&&")]
        [BinaryOperator(precedence: 2, LogicalAndExpression)]
        AmpersandAmpersandToken,
        [Token(Text = "|")]
        // TODO: Add binary operator info
        PipeToken,
        [Token(Text = "||")]
        [BinaryOperator(precedence: 1, LogicalOrExpression)]
        PipePipeToken,
        [Token(Text = "::")]
        ColonColonToken,

        // Keywords
        [Keyword("do")]
        DoKeyword,
        [Keyword("end")]
        EndKeyword,
        [Keyword("while")]
        WhileKeyword,
        [Keyword("repeat")]
        RepeatKeyword,
        [Keyword("until")]
        UntilKeyword,
        [Keyword("if")]
        IfKeyword,
        [Keyword("then")]
        ThenKeyword,
        [Keyword("elseif")]
        ElseIfKeyword,
        [Keyword("else")]
        ElseKeyword,
        [Keyword("for")]
        ForKeyword,
        [Keyword("in")]
        InKeyword,
        [Keyword("function")]
        FunctionKeyword,
        [Keyword("local")]
        LocalKeyword,
        [Keyword("return")]
        ReturnKeyword,
        [Keyword("break")]
        BreakKeyword,
        [Keyword("goto")]
        GotoKeyword,
        [Keyword("continue")]
        ContinueKeyword,
        [Keyword("and")]
        [BinaryOperator(precedence: 2, LogicalAndExpression)]
        AndKeyword,
        [Keyword("or")]
        [BinaryOperator(precedence: 1, LogicalOrExpression)]
        OrKeyword,
        [Keyword("not")]
        [UnaryOperator(precedence: 7, LogicalNotExpression)]
        NotKeyword,
        [Keyword("nil")]
        [ExtraCategories("LiteralToken")]
        NilKeyword,
        [Keyword("true")]
        [ExtraCategories("LiteralToken")]
        TrueKeyword,
        [Keyword("false")]
        [ExtraCategories("LiteralToken")]
        FalseKeyword,

        // Parameters
        NamedParameter,
        VarArgParameter,
        ParameterList,

        // Table Fields
        IdentifierKeyedTableField,
        ExpressionKeyedTableField,
        UnkeyedTableField,

        // Function Names
        SimpleFunctionName,
        MemberFunctionName,
        MethodFunctionName,

        // Function Call Arguments
        StringFunctionArgument,
        TableConstructorFunctionArgument,
        ExpressionListFunctionArgument,

        // Primary Expressions
        AnonymousFunctionExpression,
        TableConstructorExpression,
        NumericalLiteralExpression,
        StringLiteralExpression,
        TrueLiteralExpression,
        FalseLiteralExpression,
        NilLiteralExpression,
        VarArgExpression,
        [ExtraCategories("VariableExpression")]
        NameExpression,

        // Unary Expressions
        [Property("UnaryExpressionOperatorTokenKind", MinusToken)]
        UnaryMinusExpression,
        [Property("UnaryExpressionOperatorTokenKind", NotKeyword)]
        LogicalNotExpression,
        [Property("UnaryExpressionOperatorTokenKind", HashToken)]
        LengthExpression,

        // Binary Expressions
        [Property("BinaryExpressionOperatorTokenKind", PlusToken)]
        AddExpression,
        [Property("BinaryExpressionOperatorTokenKind", MinusToken)]
        SubtractExpression,
        [Property("BinaryExpressionOperatorTokenKind", StarToken)]
        MultiplyExpression,
        [Property("BinaryExpressionOperatorTokenKind", SlashToken)]
        DivideExpression,
        [Property("BinaryExpressionOperatorTokenKind", PercentToken)]
        ModuloExpression,
        [Property("BinaryExpressionOperatorTokenKind", DotDotToken)]
        ConcatExpression,
        [Property("BinaryExpressionOperatorTokenKind", EqualsEqualsToken)]
        EqualsExpression,
        [Property("BinaryExpressionOperatorTokenKind", TildeEqualsToken)]
        NotEqualsExpression,
        [Property("BinaryExpressionOperatorTokenKind", LessThanToken)]
        LessThanExpression,
        [Property("BinaryExpressionOperatorTokenKind", LessThanEqualsToken)]
        LessThanOrEqualExpression,
        [Property("BinaryExpressionOperatorTokenKind", GreaterThanToken)]
        GreaterThanExpression,
        [Property("BinaryExpressionOperatorTokenKind", GreaterThanEqualsToken)]
        GreaterThanOrEqualExpression,
        [Property("BinaryExpressionOperatorTokenKind", AndKeyword)]
        LogicalAndExpression,
        [Property("BinaryExpressionOperatorTokenKind", OrKeyword)]
        LogicalOrExpression,
        [Property("BinaryExpressionOperatorTokenKind", HatToken)]
        ExponentiateExpression,

        // Expressions
        BadExpression,
        ParenthesizedExpression,
        FunctionCallExpression,
        [ExtraCategories("VariableExpression")]
        MemberAccessExpression,
        [ExtraCategories("VariableExpression")]
        ElementAccessExpression,
        MethodCallExpression,

        // Assignment Statements
        [Property("AssignmentStatementOperatorTokenKind", EqualsToken)]
        AssignmentStatement,
        [Property("AssignmentStatementOperatorTokenKind", PlusEqualsToken)]
        AddAssignmentStatement,
        [Property("AssignmentStatementOperatorTokenKind", MinusEqualsToken)]
        SubtractAssignmentStatement,
        [Property("AssignmentStatementOperatorTokenKind", StartEqualsToken)]
        MultiplyAssignmentStatement,
        [Property("AssignmentStatementOperatorTokenKind", SlashEqualsToken)]
        DivideAssignmentStatement,
        [Property("AssignmentStatementOperatorTokenKind", PercentEqualsToken)]
        ModuloAssignmentStatement,
        [Property("AssignmentStatementOperatorTokenKind", DotDotEqualsToken)]
        ConcatAssignmentStatement,
        [Property("AssignmentStatementOperatorTokenKind", HatEqualsToken)]
        ExponentiateAssignmentStatement,

        // Control Flow Statements
        NumericForStatement,
        GenericForStatement,
        WhileStatement,
        RepeatUntilStatement,
        IfStatement,
        ElseIfClause,
        ElseClause,

        // Jump Statements
        GotoStatement,
        BreakStatement,
        ReturnStatement,
        ContinueStatement,

        // Statements
        BadStatement,
        LocalVariableDeclarationStatement,
        LocalFunctionDeclarationStatement,
        FunctionDeclarationStatement,
        DoStatement,
        GotoLabelStatement,
        ExpressionStatement,
        CompilationUnit,
    }
}
