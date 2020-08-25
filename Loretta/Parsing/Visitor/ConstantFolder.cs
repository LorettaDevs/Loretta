using System;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Visitor
{
    public class ConstantFolder : TreeFolderBase
    {
        public override LuaASTNode VisitUnaryOperation ( UnaryOperationExpression node )
        {
            var unary = ( UnaryOperationExpression ) base.VisitUnaryOperation ( node );
            Expression operand = GetGroupedExpressionInnerExpression ( unary.Operand );

            return (node.Operator.Value, operand) switch
            {
                ("not", _ ) when CanConvertToBoolean ( operand ) => ASTNodeFactory.BooleanExpression ( !IsFalsey ( operand ) ),
                ("-", NumberExpression number ) => ASTNodeFactory.NumberExpression ( -number.Value ),
                _ => unary,
            };
        }

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

        private class TableConstructorState
        {
        }

        public override LuaASTNode VisitTableConstructor ( TableConstructorExpression node )
        {
            var table = ( TableConstructorExpression ) base.VisitTableConstructor ( node );

            return table;
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
    }
}