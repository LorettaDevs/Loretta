using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Loretta.CLI.Commands
{
    public class CommandManager
    {
        private readonly Dictionary<String, Command> _commands;
        public IEnumerable<Command> Commands => this._commands.Values;

        public CommandManager ( )
        {
            this._commands = new Dictionary<String, Command> ( );
        }

        public void LoadCommands ( Type type, Object instance = null )
        {
            foreach ( MethodInfo method in type.GetMethods ( BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod ) )
            {
                if ( Attribute.IsDefined ( method, typeof ( CommandAttribute ) ) )
                {
                    var aliases = ( CommandAttribute[] ) Attribute.GetCustomAttributes ( method, typeof ( CommandAttribute ) );
                    foreach ( CommandAttribute alias in aliases )
                        this._commands.Add ( alias.Name, new Command ( alias.Name, method, instance ) );
                }
            }
        }

        public void Execute ( String line )
        {
            var args = CLICommandParser.Parse ( line ).ToArray ( );
            if ( args.Length < 1 )
                throw new Exception ( "No command invoked." );
            Command cmd = this._commands[args[0]];
            args = args.Skip ( 1 ).ToArray ( );
            cmd.Invoke ( args );
        }
    }
}
