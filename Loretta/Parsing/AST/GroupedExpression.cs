using System;
using System.Collections.Generic;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    public class GroupedExpression : Expression
    {
        public Expression InnerExpression { get; }

        public GroupedExpression ( Token<LuaTokenType> lparen, Expression inner, Token<LuaTokenType> rparen )
        {
            this.InnerExpression = inner ?? throw new ArgumentNullException ( nameof ( inner ) );
            this.Tokens = new[] { lparen, rparen };
        }

        public override IEnumerable<Token<LuaTokenType>> Tokens { get; }
        public override IEnumerable<LuaASTNode> Children { get { yield return this.InnerExpression; } }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitGroupedExpression ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitGroupedExpression ( this );
    }
}
