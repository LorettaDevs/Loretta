using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a local variable declaration.
    /// </summary>
    public class LocalVariableDeclarationStatement : Statement
    {
        /// <summary>
        /// The variable names being assigned to.
        /// </summary>
        public ImmutableArray<IdentifierExpression> Identifiers { get; }

        /// <summary>
        /// The values being assigned to the variables.
        /// </summary>
        public ImmutableArray<Expression> Values { get; }

        /// <summary>
        /// Initializes a new local variable declaration.
        /// </summary>
        /// <param name="localKw">The local keyword.</param>
        /// <param name="identifiers">The variable names.</param>
        /// <param name="commas">The variable names separator commas.</param>
        public LocalVariableDeclarationStatement ( LuaToken localKw, IEnumerable<IdentifierExpression> identifiers, IEnumerable<LuaToken> commas )
        {
            if ( identifiers == null )
                throw new System.ArgumentNullException ( nameof ( identifiers ) );
            if ( commas == null )
                throw new System.ArgumentNullException ( nameof ( commas ) );

            var toks = new List<LuaToken> { localKw };
            toks.AddRange ( commas );
            this.Tokens = toks;
            this.Identifiers = identifiers.ToImmutableArray ( );
            this.Values = ImmutableArray<Expression>.Empty;
        }

        /// <summary>
        /// Initializes a new local variable declaration.
        /// </summary>
        /// <param name="localKw">The local keyword.</param>
        /// <param name="identifiers">The variable names.</param>
        /// <param name="identifiersCommas">The variable names separator commas.</param>
        /// <param name="equals">The equals sign.</param>
        /// <param name="values">The values being assigned.</param>
        /// <param name="valuesCommas">The values separator commas.</param>
        public LocalVariableDeclarationStatement (
            LuaToken localKw,
            IEnumerable<IdentifierExpression> identifiers,
            IEnumerable<LuaToken> identifiersCommas,
            LuaToken equals,
            IEnumerable<Expression> values,
            IEnumerable<LuaToken> valuesCommas )
        {
            if ( identifiers == null )
                throw new System.ArgumentNullException ( nameof ( identifiers ) );
            if ( identifiersCommas == null )
                throw new System.ArgumentNullException ( nameof ( identifiersCommas ) );
            if ( values == null )
                throw new System.ArgumentNullException ( nameof ( values ) );
            if ( valuesCommas == null )
                throw new System.ArgumentNullException ( nameof ( valuesCommas ) );

            var toks = new List<LuaToken> { localKw };
            toks.AddRange ( identifiersCommas );
            toks.Add ( equals );
            toks.AddRange ( valuesCommas );
            this.Tokens = toks;
            this.Identifiers = identifiers.ToImmutableArray ( );
            this.Values = values.ToImmutableArray ( );
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get; }

        /// <inheritdoc />
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