using System;
using System.Collections.Generic;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a numeric for loop statement.
    /// </summary>
    public class NumericForLoopStatement : Statement
    {
        /// <summary>
        /// The scope.
        /// </summary>
        public Scope Scope { get; }

        /// <summary>
        /// The iteration variable.
        /// </summary>
        public IdentifierExpression Variable { get; }

        /// <summary>
        /// The initial value.
        /// </summary>
        public Expression Initial { get; }

        /// <summary>
        /// The final value.
        /// </summary>
        public Expression Final { get; }

        /// <summary>
        /// The iteration step increment value.
        /// </summary>
        public Expression? Step { get; }

        /// <summary>
        /// The body.
        /// </summary>
        public StatementList Body { get; }

        /// <summary>
        /// Initializes a new numeric for loop statement.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="forKw">The for keyword token.</param>
        /// <param name="variable">The iteration variable.</param>
        /// <param name="equals">The equals sign token.</param>
        /// <param name="initial">The initial value.</param>
        /// <param name="comma1">The first comma token.</param>
        /// <param name="final">The final value.</param>
        /// <param name="doKw">The do keyword.</param>
        /// <param name="body">The body.</param>
        /// <param name="endKw">The end keyword.</param>
        public NumericForLoopStatement (
            Scope scope,
            LuaToken forKw,
            IdentifierExpression variable,
            LuaToken equals,
            Expression initial,
            LuaToken comma1,
            Expression final,
            LuaToken doKw,
            StatementList body,
            LuaToken endKw )
        {
            this.Scope = scope ?? throw new ArgumentNullException ( nameof ( scope ) );
            this.Variable = variable ?? throw new ArgumentNullException ( nameof ( variable ) );
            this.Initial = initial ?? throw new ArgumentNullException ( nameof ( initial ) );
            this.Final = final ?? throw new ArgumentNullException ( nameof ( final ) );
            this.Step = null;
            this.Body = body ?? throw new ArgumentNullException ( nameof ( body ) );
            this.Tokens = new[] { forKw, equals, comma1, doKw, endKw };
        }

        /// <summary>
        /// Initializes a new numeric for loop statement.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="forKw">The for keyword token.</param>
        /// <param name="variable">The iteration variable.</param>
        /// <param name="equals">The equals sign token.</param>
        /// <param name="initial">The initial value.</param>
        /// <param name="comma1">The first comma token.</param>
        /// <param name="final">The final value.</param>
        /// <param name="comma2">The second comma token.</param>
        /// <param name="step">The step increment value.</param>
        /// <param name="doKw">The do keyword.</param>
        /// <param name="body">The body.</param>
        /// <param name="endKw">The end keyword.</param>
        public NumericForLoopStatement (
            Scope scope,
            LuaToken forKw,
            IdentifierExpression variable,
            LuaToken equals,
            Expression initial,
            LuaToken comma1,
            Expression final,
            LuaToken comma2,
            Expression step,
            LuaToken doKw,
            StatementList body,
            LuaToken endKw )
        {
            this.Scope = scope ?? throw new ArgumentNullException ( nameof ( scope ) );
            this.Variable = variable ?? throw new ArgumentNullException ( nameof ( variable ) );
            this.Initial = initial ?? throw new ArgumentNullException ( nameof ( initial ) );
            this.Final = final ?? throw new ArgumentNullException ( nameof ( final ) );
            this.Step = step ?? throw new ArgumentNullException ( nameof ( step ) );
            this.Body = body ?? throw new ArgumentNullException ( nameof ( body ) );
            this.Tokens = new[] { forKw, equals, comma1, comma2, doKw, endKw };
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get; }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                yield return this.Variable;
                yield return this.Initial;
                yield return this.Final;
                if ( !( this.Step is null ) )
                    yield return this.Step;
                yield return this.Body;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitNumericFor ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitNumericFor ( this );
    }
}