using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// The base class for a statement.
    /// </summary>
    public abstract class Statement : LuaASTNode
    {
        /// <summary>
        /// The statement's semicolon. Null if none.
        /// </summary>
        public LuaToken? Semicolon { get; set; } = null;
    }
}
