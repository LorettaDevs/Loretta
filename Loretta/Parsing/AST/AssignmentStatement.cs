using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents an assignment statement.
    /// </summary>
    public class AssignmentStatement : Statement
    {
        /// <summary>
        /// This assignment's variables. May have a larger or smaller number of entries than the
        /// <see cref="Values" />.
        /// </summary>
        public ImmutableArray<Expression> Variables { get; }

        /// <summary>
        /// This assignment's values. May have a larger or smaller number of entries than the the
        /// <see cref="Variables" />.
        /// </summary>
        public ImmutableArray<Expression> Values { get; }

        /// <summary>
        /// Initializes a new assignment statement.
        /// </summary>
        /// <param name="variables">The assignment variables.</param>
        /// <param name="variablesCommas">The assignment variables' commas.</param>
        /// <param name="equals">The assignment equals sign.</param>
        /// <param name="values">The assignment values.</param>
        /// <param name="valuesCommas">The assignment values' commas.</param>
        public AssignmentStatement ( IEnumerable<Expression> variables, IEnumerable<LuaToken> variablesCommas, LuaToken equals, IEnumerable<Expression> values, IEnumerable<LuaToken> valuesCommas )
        {
            this.Variables = variables.ToImmutableArray ( );
            this.Values = values.ToImmutableArray ( );
            var toks = new List<LuaToken> ( variablesCommas )
            {
                equals
            };
            toks.AddRange ( valuesCommas );
            this.Tokens = toks;
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get; }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                foreach ( Expression var in this.Variables )
                    yield return var;
                foreach ( Expression val in this.Values )
                    yield return val;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitAssignment ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitAssignment ( this );
    }
}