using System;

namespace Loretta.Parsing
{
    /// <summary>
    /// Represents a goto label.
    /// </summary>
    public class GotoLabel
    {
        /// <summary>
        /// The label's containing scope.
        /// </summary>
        public Scope Scope { get; private set; }

        /// <summary>
        /// The label's identifier.
        /// </summary>
        public String Identifier { get; private set; }

        /// <summary>
        /// Initializes a new goto label.
        /// </summary>
        /// <param name="scope"><inheritdoc cref="Scope" /></param>
        /// <param name="identifier"><inheritdoc cref="Identifier" /></param>
        public GotoLabel ( Scope scope, String identifier )
        {
            this.Scope = scope;
            this.Identifier = identifier;
        }
    }
}