using System;
using System.Collections.Generic;
using System.Linq;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class StringExpression : Expression
    {
        private readonly LuaToken Token;

        public Boolean IsLong => this.Token.Type == LuaTokenType.LongString;
        public String Value => ( String ) this.Token.Value;

        public StringExpression ( LuaToken token )
        {
            this.Token = token;
        }

        public override IEnumerable<LuaToken> Tokens
        {
            get
            {
                yield return this.Token;
            }
        }

        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitString ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitString ( this );
    }
}
