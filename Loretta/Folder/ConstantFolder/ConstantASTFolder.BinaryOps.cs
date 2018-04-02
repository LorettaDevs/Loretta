using System;
using Loretta.Parsing.Nodes;
using Loretta.Parsing.Nodes.Constants;
using Loretta.Parsing.Nodes.Operators;

namespace Loretta.Folder
{
    public partial class ConstantASTFolder : BaseASTFolder
    {
        protected override ASTNode FoldBinaryOperatorExpression ( BinaryOperatorExpression node, params Object[] args )
        {
            node = ( BinaryOperatorExpression ) base.FoldBinaryOperatorExpression ( node, args );
            ASTNode lhsNode = node.LeftOperand;
            ASTNode rhsNode = node.RightOperand;

            if ( ( lhsNode is ConstantExpression || lhsNode is TableConstructorExpression )
                && ( rhsNode is ConstantExpression || rhsNode is TableConstructorExpression ) )
            {
                switch ( node.Operator )
                {
                    case "<<":
                    {
                        Double? lhs = null, rhs = null;

                        if ( lhsNode is StringExpression lhsStr )
                            lhs = StringToNumber ( lhsStr.Value.Trim ( ) );
                        else if ( lhsNode is NumberExpression lhsNum )
                            lhs = lhsNum.Value;

                        if ( rhsNode is StringExpression rhsStr )
                            rhs = StringToNumber ( rhsStr.Value.Trim ( ) );
                        else if ( rhsNode is NumberExpression rhsNum )
                            rhs = rhsNum.Value;

                        if ( lhs == null || rhs == null )
                        {
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to perform arithmetic on non-number(s)." ) );
                            return node;
                        }

                        return GetNumberExpression ( node, ( Int64 ) lhs << ( Int32 ) rhs );
                    }

                    case ">>":
                    {
                        Double? lhs = null, rhs = null;

                        if ( lhsNode is StringExpression lhsStr )
                            lhs = StringToNumber ( lhsStr.Value.Trim ( ) );
                        else if ( lhsNode is NumberExpression lhsNum )
                            lhs = lhsNum.Value;

                        if ( rhsNode is StringExpression rhsStr )
                            rhs = StringToNumber ( rhsStr.Value.Trim ( ) );
                        else if ( rhsNode is NumberExpression rhsNum )
                            rhs = rhsNum.Value;

                        if ( lhs == null || rhs == null )
                        {
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to perform arithmetic on non-number(s)." ) );
                            return node;
                        }

                        return GetNumberExpression ( node, ( Int64 ) lhs >> ( Int32 ) rhs );
                    }

                    case "+":
                    {
                        Double? lhs = null, rhs = null;

                        if ( lhsNode is StringExpression lhsStr )
                            lhs = StringToNumber ( lhsStr.Value.Trim ( ) );
                        else if ( lhsNode is NumberExpression lhsNum )
                            lhs = lhsNum.Value;

                        if ( rhsNode is StringExpression rhsStr )
                            rhs = StringToNumber ( rhsStr.Value.Trim ( ) );
                        else if ( rhsNode is NumberExpression rhsNum )
                            rhs = rhsNum.Value;

                        if ( lhs == null || rhs == null )
                        {
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to perform arithmetic on non-number(s)." ) );
                            return node;
                        }

                        return GetNumberExpression ( node, ( Double ) ( lhs + rhs ) );
                    }

                    case "-":
                    {
                        Double? lhs = null, rhs = null;

                        if ( lhsNode is StringExpression lhsStr )
                            lhs = StringToNumber ( lhsStr.Value.Trim ( ) );
                        else if ( lhsNode is NumberExpression lhsNum )
                            lhs = lhsNum.Value;

                        if ( rhsNode is StringExpression rhsStr )
                            rhs = StringToNumber ( rhsStr.Value.Trim ( ) );
                        else if ( rhsNode is NumberExpression rhsNum )
                            rhs = rhsNum.Value;

                        if ( lhs == null || rhs == null )
                        {
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to perform arithmetic on non-number(s)." ) );
                            return node;
                        }

                        return GetNumberExpression ( node, ( Double ) ( lhs - rhs ) );
                    }

                    case "|":
                    {
                        Double? lhs = null, rhs = null;

                        if ( lhsNode is StringExpression lhsStr )
                            lhs = StringToNumber ( lhsStr.Value.Trim ( ) );
                        else if ( lhsNode is NumberExpression lhsNum )
                            lhs = lhsNum.Value;

                        if ( rhsNode is StringExpression rhsStr )
                            rhs = StringToNumber ( rhsStr.Value.Trim ( ) );
                        else if ( rhsNode is NumberExpression rhsNum )
                            rhs = rhsNum.Value;

                        if ( lhs == null || rhs == null )
                        {
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to perform arithmetic on non-number(s)." ) );
                            return node;
                        }

                        return GetNumberExpression ( node, ( Int64 ) lhs | ( Int64 ) rhs );
                    }

                    case "&":
                    {
                        Double? lhs = null, rhs = null;

                        if ( lhsNode is StringExpression lhsStr )
                            lhs = StringToNumber ( lhsStr.Value.Trim ( ) );
                        else if ( lhsNode is NumberExpression lhsNum )
                            lhs = lhsNum.Value;

                        if ( rhsNode is StringExpression rhsStr )
                            rhs = StringToNumber ( rhsStr.Value.Trim ( ) );
                        else if ( rhsNode is NumberExpression rhsNum )
                            rhs = rhsNum.Value;

                        if ( lhs == null || rhs == null )
                        {
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to perform arithmetic on non-number(s)." ) );
                            return node;
                        }

                        return GetNumberExpression ( node, ( Int64 ) lhs & ( Int64 ) rhs );
                    }

                    case "%":
                    {
                        Double? lhs = null, rhs = null;

                        if ( lhsNode is StringExpression lhsStr )
                            lhs = StringToNumber ( lhsStr.Value.Trim ( ) );
                        else if ( lhsNode is NumberExpression lhsNum )
                            lhs = lhsNum.Value;

                        if ( rhsNode is StringExpression rhsStr )
                            rhs = StringToNumber ( rhsStr.Value.Trim ( ) );
                        else if ( rhsNode is NumberExpression rhsNum )
                            rhs = rhsNum.Value;

                        if ( lhs == null || rhs == null )
                        {
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to perform arithmetic on non-number(s)." ) );
                            return node;
                        }

                        return GetNumberExpression ( node, ( Double ) ( lhs % rhs ) );
                    }

                    case "/":
                    {
                        Double? lhs = null, rhs = null;

                        if ( lhsNode is StringExpression lhsStr )
                            lhs = StringToNumber ( lhsStr.Value.Trim ( ) );
                        else if ( lhsNode is NumberExpression lhsNum )
                            lhs = lhsNum.Value;

                        if ( rhsNode is StringExpression rhsStr )
                            rhs = StringToNumber ( rhsStr.Value.Trim ( ) );
                        else if ( rhsNode is NumberExpression rhsNum )
                            rhs = rhsNum.Value;

                        if ( lhs == null || rhs == null )
                        {
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to perform arithmetic on non-number(s)." ) );
                            return node;
                        }

                        return GetNumberExpression ( node, ( Double ) ( lhs / rhs ) );
                    }

                    case "*":
                    {
                        Double? lhs = null, rhs = null;

                        if ( lhsNode is StringExpression lhsStr )
                            lhs = StringToNumber ( lhsStr.Value.Trim ( ) );
                        else if ( lhsNode is NumberExpression lhsNum )
                            lhs = lhsNum.Value;

                        if ( rhsNode is StringExpression rhsStr )
                            rhs = StringToNumber ( rhsStr.Value.Trim ( ) );
                        else if ( rhsNode is NumberExpression rhsNum )
                            rhs = rhsNum.Value;

                        if ( lhs == null || rhs == null )
                        {
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to perform arithmetic on non-number(s)." ) );
                            return node;
                        }

                        return GetNumberExpression ( node, ( Double ) ( lhs * rhs ) );
                    }

                    case "^":
                    {
                        Double? lhs = null, rhs = null;

                        if ( lhsNode is StringExpression lhsStr )
                            lhs = StringToNumber ( lhsStr.Value.Trim ( ) );
                        else if ( lhsNode is NumberExpression lhsNum )
                            lhs = lhsNum.Value;

                        if ( rhsNode is StringExpression rhsStr )
                            rhs = StringToNumber ( rhsStr.Value.Trim ( ) );
                        else if ( rhsNode is NumberExpression rhsNum )
                            rhs = rhsNum.Value;

                        if ( lhs == null || rhs == null )
                        {
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to perform arithmetic on non-number(s)." ) );
                            return node;
                        }

                        return GetNumberExpression ( node, Math.Pow ( lhs.Value, rhs.Value ) );
                    }

                    case "..":
                    {
                        String lhs = null, rhs = null;

                        if ( lhsNode is StringExpression lhsStr )
                            lhs = lhsStr.UnescapedValue;
                        else if ( lhsNode is NumberExpression lhsNum )
                            lhs = lhsNum.Value.ToString ( );

                        if ( rhsNode is StringExpression rhsStr )
                            rhs = rhsStr.UnescapedValue;
                        else if ( rhsNode is NumberExpression rhsNum )
                            rhs = rhsNum.Value.ToString ( );

                        if ( lhs == null || rhs == null )
                        {
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to perform arithmetic on non-number(s)." ) );
                            return node;
                        }

                        return GetStringExpression ( node, lhs + rhs );
                    }

                    case "==":
                    {
                        // number == number
                        if ( lhsNode is NumberExpression lhsNum && rhsNode is NumberExpression rhsNum )
                            return GetBooleanExpression ( node, lhsNum.Value == rhsNum.Value );
                        // string == string
                        else if ( lhsNode is StringExpression lhsStr && rhsNode is StringExpression rhsStr )
                            return GetBooleanExpression ( node, lhsStr.Value == rhsStr.Value );
                        // nil == nil
                        else if ( lhsNode is NilExpression && rhsNode is NilExpression )
                            return GetBooleanExpression ( node, true );
                        // bool == bool
                        else if ( lhsNode is BooleanExpression lhsBool && rhsNode is BooleanExpression rhsBool )
                            return GetBooleanExpression ( node, lhsBool.Value == rhsBool.Value );
                        // {} == {} (always false)
                        else if ( lhsNode is TableConstructorExpression && rhsNode is TableConstructorExpression )
                            return GetBooleanExpression ( node, false );
                        // diff constant types are always not equal
                        else if ( lhsNode.GetType ( ) != rhsNode.GetType ( ) )
                            return GetBooleanExpression ( node, false );
                        return node;
                    }

                    case "!=":
                    case "~=":
                    {
                        // number == number
                        if ( lhsNode is NumberExpression lhsNum && rhsNode is NumberExpression rhsNum )
                            return GetBooleanExpression ( node, lhsNum.Value != rhsNum.Value );
                        // string == string
                        else if ( lhsNode is StringExpression lhsStr && rhsNode is StringExpression rhsStr )
                            return GetBooleanExpression ( node, lhsStr.Value != rhsStr.Value );
                        // nil == nil
                        else if ( lhsNode is NilExpression && rhsNode is NilExpression )
                            return GetBooleanExpression ( node, false );
                        // bool == bool
                        else if ( lhsNode is BooleanExpression lhsBool && rhsNode is BooleanExpression rhsBool )
                            return GetBooleanExpression ( node, lhsBool.Value != rhsBool.Value );
                        // {} == {} (always false)
                        else if ( lhsNode is TableConstructorExpression && rhsNode is TableConstructorExpression )
                            return GetBooleanExpression ( node, true );
                        // diff constant types are always not equal
                        else if ( lhsNode.GetType ( ) != rhsNode.GetType ( ) )
                            return GetBooleanExpression ( node, true );
                        return node;
                    }

                    case "<":
                    {
                        // number < number
                        if ( lhsNode is NumberExpression lhsNum && rhsNode is NumberExpression rhsNum )
                            return GetBooleanExpression ( node, lhsNum.Value < rhsNum.Value );
                        // string < string
                        else if ( lhsNode is StringExpression lhsStr && rhsNode is StringExpression rhsStr )
                            return GetBooleanExpression ( node, lhsStr.Value.CompareTo ( rhsStr.Value ) == -1 );
                        // nil < nil
                        else if ( lhsNode is NilExpression || rhsNode is NilExpression )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to compare a value with nil" ) );
                        // bool < bool
                        else if ( lhsNode is BooleanExpression lhsBool || rhsNode is BooleanExpression rhsBool )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to compare a value with a boolean" ) );
                        // {} < {}
                        else if ( lhsNode is TableConstructorExpression || rhsNode is TableConstructorExpression )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to compare a value with a table" ) );

                        return node;
                    }

                    case "<=":
                    {
                        // number <= number
                        if ( lhsNode is NumberExpression lhsNum && rhsNode is NumberExpression rhsNum )
                            return GetBooleanExpression ( node, lhsNum.Value <= rhsNum.Value );
                        // string <= string
                        else if ( lhsNode is StringExpression lhsStr && rhsNode is StringExpression rhsStr )
                            return GetBooleanExpression ( node, lhsStr.Value.CompareTo ( rhsStr.Value ) < 1 );
                        // nil <= nil
                        else if ( lhsNode is NilExpression || rhsNode is NilExpression )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to compare a value with nil" ) );
                        // bool <= bool
                        else if ( lhsNode is BooleanExpression lhsBool || rhsNode is BooleanExpression rhsBool )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to compare a value with a boolean" ) );
                        // {} <= {}
                        else if ( lhsNode is TableConstructorExpression || rhsNode is TableConstructorExpression )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to compare a value with a table" ) );

                        return node;
                    }

                    case ">":
                    {
                        // number > number
                        if ( lhsNode is NumberExpression lhsNum && rhsNode is NumberExpression rhsNum )
                            return GetBooleanExpression ( node, lhsNum.Value > rhsNum.Value );
                        // string > string
                        else if ( lhsNode is StringExpression lhsStr && rhsNode is StringExpression rhsStr )
                            return GetBooleanExpression ( node, lhsStr.Value.CompareTo ( rhsStr.Value ) == 1 );
                        // nil > nil
                        else if ( lhsNode is NilExpression || rhsNode is NilExpression )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to compare a value with nil" ) );
                        // bool > bool
                        else if ( lhsNode is BooleanExpression lhsBool || rhsNode is BooleanExpression rhsBool )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to compare a value with a boolean" ) );
                        // {} > {}
                        else if ( lhsNode is TableConstructorExpression || rhsNode is TableConstructorExpression )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to compare a value with a table" ) );

                        return node;
                    }

                    case ">=":
                    {
                        // number >= number
                        if ( lhsNode is NumberExpression lhsNum && rhsNode is NumberExpression rhsNum )
                            return GetBooleanExpression ( node, lhsNum.Value >= rhsNum.Value );
                        // string >= string
                        else if ( lhsNode is StringExpression lhsStr && rhsNode is StringExpression rhsStr )
                            return GetBooleanExpression ( node, lhsStr.Value.CompareTo ( rhsStr.Value ) > -1 );
                        // nil >= nil
                        else if ( lhsNode is NilExpression || rhsNode is NilExpression )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to compare a value with nil" ) );
                        // bool >= bool
                        else if ( lhsNode is BooleanExpression lhsBool || rhsNode is BooleanExpression rhsBool )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to compare a value with a boolean" ) );
                        // {} >= {}
                        else if ( lhsNode is TableConstructorExpression || rhsNode is TableConstructorExpression )
                            this.File.Errors.Add ( new Error ( ErrorType.Error, node, "Attempted to compare a value with a table" ) );

                        return node;
                    }

                    case "&&":
                    case "and":
                    {
                        Boolean lhs, rhs;

                        if ( lhsNode is BooleanExpression lhsBool )
                            lhs = lhsBool.Value;
                        else if ( lhsNode is NilExpression )
                            lhs = false;
                        else
                            lhs = true;

                        if ( rhsNode is BooleanExpression rhsBool )
                            rhs = rhsBool.Value;
                        else if ( rhsNode is NilExpression )
                            rhs = false;
                        else
                            rhs = true;

                        return GetBooleanExpression ( node, lhs && rhs );
                    }

                    case "||":
                    case "or":
                    {
                        Boolean lhs, rhs;

                        if ( lhsNode is BooleanExpression lhsBool )
                            lhs = lhsBool.Value;
                        else if ( lhsNode is NilExpression )
                            lhs = false;
                        else
                            lhs = true;

                        if ( rhsNode is BooleanExpression rhsBool )
                            rhs = rhsBool.Value;
                        else if ( rhsNode is NilExpression )
                            rhs = false;
                        else
                            rhs = true;

                        return GetBooleanExpression ( node, lhs || rhs );
                    }
                }
            }

            return node;
        }
    }
}
