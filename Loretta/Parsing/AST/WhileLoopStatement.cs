using System.Collections.Generic;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class WhileLoopStatement : Statement
    {
        public Expression Condition { get; }
        public StatementList Body { get; }

        public WhileLoopStatement ( LuaToken whileKw, Expression condition, LuaToken doKw, StatementList body, LuaToken endKw )
        {
            this.Condition = condition;
            this.Body = body;
            this.Tokens = new[] { whileKw, doKw, endKw };
        }

        public override IEnumerable<LuaToken> Tokens { get; }

        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                yield return this.Condition;
                yield return this.Body;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitWhileLoop ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitWhileLoop ( this );
    }
}