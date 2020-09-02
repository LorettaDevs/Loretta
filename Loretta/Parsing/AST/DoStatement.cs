using System;
using System.Collections.Generic;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

// This is basically a wrapper around StatementList
namespace Loretta.Parsing.AST
{
    /// <summary>
    /// A do statement.
    /// </summary>
    public class DoStatement : Statement
    {
        /// <summary>
        /// The do statement body.
        /// </summary>
        public StatementList Body { get; }

        /// <summary>
        /// Initializes a new do statement.
        /// </summary>
        /// <param name="doKw">The do keyword.</param>
        /// <param name="body">The do statement body.</param>
        /// <param name="endKw">The end keyword.</param>
        public DoStatement ( LuaToken doKw, StatementList body, LuaToken endKw )
        {
            this.Tokens = new[] { doKw, endKw };
            this.Body = body ?? throw new ArgumentNullException ( nameof ( body ) );
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get; }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children { get { yield return this.Body; } }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitDo ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitDo ( this );
    }
}