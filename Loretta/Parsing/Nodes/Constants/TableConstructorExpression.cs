using System;
using System.Collections.Generic;
using GParse.Lexing;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Constants
{
    public class TableConstructorExpression : ASTNode, IEquatable<TableConstructorExpression>
    {
        public IList<TableKeyValue> Fields { get; }

        public Int32 SequentialIndex { get; private set; }

        public TableConstructorExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
            this.Fields = new List<TableKeyValue> ( );
            this.SequentialIndex = 0;
        }

        public TableKeyValue AddExplicitKeyField ( ASTNode key, ASTNode value, String separator, IList<LToken> tokenList )
        {
            TableKeyValue keyValue;
            this.Fields.Add ( keyValue = new TableKeyValue ( this, this.Scope, tokenList )
            {
                Key = key,
                Value = value,
                Separator = separator
            } );
            this.AddChild ( keyValue );

            return keyValue;
        }

        public TableKeyValue AddSequentialField ( ASTNode value, String separator, IList<LToken> tokenList )
        {
            TableKeyValue keyValue;
            this.Fields.Add ( keyValue = new TableKeyValue ( this, this.Scope, tokenList )
            {
                Value = value,
                Separator = separator,
                IsSequential = true
            } );
            this.AddChild ( keyValue );

            this.SequentialIndex++;
            keyValue.Key = new NumberExpression ( keyValue, keyValue.Scope, new[] {
                new LToken ( "number", this.SequentialIndex.ToString ( ), ( Double ) this.SequentialIndex, TokenType.Number, SourceRange.Zero )
            } );

            return keyValue;
        }

        public void SetFields ( IEnumerable<ASTNode> fields )
        {
            this.Children.Clear ( );
            this.Fields.Clear ( );
            this.SequentialIndex = 0;
            foreach ( TableKeyValue field in fields )
            {
                if ( field.IsSequential )
                    this.SequentialIndex++;
                this.AddChild ( field );
                this.Fields.Add ( field );
            }
        }

        public override ASTNode Clone ( )
        {
            var tbl = new TableConstructorExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) )
            {
                SequentialIndex = this.SequentialIndex
            };
            foreach ( TableKeyValue field in this.Fields )
            {
                var clone = ( TableKeyValue ) field.Clone ( );
                tbl.AddChild ( clone );
                tbl.Fields.Add ( clone );
            }
            return tbl;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as TableConstructorExpression );
        }

        public Boolean Equals ( TableConstructorExpression other )
        {
            if ( other == null || this.SequentialIndex != other.SequentialIndex
                || this.Fields.Count != other.Fields.Count )
                return false;
            for ( var i = 0; i < this.Fields.Count; i++ )
                if ( this.Fields[i] != other.Fields[i] )
                    return false;
            return true;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = 2001713582;
            foreach ( TableKeyValue field in this.Fields )
                hashCode *= -1521134295 + EqualityComparer<TableKeyValue>.Default.GetHashCode ( field );
            hashCode = hashCode * -1521134295 + this.SequentialIndex.GetHashCode ( );
            return hashCode;
        }

        public static Boolean operator == ( TableConstructorExpression expression1, TableConstructorExpression expression2 )
            => EqualityComparer<TableConstructorExpression>.Default.Equals ( expression1, expression2 );

        public static Boolean operator != ( TableConstructorExpression expression1, TableConstructorExpression expression2 )
            => !( expression1 == expression2 );

        #endregion Generated Code
    }

    public class TableKeyValue : ASTNode, IEquatable<TableKeyValue>
    {
        public ASTNode Key { get; set; }

        public ASTNode Value { get; set; }

        public String Separator { get; set; }

        public Boolean IsSequential { get; set; }

        public TableKeyValue ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public override ASTNode Clone ( )
        {
            return new TableKeyValue ( this.Parent, this.Scope, this.CloneTokenList ( ) )
            {
                Key = this.Key.Clone ( ),
                Value = this.Value.Clone ( ),
                Separator = this.Separator,
                IsSequential = this.IsSequential
            };
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as TableKeyValue );
        }

        public Boolean Equals ( TableKeyValue other )
        {
            return other != null &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.Key, other.Key ) &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.Value, other.Value ) &&
                     this.Separator == other.Separator &&
                     this.IsSequential == other.IsSequential;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = 736414032;
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.Key );
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.Value );
            hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode ( this.Separator );
            hashCode = hashCode * -1521134295 + this.IsSequential.GetHashCode ( );
            return hashCode;
        }

        public static Boolean operator == ( TableKeyValue value1, TableKeyValue value2 )
            => EqualityComparer<TableKeyValue>.Default.Equals ( value1, value2 );

        public static Boolean operator != ( TableKeyValue value1, TableKeyValue value2 )
            => !( value1 == value2 );

        #endregion Generated Code
    }
}
