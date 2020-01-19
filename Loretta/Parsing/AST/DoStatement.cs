using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

// This is basically a wrapper around StatementList
namespace Loretta.Parsing.AST
{
    public class DoStatement : Statement
    {
        public StatementList Body { get; }

        public DoStatement ( LuaToken doKw, StatementList body, LuaToken endKw )
        {
            if ( doKw == null )
                throw new ArgumentNullException ( nameof ( doKw ) );
            if ( endKw == null )
                throw new ArgumentNullException ( nameof ( endKw ) );

            this.Tokens = new[] { doKw, endKw };
            this.Body = body ?? throw new ArgumentNullException ( nameof ( body ) );
        }

        public override IEnumerable<LuaToken> Tokens { get; }
        public override IEnumerable<LuaASTNode> Children { get { yield return this.Body; } }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitDo ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitDo ( this );
    }
}
