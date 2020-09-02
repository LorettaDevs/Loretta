using System;
using System.Collections.Generic;
using Loretta.Lexing;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// The type of indexing this is
    /// </summary>
    public enum IndexType
    {
        /// <summary>
        /// Indexing done with an expression ('[', expr, ']').
        /// </summary>
        Indexer,

        /// <summary>
        /// Indexing done with an identifier ('.' ident).
        /// </summary>
        Member,

        /// <summary>
        /// Function call done on an indexing done with an identifier (':' ident &lt;funccall&gt;).
        /// </summary>
        Method
    }

    /// <summary>
    /// Represents an indexing expression.
    /// </summary>
    public class IndexExpression : Expression
    {
        /// <summary>
        /// The type of indexing operation being done.
        /// </summary>
        public IndexType Type { get; }

        /// <summary>
        /// The expression being indexed.
        /// </summary>
        public Expression Indexee { get; }

        /// <summary>
        /// THe expression being used to index.
        /// </summary>
        public Expression Indexer { get; }

        /// <inheritdoc />
        public override Boolean IsConstant => false;

        /// <inheritdoc />
        public override Object ConstantValue => throw new InvalidOperationException ( "This is not a constant node." );

        private IndexExpression ( IEnumerable<LuaToken> tokens, IndexType type, Expression indexee, Expression indexer )
        {
            this.Tokens = tokens;
            this.Type = type;
            this.Indexee = indexee;
            this.Indexer = indexer;
        }

        /// <summary>
        /// Initializes a new expression-based indexing operation.
        /// </summary>
        /// <param name="indexee">The expresion being indexed.</param>
        /// <param name="lbracket">The left bracket token.</param>
        /// <param name="indexer">The expression being used to index.</param>
        /// <param name="rbracket">The right bracket token.</param>
        public IndexExpression ( Expression indexee, LuaToken lbracket, Expression indexer, LuaToken rbracket )
            : this ( new[] { lbracket, rbracket }, IndexType.Indexer, indexee, indexer )
        {
            if ( lbracket.Type != LuaTokenType.LBracket )
                throw new ArgumentException ( "LBracket must be a lbracket", nameof ( lbracket ) );
            if ( rbracket.Type != LuaTokenType.RBracket )
                throw new ArgumentException ( "RBracket must be a rbracket", nameof ( rbracket ) );
        }

        /// <summary>
        /// Initializes a new identifier-based indexing expression.
        /// </summary>
        /// <param name="indexee">The expression being indexed.</param>
        /// <param name="separator">The indexing operation separator.</param>
        /// <param name="indexer">The identifier being used to index.</param>
        public IndexExpression ( Expression indexee, LuaToken separator, IdentifierExpression indexer )
            : this ( new[] { separator }, separator.Type == LuaTokenType.Colon ? IndexType.Method : IndexType.Member, indexee, indexer )
        {
            if ( separator.Type != LuaTokenType.Colon && separator.Type != LuaTokenType.Dot )
                throw new ArgumentException ( "Separator must be either a colon or a dot", nameof ( separator ) );
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get; }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                yield return this.Indexee;
                yield return this.Indexer;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitIndex ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitIndex ( this );
    }
}