using System;
using System.Collections.Generic;
using Loretta.Lexing;
using Loretta.Parsing.Nodes.Constants;

namespace Loretta.Parsing.Nodes.Functions
{
    public class StringFunctionCallExpression : ASTStatement, IFunctionCall, IEquatable<StringFunctionCallExpression>
    {
        public ASTNode Base { get; private set; } = null;

        public StringExpression Argument { get; private set; }

        public StringFunctionCallExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetArgument ( StringExpression arg )
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
            var call = new StringFunctionCallExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            call.SetBase ( this.Base.Clone ( ) );
            call.SetArgument ( ( StringExpression ) this.Argument.Clone ( ) );
            return call;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as StringFunctionCallExpression );
        }

        public Boolean Equals ( StringFunctionCallExpression other )
        {
            return other != null &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.Base, other.Base ) &&
                    EqualityComparer<StringExpression>.Default.Equals ( this.Argument, other.Argument );
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = -2020579314;
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.Base );
            hashCode = hashCode * -1521134295 + EqualityComparer<StringExpression>.Default.GetHashCode ( this.Argument );
            return hashCode;
        }

        public static Boolean operator == ( StringFunctionCallExpression expression1, StringFunctionCallExpression expression2 ) => EqualityComparer<StringFunctionCallExpression>.Default.Equals ( expression1, expression2 );

        public static Boolean operator != ( StringFunctionCallExpression expression1, StringFunctionCallExpression expression2 ) => !( expression1 == expression2 );

        #endregion Generated Code
    }
}
