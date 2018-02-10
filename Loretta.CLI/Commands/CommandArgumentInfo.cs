using System;

namespace Loretta.CLI.Commands
{
    public struct CommandArgumentInfo
    {
        /// <summary>
        /// Name of the argument
        /// </summary>
        public String Name;

        /// <summary>
        /// Type of the argument
        /// </summary>
        public Type Type;

        /// <summary>
        /// Whether this parameter is optional
        /// </summary>
        public Boolean IsOptional;

        /// <summary>
        /// Whether this should take the rest of the line
        /// </summary>
        public Boolean Rest;
    }
}
