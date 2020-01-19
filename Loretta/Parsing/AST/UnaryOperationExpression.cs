using System;
using System.Collections.Generic;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public enum UnaryOperationFix
    {
        Prefix,
        Postfix
    }

    public class UnaryOperationExpression : Expression
    {
        public UnaryOperationFix Fix { get; }
        public LuaToken Operator { get; }
        public Expression Operand { get; }

        public UnaryOperationExpression ( UnaryOperationFix fix, LuaToken op, Expression expr )
        {
            this.Fix = fix;
            this.Operator = op;
            this.Operand = expr ?? throw new ArgumentNullException ( nameof ( expr ) );
        }

        #region LuaASTNode

        public override IEnumerable<LuaToken> Tokens
        {
            get
            {
                yield return this.Operator;
            }
        }

        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                yield return this.Operand;
            }
        }

        #endregion LuaASTNode

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitUnaryOperation ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitUnaryOperation ( this );
    }
}