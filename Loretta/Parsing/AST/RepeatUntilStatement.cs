using System.Collections.Generic;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class RepeatUntilStatement : Statement
    {
        public Scope Scope { get; }
        public StatementList Body { get; }
        public Expression Condition { get; }

        public RepeatUntilStatement ( Scope scope, LuaToken repeatKw, StatementList body, LuaToken untilKw, Expression condition )
        {
            this.Scope = scope;
            this.Body = body;
            this.Condition = condition;
            this.Tokens = new[] { repeatKw, untilKw };
        }

        public override IEnumerable<LuaToken> Tokens { get; }

        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                yield return this.Body;
                yield return this.Condition;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitRepeatUntil ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitRepeatUntil ( this );
    }
}