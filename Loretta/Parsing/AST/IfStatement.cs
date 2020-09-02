using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents an if clause.
    /// </summary>
    public class IfClause : LuaASTNode
    {
        /// <summary>
        /// The clause's condition.
        /// </summary>
        public Expression Condition { get; }

        /// <summary>
        /// The clause's body.
        /// </summary>
        public StatementList Body { get; }

        /// <summary>
        /// Initializes a new if clause.
        /// </summary>
        /// <param name="preCondTok">The clause's if/elseif token.</param>
        /// <param name="condition">The clause's condition.</param>
        /// <param name="thenKw">The clause's then token.</param>
        /// <param name="body">The clause's body.</param>
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

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get; }

        /// <inheritdoc />
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

    /// <summary>
    /// Represents an if statement.
    /// </summary>
    public class IfStatement : Statement
    {
        /// <summary>
        /// The if/elseif statement clauses.
        /// </summary>
        public ImmutableArray<IfClause> Clauses { get; }

        /// <summary>
        /// The else block body.
        /// </summary>
        public StatementList? ElseBlock { get; }

        /// <summary>
        /// Initializes a new if statement.
        /// </summary>
        /// <param name="clauses">The if clauses.</param>
        /// <param name="endTok">The end keyword token.</param>
        public IfStatement ( IEnumerable<IfClause> clauses, LuaToken endTok )
        {
            this.Clauses = clauses.ToImmutableArray ( );
            this.Tokens = new[] { endTok };
        }

        /// <summary>
        /// Initializes a new if statemnet.
        /// </summary>
        /// <param name="clauses">The if clauses</param>
        /// <param name="elseKw">The else keyword.</param>
        /// <param name="elseBlock">The else block.</param>
        /// <param name="endTok">The end token.</param>
        public IfStatement ( IEnumerable<IfClause> clauses, LuaToken elseKw, StatementList elseBlock, LuaToken endTok )
        {
            this.Clauses = clauses.ToImmutableArray ( );
            this.ElseBlock = elseBlock;
            this.Tokens = new[] { elseKw, endTok };
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get; }

        /// <inheritdoc />
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