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
                    _ => ASTNodeFactory.BooleanExpression ( true )
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
            // All checks will be done on the <side>Expression vars since the <side> var could be a
            // parenthesized expression, but when returning, we use the original <side> vars.
            Expression leftExpression = left;
            Expression rightExpression = right;
            if ( left is GroupedExpression leftGrouped )
                leftExpression = leftGrouped.InnerExpression;

            if ( right is GroupedExpression rightGrouped )
                rightExpression = rightGrouped.InnerExpression;

            if ( leftExpression is NumberExpression { Value: var leftNumber } && rightExpression is NumberExpression { Value: var rightNumber } )
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
            }
            else if ( leftExpression is NilExpression )
            {
                switch ( @operator )
                {
                    case "==": return ASTNodeFactory.BooleanExpression ( rightExpression is NilExpression );
                    case "~=": return ASTNodeFactory.BooleanExpression ( !( rightExpression is NilExpression ) );
                    case "and": return ASTNodeFactory.Nil ( );
                    case "or": return right;
                }
            }
            else if ( canConvertToBoolean ( leftExpression ) && rightExpression is NilExpression )
            {
                switch ( @operator )
                {
                    case "==": return ASTNodeFactory.BooleanExpression ( leftExpression is NilExpression );
                    case "~=": return ASTNodeFactory.BooleanExpression ( !( leftExpression is NilExpression ) );
                    case "and":
                        if ( isFalsey ( leftExpression ) )
                            return left;
                        return ASTNodeFactory.Nil ( );

                    case "or":
                        if ( isFalsey ( leftExpression ) )
                            return ASTNodeFactory.Nil ( );
                        return left;
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

            // Whether the provided expression is falsey according to lua rules
            static Boolean isFalsey ( Expression expression ) =>
                expression is NilExpression || expression is BooleanExpression { Value: false };

            // Checks whether we can statically convert this to a boolean (function calls, indexing
            // operations and identifiers can't be converted since we don't know the values they
            // might return)
            static Boolean canConvertToBoolean ( Expression expression ) =>
                expression is NilExpression
                || expression is BooleanExpression
                || expression is NumberExpression
                || expression is StringExpression
                || expression is TableConstructorExpression
                || expression is AnonymousFunctionExpression;
        }

        private class TableConstructorState
        {
        }

        public override LuaASTNode VisitTableConstructor ( TableConstructorExpression node )
        {
            var table = ( TableConstructorExpression ) base.VisitTableConstructor ( node );

            return table;
        }
    }
}