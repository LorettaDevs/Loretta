using System;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.Nodes;

namespace Loretta.Env
{
    public enum ErrorType
    {
        Fatal,
        Error,
        Warning,
        Information
    }

    public struct Error
    {
        public ErrorType Type;
        public SourceLocation Location;
        public String Message;

        public Error ( ErrorType Type, ASTNode Node, String Message )
        {
            this.Type = Type;
            this.Location = Node.Tokens[0].Range.Start;
            this.Message = Message;
        }

        public Error ( ErrorType Type, LToken Token, String Message )
        {
            this.Type = Type;
            this.Location = Token.Range.Start;
            this.Message = Message;
        }

        public Error ( ErrorType Type, SourceLocation Location, String Message )
        {
            this.Type = Type;
            this.Location = Location;
            this.Message = Message;
        }
    }
}
