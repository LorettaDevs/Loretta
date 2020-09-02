using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a list of statements.
    /// </summary>
    public class StatementList : Statement
    {
        /// <summary>
        /// The scope.
        /// </summary>
        public readonly Scope Scope;

        /// <summary>
        /// The statements in this list.
        /// </summary>
        public readonly ImmutableArray<Statement> Body;

        /// <summary>
        /// Initializes a list of statement.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="statements">The statements in this list.</param>
        public StatementList ( Scope scope, IEnumerable<Statement> statements )
        {
            this.Scope = scope;
            this.Body = statements.ToImmutableArray ( );
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens => Enumerable.Empty<LuaToken> ( );

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children => this.Body;

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitStatementList ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitStatementList ( this );
    }
}