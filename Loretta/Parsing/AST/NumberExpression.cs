using System;
using System.Collections.Generic;
using System.Linq;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class NumberExpression : Expression
    {
        private readonly LuaToken Token;
        public readonly Double Value;

        public override Boolean IsConstant => true;
        public override Object ConstantValue => this.Value;

        public NumberExpression ( LuaToken token )
        {
            this.Token = token;
            this.Value = ( Double ) token.Value!;
        }

        public override IEnumerable<LuaToken> Tokens
        {
            get
            {
                yield return this.Token;
            }
        }

        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitNumber ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitNumber ( this );
    }
}
