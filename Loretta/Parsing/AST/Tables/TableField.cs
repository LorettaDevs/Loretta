using System;
using System.Collections.Generic;
using System.Linq;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST.Tables
{
    using System.Diagnostics;
    using Loretta.Parsing.Visitor;

    /// <summary>
    /// The type of key of the <see cref="TableField" />.
    /// </summary>
    public enum TableFieldKeyType
    {
        /// <summary>
        /// No key (sequential). Format: &lt;expr&gt;.
        /// </summary>
        None,

        /// <summary>
        /// An identifier key. Format: &lt;ident&gt; = &lt;expr&gt;.
        /// </summary>
        Identifier,

        /// <summary>
        /// An expression. Format: [&lt;expr&gt;] = &lt;expr&gt;.
        /// </summary>
        Expression
    }

    /// <summary>
    /// Represents a table field.
    /// </summary>
    public class TableField : LuaASTNode
    {
        private readonly LuaToken[] _tokens;

        /// <summary>
        /// The type of key this table field has.
        /// </summary>
        public TableFieldKeyType KeyType { get; }

        /// <summary>
        /// The key of the table entry. <see langword="null" /> if the field is a sequential one, an
        /// <see cref="IdentifierExpression" /> if the key was an identifier or some other
        /// expression if it was the bracket-based index.
        /// </summary>
        public Expression? Key { get; }

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
            this.Value = null!;
        }

        /// <summary>
        /// Creates a sequential table field.
        /// </summary>
        /// <param name="value">The table field's value.</param>
        /// <param name="delim">The table field's delimiter.</param>
        public TableField ( Expression value, LuaToken delim ) : this ( Array.Empty<LuaToken> ( ), delim )
        {
            this.KeyType = TableFieldKeyType.None;
            this.Key = null;
            this.Value = value ?? throw new ArgumentNullException ( nameof ( value ) );
        }

        /// <summary>
        /// Initializes an identifier-keyed field.
        /// </summary>
        /// <param name="ident">The table field's identifier key.</param>
        /// <param name="equals">The table field's equals sign.</param>
        /// <param name="value">The table field's value.</param>
        /// <param name="delim">The table field's delimiter.</param>
        public TableField ( LuaToken ident, LuaToken equals, Expression value, LuaToken delim )
            : this ( new[] { ident, equals }, delim )
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
        /// Initializes a expression-keyed field.
        /// </summary>
        /// <param name="lbracket">The table field's left bracket '['.</param>
        /// <param name="key">The table field's key expression.</param>
        /// <param name="rbracket">The table field's right bracket ']'.</param>
        /// <param name="equals">The table field's equals sign.</param>
        /// <param name="value">The table field's value expression.</param>
        /// <param name="delim">The table field's delimiter.</param>
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

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens => this._tokens;

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                if ( this.Key != null )
                    yield return this.Key;
                yield return this.Value;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitTableField ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitTableField ( this );
    }
}