using System;
using System.Collections.Generic;
using Loretta.Lexing;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Parsing.Nodes.Functions
{
    public class LocalFunctionStatement : ASTStatement, IFunctionDefinition, IEquatable<LocalFunctionStatement>
    {
        public List<ASTNode> Arguments { get; } = new List<ASTNode> ( );

        public VariableExpression Identifier { get; private set; }

        public StatementList Body { get; private set; }

        public LocalFunctionStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetIdentifier ( VariableExpression ident )
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

        public void SetArguments ( IEnumerable<ASTNode> arguments )
        {
            foreach ( ASTNode argument in this.Arguments )
                this.RemoveChild ( argument );
            this.Arguments.Clear ( );
            foreach ( ASTNode argument in arguments )
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
            var func = new LocalFunctionStatement ( this.Parent, this.Scope, this.CloneTokenList() );
            func.SetIdentifier ( ( VariableExpression ) this.Identifier.Clone ( ) );
            func.SetBody ( ( StatementList ) this.Body.Clone ( ) );
            foreach ( ASTNode arg in this.Arguments )
                func.AddArgument ( arg );
            return func;
        }

        #region Genrated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as LocalFunctionStatement );
        }

        public Boolean Equals ( LocalFunctionStatement other )
        {
            if ( other == null || !EqualityComparer<VariableExpression>.Default.Equals ( this.Identifier, other.Identifier )
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
            hashCode = hashCode * -1521134295 + EqualityComparer<VariableExpression>.Default.GetHashCode ( this.Identifier );
            hashCode = hashCode * -1521134295 + EqualityComparer<StatementList>.Default.GetHashCode ( this.Body );
            return hashCode;
        }

        public static Boolean operator == ( LocalFunctionStatement statement1, LocalFunctionStatement statement2 ) => EqualityComparer<LocalFunctionStatement>.Default.Equals ( statement1, statement2 );

        public static Boolean operator != ( LocalFunctionStatement statement1, LocalFunctionStatement statement2 ) => !( statement1 == statement2 );

        #endregion Genrated Code
    }
}
