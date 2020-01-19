using System;
using System.Collections.Generic;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    public class VarArgExpression : Expression
    {
        private readonly Token<LuaTokenType> Token;

        public VarArgExpression ( Token<LuaTokenType> token )
        {
            if ( token.Type != LuaTokenType.VarArg )
                throw new ArgumentException ( "Argument is not a vararg token", nameof ( token ) );
            this.Token = token;
        }

        public override IEnumerable<Token<LuaTokenType>> Tokens { get { yield return this.Token; } }
        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitVarArg ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitVarArg ( this );
    }
}
