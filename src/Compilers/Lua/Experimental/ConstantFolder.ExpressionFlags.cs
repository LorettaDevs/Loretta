using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.CodeAnalysis.Lua.Experimental
{
    internal partial class ConstantFolder
    {
        [Flags]
        private enum ExpressionFlags
        {
            None = 0,
            IsNil = 1 << 0,
            IsDouble = 1 << 1,
            IsStr = 1 << 2,
            IsBool = 1 << 3,
            IsTruthy = 1 << 4,
            IsFalsey = 1 << 5,
            IsConstantTable = 1 << 6,
            IsAnonymousFunction = 1 << 7,
            IsLong = 1 << 8,
            IsStringWithNumber = 1 << 9,

            CanConvertToBool = IsTruthy | IsFalsey,
            IsScalar = IsNil | IsDouble | IsLong | IsStr | IsBool,
            IsConstant = IsScalar | IsConstantTable | IsAnonymousFunction,
            IsNum = IsDouble | IsLong | IsStringWithNumber,
        }

        private readonly Dictionary<SyntaxNode, ExpressionFlags> _exprFlags = new();
        private readonly Dictionary<SyntaxNode, dynamic> _innerStringNumericValue = new();

        private ExpressionFlags GetFlags(SyntaxNode node)
        {
            if (!_exprFlags.TryGetValue(node, out var flags))
            {
                var innerNode = GetInnerExpression(node);
                LorettaDebug.Assert(innerNode is ExpressionSyntax);

                flags = ExpressionFlags.None;
                if (innerNode.IsKind(SyntaxKind.NilLiteralExpression))
                    flags |= ExpressionFlags.IsNil;
                if (innerNode.IsKind(SyntaxKind.NumericalLiteralExpression))
                    flags |= ((LiteralExpressionSyntax) innerNode).Token.Value is double ? ExpressionFlags.IsDouble : ExpressionFlags.IsLong;
                if (innerNode.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    flags |= ExpressionFlags.IsStr;
                    if (_options.ExtractNumbersFromStrings
                        && TryParseNumberInString(GetValue<string>(innerNode), out var parsed))
                    {
                        flags |= ExpressionFlags.IsStringWithNumber;
                        _innerStringNumericValue[node] = parsed!;
                    }
                }
                if (innerNode.IsKind(SyntaxKind.TrueLiteralExpression) || innerNode.IsKind(SyntaxKind.FalseLiteralExpression))
                    flags |= ExpressionFlags.IsBool;
                if (CanConvertToBoolean(innerNode))
                    flags |= IsFalsey(innerNode) ? ExpressionFlags.IsFalsey : ExpressionFlags.IsTruthy;
                if (innerNode.IsKind(SyntaxKind.TableConstructorExpression)
                    && IsConstTable((TableConstructorExpressionSyntax) innerNode))
                {
                    flags |= ExpressionFlags.IsConstantTable;
                }
                if (innerNode.IsKind(SyntaxKind.AnonymousFunctionExpression))
                    flags |= ExpressionFlags.IsAnonymousFunction;

                // Set all parenthesized expression flags as well
                for (var tmp = node as ParenthesizedExpressionSyntax;
                    tmp is not null;
                    tmp = tmp.Expression as ParenthesizedExpressionSyntax)
                {
                    _exprFlags[tmp] = flags;
                }

                _exprFlags[innerNode] = flags;
            }

            return flags;
        }

        private bool IsConstTable(TableConstructorExpressionSyntax tableConstructor)
        {
            foreach (var field in tableConstructor.Fields)
            {
                switch (field.Kind())
                {
                    case SyntaxKind.IdentifierKeyedTableField:
                    {
                        var typedField = (IdentifierKeyedTableFieldSyntax) field;
                        if (!isConst(typedField.Value))
                            return false;
                        break;
                    }

                    case SyntaxKind.ExpressionKeyedTableField:
                    {
                        var typedField = (ExpressionKeyedTableFieldSyntax) field;
                        if (!isConst(typedField.Key) || !isConst(typedField.Value))
                            return false;
                        break;
                    }

                    case SyntaxKind.UnkeyedTableField:
                    {
                        var typedField = (UnkeyedTableFieldSyntax) field;
                        if (!isConst(typedField.Value))
                            return false;
                        break;
                    }

                    default:
                        throw ExceptionUtilities.UnexpectedValue(field.Kind());
                }
            }

            return true;

            bool isConst(SyntaxNode node) =>
                HasEFlag(GetFlags(node), ExpressionFlags.IsConstant | ExpressionFlags.IsConstantTable);
        }

        private static bool HasEFlag(ExpressionFlags flags, ExpressionFlags wantedFlag) => (flags & wantedFlag) != 0;

        private bool HasEFlag(SyntaxNode node, ExpressionFlags wantedFlag) =>
            HasEFlag(GetFlags(node), wantedFlag);
    }
}
