using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class GenericForLoopStatement : Statement
    {
        public Scope Scope { get; }
        public ImmutableArray<IdentifierExpression> Variables { get; }
        public Expression Iteratable { get; }
        public StatementList Body { get; }

        public GenericForLoopStatement ( Scope scope, LuaToken forKw, IEnumerable<IdentifierExpression> variables, IEnumerable<LuaToken> commas, LuaToken inKw, Expression iteratable, LuaToken doKw, StatementList body, LuaToken endKw )
        {
            this.Scope = scope;
            this.Variables = variables.ToImmutableArray ( );
            this.Iteratable = iteratable;
            this.Body = body;

            var toks = new List<LuaToken> { forKw };
            toks.AddRange ( commas );
            toks.Add ( inKw ); toks.Add ( doKw ); toks.Add ( endKw );
            this.Tokens = toks;
        }

        public override IEnumerable<LuaToken> Tokens { get; }

        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                foreach ( IdentifierExpression var in this.Variables )
                    yield return var;
                yield return this.Iteratable;
                yield return this.Body;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitGenericFor ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitGenericFor ( this );
    }
}
