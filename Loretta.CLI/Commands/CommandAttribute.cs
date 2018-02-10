using System;

namespace Loretta.CLI.Commands
{
    [AttributeUsage ( AttributeTargets.Method, Inherited = false, AllowMultiple = false )]
    internal sealed class CommandAttribute : Attribute
    {
        private readonly String _name;

        private static Object Noop ( ) => null;

        public CommandAttribute ( String Name )
        {
            this._name = Name;
        }

        public String Name => this._name;
    }
}
