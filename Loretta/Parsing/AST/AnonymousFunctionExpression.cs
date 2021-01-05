using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents an anonymous function expression.
    /// </summary>
    public class AnonymousFunctionExpression : Expression
    {
        /// <summary>
        /// This function's arguments.
        /// </summary>
        public ImmutableArray<Expression> Arguments { get; }

        /// <summary>
        /// This function's body.
        /// </summary>
        public StatementList Body { get; }

        /// <inheritdoc />
        public override Boolean IsConstant => false;

        /// <inheritdoc />
        public override Object ConstantValue => throw new InvalidOperationException ( "This is not a constant node." );

        /// <summary>
        /// Initializes a new anonymous function expression.
        /// </summary>
        /// <param name="tokens">The function's tokens.</param>
        /// <param name="arguments">The function's arguments.</param>
        /// <param name="body">The function's body.</param>
        public AnonymousFunctionExpression ( IEnumerable<LuaToken> tokens, IEnumerable<Expression> arguments, StatementList body )
        {
            this.Tokens = tokens.ToImmutableArray ( );
            this.Arguments = arguments.ToImmutableArray ( );
            this.Body = body;
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get; }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                foreach ( Expression argument in this.Arguments )
                    yield return argument;
                yield return this.Body;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitAnonymousFunction ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitAnonymousFunction ( this );
    }
}