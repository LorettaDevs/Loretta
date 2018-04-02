using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Functions
{
    public class NamedFunctionStatement : ASTStatement, IFunctionDefinition, IEquatable<NamedFunctionStatement>
    {
        public List<ASTNode> Arguments { get; } = new List<ASTNode> ( );

        public ASTNode Identifier { get; private set; }

        public StatementList Body { get; private set; }

        public NamedFunctionStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetIdentifier ( ASTNode ident )
        {
            if ( this.Children.Contains ( ident ) )
                this.RemoveChild ( ident );
            this.AddChild ( ident );

            this.Identifier = ident;
        }

        public void AddArgument ( ASTNode arg )
        {
            this.Arguments.Add ( arg );
            this.AddChild ( arg );
        }

        public void RemoveArgument ( ASTNode arg )
        {
            this.Arguments.Remove ( arg );
            this.RemoveChild ( arg );
        }

        public void SetArguments ( IEnumerable<ASTNode> args )
        {
            foreach ( ASTNode argument in this.Arguments )
                this.RemoveChild ( argument );
            this.Arguments.Clear ( );
            foreach ( ASTNode argument in args )
                this.AddArgument ( argument );
        }

        public void SetBody ( StatementList body )
        {
            if ( this.Children.Contains ( body ) )
                this.RemoveChild ( body );
            this.AddChild ( body );

            this.Body = body;
        }

        public override ASTNode Clone ( )
        {
            var func = new NamedFunctionStatement ( this.Parent,  this.Scope, this.CloneTokenList ( ) );
            func.SetIdentifier ( this.Identifier.Clone ( ) );
            func.SetBody ( ( StatementList ) this.Body.Clone ( ) );
            foreach ( ASTNode arg in this.Arguments )
                func.AddArgument ( arg );
            return func;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as NamedFunctionStatement );
        }

        public Boolean Equals ( NamedFunctionStatement other )
        {
            if ( other == null || !EqualityComparer<ASTNode>.Default.Equals ( this.Identifier, other.Identifier )
                || !EqualityComparer<StatementList>.Default.Equals ( this.Body, other.Body )
                || this.Arguments.Count != other.Arguments.Count )
                return false;
            for ( var i = 0; i < this.Arguments.Count; i++ )
                if ( this.Arguments[i] != other.Arguments[i] )
                    return false;
            return true;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = 1116385874;
            foreach ( ASTNode node in this.Arguments )
                hashCode *= -1521134295 + node.GetHashCode ( );
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.Identifier );
            hashCode = hashCode * -1521134295 + EqualityComparer<StatementList>.Default.GetHashCode ( this.Body );
            return hashCode;
        }

        public static Boolean operator == ( NamedFunctionStatement statement1, NamedFunctionStatement statement2 ) => EqualityComparer<NamedFunctionStatement>.Default.Equals ( statement1, statement2 );

        public static Boolean operator != ( NamedFunctionStatement statement1, NamedFunctionStatement statement2 ) => !( statement1 == statement2 );

        #endregion Generated Code
    }
}
