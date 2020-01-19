using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    public class FunctionCallExpression : Expression
    {
        public Expression Function { get; }
        public ImmutableArray<Expression> Arguments { get; }
        public Boolean HasParenthesis { get; }

        public FunctionCallExpression ( Expression function, Expression argument )
        {
            this.HasParenthesis = false;
            this.Function = function;
            this.Arguments = ImmutableArray.Create ( argument );
            this.Tokens = Enumerable.Empty<Token<LuaTokenType>> ( );
        }

        public FunctionCallExpression ( Expression function, Token<LuaTokenType> lparen, IEnumerable<Expression> arguments, IEnumerable<Token<LuaTokenType>> commas, Token<LuaTokenType> rparen )
        {
            this.HasParenthesis = true;
            this.Function = function;
            this.Arguments = arguments.ToImmutableArray ( );
            var toks = new List<Token<LuaTokenType>>
            {
                lparen
            };
            toks.AddRange ( commas );
            toks.Add ( rparen );
            this.Tokens = toks.ToArray ( );
        }

        public override IEnumerable<Token<LuaTokenType>> Tokens { get; }

        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                yield return this.Function;
                foreach ( Expression arg in this.Arguments )
                    yield return arg;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitFunctionCall ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitFunctionCall ( this );
    }
}
