using System;

namespace LuaParse.Tokens
{
    public class CommentToken : Token
    {
        public String Value;

        public CommentToken ( String Value, String Raw ) : base ( Raw )
        {
            this.Value = Value;
            this.Type = TokenType.Comment;
        }
    }
}
