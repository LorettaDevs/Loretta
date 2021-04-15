using System;
using System.Collections.Generic;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.Utilities;
using static Loretta.CodeAnalysis.Lua.SyntaxFactory;

namespace Loretta.CodeAnalysis.Lua.Experimental
{
    internal class ConstantFolder : LuaSyntaxRewriter
    {
        public static SyntaxNode Fold(SyntaxNode input)
        {
            var folder = new ConstantFolder();
            return folder.Visit(input)!;
        }

        private ConstantFolder()
        {
        }

        public override SyntaxNode? VisitUnaryExpression(UnaryExpressionSyntax node)
        {
            var operand = (ExpressionSyntax) Visit(node.Operand);
            var operandFlags = GetFlags(operand);
            return node.Kind() switch
            {
                SyntaxKind.UnaryMinusExpression when HasEFlag(operandFlags, ExpressionFlags.IsNum) =>
                    LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(-GetValue<double>(operand))),
                SyntaxKind.LogicalNotExpression when TryConvertToBool(operand, out var value) =>
                    LiteralExpression(value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression),
                SyntaxKind.BitwiseNotExpression when HasEFlag(operandFlags, ExpressionFlags.IsNum)
                    && TryGetInt64(operand, out var value)
                    && TryConvertToDouble(~value, out var result) =>
                    LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(result)),
                _ => node.Update(node.OperatorToken, operand),
            };
        }

        public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            ExpressionSyntax left = (ExpressionSyntax) Visit(node.Left),
                             right = (ExpressionSyntax) Visit(node.Right);
            ExpressionFlags leftFlags = GetFlags(left), rightFlags = GetFlags(right);

            switch (node.Kind())
            {
                case SyntaxKind.AddExpression when HasEFlag(leftFlags, ExpressionFlags.IsNum)
                    && HasEFlag(rightFlags, ExpressionFlags.IsNum):
                {
                    var result = GetValue<double>(left) + GetValue<double>(right);
                    if (double.IsNaN(result) && double.IsInfinity(result))
                        break;
                    return LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(result));
                }

                case SyntaxKind.SubtractExpression when HasEFlag(leftFlags, ExpressionFlags.IsNum)
                    && HasEFlag(rightFlags, ExpressionFlags.IsNum):
                {
                    var result = GetValue<double>(left) - GetValue<double>(right);
                    if (double.IsNaN(result) && double.IsInfinity(result))
                        break;
                    return LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(result));
                }

                case SyntaxKind.MultiplyExpression when HasEFlag(leftFlags, ExpressionFlags.IsNum)
                    && HasEFlag(rightFlags, ExpressionFlags.IsNum):
                {
                    var result = GetValue<double>(left) * GetValue<double>(right);
                    if (double.IsNaN(result) && double.IsInfinity(result))
                        break;
                    return LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(result));
                }

                case SyntaxKind.DivideExpression when HasEFlag(leftFlags, ExpressionFlags.IsNum)
                    && HasEFlag(rightFlags, ExpressionFlags.IsNum):
                {
                    var result = GetValue<double>(left) / GetValue<double>(right);
                    if (double.IsNaN(result) && double.IsInfinity(result))
                        break;
                    return LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(result));
                }

                case SyntaxKind.ModuloExpression when HasEFlag(leftFlags, ExpressionFlags.IsNum)
                    && HasEFlag(rightFlags, ExpressionFlags.IsNum):
                {
                    var result = GetValue<double>(left) % GetValue<double>(right);
                    if (double.IsNaN(result) && double.IsInfinity(result))
                        break;
                    return LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(result));
                }

                case SyntaxKind.ExponentiateExpression when HasEFlag(leftFlags, ExpressionFlags.IsNum)
                    && HasEFlag(rightFlags, ExpressionFlags.IsNum):
                {
                    var result = Math.Pow(GetValue<double>(left), GetValue<double>(right));
                    if (double.IsNaN(result) && double.IsInfinity(result))
                        break;
                    return LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(result));
                }

                case SyntaxKind.ConcatExpression when HasEFlag(leftFlags, ExpressionFlags.IsStr | ExpressionFlags.IsBool)
                    && HasEFlag(rightFlags, ExpressionFlags.IsStr | ExpressionFlags.IsBool):
                {
                    var leftStr = left.Kind() switch
                    {
                        SyntaxKind.TrueLiteralExpression => "true",
                        SyntaxKind.FalseLiteralExpression => "false",
                        SyntaxKind.StringLiteralExpression => GetValue<string>(left),
                        _ => throw ExceptionUtilities.Unreachable,
                    };
                    var rightStr = right.Kind() switch
                    {
                        SyntaxKind.TrueLiteralExpression => "true",
                        SyntaxKind.FalseLiteralExpression => "false",
                        SyntaxKind.StringLiteralExpression => GetValue<string>(right),
                        _ => throw ExceptionUtilities.Unreachable,
                    };
                    return LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        Literal(leftStr + rightStr));
                }

                case SyntaxKind.EqualsExpression when HasEFlag(leftFlags, ExpressionFlags.IsConstant)
                    && HasEFlag(rightFlags, ExpressionFlags.IsConstant):
                {
                    var result = exprEquals(left, right, leftFlags, rightFlags);
                    return LiteralExpression(result ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
                }

                case SyntaxKind.NotEqualsExpression when HasEFlag(leftFlags, ExpressionFlags.IsConstant)
                    && HasEFlag(rightFlags, ExpressionFlags.IsConstant):
                {
                    var result = !exprEquals(left, right, leftFlags, rightFlags);
                    return LiteralExpression(result ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
                }

                case SyntaxKind.LessThanExpression when canCompare(leftFlags, rightFlags):
                {
                    var result = compare(left, right, leftFlags, rightFlags);
                    return LiteralExpression(result < 0 ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
                }

                case SyntaxKind.LessThanOrEqualExpression when canCompare(leftFlags, rightFlags):
                {
                    var result = compare(left, right, leftFlags, rightFlags);
                    return LiteralExpression(result <= 0 ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
                }

                case SyntaxKind.GreaterThanExpression when canCompare(leftFlags, rightFlags):
                {
                    var result = compare(left, right, leftFlags, rightFlags);
                    return LiteralExpression(result > 0 ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
                }

                case SyntaxKind.GreaterThanOrEqualExpression when canCompare(leftFlags, rightFlags):
                {
                    var result = compare(left, right, leftFlags, rightFlags);
                    return LiteralExpression(result >= 0 ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
                }

                case SyntaxKind.LogicalAndExpression when TryConvertToBool(left, out var result):
                    return result ? right : left;

                case SyntaxKind.LogicalOrExpression when TryConvertToBool(left, out var result):
                    return !result ? right : left;

                case SyntaxKind.BitwiseOrExpression when HasEFlag(leftFlags, ExpressionFlags.IsNum)
                    && HasEFlag(rightFlags, ExpressionFlags.IsNum)
                    && TryGetInt64(left, out var leftValue)
                    && TryGetInt64(right, out var rightValue)
                    && TryConvertToDouble(leftValue | rightValue, out var result):
                    return LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(result));

                case SyntaxKind.BitwiseAndExpression when HasEFlag(leftFlags, ExpressionFlags.IsNum)
                    && HasEFlag(rightFlags, ExpressionFlags.IsNum)
                    && TryGetInt64(left, out var leftValue)
                    && TryGetInt64(right, out var rightValue)
                    && TryConvertToDouble(leftValue & rightValue, out var result):
                    return LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(result));

                case SyntaxKind.RightShiftExpression when HasEFlag(leftFlags, ExpressionFlags.IsNum)
                    && HasEFlag(rightFlags, ExpressionFlags.IsNum)
                    && TryGetInt64(left, out var leftValue)
                    && TryGetInt32(right, out var rightValue)
                    && TryConvertToDouble(leftValue >> rightValue, out var result):
                    return LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(result));

                case SyntaxKind.LeftShiftExpression when HasEFlag(leftFlags, ExpressionFlags.IsNum)
                    && HasEFlag(rightFlags, ExpressionFlags.IsNum)
                    && TryGetInt64(left, out var leftValue)
                    && TryGetInt32(right, out var rightValue)
                    && TryConvertToDouble(leftValue << rightValue, out var result):
                    return LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(result));

                case SyntaxKind.ExclusiveOrExpression when HasEFlag(leftFlags, ExpressionFlags.IsNum)
                    && HasEFlag(rightFlags, ExpressionFlags.IsNum)
                    && TryGetInt64(left, out var leftValue)
                    && TryGetInt64(right, out var rightValue)
                    && TryConvertToDouble(leftValue ^ rightValue, out var result):
                    return LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(result));
            }

            return node.Update(left, node.OperatorToken, right);

            static bool exprEquals(ExpressionSyntax left, ExpressionSyntax right, ExpressionFlags leftFlags, ExpressionFlags rightFlags)
            {
                var result = false;
                if (HasEFlag(leftFlags, ExpressionFlags.IsNil) && HasEFlag(rightFlags, ExpressionFlags.IsNil))
                    result = true;
                else if (HasEFlag(leftFlags, ExpressionFlags.IsNum) && HasEFlag(rightFlags, ExpressionFlags.IsNum))
                    result = GetValue<double>(left) == GetValue<double>(right);
                else if (HasEFlag(leftFlags, ExpressionFlags.IsStr) && HasEFlag(rightFlags, ExpressionFlags.IsStr))
                    result = string.Equals(GetValue<string>(left), GetValue<string>(right), StringComparison.Ordinal);
                else if (HasEFlag(leftFlags, ExpressionFlags.IsBool) && HasEFlag(rightFlags, ExpressionFlags.IsBool))
                    result = HasEFlag(leftFlags, ExpressionFlags.IsTruthy) == HasEFlag(rightFlags, ExpressionFlags.IsTruthy);
                return result;
            }

            static bool canCompare(ExpressionFlags leftFlags, ExpressionFlags rightFlags) =>
                (HasEFlag(leftFlags, ExpressionFlags.IsNum) && HasEFlag(rightFlags, ExpressionFlags.IsNum))
                || (HasEFlag(leftFlags, ExpressionFlags.IsStr) && HasEFlag(rightFlags, ExpressionFlags.IsStr));

            static int compare(ExpressionSyntax left, ExpressionSyntax right, ExpressionFlags leftFlags, ExpressionFlags rightFlags)
            {
                if (HasEFlag(leftFlags, ExpressionFlags.IsNum) && HasEFlag(rightFlags, ExpressionFlags.IsNum))
                    return Comparer<double>.Default.Compare(GetValue<double>(left), GetValue<double>(right));
                else if (HasEFlag(leftFlags, ExpressionFlags.IsStr) && HasEFlag(rightFlags, ExpressionFlags.IsStr))
                    return string.CompareOrdinal(GetValue<string>(left), GetValue<string>(right));
                throw new InvalidOperationException("Both expressions must have the same type.");
            }
        }

        private enum ExpressionFlags : byte
        {
            None = 0,
            IsNil = 1 << 0,
            IsNum = 1 << 1,
            IsStr = 1 << 2,
            IsBool = 1 << 3,
            IsTbl = 1 << 4,
            IsTruthy = 1 << 5,
            IsFalsey = 1 << 6,
            IsConstTbl = 1 << 7,

            CanConvertToBool = IsTruthy | IsFalsey,
            IsConstant = IsNil | IsNum | IsStr | IsBool,
        }

        private static SyntaxNode GetInnerExpression(SyntaxNode node) =>
            node.IsKind(SyntaxKind.ParenthesizedExpression)
            ? GetInnerExpression(((ParenthesizedExpressionSyntax) node).Expression)
            : node;

        private static ExpressionFlags GetFlags(SyntaxNode node)
        {
            node = GetInnerExpression(node);
            RoslynDebug.Assert(node is ExpressionSyntax);

            var flags = ExpressionFlags.None;
            if (node.IsKind(SyntaxKind.NumericalLiteralExpression))
                flags |= ExpressionFlags.IsNum;
            if (node.IsKind(SyntaxKind.StringLiteralExpression))
                flags |= ExpressionFlags.IsStr;
            if (CanConvertToBoolean(node))
                flags |= IsFalsey(node) ? ExpressionFlags.IsFalsey : ExpressionFlags.IsTruthy;
            return flags;
        }

        private static bool HasEFlag(ExpressionFlags flags, ExpressionFlags wantedFlag) => (flags & wantedFlag) != 0;

        /// <summary>
        /// Checks whether we can statically convert this to a boolean (function calls, indexing
        /// operations and identifiers can't be converted since we don't know the values they might return)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static bool CanConvertToBoolean(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.NilLiteralExpression:
                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                case SyntaxKind.NumericalLiteralExpression:
                case SyntaxKind.StringLiteralExpression:
                case SyntaxKind.AnonymousFunctionExpression:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks whether the value is false according to lua's rules.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static bool IsFalsey(SyntaxNode node)
        {
            RoslynDebug.Assert(CanConvertToBoolean(node));
            return node.Kind() is SyntaxKind.NilLiteralExpression or SyntaxKind.FalseLiteralExpression;
        }

        private static bool TryConvertToBool(SyntaxNode node, out bool value)
        {
            var innerNode = GetInnerExpression(node);
            if (CanConvertToBoolean(innerNode))
            {
                value = !IsFalsey(innerNode);
                return true;
            }

            value = false;
            return false;
        }

        /// <summary>
        /// Obtains the value from the provided node as a <see cref="LiteralExpressionSyntax"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        private static T GetValue<T>(SyntaxNode node)
        {
            node = GetInnerExpression(node);
            RoslynDebug.Assert(node is LiteralExpressionSyntax);
            return (T) ((LiteralExpressionSyntax) node).Token.Value!;
        }

        private static bool TryGetInt32(SyntaxNode node, out int converted)
        {
            var value = GetValue<double>(node);
            converted = (int) value;
            return value == converted;
        }

        private static bool TryGetInt64(SyntaxNode node, out long converted)
        {
            var value = GetValue<double>(node);
            converted = (long) value;
            return value == converted;
        }

        private static bool TryConvertToDouble(long value, out double converted)
        {
            converted = value;
            return value == converted;
        }
    }

    public static partial class SyntaxExtensions
    {
        public static SyntaxNode FoldConstants(this SyntaxNode node) =>
            ConstantFolder.Fold(node);
    }
}
