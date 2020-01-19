using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing.AST;
using Loretta.Parsing.AST.Tables;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class TableConstructorExpression : Expression
    {
        private readonly ImmutableArray<LuaToken> tokens;
        public readonly ImmutableArray<TableField> Fields;

        public TableConstructorExpression ( LuaToken lcurly, TableField[] fields, LuaToken rcurly )
        {
            this.tokens = ImmutableArray.Create ( lcurly, rcurly );
            this.Fields = fields.ToImmutableArray ( );
        }

        public override IEnumerable<LuaToken> Tokens => this.tokens;
        public override IEnumerable<LuaASTNode> Children => this.Fields;

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitTableConstructor ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitTableConstructor ( this );
    }
}
