using System;
using System.Collections.Generic;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class NumericForLoopStatement : Statement
    {
        public Scope Scope { get; }
        public IdentifierExpression Variable { get; }
        public Expression Initial { get; }
        public Expression Final { get; }
        public Expression? Step { get; }
        public StatementList Body { get; }

        public NumericForLoopStatement ( Scope scope, LuaToken forKw, IdentifierExpression variable, LuaToken equals, Expression initial, LuaToken comma1, Expression final, LuaToken doKw, StatementList body, LuaToken endKw )
        {
            this.Scope = scope ?? throw new ArgumentNullException ( nameof ( scope ) );
            this.Variable = variable ?? throw new ArgumentNullException ( nameof ( variable ) );
            this.Initial = initial ?? throw new ArgumentNullException ( nameof ( initial ) );
            this.Final = final ?? throw new ArgumentNullException ( nameof ( final ) );
            this.Step = null;
            this.Body = body ?? throw new ArgumentNullException ( nameof ( body ) );
            this.Tokens = new[] { forKw, equals, comma1, doKw, endKw };
        }

        public NumericForLoopStatement ( Scope scope, LuaToken forKw, IdentifierExpression variable, LuaToken equals, Expression initial, LuaToken comma1, Expression final, LuaToken comma2, Expression step, LuaToken doKw, StatementList body, LuaToken endKw )
        {
            this.Scope = scope ?? throw new ArgumentNullException ( nameof ( scope ) );
            this.Variable = variable ?? throw new ArgumentNullException ( nameof ( variable ) );
            this.Initial = initial ?? throw new ArgumentNullException ( nameof ( initial ) );
            this.Final = final ?? throw new ArgumentNullException ( nameof ( final ) );
            this.Step = step ?? throw new ArgumentNullException ( nameof ( step ) );
            this.Body = body ?? throw new ArgumentNullException ( nameof ( body ) );
            this.Tokens = new[] { forKw, equals, comma1, comma2, doKw, endKw };
        }

        public override IEnumerable<LuaToken> Tokens { get; }

        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                yield return this.Variable;
                yield return this.Initial;
                yield return this.Final;
                if ( !( this.Step is null ) )
                    yield return this.Step;
                yield return this.Body;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitNumericFor ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitNumericFor ( this );
    }
}