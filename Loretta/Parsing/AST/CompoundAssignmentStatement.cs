using System.Collections.Generic;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    public class CompoundAssignmentStatement : Statement
    {
        public Expression Assignee { get; }

        public Token<LuaTokenType> OperatorToken { get; }

        public Expression ValueExpression { get; }

        public CompoundAssignmentStatement ( Expression assignee, Token<LuaTokenType> operatorToken, Expression valueExpression )
        {
            this.Assignee = assignee;
            this.OperatorToken = operatorToken;
            this.ValueExpression = valueExpression;

            this.Tokens = this.Assignee.Tokens.Concat ( new[] { this.OperatorToken } )
                                              .Concat ( this.ValueExpression.Tokens );
        }

        public override IEnumerable<Token<LuaTokenType>> Tokens { get; }

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