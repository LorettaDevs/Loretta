using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a return statement.
    /// </summary>
    public class ReturnStatement : Statement
    {
        /// <summary>
        /// The values being returned.
        /// </summary>
        public ImmutableArray<Expression> Values { get; }

        /// <summary>
        /// Initializes a new return statement.
        /// </summary>
        /// <param name="returnKw">The return keyword token.</param>
        /// <param name="values">The values being returned.</param>
        /// <param name="commas">The values being returned comma separators.</param>
        public ReturnStatement ( LuaToken returnKw, IEnumerable<Expression> values, IEnumerable<LuaToken> commas )
        {
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

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get; }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children { get; }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitReturn ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitReturn ( this );
    }
}