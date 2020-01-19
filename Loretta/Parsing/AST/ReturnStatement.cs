using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class ReturnStatement : Statement
    {
        public ImmutableArray<Expression> Values { get; }

        public ReturnStatement ( LuaToken returnKw, IEnumerable<Expression> values, IEnumerable<LuaToken> commas )
        {
            if ( returnKw == null )
                throw new System.ArgumentNullException ( nameof ( returnKw ) );
            if ( values == null )
                throw new System.ArgumentNullException ( nameof ( values ) );
            if ( commas == null )
                throw new System.ArgumentNullException ( nameof ( commas ) );

            this.Values = values.ToImmutableArray ( );
            this.Children = this.Values;
            var toks = new List<LuaToken> { returnKw };
            toks.AddRange ( commas );
            this.Tokens = toks;
        }

        public override IEnumerable<LuaToken> Tokens { get; }
        public override IEnumerable<LuaASTNode> Children { get; }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitReturn ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitReturn ( this );
    }
}