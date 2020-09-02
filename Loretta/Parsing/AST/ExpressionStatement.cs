using System.Collections.Generic;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents an expression as a statement.
    /// </summary>
    public class ExpressionStatement : Statement
    {
        /// <summary>
        /// The expression being used as a statement.
        /// </summary>
        public Expression Expression { get; }

        /// <summary>
        /// Initializes a new expression statement.
        /// </summary>
        /// <param name="expression">The statement's expression.</param>
        public ExpressionStatement ( Expression expression )
        {
            this.Expression = expression ?? throw new System.ArgumentNullException ( nameof ( expression ) );
        }

        /// <inheritdoc />
        public override IEnumerable<Token<LuaTokenType>> Tokens => Enumerable.Empty<Token<LuaTokenType>> ( );

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children { get { yield return this.Expression; } }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitExpressionStatement ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitExpressionStatement ( this );
    }
}