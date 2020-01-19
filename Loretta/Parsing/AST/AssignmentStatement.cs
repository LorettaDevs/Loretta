using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class AssignmentStatement : Statement
    {
        public ImmutableArray<Expression> Variables { get; }
        public ImmutableArray<Expression> Values { get; }

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

        public override IEnumerable<LuaToken> Tokens { get; }

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
