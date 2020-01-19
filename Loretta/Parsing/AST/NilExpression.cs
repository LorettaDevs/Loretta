using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    public class NilExpression : Expression
    {
        private readonly Token<LuaTokenType> Token;

        public NilExpression ( Token<LuaTokenType> tok )
        {
            this.Token = tok;
        }

        public override IEnumerable<Token<LuaTokenType>> Tokens
        {
            get
            {
                yield return this.Token;
            }
        }

        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitNil ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitNil ( this );
    }
}
