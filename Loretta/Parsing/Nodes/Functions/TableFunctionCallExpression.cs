using System;
using System.Collections.Generic;
using Loretta.Lexing;
using Loretta.Parsing.Nodes.Constants;

namespace Loretta.Parsing.Nodes.Functions
{
    public class TableFunctionCallExpression : ASTStatement, IFunctionCall, IEquatable<TableFunctionCallExpression>
    {
        public TableConstructorExpression Argument { get; private set; } = null;

        public ASTNode Base { get; private set; } = null;

        public TableFunctionCallExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetArgument ( TableConstructorExpression arg )
        {
            if ( this.Argument != null )
                this.RemoveChild ( this.Argument );
            this.AddChild ( arg );
            this.Argument = arg;
        }

        public void SetBase ( ASTNode @base )
        {
            if ( this.Base != null )
                this.RemoveChild ( this.Base );
            this.AddChild ( @base );
            this.Base = @base;
        }

        public override ASTNode Clone ( )
        {
            var call = new TableFunctionCallExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            call.SetBase ( this.Base.Clone ( ) );
            call.SetArgument ( ( TableConstructorExpression ) this.Argument.Clone ( ) );
            return call;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as TableFunctionCallExpression );
        }

        public Boolean Equals ( TableFunctionCallExpression other )
        {
            return other != null &&
                    EqualityComparer<TableConstructorExpression>.Default.Equals ( this.Argument, other.Argument ) &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.Base, other.Base );
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = 988583102;
            hashCode = hashCode * -1521134295 + EqualityComparer<TableConstructorExpression>.Default.GetHashCode ( this.Argument );
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.Base );
            return hashCode;
        }

        public static Boolean operator == ( TableFunctionCallExpression expression1, TableFunctionCallExpression expression2 ) => EqualityComparer<TableFunctionCallExpression>.Default.Equals ( expression1, expression2 );

        public static Boolean operator != ( TableFunctionCallExpression expression1, TableFunctionCallExpression expression2 ) => !( expression1 == expression2 );

        #endregion Generated Code
    }
}
