using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a function call expression.
    /// </summary>
    public class FunctionCallExpression : Expression
    {
        /// <summary>
        /// The expression being called.
        /// </summary>
        public Expression Function { get; }

        /// <summary>
        /// The call's arguments.
        /// </summary>
        public ImmutableArray<Expression> Arguments { get; }

        /// <summary>
        /// Whether the call has parenthesis.
        /// </summary>
        public Boolean HasParenthesis { get; }

        /// <inheritdoc />
        public override Boolean IsConstant => false;

        /// <inheritdoc />
        public override Object ConstantValue => throw new InvalidOperationException ( "This is not a constant node." );

        /// <summary>
        /// Initializes a new function call expression without parenthesis.
        /// </summary>
        /// <param name="function">The expression being called.</param>
        /// <param name="argument">The call's argument.</param>
        public FunctionCallExpression ( Expression function, Expression argument )
        {
            this.HasParenthesis = false;
            this.Function = function;
            this.Arguments = ImmutableArray.Create ( argument );
            this.Tokens = Enumerable.Empty<Token<LuaTokenType>> ( );
        }

        /// <summary>
        /// Initializes a new function call expression with parenthesis.
        /// </summary>
        /// <param name="function">The expression being called.</param>
        /// <param name="lparen">The left parenthesis token.</param>
        /// <param name="arguments">The call's arguments.</param>
        /// <param name="commas">The arguments' commas.</param>
        /// <param name="rparen">The right parenthesis token.</param>
        public FunctionCallExpression (
            Expression function,
            Token<LuaTokenType> lparen,
            IEnumerable<Expression> arguments,
            IEnumerable<Token<LuaTokenType>> commas,
            Token<LuaTokenType> rparen )
        {
            this.HasParenthesis = true;
            this.Function = function;
            this.Arguments = arguments.ToImmutableArray ( );
            var toks = new List<Token<LuaTokenType>>
            {
                lparen
            };
            toks.AddRange ( commas );
            toks.Add ( rparen );
            this.Tokens = toks.ToArray ( );
        }

        /// <inheritdoc />
        public override IEnumerable<Token<LuaTokenType>> Tokens { get; }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                yield return this.Function;
                foreach ( Expression arg in this.Arguments )
                    yield return arg;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitFunctionCall ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitFunctionCall ( this );
    }
}