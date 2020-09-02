using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing.AST.Tables;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a table literal expression.
    /// </summary>
    public class TableConstructorExpression : Expression
    {
        private readonly ImmutableArray<LuaToken> tokens;

        /// <summary>
        /// The table fields.
        /// </summary>
        public ImmutableArray<TableField> Fields { get; }

        /// <inheritdoc />
        public override Boolean IsConstant => false;

        /// <inheritdoc />
        public override Object ConstantValue => throw new InvalidOperationException ( "This is not a constant node." );

        /// <summary>
        /// Initializes a table literal expression.
        /// </summary>
        /// <param name="lcurly">The left curly bracket token.</param>
        /// <param name="fields">The table fields.</param>
        /// <param name="rcurly">The right curly bracket token.</param>
        public TableConstructorExpression ( LuaToken lcurly, TableField[] fields, LuaToken rcurly )
        {
            this.tokens = ImmutableArray.Create ( lcurly, rcurly );
            this.Fields = fields.ToImmutableArray ( );
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens => this.tokens;

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children => this.Fields;

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitTableConstructor ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitTableConstructor ( this );
    }
}