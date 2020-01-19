using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.Parsing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class StatementList : Statement
    {
        public readonly Scope Scope;
        public readonly ImmutableArray<Statement> Body;

        public StatementList ( Scope scope, IEnumerable<Statement> statements )
        {
            this.Scope = scope;
            this.Body = statements.ToImmutableArray ( );
        }

        public override IEnumerable<LuaToken> Tokens => Enumerable.Empty<LuaToken> ( );
        public override IEnumerable<LuaASTNode> Children => this.Body;

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitStatementList ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitStatementList ( this );
    }
}
