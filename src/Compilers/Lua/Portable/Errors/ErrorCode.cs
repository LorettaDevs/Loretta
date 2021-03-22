namespace Loretta.CodeAnalysis.Lua
{
    internal enum ErrorCode
    {
        Void = InternalErrorCode.Void,
        Unknonw = InternalErrorCode.Unknown,

        // Lexer Errors
        ERR_InvalidStringEscape = 1,
        ERR_UnescapedLineBreakInString = 2,
        ERR_UnfinishedString = 3,
        ERR_InvalidNumber = 4,
        ERR_NumericLiteralTooLarge = 5,
        ERR_UnfinishedLongComment = 6,
        ERR_ShebangNotSupportedInLuaVersion = 7,
        ERR_BinaryNumericLiteralNotSupportedInVersion = 8,
        ERR_OctalNumericLiteralNotSupportedInVersion = 9,
        ERR_HexFloatLiteralNotSupportedInVersion = 10,
        ERR_UnderscoreInNumericLiteralNotSupportedInVersion = 11,
        ERR_CCommentsNotSupportedInVersion = 12,
        ERR_LuajitIdentifierRulesNotSupportedInVersion = 13,
        ERR_BadCharacter = 14,
        ERR_UnexpectedToken = 15,
        ERR_HexStringEscapesNotSupportedInVersion = 16,
        ERR_AmbiguousFunctionCallOrNewStatement = 17,
        ERR_NonFunctionCallBeingUsedAsStatement = 18,
        ERR_CannotBeAssignedTo = 19,
        ERR_DoubleOverflow = 20,

        // Parser Errors
        ERR_IdentifierExpectedKW = 1000,
        ERR_IdentifierExpected = 1001,
        ERR_SemicolonExpected = 1002,
        ERR_CloseParenExpected = 1003,
        ERR_LbraceExpected = 1004,
        ERR_RbraceExpected = 1005,
        ERR_SyntaxError = 1006,
        ERR_InsufficientStack = 1007,

        // MessageProvider stuff
        ERR_BadDocumentationMode = 2000,
    }
}
