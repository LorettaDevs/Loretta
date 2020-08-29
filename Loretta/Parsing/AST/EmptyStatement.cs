using System;
using System.Collections.Generic;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    public class EmptyStatement : Statement
    {
        public override IEnumerable<Token<LuaTokenType>> Tokens
        {
            get
            {
                if ( this.Semicolon.HasValue )
                    yield return this.Semicolon.Value;
            }
        }

        public override IEnumerable<LuaASTNode> Children =>
            Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitEmptyStatement ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitEmptyStatement ( this );
    }
}