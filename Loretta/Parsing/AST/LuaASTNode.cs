using System.Collections.Generic;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// The base class for all lua AST nodes.
    /// </summary>
    public abstract class LuaASTNode
    {
        internal abstract void Accept ( ITreeVisitor visitor );

        internal abstract T Accept<T> ( ITreeVisitor<T> visitor );

        /// <summary>
        /// The tokens contained in this node.
        /// </summary>
        public abstract IEnumerable<LuaToken> Tokens { get; }

        /// <summary>
        /// The children contained in this node.
        /// </summary>
        public abstract IEnumerable<LuaASTNode> Children { get; }
    }
}