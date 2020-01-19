using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class LocalVariableDeclarationStatement : Statement
    {
        public ImmutableArray<IdentifierExpression> Identifiers { get; }
        public ImmutableArray<Expression> Values { get; }

        public LocalVariableDeclarationStatement ( LuaToken localKw, IEnumerable<IdentifierExpression> identifiers, IEnumerable<LuaToken> commas )
        {
            if ( localKw == null )
                throw new System.ArgumentNullException ( nameof ( localKw ) );
            if ( identifiers == null )
                throw new System.ArgumentNullException ( nameof ( identifiers ) );
            if ( commas == null )
                throw new System.ArgumentNullException ( nameof ( commas ) );

            var toks         = new List<LuaToken> { localKw };
            toks.AddRange ( commas );
            this.Tokens      = toks;
            this.Identifiers = identifiers.ToImmutableArray ( );
            this.Values      = ImmutableArray<Expression>.Empty;
        }

        public LocalVariableDeclarationStatement ( LuaToken localKw,
                                                   IEnumerable<IdentifierExpression> identifiers,
                                                   IEnumerable<LuaToken> identifiersCommas,
                                                   LuaToken equals,
                                                   IEnumerable<Expression> values,
                                                   IEnumerable<LuaToken> valuesCommas )
        {
            if ( localKw == null )
                throw new System.ArgumentNullException ( nameof ( localKw ) );
            if ( identifiers == null )
                throw new System.ArgumentNullException ( nameof ( identifiers ) );
            if ( identifiersCommas == null )
                throw new System.ArgumentNullException ( nameof ( identifiersCommas ) );
            if ( equals == null )
                throw new System.ArgumentNullException ( nameof ( equals ) );
            if ( values == null )
                throw new System.ArgumentNullException ( nameof ( values ) );
            if ( valuesCommas == null )
                throw new System.ArgumentNullException ( nameof ( valuesCommas ) );

            var toks         = new List<LuaToken> { localKw };
            toks.AddRange ( identifiersCommas );
            toks.Add ( equals );
            toks.AddRange ( valuesCommas );
            this.Tokens      = toks;
            this.Identifiers = identifiers.ToImmutableArray ( );
            this.Values      = values.ToImmutableArray ( );
        }

        public override IEnumerable<LuaToken> Tokens { get; }

        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                foreach ( IdentifierExpression ident in this.Identifiers )
                    yield return ident;
                foreach ( Expression val in this.Values )
                    yield return val;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitLocalVariableDeclaration ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitLocalVariableDeclaration ( this );
    }
}
