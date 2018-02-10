using System;
using Loretta.CLI.Commands;

namespace Loretta.CLI
{
    internal class Program
    {
        private static readonly CommandManager commandManager = new CommandManager ( );

        public static Boolean ShouldRun { get; private set; } = true;

        private static void Main ( )
        {
            commandManager.LoadCommands ( typeof ( Program ), instance: null );
            while ( ShouldRun )
            {
            }
        }
    }
}
