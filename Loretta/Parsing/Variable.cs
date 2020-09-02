using System;

namespace Loretta.Parsing
{
    /// <summary>
    /// Represents a variable.
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// The global table variable.
        /// </summary>
        public static readonly Variable _G = new Variable ( "_G", null! );

        /// <summary>
        /// The variable's identifier.
        /// </summary>
        public String Identifier { get; private set; }

        /// <summary>
        /// The variable's containing scope.
        /// </summary>
        public readonly Scope ParentScope;

        /// <summary>
        /// Initializes a new variable.
        /// </summary>
        /// <param name="name"><inheritdoc cref="Identifier" /></param>
        /// <param name="scope"><inheritdoc cref="ParentScope" /></param>
        public Variable ( String name, Scope scope )
        {
            this.Identifier = name;
            this.ParentScope = scope;
        }

        /// <summary>
        /// Renames this variable.
        /// </summary>
        /// <param name="newIdentifier">The new identifier to be used by this variable.</param>
        public void Rename ( String newIdentifier ) => this.Identifier = newIdentifier;
    }
}