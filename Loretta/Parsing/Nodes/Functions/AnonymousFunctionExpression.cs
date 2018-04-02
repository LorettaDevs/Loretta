using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Functions
{
    public class AnonymousFunctionExpression : ASTNode, IFunctionDefinition, IEquatable<AnonymousFunctionExpression>
    {
        public List<ASTNode> Arguments { get; } = new List<ASTNode> ( );

        public StatementList Body { get; private set; }

        public AnonymousFunctionExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetArguments ( IEnumerable<ASTNode> arguments )
        {
            foreach ( ASTNode argument in this.Arguments )
                this.RemoveChild ( argument );
            this.Arguments.Clear ( );
            foreach ( ASTNode argument in arguments )
                this.AddArgument ( argument );
        }

        public void AddArgument ( ASTNode arg )
        {
            this.Arguments.Add ( arg );
            this.AddChild ( arg );
        }

        public void SetBody ( StatementList body )
        {
            if ( this.Children.Contains ( body ) )
                this.RemoveChild ( body );
            if ( body != null )
                this.AddChild ( body );
            this.Body = body;
        }

        public override ASTNode Clone ( )
        {
            var func = new AnonymousFunctionExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            foreach ( ASTNode arg in this.Arguments )
                func.AddArgument ( arg.Clone ( ) );
            func.SetBody ( ( StatementList ) this.Body.Clone ( ) );
            return func;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as AnonymousFunctionExpression );
        }

        public Boolean Equals ( AnonymousFunctionExpression other )
        {
            if ( other == null || !EqualityComparer<StatementList>.Default.Equals ( this.Body, other.Body )
                || this.Arguments.Count != other.Arguments.Count )
                return false;
            for ( var i = 0; i < this.Arguments.Count; i++ )
                if ( this.Arguments[i] != other.Arguments[i] )
                    return false;
            return true;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = 1484195500;
            foreach ( ASTNode arg in this.Arguments )
                hashCode *= -1521134295 + arg.GetHashCode ( );
            hashCode *= -1521134295 + EqualityComparer<StatementList>.Default.GetHashCode ( this.Body );
            return hashCode;
        }

        public static Boolean operator == ( AnonymousFunctionExpression expression1, AnonymousFunctionExpression expression2 )
            => EqualityComparer<AnonymousFunctionExpression>.Default.Equals ( expression1, expression2 );

        public static Boolean operator != ( AnonymousFunctionExpression expression1, AnonymousFunctionExpression expression2 )
            => !( expression1 == expression2 );

        #endregion Generated Code
    }
}
