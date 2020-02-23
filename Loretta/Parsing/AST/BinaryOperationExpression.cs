using System;
using System.Collections.Generic;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class BinaryOperationExpression : Expression
    {
        public Expression Left { get; }
        public LuaToken Operator { get; }
        public Expression Right { get; }

        public override Boolean IsConstant => false;
        public override Object ConstantValue => throw new InvalidOperationException ( "This is not a constant node." );

        public BinaryOperationExpression ( Expression left, LuaToken op, Expression right )
        {
            this.Left = left ?? throw new ArgumentNullException ( nameof ( left ) );
            this.Operator = op;
            this.Right = right ?? throw new ArgumentNullException ( nameof ( right ) );
        }

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
                yield return this.Left;
                yield return this.Right;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitBinaryOperation ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitBinaryOperation ( this );
    }
}
