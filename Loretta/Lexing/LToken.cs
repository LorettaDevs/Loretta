using System;
using System.Collections.Generic;
using GParse.Lexing;

namespace Loretta.Lexing
{
    public class LToken : Token
    {
        /// <summary>
        /// Whitespaces and comments that preceed this token
        /// </summary>
        public List<Token> LeadingFlair = new List<Token> ( );

        /// <summary>
        /// Starting location of this token
        /// </summary>
        public SourceLocation Location => this.Range.Start;

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
            this.LeadingFlair.AddRange ( leading );
        }

        /// <summary>
        /// Token upgrading
        /// </summary>
        /// <param name="token"></param>
        public LToken ( Token token ) : this ( token.ID, token.Raw, token.Value, token.Type, token.Range )
        {
        }

        /// <summary>
        /// Clones a token
        /// </summary>
        /// <param name="token"></param>
        public LToken ( LToken token ) : this ( token.ID, token.Raw, token.Value, token.Type, token.Range, token.LeadingFlair )
        {
        }
    }
}
