using System;
using System.Collections.Generic;
using System.Linq;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST.Tables
{
    using System.Diagnostics;
    using Loretta.Parsing.Visitor;

    /// <summary>
    /// The type of key this <see cref="TableField"/> has
    /// </summary>
    public enum TableFieldKeyType
    {
        None,
        Identifier,
        Expression
    }

    public class TableField : LuaASTNode
    {
        private readonly LuaToken[] _tokens;

        /// <summary>
        /// The type of key this table field has.
        /// </summary>
        public TableFieldKeyType KeyType { get; }

        /// <summary>
        /// The key of the table entry. <see langword="null"/> if the field is a sequential one, an
        /// <see cref="IdentifierExpression"/> if the key was an identifier or some other expression
        /// if it was the bracket-based index.
        /// </summary>
        public Expression Key { get; }

        /// <summary>
        /// The value of the table entry.
        /// </summary>
        public Expression Value { get; }

        /// <summary>
        /// The delimiter of the table field
        /// </summary>
        public LuaToken Delimiter { get; }

        private TableField ( IEnumerable<LuaToken> tokens, LuaToken delimiter )
        {
            Debug.Assert ( tokens != null && tokens.All ( tok => tok != null ) );
            var toks = new List<LuaToken> ( tokens );
            if ( delimiter != default )
                toks.Add ( delimiter );
            this._tokens = toks.ToArray ( );
            this.Delimiter = delimiter;
        }

        /// <summary>
        /// This constructor creates a sequential field
        /// </summary>
        /// <param name="value"></param>
        /// <param name="equalsSign"></param>
        /// <param name="delim"></param>
        public TableField ( Expression value, LuaToken delim ) : this ( Array.Empty<LuaToken> ( ), delim )
        {
            this.KeyType = TableFieldKeyType.None;
            this.Key = null;
            this.Value = value ?? throw new ArgumentNullException ( nameof ( value ) );
        }

        /// <summary>
        /// This constructor creates a identifier keyed field
        /// </summary>
        /// <param name="ident"></param>
        /// <param name="equals"></param>
        /// <param name="value"></param>
        /// <param name="delim"></param>
        public TableField ( LuaToken ident, LuaToken equals, Expression value, LuaToken delim ) : this ( new[] { ident, equals }, delim )
        {
            if ( ident == null )
                throw new ArgumentNullException ( nameof ( ident ) );
            if ( equals == null )
                throw new ArgumentNullException ( nameof ( equals ) );

            this.KeyType = TableFieldKeyType.Identifier;
            this.Key = new IdentifierExpression ( ident, null );
            this.Value = value ?? throw new ArgumentNullException ( nameof ( value ) );
        }

        /// <summary>
        /// This constructor creates a expression keyed field
        /// </summary>
        /// <param name="lbracket"></param>
        /// <param name="key"></param>
        /// <param name="rbracket"></param>
        /// <param name="equals"></param>
        /// <param name="value"></param>
        /// <param name="delim"></param>
        public TableField ( LuaToken lbracket, Expression key, LuaToken rbracket, LuaToken equals, Expression value, LuaToken delim ) : this ( new[] { lbracket, rbracket, equals }, delim )
        {
            if ( lbracket == null )
                throw new ArgumentNullException ( nameof ( lbracket ) );
            if ( rbracket == null )
                throw new ArgumentNullException ( nameof ( rbracket ) );
            if ( equals == null )
                throw new ArgumentNullException ( nameof ( equals ) );

            this.KeyType = TableFieldKeyType.Expression;
            this.Key = key ?? throw new ArgumentNullException ( nameof ( key ) );
            this.Value = value ?? throw new ArgumentNullException ( nameof ( value ) );
        }

        public override IEnumerable<LuaToken> Tokens => this._tokens;

        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                yield return this.Key;
                yield return this.Value;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitTableField ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitTableField ( this );
    }
}