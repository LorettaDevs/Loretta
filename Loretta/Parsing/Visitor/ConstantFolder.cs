using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Loretta.Parsing.AST;
using Loretta.Parsing.AST.Tables;

namespace Loretta.Parsing.Visitor
{
    /// <summary>
    /// A folder that does constant folding (evaluates expressions with constant values to their results).
    /// </summary>
    public class ConstantFolder : TreeFolderBase
    {
        /// <summary>
        /// Folds unary operations.
        /// </summary>
        /// <param name="node">The unary operation to fold.</param>
        /// <returns>The folded node or the node with its operand folded.</returns>
        public override LuaASTNode VisitUnaryOperation ( UnaryOperationExpression node )
        {
            var unary = ( UnaryOperationExpression ) base.VisitUnaryOperation ( node );
            Expression operand = GetGroupedExpressionInnerExpression ( unary.Operand );

            return (node.Operator.Value, operand) switch
            {
                ("not", _ ) when CanConvertToBoolean ( operand ) => ASTNodeFactory.BooleanExpression ( !IsFalsey ( operand ) ),
                ("-", NumberExpression number ) => ASTNodeFactory.NumberExpression ( -number.Value ),
                //("#", TableConstructorExpression tableConstructor ) when TryGetTableLength ( tableConstructor, out var length ) => ASTNodeFactory.NumberExpression ( length ),
                _ => unary,
            };
        }

        /// <summary>
        /// Folds binary operations.
        /// </summary>
        /// <param name="node">The binary operation to fold.</param>
        /// <returns>The folded node or the node with its operands folded.</returns>
        public override LuaASTNode VisitBinaryOperation ( BinaryOperationExpression node )
        {
            var @operator = node.Operator.Value;
            var left = ( Expression ) this.VisitNode ( node.Left );
            var right = ( Expression ) this.VisitNode ( node.Right );

            // All checks will be done on the &lt;side&gt; Expression vars since the &lt;side&gt;
            // var could be a parenthesized expression, but when returning, we use the original
            // &gt;side&lt; vars.
            Expression leftExpression = GetGroupedExpressionInnerExpression ( left );
            Expression rightExpression = GetGroupedExpressionInnerExpression ( right );

            switch ( (leftExpression, rightExpression) )
            {
                case (NumberExpression { Value: var leftNumber }, NumberExpression { Value: var rightNumber } ):
                {
                    switch ( @operator )
                    {
                        case "+": return ASTNodeFactory.NumberExpression ( leftNumber + rightNumber );
                        case "-": return ASTNodeFactory.NumberExpression ( leftNumber - rightNumber );
                        case "/": return ASTNodeFactory.NumberExpression ( leftNumber / rightNumber );
                        case "*": return ASTNodeFactory.NumberExpression ( leftNumber * rightNumber );
                        case "^": return ASTNodeFactory.NumberExpression ( Math.Pow ( leftNumber, rightNumber ) );
                        case "%": return ASTNodeFactory.NumberExpression ( leftNumber % rightNumber );
                        case "<": return ASTNodeFactory.BooleanExpression ( leftNumber < rightNumber );
                        case "<=": return ASTNodeFactory.BooleanExpression ( leftNumber <= rightNumber );
                        case ">=": return ASTNodeFactory.BooleanExpression ( leftNumber >= rightNumber );
                        case "==": return ASTNodeFactory.BooleanExpression ( leftNumber == rightNumber );
                        case "~=": return ASTNodeFactory.BooleanExpression ( leftNumber != rightNumber );
                        case "and": return right;
                        case "or": return left;
                    }
                    break;
                }

                case (NilExpression _, _ ):
                {
                    switch ( @operator )
                    {
                        case "==": return ASTNodeFactory.BooleanExpression ( rightExpression is NilExpression );
                        case "~=": return ASTNodeFactory.BooleanExpression ( !( rightExpression is NilExpression ) );
                        case "and": return ASTNodeFactory.Nil ( );
                        case "or": return right;
                    }
                    break;
                }

                case (_, NilExpression _ ) when CanConvertToBoolean ( leftExpression ):
                {
                    switch ( @operator )
                    {
                        case "==": return ASTNodeFactory.BooleanExpression ( leftExpression is NilExpression );
                        case "~=": return ASTNodeFactory.BooleanExpression ( !( leftExpression is NilExpression ) );
                        case "and": return IsFalsey ( leftExpression ) ? left : ASTNodeFactory.Nil ( );
                        case "or": return IsFalsey ( leftExpression ) ? ASTNodeFactory.Nil ( ) : left;
                    }
                    break;
                }

                case (BooleanExpression { Value: var leftBoolean }, BooleanExpression { Value: var rightBoolean } ):
                {
                    switch ( @operator )
                    {
                        case "==": return ASTNodeFactory.BooleanExpression ( leftBoolean == rightBoolean );
                        case "~=": return ASTNodeFactory.BooleanExpression ( leftBoolean != rightBoolean );
                        case "and": return ASTNodeFactory.BooleanExpression ( leftBoolean && rightBoolean );
                        case "or": return ASTNodeFactory.BooleanExpression ( leftBoolean || rightBoolean );
                    }
                    break;
                }
            }

            if ( left != node.Left || right != node.Right )
            {
                return new BinaryOperationExpression ( left, node.Operator, right );
            }
            else
            {
                return node;
            }
        }

