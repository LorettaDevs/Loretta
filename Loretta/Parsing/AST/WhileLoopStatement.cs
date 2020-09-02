using System.Collections.Generic;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a while loop statement.
    /// </summary>
    public class WhileLoopStatement : Statement
    {
        /// <summary>
        /// The loop's condition.
        /// </summary>
        public Expression Condition { get; }

        /// <summary>
        /// The loop's body.
        /// </summary>
        public StatementList Body { get; }

        /// <summary>
        /// Initializes a new while loop statement.
        /// </summary>
        /// <param name="whileKw">The while keyword token.</param>
        /// <param name="condition">The loop's condition.</param>
        /// <param name="doKw">The do keyword token.</param>
        /// <param name="body">The loop's body.</param>
        /// <param name="endKw">The end keyword token.</param>
        public WhileLoopStatement (
            LuaToken whileKw,
            Expression condition,
            LuaToken doKw,
            StatementList body,
            LuaToken endKw )
        {
            this.Condition = condition;
            this.Body = body;
            this.Tokens = new[] { whileKw, doKw, endKw };
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

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitWhileLoop ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitWhileLoop ( this );
    }
}