using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class FunctionDefinitionStatement : Statement
    {
        public Boolean IsLocal { get; }
        public Expression Name { get; }
        public ImmutableArray<Expression> Arguments { get; }
        public StatementList Body { get; }

        public FunctionDefinitionStatement ( LuaToken functionKw, Expression name, LuaToken lparen, IEnumerable<Expression> arguments, IEnumerable<LuaToken> commas, LuaToken rparen, StatementList body, LuaToken endKw )
        {
            this.IsLocal   = false;
            this.Name      = name;
            this.Arguments = arguments.ToImmutableArray ( );
            this.Body      = body;
            var toks       = new List<LuaToken> { functionKw, lparen };
            toks.AddRange ( commas );
            toks.Add ( rparen );
            toks.Add ( endKw );
            this.Tokens    = toks;
        }

        public FunctionDefinitionStatement ( LuaToken localKw, LuaToken functionKw, Expression name, LuaToken lparen, IEnumerable<Expression> arguments, IEnumerable<LuaToken> commas, LuaToken rparen, StatementList body, LuaToken endKw )
        {
            this.IsLocal   = true;
            this.Name      = name;
            this.Arguments = arguments.ToImmutableArray ( );
            this.Body      = body;
            var toks       = new List<LuaToken> { localKw, functionKw, lparen };
            toks.AddRange ( commas );
            toks.Add ( rparen );
            toks.Add ( endKw );
            this.Tokens    = toks;
        }

        public override IEnumerable<LuaToken> Tokens { get; }

        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                yield return this.Name;
                foreach ( IdentifierExpression arg in this.Arguments )
                    yield return arg;
                yield return this.Body;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitFunctionDefinition ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitFunctionDefinition ( this );
    }
}