        /// <summary>
        /// Recursively fetches the innermost expression in a grouped expression of a {grouped
        /// expression | non-grouped expression}.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static Expression GetGroupedExpressionInnerExpression ( Expression expression ) =>
            expression is GroupedExpression groupedExpression
            ? GetGroupedExpressionInnerExpression ( groupedExpression.InnerExpression )
            : expression;

        /// <summary>
        /// Checks whether the value is false according to lua's rules.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static Boolean IsFalsey ( Expression expression ) =>
            expression is NilExpression || expression is BooleanExpression { Value: false };

        /// <summary>
        /// Checks whether we can statically convert this to a boolean (function calls, indexing
        /// operations and identifiers can't be converted since we don't know the values they might return)
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static Boolean CanConvertToBoolean ( Expression expression ) =>
            expression is NilExpression
            || expression is BooleanExpression
            || expression is NumberExpression
            || expression is StringExpression
            || expression is TableConstructorExpression
            || expression is AnonymousFunctionExpression;

        /// <summary>
        /// Attempts to build a dictionary with all table indexes and values. The dictionary can
        /// only be built if all table keys are identifiers, constants or sequential.
        /// </summary>
        /// <param name="tableConstructor"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private static Boolean TryGetDictionaryFromTable (
            TableConstructorExpression tableConstructor,
            [NotNullWhen ( true )] out IImmutableDictionary<Object, Expression>? dictionary )
        {
            if ( tableConstructor.Fields.All ( field => field.Key is null || field.KeyType == TableFieldKeyType.Identifier || field.Key.IsConstant ) )
            {
                ImmutableDictionary<Object, Expression>.Builder builder = ImmutableDictionary.CreateBuilder<Object, Expression> ( );
                var sequentialIndex = 0;
                foreach ( TableField? field in tableConstructor.Fields )
                {
                    switch ( field.KeyType )
                    {
                        case TableFieldKeyType.None:
                            builder[++sequentialIndex] = field.Value;
                            break;

                        case TableFieldKeyType.Identifier:
                            builder[( ( IdentifierExpression ) field.Key! ).Identifier] = field.Value;
                            break;

                        case TableFieldKeyType.Expression:
                            builder[field.Value.ConstantValue!] = field.Value;
                            break;
                    }
                }
                dictionary = builder.ToImmutable ( );
                return true;
            }

            dictionary = default;
            return false;
        }

        /// <summary>
        /// Attempts to get the table length. The length can only be obtained statically if all
        /// table keys are constants, identifiers or sequential.
        /// </summary>
        /// <param name="tableConstructor"></param>
        /// <param name="sequentialKeysCount"></param>
        /// <returns></returns>
        private static Boolean TryGetTableLength ( TableConstructorExpression tableConstructor, out Int32 sequentialKeysCount )
        {
            if ( TryGetDictionaryFromTable ( tableConstructor, out IImmutableDictionary<Object, Expression>? dictionary ) )
            {
                var keys = dictionary.Keys.OfType<Int32> ( ).OrderBy ( n => n ).ToImmutableArray ( );
                /* Here we basically take advantage that lua indexing is 1-based while C#'s is
                 * 0-based so the difference between the C# index and the lua key should be
                 * exactly 1 for sequential keys:
                 * |-----------------------|
                 * | C#  | Lua | Lua - C#  |
                 * |-----|-----|-----------|
                 * |  0  |  1  |     1     |
                 * |  1  |  2  |     1     |
                 * | ... | ... |    ...    |
                 * |-----------------------|
                 */
                sequentialKeysCount = 0;
                while ( sequentialKeysCount < keys.Length && keys[sequentialKeysCount] - sequentialKeysCount > 1 )
                    sequentialKeysCount++;
                return true;
            }

            sequentialKeysCount = default;
            return false;
        }
    }
}