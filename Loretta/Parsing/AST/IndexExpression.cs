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
        /// Indexing done with an expression ('[', expr, ']')
        /// </summary>
        Indexer, // '[', expr, ']'
        /// <summary>
        /// Indexing done with an identifier ('.' ident)
        /// </summary>
        Member,     // '.' ident
        /// <summary>
        /// Function call done on an indexing done with an identifier (':' ident <funccall>)
        /// </summary>
        Method      // ':' ident <funccall>
    }

    public class IndexExpression : Expression
    {
        /// <summary>
        /// Whether this is an indexing in the format a.b or a[b]
        /// </summary>
        public IndexType Type { get; }

        public Expression Indexee { get; }

        public Expression Indexer { get; }

        public override Boolean IsConstant => false;
        public override Object ConstantValue => throw new InvalidOperationException ( "This is not a constant node." );

        private IndexExpression ( IEnumerable<LuaToken> tokens, IndexType type, Expression indexee, Expression indexer )
        {
            this.Tokens = tokens;
            this.Type = type;
            this.Indexee = indexee;
            this.Indexer = indexer;
        }

        public IndexExpression ( Expression indexee, LuaToken lbracket, Expression indexer, LuaToken rbracket )
            : this ( new[] { lbracket, rbracket }, IndexType.Indexer, indexee, indexer )
        {
            if ( lbracket.Type != LuaTokenType.LBracket )
                throw new ArgumentException ( "LBracket must be a lbracket", nameof ( lbracket ) );
            if ( rbracket.Type != LuaTokenType.RBracket )
                throw new ArgumentException ( "RBracket must be a rbracket", nameof ( rbracket ) );
        }

        public IndexExpression ( Expression indexee, LuaToken separator, IdentifierExpression indexer )
            : this ( new[] { separator }, separator.Type == LuaTokenType.Colon ? IndexType.Method : IndexType.Member, indexee, indexer )
        {
            if ( separator.Type != LuaTokenType.Colon && separator.Type != LuaTokenType.Dot )
                throw new ArgumentException ( "Separator must be either a colon or a dot", nameof ( separator ) );
        }

        public override IEnumerable<LuaToken> Tokens { get; }

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