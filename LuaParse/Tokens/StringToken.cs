using System;
using System.Collections.Generic;
using System.Text;

namespace LuaParse.Tokens
{
    public class StringToken : Token
    {
        /// <summary>
        /// The value of the string
        /// </summary>
        public String Value { get; set; }

        public StringToken ( String Value, String Raw ) : base ( Raw )
        {
            this.Type = TokenType.String;
            this.Value = Value;
        }
    }
}
