using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a generic for loop statement.
    /// </summary>
    public class GenericForLoopStatement : Statement
    {
        /// <summary>
        /// The for loop's scope.
        /// </summary>
        public Scope Scope { get; }

        /// <summary>
        /// The for loop's iteration variables.
        /// </summary>
        public ImmutableArray<IdentifierExpression> Variables { get; }

        /// <summary>
        /// The for loop's expressions.
        /// </summary>
        public ImmutableArray<Expression> Expressions { get; }

        /// <summary>
        /// The for loop's body.
        /// </summary>
        public StatementList Body { get; }

        /// <summary>
        /// Initializes a new generic for loop statement.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="forKw">The for keyword.</param>
        /// <param name="variables">The variables.</param>
        /// <param name="commas">The commas.</param>
        /// <param name="inKw">The in keyword.</param>
        /// <param name="expressions">The expressions.</param>
        /// <param name="doKw">The do keyword.</param>
        /// <param name="body">The loop's body.</param>
        /// <param name="endKw">The end keyword.</param>
        public GenericForLoopStatement (
            Scope scope,
            LuaToken forKw,
            IEnumerable<IdentifierExpression> variables,
            IEnumerable<LuaToken> commas,
            LuaToken inKw,
            IEnumerable<Expression> expressions,
            LuaToken doKw,
            StatementList body,
            LuaToken endKw )
        {
            this.Scope = scope;
            this.Variables = variables.ToImmutableArray ( );
            this.Expressions = expressions.ToImmutableArray ( );
            this.Body = body;

            var toks = new List<LuaToken> { forKw };
            toks.AddRange ( commas );
            toks.Add ( inKw ); toks.Add ( doKw ); toks.Add ( endKw );
            this.Tokens = toks;
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get; }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                foreach ( IdentifierExpression var in this.Variables )
                    yield return var;
                foreach ( Expression iteratable in this.Expressions )
                    yield return iteratable;
                yield return this.Body;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitGenericFor ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitGenericFor ( this );
    }
}