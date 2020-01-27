using System;
using System.Collections.Generic;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    public class BooleanExpression : Expression
    {
        private readonly Token<LuaTokenType> Token;
        public Boolean Value { get; }

        public override Boolean IsConstant => true;
        public override Object ConstantValue => this.Value;

        public BooleanExpression ( Token<LuaTokenType> token, Boolean value )
        {
            this.Value = value;
            this.Token = token;
        }

        public override IEnumerable<Token<LuaTokenType>> Tokens { get { yield return this.Token; } }
        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitBoolean ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitBoolean ( this );
    }
}
