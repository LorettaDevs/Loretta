using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public abstract class Statement : LuaASTNode
    {
        public LuaToken? Semicolon { get; set; } = null;

        protected Statement ( )
        {
        }
    }
}
