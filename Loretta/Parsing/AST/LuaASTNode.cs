using System.Collections.Generic;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public abstract class LuaASTNode
    {
        internal abstract void Accept ( ITreeVisitor visitor );
        internal abstract T Accept<T> ( ITreeVisitor<T> visitor );

        public abstract IEnumerable<LuaToken> Tokens { get; }
        public abstract IEnumerable<LuaASTNode> Children { get; }
    }
}
