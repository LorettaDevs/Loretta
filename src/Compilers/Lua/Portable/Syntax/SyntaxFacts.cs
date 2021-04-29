using System;
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
        /// Whether two tokens/trivia require a separator between them.
        /// </summary>
        /// <param name="kindA"></param>
        /// <param name="kindAText"></param>
        /// <param name="kindB"></param>
        /// <param name="kindBText"></param>
        /// <returns></returns>
        internal static bool RequiresSeparator(SyntaxKind kindA, string kindAText, SyntaxKind kindB, string kindBText)
        {
            if (kindAText is null)
                throw new ArgumentNullException(nameof(kindAText));
            if (kindBText is null)
                throw new ArgumentNullException(nameof(kindBText));

            var kindAIsKeyword = SyntaxFacts.IsKeyword(kindA);
            var kindBIsKeyowrd = SyntaxFacts.IsKeyword(kindB);

            if (kindA is SyntaxKind.IdentifierToken && kindB is SyntaxKind.IdentifierToken)
                return true;
            if (kindAIsKeyword && kindBIsKeyowrd)
                return true;
            if (kindAIsKeyword && kindB is SyntaxKind.IdentifierToken)
                return true;
            if (kindA is SyntaxKind.IdentifierToken && kindBIsKeyowrd)
                return true;
            if (kindA is SyntaxKind.IdentifierToken && kindB is SyntaxKind.NumericLiteralToken)
                return true;
            if (kindA is SyntaxKind.NumericLiteralToken && kindB is SyntaxKind.IdentifierToken)
                return true;
            if (kindA is SyntaxKind.NumericLiteralToken && kindBIsKeyowrd)
                return true;
            if (kindA is SyntaxKind.NumericLiteralToken && kindB is SyntaxKind.DotToken or SyntaxKind.DotDotToken or SyntaxKind.DotDotDotToken or SyntaxKind.DotDotEqualsToken)
                return true;
            if (kindAIsKeyword && kindB is SyntaxKind.NumericLiteralToken)
                return true;
            if (kindA is SyntaxKind.NumericLiteralToken && kindB is SyntaxKind.NumericLiteralToken)
                return true;
            if (kindA is SyntaxKind.OpenBracketToken && kindB is SyntaxKind.OpenBracketToken)
                return true;
            if (kindA is SyntaxKind.OpenBracketToken && kindB == SyntaxKind.StringLiteralToken && kindBText.StartsWith("["))
                return true;
            if (kindA is SyntaxKind.ColonToken && kindB is SyntaxKind.ColonToken or SyntaxKind.ColonColonToken)
                return true;
            if (kindA is SyntaxKind.PlusToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.MinusToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.MinusToken && kindB is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia && kindBText.StartsWith("-"))
                return true;
            if (kindA is SyntaxKind.MinusToken && kindB is SyntaxKind.MinusToken or SyntaxKind.MinusEqualsToken)
                return true;
            if (kindA is SyntaxKind.StarToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.SlashToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.SlashEqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.SlashToken && kindB is SyntaxKind.SlashToken or SyntaxKind.StarToken or SyntaxKind.StartEqualsToken)
                return true;
            if (kindA is SyntaxKind.SlashToken && kindB is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia && kindBText.StartsWith("/"))
                return true;
            if (kindA is SyntaxKind.HatToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.PercentToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.DotDotToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.DotToken or SyntaxKind.DotDotToken && kindB is SyntaxKind.DotToken or SyntaxKind.DotDotToken or SyntaxKind.DotDotDotToken or SyntaxKind.DotDotEqualsToken)
                return true;
            if (kindA is SyntaxKind.EqualsToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.BangToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.LessThanToken && kindB is SyntaxKind.LessThanToken or SyntaxKind.LessThanEqualsToken or SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken or SyntaxKind.LessThanLessThanToken)
                return true;
            if (kindA is SyntaxKind.GreaterThanToken && kindB is SyntaxKind.GreaterThanToken or SyntaxKind.GreaterThanEqualsToken or SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken or SyntaxKind.GreaterThanGreaterThanToken)
                return true;
            if (kindA is SyntaxKind.AmpersandToken && kindB is SyntaxKind.AmpersandToken or SyntaxKind.AmpersandAmpersandToken)
                return true;
            if (kindA is SyntaxKind.PipeToken && kindB is SyntaxKind.PipeToken or SyntaxKind.PipePipeToken)
                return true;
            // Dot can be the start of a number
            if (kindA is SyntaxKind.DotToken or SyntaxKind.DotDotToken or SyntaxKind.DotDotDotToken && kindB is SyntaxKind.NumericLiteralToken)
                return true;
            // Shebang
            if (kindA is SyntaxKind.HashToken && kindB is SyntaxKind.BangToken or SyntaxKind.BangEqualsToken)
                return true;
            if (kindA is SyntaxKind.TildeToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;

            return false;
        }

        /// <summary>
        /// Obtains the constant value of the token kind.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static Option<object?> GetConstantValue(SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.TrueLiteralExpression => Boxes.BoxedTrue,
                SyntaxKind.FalseLiteralExpression => Boxes.BoxedFalse,
                SyntaxKind.NilLiteralExpression => null,
                _ => default
            };
        }

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
        /// Obtains the kind of the literal expression node from the token kind.
        /// </summary>
        /// <param name="kind">The token's kind.</param>
        /// <returns></returns>
        public static partial Option<SyntaxKind> GetLiteralExpression(SyntaxKind kind);

        /// <summary>
        /// Obtains the operator token's kind from the expression node kind.
        /// </summary>
        /// <param name="kind">The unary expression node's kind.</param>
        /// <returns></returns>
        public static partial Option<SyntaxKind> GetOperatorTokenKind(SyntaxKind kind);
    }
}
