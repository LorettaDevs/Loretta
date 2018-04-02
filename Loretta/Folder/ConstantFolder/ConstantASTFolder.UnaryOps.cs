using System;
using System.Collections.Generic;
using Loretta.Parsing.Nodes;
using Loretta.Parsing.Nodes.Constants;
using Loretta.Parsing.Nodes.Operators;

namespace Loretta.Folder
{
    public partial class ConstantASTFolder : BaseASTFolder
    {
        protected override ASTNode FoldUnaryOperatorExpression ( UnaryOperatorExpression node, params Object[] args )
        {
            node = ( UnaryOperatorExpression ) base.FoldUnaryOperatorExpression ( node );

            if ( node.Operand is ConstantExpression || node.Operand is TableConstructorExpression )
            {
                switch ( node.Operator )
                {
                    case "!":
                    case "not":
                    {
                        if ( node.Operand is NilExpression )
                            return GetBooleanExpression ( node, true );
                        else if ( node.Operand is BooleanExpression boolExpr )
                            return GetBooleanExpression ( node, !boolExpr.Value );
                        else
                            return GetBooleanExpression ( node, false );
                    }

                    case "-":
                    {
                        if ( node.Operand is NumberExpression numExpr )
                            return GetNumberExpression ( node, -numExpr.Value );

                        this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempt to negate non-numeric value." ) );
                        break;
                    }

                    case "#":
                    {
                        if ( node.Operand is NumberExpression )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to get length of a number value." ) );
                        else if ( node.Operand is NilExpression )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to get length of a nil value" ) );
                        else if ( node.Operand is BooleanExpression )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to get length of a boolean value" ) );
                        // now to those that *actually* do something
                        else if ( node.Operand is StringExpression strExp )
                            return GetNumberExpression ( node, strExp.Value.Length );
                        else if ( node.Operand is TableConstructorExpression tableExpr )
                        {
                            // Thanks to mijyuoon for the idea of
                            // storing indexes and finding a gap
                            // through deltas
                            var indexList = new List<Double> ( );
                            foreach ( TableKeyValue keyVal in tableExpr.Fields )
                            {
                                if ( keyVal.Key is NumberExpression numExpr && Math.Floor ( numExpr.Value ) == numExpr.Value )
                                    indexList.Add ( numExpr.Value );
                            }
                            indexList.Sort ( );

                            var length = 0;
                            for ( var i = 0; i < indexList.Count - 1; i++ )
                            {
                                if ( indexList[i] - indexList[i + 1] > 1 )
                                    break;
                                length++;
                            }

                            return GetNumberExpression ( node, length );
                        }
                        break;
                    }
                }
            }

            return node;
        }
    }
}
