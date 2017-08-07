using System;

namespace LuaParse.Tokens
{
    public class NumberToken : Token
    {
        public Double Value;

        public NumberToken ( String Raw, Double Value ) : base ( Raw )
        {
            this.Value = Value;
            this.Type = TokenType.Number;
        }
    }
}
