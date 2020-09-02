using System.Collections.Generic;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a repeat until statement.
    /// </summary>
    public class RepeatUntilStatement : Statement
    {
        /// <summary>
        /// The scope.
        /// </summary>
        public Scope Scope { get; }

        /// <summary>
        /// The body.
        /// </summary>
        public StatementList Body { get; }

        /// <summary>
        /// The condition.
        /// </summary>
        public Expression Condition { get; }

        /// <summary>
        /// Initializes a new repeat until statement.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="repeatKw">The repeat keyword.</param>
        /// <param name="body">The body.</param>
        /// <param name="untilKw">The until keyword.</param>
        /// <param name="condition">The condition expression.</param>
        public RepeatUntilStatement ( Scope scope, LuaToken repeatKw, StatementList body, LuaToken untilKw, Expression condition )
        {
            this.Scope = scope;
            this.Body = body;
            this.Condition = condition;
            this.Tokens = new[] { repeatKw, untilKw };
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get; }

        /// <inheritdoc />
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