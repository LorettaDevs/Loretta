using System;
using System.Collections.Generic;
using GParse.Lexing;

namespace Loretta.Common
{
    public class LToken : Token
    {
        public List<Token> LeadingTokens = new List<Token> ( );

        public LToken ( String ID, String raw, Object value, TokenType type, SourceRange range ) : base ( ID, raw, value, type, range )
        {
        }

        /// <summary>
        /// Create a new LToken with the leading tokens
        /// (whitespace, comments)
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="raw"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="range"></param>
        /// <param name="leading"></param>
        public LToken ( String ID, String raw, Object value, TokenType type, SourceRange range, IEnumerable<Token> leading ) : base ( ID, raw, value, type, range )
        {
            this.LeadingTokens.AddRange ( leading );
        }

        /// <summary>
        /// Token escalation
        /// </summary>
        /// <param name="token"></param>
        public LToken ( Token token ) : base ( token.ID, token.Raw, token.Value, token.Type, token.Range )
        {
        }
    }
}
