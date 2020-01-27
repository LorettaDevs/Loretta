using System;
using System.Collections.Generic;
using System.Linq;
using Loretta.Parsing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class IdentifierExpression : Expression
    {
        private readonly LuaToken Token;

        public readonly Variable Variable;
        public String Identifier => this.Variable?.Identifier ?? ( String ) this.Token.Value;

        public override Boolean IsConstant => false;
        public override Object ConstantValue => throw new InvalidOperationException ( "This is not a constant node." );

        public IdentifierExpression ( LuaToken token, Variable variable )
        {
            this.Token = token;
            this.Variable = variable;
        }

        public override IEnumerable<LuaToken> Tokens
        {
            get
            {
                yield return this.Token;
            }
        }

        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitIdentifier ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitIdentifier ( this );
    }
}
