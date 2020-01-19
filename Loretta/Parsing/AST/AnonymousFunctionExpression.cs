using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class AnonymousFunctionExpression : Expression
    {
        public ImmutableArray<Expression> Arguments { get; }
        public StatementList Body { get; }

        public AnonymousFunctionExpression ( IEnumerable<LuaToken> tokens, IEnumerable<Expression> arguments, StatementList body )
        {
            this.Tokens    = tokens.ToImmutableArray ( );
            this.Arguments = arguments.ToImmutableArray ( );
            this.Body      = body;
        }

        public override IEnumerable<LuaToken> Tokens { get; }

        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                foreach ( IdentifierExpression argument in this.Arguments )
                    yield return argument;
                yield return this.Body;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitAnonymousFunction ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitAnonymousFunction ( this );
    }
}
