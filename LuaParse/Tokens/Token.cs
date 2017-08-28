using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LuaParse.Tokens
{
    public class Token
    {
        /// <summary>
        /// Tokens preceding it
        /// </summary>
        public IList<Token> TokensBefore = new List<Token> ( );

        /// <summary>
        /// </summary>
        public IList<Token> TokensAfter = new List<Token> ( );

        /// <summary>
        /// The raw token
        /// </summary>
        public String Raw { get; set; }

        /// <summary>
        /// The type of the token
        /// </summary>
        public TokenType Type;

        public Token ( String Raw )
        {
            this.Raw = Raw;
        }

        /// <summary>
        /// Whether this token can be cosnidered a possible value
        /// (function return, identifier, number, nil or string)
        /// </summary>
        /// <returns></returns>
        public Boolean IsPossibleValue ( )
        {
            return ( this.Type & TokenType.PossibleValue ) != 0;
        }
    }
}
