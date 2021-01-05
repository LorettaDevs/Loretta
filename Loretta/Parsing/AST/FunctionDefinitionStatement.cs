using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a function definition statement.
    /// </summary>
    public class FunctionDefinitionStatement : Statement
    {
        /// <summary>
        /// Whether the function is local.
        /// </summary>
        public Boolean IsLocal { get; }

        /// <summary>
        /// The function name. Can be an <see cref="IndexExpression" /> or an <see
        /// cref="IdentifierExpression" />.
        /// </summary>
        public Expression Name { get; }

        /// <summary>
        /// The function's arguments. Can be an <see cref="IdentifierExpression" /> or a <see
        /// cref="VarArgExpression" />.
        /// </summary>
        public ImmutableArray<Expression> Arguments { get; }

        /// <summary>
        /// The function's body.
        /// </summary>
        public StatementList Body { get; }

        /// <summary>
        /// Initializes a new function definition statement.
        /// </summary>
        /// <param name="functionKw">The function keyword token.</param>
        /// <param name="name">The function's name.</param>
        /// <param name="lparen">The left parenthesis token.</param>
        /// <param name="arguments">The argument names.</param>
        /// <param name="commas">The arguments' commas.</param>
        /// <param name="rparen">The right parenthesis token.</param>
        /// <param name="body">The function's body.</param>
        /// <param name="endKw">The end keyword token.</param>
        public FunctionDefinitionStatement (
            LuaToken functionKw,
            Expression name,
            LuaToken lparen,
            IEnumerable<Expression> arguments,
            IEnumerable<LuaToken> commas,
            LuaToken rparen,
            StatementList body,
            LuaToken endKw )
        {
            this.IsLocal = false;
            this.Name = name;
            this.Arguments = arguments.ToImmutableArray ( );
            this.Body = body;
            var toks = new List<LuaToken> { functionKw, lparen };
            toks.AddRange ( commas );
            toks.Add ( rparen );
            toks.Add ( endKw );
            this.Tokens = toks;
        }

        /// <summary>
        /// Initializes a new local function definition statement.
        /// </summary>
        /// <param name="localKw">The local keyword token.</param>
        /// <param name="functionKw">The function keyword token.</param>
        /// <param name="name">The function's name.</param>
        /// <param name="lparen">The left parenthesis token.</param>
        /// <param name="arguments">The function's arguments.</param>
        /// <param name="commas">The function's arguments' commas.</param>
        /// <param name="rparen">The right parenthesis token.</param>
        /// <param name="body">The function's body.</param>
        /// <param name="endKw">The end keyword token.</param>
        public FunctionDefinitionStatement (
            LuaToken localKw,
            LuaToken functionKw,
            Expression name,
            LuaToken lparen,
            IEnumerable<Expression> arguments,
            IEnumerable<LuaToken> commas,
            LuaToken rparen,
            StatementList body,
            LuaToken endKw )
        {
            this.IsLocal = true;
            this.Name = name;
            this.Arguments = arguments.ToImmutableArray ( );
            this.Body = body;
            var toks = new List<LuaToken> { localKw, functionKw, lparen };
            toks.AddRange ( commas );
            toks.Add ( rparen );
            toks.Add ( endKw );
            this.Tokens = toks;
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get; }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                yield return this.Name;
                foreach ( Expression arg in this.Arguments )
                    yield return arg;
                yield return this.Body;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitFunctionDefinition ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitFunctionDefinition ( this );
    }
}