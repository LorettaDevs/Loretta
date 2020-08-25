using System;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Visitor
{
    public class ConstantFolder : TreeFolderBase
    {
        public override LuaASTNode VisitUnaryOperation ( UnaryOperationExpression node )
        {
            var @operator = node.Operator.Value;
            var operand = ( Expression ) this.VisitNode ( node.Operand );

            return @operator switch
            {
                "not" => operand switch
                {
                    NilExpression _ => ASTNodeFactory.BooleanExpression ( false ),
                    BooleanExpression boolean => ASTNodeFactory.BooleanExpression ( !boolean.Value ),
                    _ => node.Operand != operand
                        ? new UnaryOperationExpression ( node.Fix, node.Operator, operand )
                        : node
                },
                "-" => operand switch
                {
                    NumberExpression number => ASTNodeFactory.NumberExpression ( -number.Value ),
                    _ => createNewNodeIfNecessary ( node, operand )
                },
                // Not implemented.
                //"#" => operand switch
                //{
                //},
                _ => createNewNodeIfNecessary ( node, operand ),
            };

            static UnaryOperationExpression createNewNodeIfNecessary ( UnaryOperationExpression expression, Expression operand )
            {
                if ( expression.Operand != operand )
                {
                    return new UnaryOperationExpression ( expression.Fix, expression.Operator, operand );
                }

                return expression;
            }
        }

        public override LuaASTNode VisitBinaryOperation ( BinaryOperationExpression node )
        {
            var @operator = node.Operator.Value;
            var left = ( Expression ) this.VisitNode ( node.Left );
            var right = ( Expression ) this.VisitNode ( node.Right );

            // All checks will be done on the &lt;side&gt; Expression vars since the &lt;side&gt;
            // var could be a parenthesized expression, but when returning, we use the original
            // &gt;side&lt; vars.
            Expression leftExpression = left;
            Expression rightExpression = right;
            if ( left is GroupedExpression leftGrouped )
                leftExpression = leftGrouped.InnerExpression;

            if ( right is GroupedExpression rightGrouped )
                rightExpression = rightGrouped.InnerExpression;

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

        // Checks whether the value is false according to lua's rules.
        private static Boolean IsFalsey ( Expression expression ) =>
            expression is NilExpression || expression is BooleanExpression { Value: false };

        // Checks whether we can statically convert this to a boolean (function calls, indexing
        // operations and identifiers can't be converted since we don't know the values they might return)
        private static Boolean CanConvertToBoolean ( Expression expression ) =>
            expression is NilExpression
            || expression is BooleanExpression
            || expression is NumberExpression
            || expression is StringExpression
            || expression is TableConstructorExpression
            || expression is AnonymousFunctionExpression;
    }
}