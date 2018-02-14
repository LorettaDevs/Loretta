using System;
using System.Collections.Generic;
using Loretta.Env;
using Loretta.Lexing;
using Loretta.Parsing;
using Loretta.Parsing.Nodes;

namespace Loretta
{
    public class LuaEnvironment
    {
        public Dictionary<String, Variable> Globals { get; internal set; } = new Dictionary<String, Variable> ( );

        public Dictionary<GLuaLexer, EnvFile> LexerFiles = new Dictionary<GLuaLexer, EnvFile> ( );
        public Dictionary<GLuaParser, EnvFile> ParserFiles = new Dictionary<GLuaParser, EnvFile> ( );
        public Dictionary<String, EnvFile> PathFiles = new Dictionary<String, EnvFile> ( );

        public EnvFile ProcessFile ( String name, String contents )
        {
            if ( this.PathFiles.ContainsKey ( name ) )
                return this.PathFiles[name];

            var file = new EnvFile ( name, contents );
            var tokenizer = new GLuaLexer ( contents );
            var parser = new GLuaParser ( tokenizer, this );

            this.PathFiles[name] = file;
            this.LexerFiles[tokenizer] = file;
            this.ParserFiles[parser] = file;

            file.Successful = true;
            try
            {
                file.AST = parser.Parse ( );
            }
            catch ( Exception )
            {
                file.Successful = false;
            }
            foreach ( Error err in file.Errors )
                if ( err.Type == ErrorType.Error || err.Type == ErrorType.Fatal )
                {
                    file.Successful = false;
                    break;
                }

            return file;
        }

        public EnvFile GetFile ( String name )
            => this.PathFiles.ContainsKey ( name ) ? this.PathFiles[name] : null;

        public EnvFile GetFile ( GLuaLexer lexer )
            => this.LexerFiles.ContainsKey ( lexer ) ? this.LexerFiles[lexer] : null;

        public EnvFile GetFile ( GLuaParser parser )
            => this.ParserFiles.ContainsKey ( parser ) ? this.ParserFiles[parser] : null;
    }
}
