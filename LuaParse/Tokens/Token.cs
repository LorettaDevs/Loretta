using LuaParse.Tokens.Abstract;
using System;
using System.Text;

namespace LuaParse.Tokens
{
    public class Token : IToken
    {
        /// <summary>
        /// Whitespace after the token
        /// </summary>
        public StringBuilder WhitespaceAfter { get; set; } = new StringBuilder ( );

        /// <summary>
        /// Whitespace before the token
        /// </summary>
        public StringBuilder WhitespaceBefore { get; set; } = new StringBuilder ( );

        /// <summary>
        /// The raw token
        /// </summary>
        public String Raw { get; set; }

        /// <summary>
        /// The type of the token
        /// </summary>
        public TokenType Type { get; set; }

        public Token ( String Raw )
        {
            this.Raw = Raw;
        }

        /// <summary>
        /// Clears and sets the whitespace after the token
        /// </summary>
        /// <param name="wp">Whitespace to be set to</param>
        public void SetWhitespaceAfter ( String wp )
        {
            this.WhitespaceAfter.Clear ( );
            this.WhitespaceAfter.Append ( wp );
        }

        /// <summary>
        /// </summary>
        /// <param name="wp">Whitespace to append</param>
        public void AddWhitespaceAfter ( String wp )
        {
            this.WhitespaceAfter.Append ( wp );
        }

        /// <summary>
        /// Clears and sets the whitespace before the token
        /// </summary>
        /// <param name="wp">Whitespace to be set to</param>
        public void SetWhitespaceBefore ( String wp )
        {
            this.WhitespaceBefore.Clear ( );
            this.WhitespaceBefore.Append ( wp );
        }

        /// <summary>
        /// Appends the whitespace before the token
        /// </summary>
        /// <param name="wp">Whitespace to be added</param>
        public void AddWhitespaceBefore ( String wp )
        {
            this.WhitespaceBefore.Append ( wp );
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
