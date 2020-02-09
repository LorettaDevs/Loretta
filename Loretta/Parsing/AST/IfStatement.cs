using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class IfClause : LuaASTNode
    {
        public Expression Condition { get; }
        public StatementList Body { get; }

        public IfClause ( LuaToken preCondTok, Expression condition, LuaToken thenKw, StatementList body )
        {
            if ( preCondTok == null )
                throw new ArgumentNullException ( nameof ( preCondTok ) );
            if ( thenKw == null )
                throw new ArgumentNullException ( nameof ( thenKw ) );

            this.Tokens = new[] { preCondTok, thenKw };
            this.Condition = condition ?? throw new ArgumentNullException ( nameof ( condition ) );
            this.Body = body ?? throw new ArgumentNullException ( nameof ( body ) );
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

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitNode ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitNode ( this );
    }

    public class IfStatement : Statement
    {
        public ImmutableArray<IfClause> Clauses { get; }

        public StatementList? ElseBlock { get; }

        public IfStatement ( IEnumerable<IfClause> clauses, LuaToken endTok )
        {
            this.Clauses = clauses.ToImmutableArray ( );
            this.Tokens = new[] { endTok };
        }

        public IfStatement ( IEnumerable<IfClause> clauses, LuaToken elseKw, StatementList elseBlock, LuaToken endTok )
        {
            this.Clauses = clauses.ToImmutableArray ( );
            this.ElseBlock = elseBlock;
            this.Tokens = new[] { elseKw, endTok };
        }

        public override IEnumerable<LuaToken> Tokens { get; }

        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                foreach ( IfClause clause in this.Clauses )
                    yield return clause;
                if ( this.ElseBlock is StatementList )
                    yield return this.ElseBlock;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitIfStatement ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitIfStatement ( this );
    }
}