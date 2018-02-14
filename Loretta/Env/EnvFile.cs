using System;
using System.Collections.Generic;
using Loretta.Parsing.Nodes;

namespace Loretta.Env
{
    public class EnvFile
    {
        public String Name { get; set; }

        public String Contents { get; set; }

        public List<Error> Errors { get; } = new List<Error> ( );

        public StatementList AST { get; internal set; }

        public Boolean Successful { get; internal set; }

        public EnvFile ( String name, String contents )
        {
            this.Name = name;
            this.Contents = contents;
        }
    }
}
