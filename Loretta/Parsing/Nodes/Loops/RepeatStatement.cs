using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Loops
{
    public class RepeatStatement : ASTStatement, IEquatable<RepeatStatement>
    {
        public ASTNode Condition { get; private set; }

        public StatementList Body { get; private set; }

        public RepeatStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetCondition ( ASTNode cond )
        {
            if ( this.Condition != null )
                this.RemoveChild ( this.Condition );
            this.AddChild ( cond );
            this.Condition = cond;
        }

        public void SetBody ( StatementList body )
        {
            if ( this.Body != null )
                this.RemoveChild ( this.Body );
            this.AddChild ( body );
            this.Body = body;
        }

        public override ASTNode Clone ( )
        {
            var repeat = new RepeatStatement ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            repeat.SetCondition ( this.Condition.Clone ( ) );
            repeat.SetBody ( ( StatementList ) this.Body.Clone ( ) );
            return repeat;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as RepeatStatement );
        }

        public Boolean Equals ( RepeatStatement other )
        {
            return other != null &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.Condition, other.Condition ) &&
                    EqualityComparer<StatementList>.Default.Equals ( this.Body, other.Body );
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = -2019592797;
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.Condition );
            hashCode = hashCode * -1521134295 + EqualityComparer<StatementList>.Default.GetHashCode ( this.Body );
            return hashCode;
        }

        public static Boolean operator == ( RepeatStatement statement1, RepeatStatement statement2 ) => EqualityComparer<RepeatStatement>.Default.Equals ( statement1, statement2 );

        public static Boolean operator != ( RepeatStatement statement1, RepeatStatement statement2 ) => !( statement1 == statement2 );

        #endregion Generated Code
    }
}
