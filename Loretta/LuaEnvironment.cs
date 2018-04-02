using System;
using System.Collections.Generic;
using System.Linq;
using Loretta.Analysis;
using Loretta.Env;
using Loretta.Folder;
using Loretta.Lexing;
using Loretta.Parsing;
using Loretta.Parsing.Nodes;

namespace Loretta
{
    public class LuaEnvironment
    {
        private interface IPriorityContainer
        {
            Int32 Priority { get; }
        }

        private struct AnalyserEntry : IPriorityContainer
        {
            public Int32 Priority { get; set; }

            public BaseASTAnalyser Analyser;
        }

        private struct FolderEntry : IPriorityContainer
        {
            public Int32 Priority { get; set; }

            public BaseASTFolder Folder;
        }

        public Dictionary<String, Variable> Globals { get; internal set; } = new Dictionary<String, Variable> ( );

        private readonly Dictionary<GLuaLexer, EnvFile> LexerFiles = new Dictionary<GLuaLexer, EnvFile> ( );
        private readonly Dictionary<GLuaParser, EnvFile> ParserFiles = new Dictionary<GLuaParser, EnvFile> ( );
        private readonly Dictionary<String, EnvFile> PathFiles = new Dictionary<String, EnvFile> ( );

        private readonly Dictionary<String, IPriorityContainer> Modules = new Dictionary<String, IPriorityContainer> ( );

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
                throw;
            }

            if ( file.Successful )
            {
                foreach ( IPriorityContainer module in this.Modules.Values.OrderBy ( con => con.Priority ) )
                {
                    if ( module is AnalyserEntry entryA )
                    {
                        entryA.Analyser.File = file;
                        entryA.Analyser.Analyse ( file.AST );
                    }
                    else if ( module is FolderEntry entryB )
                    {
                        entryB.Folder.File = file;
                        file.AST = ( StatementList ) entryB.Folder.Fold ( file.AST );
                    }
                }

                foreach ( Error err in file.Errors )
                {
                    if ( err.Type == ErrorType.Error || err.Type == ErrorType.Fatal )
                    {
                        file.Successful = false;
                        break;
                    }
                }
            }

            return file;
        }

        public void AddAnalyser ( String ID, BaseASTAnalyser analyser )
            => this.AddAnalyser ( ID, this.Modules.Count, analyser );

        public void AddAnalyser ( String ID, Int32 Priority, BaseASTAnalyser Analyser )
        {
            Analyser.Environment = this;
            this.Modules[ID] = new AnalyserEntry
            {
                Priority = Priority,
                Analyser = Analyser
            };
        }

        public void RemoveAnalyser ( String ID )
        {
            if ( this.Modules.ContainsKey ( ID ) )
                this.Modules.Remove ( ID );
        }

        public void AddFolder ( String ID, BaseASTFolder folder )
            => this.AddFolder ( ID, this.Modules.Count, folder );

        public void AddFolder ( String ID, Int32 Priority, BaseASTFolder Folder )
        {
            Folder.Environment = this;
            this.Modules[ID] = new FolderEntry
            {
                Priority = Priority,
                Folder = Folder
            };
        }

        public void RemoveFolder ( String ID )
        {
            if ( this.Modules.ContainsKey ( ID ) )
                this.Modules.Remove ( ID );
        }

        public EnvFile GetFile ( String name )
            => this.PathFiles.ContainsKey ( name ) ? this.PathFiles[name] : null;

        public EnvFile GetFile ( GLuaLexer lexer )
            => this.LexerFiles.ContainsKey ( lexer ) ? this.LexerFiles[lexer] : null;

        public EnvFile GetFile ( GLuaParser parser )
            => this.ParserFiles.ContainsKey ( parser ) ? this.ParserFiles[parser] : null;
    }
}
