using System.Collections.Generic;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a compound assignment statement.
    /// </summary>
    public class CompoundAssignmentStatement : Statement
    {
        /// <summary>
        /// The expression being assigned to.
        /// </summary>
        public Expression Assignee { get; }

        /// <summary>
        /// The compound assignment operator.
        /// </summary>
        public Token<LuaTokenType> OperatorToken { get; }

        /// <summary>
        /// The value being assigned.
        /// </summary>
        public Expression ValueExpression { get; }

        /// <summary>
        /// Initializes a new compound assignment statement.
        /// </summary>
        /// <param name="assignee">The expression being assigned to.</param>
        /// <param name="operatorToken">The compound assignment operator.</param>
        /// <param name="valueExpression">The expression value.</param>
        public CompoundAssignmentStatement ( Expression assignee, Token<LuaTokenType> operatorToken, Expression valueExpression )
        {
            this.Assignee = assignee;
            this.OperatorToken = operatorToken;
            this.ValueExpression = valueExpression;

            this.Tokens = new[] { this.OperatorToken };
        }

        /// <inheritdoc />
        public override IEnumerable<Token<LuaTokenType>> Tokens { get; }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                yield return this.Assignee;
                yield return this.ValueExpression;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitCompoundAssignmentStatement ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitCompoundAssignmentStatement ( this );
    }
}