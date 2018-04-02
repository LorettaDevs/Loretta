using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Functions
{
    public class FunctionCallExpression : ASTStatement, IFunctionCall, IEquatable<FunctionCallExpression>
    {
        public List<ASTNode> Arguments { get; } = new List<ASTNode> ( );

        public ASTNode Base { get; private set; } = null;

        public FunctionCallExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void AddArgument ( ASTNode expr )
        {
            this.Arguments.Add ( expr );
            this.AddChild ( expr );
        }

        public void SetArguments ( IEnumerable<ASTNode> arguments )
        {
            foreach ( ASTNode argument in this.Arguments )
                this.RemoveChild ( argument );
            this.Arguments.Clear ( );

            foreach ( ASTNode argument in arguments )
                this.AddArgument ( argument );
        }

        public void SetBase ( ASTNode @base )
        {
            if ( this.Children.Contains ( @base ) )
                this.RemoveChild ( @base );
            this.AddChild ( @base );

            this.Base = @base;
        }

        public override ASTNode Clone ( )
        {
            var func = new FunctionCallExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            func.SetBase ( this.Base.Clone ( ) );
            foreach ( ASTNode arg in this.Arguments )
                func.AddArgument ( arg.Clone ( ) );
            return func;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as FunctionCallExpression );
        }

        public Boolean Equals ( FunctionCallExpression other )
        {
            if ( other == null || !EqualityComparer<ASTNode>.Default.Equals ( this.Base, other.Base )
                || this.Arguments.Count != other.Arguments.Count )
                return false;
            for ( var i = 0; i < this.Arguments.Count; i++ )
                if ( this.Arguments[i] != other.Arguments[i] )
                    return false;
            return true;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = -1163320817;
            foreach ( ASTNode node in this.Arguments )
                hashCode *= -1521134295 + node.GetHashCode ( );
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.Base );
            return hashCode;
        }

        public static Boolean operator == ( FunctionCallExpression expression1, FunctionCallExpression expression2 ) => EqualityComparer<FunctionCallExpression>.Default.Equals ( expression1, expression2 );

        public static Boolean operator != ( FunctionCallExpression expression1, FunctionCallExpression expression2 ) => !( expression1 == expression2 );

        #endregion Generated Code
    }
}
