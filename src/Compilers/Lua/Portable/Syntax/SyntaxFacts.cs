using Tsu;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// A static class containing facts about Lua's Syntax.
    /// </summary>
    public static partial class SyntaxFacts
    {
        /// <summary>
        /// Checks whether a <see cref="SyntaxKind"/> is a comment's.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static bool IsComment(SyntaxKind kind) =>
            kind is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia;

        /// <summary>
        /// Checks whether a given <see cref="SyntaxKind"/> is a right associative operator's.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static bool IsRightAssociative(SyntaxKind kind) =>
            kind is SyntaxKind.HatToken;

        /// <summary>
        /// Checks whether a given kind is a reserved keyword.
        /// </summary>
        /// <param name="actual"></param>
        /// <param name="syntaxOptions"></param>
        /// <returns></returns>
        public static bool IsReservedKeyword(SyntaxKind actual, LuaSyntaxOptions syntaxOptions) =>
            actual switch
            {
                SyntaxKind.ContinueKeyword => syntaxOptions.ContinueType == ContinueType.Keyword,
                _ => IsKeyword(actual)
            };

        /// <summary>
        /// Checks whether a given kind is a contextual keyword.
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="syntaxOptions"></param>
        /// <returns></returns>
        public static bool IsContextualKeyword(SyntaxKind kind, LuaSyntaxOptions syntaxOptions) =>
            kind switch
            {
                SyntaxKind.ContinueKeyword => syntaxOptions.ContinueType == ContinueType.ContextualKeyword,
                _ => false
            };

        /// <summary>
        /// Obtains the kind of the operator of the compound assignment operator.
        /// </summary>
        /// <param name="kind">The the compound operator kind.</param>
        /// <returns></returns>
        public static partial Option<SyntaxKind> GetCompoundAssignmentOperator(SyntaxKind kind);

        /// <summary>
        /// Obtains the kind of the compound assignment operator node from the
        /// assignment operator kind.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static partial Option<SyntaxKind> GetCompoundAssignmentStatement(SyntaxKind kind);

        /// <summary>
        /// Obtains the constant value of the token kind.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static partial Option<object?> GetConstantValue(SyntaxKind kind);

        /// <summary>
        /// Obtains the kind of the literal expression node from the token kind.
        /// </summary>
        /// <param name="kind">The token's kind.</param>
        /// <returns></returns>
        public static partial Option<SyntaxKind> GetLiteralExpression(SyntaxKind kind);

        /// <summary>
        /// Obtains the operator token's kind from the unary expression node kind.
        /// </summary>
        /// <param name="kind">The unary expression node's kind.</param>
        /// <returns></returns>
        public static partial Option<SyntaxKind> GetUnaryExpressionOperatorTokenKind(SyntaxKind kind);

        /// <summary>
        /// Obtains the operator token's kind from the binary expression node kind.
        /// </summary>
        /// <param name="kind">The binary expression node's kind.</param>
        /// <returns></returns>
        public static partial Option<SyntaxKind> GetBinaryExpressionOperatorTokenKind(SyntaxKind kind);

        /// <summary>
        /// Obtains the operator token's kind from the assignment expression node kind.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static partial Option<SyntaxKind> GetAssignmentStatementOperatorTokenKind(SyntaxKind kind);
    }
}
