using System.Collections.Generic;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The interface for a goto label.
    /// </summary>
    public interface IGotoLabel
    {
        /// <summary>
        /// The label's name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The label's location.
        /// </summary>
        SyntaxReference Location { get; }

        /// <summary>
        /// The nodes that jump to this label.
        /// </summary>
        IEnumerable<SyntaxReference> Jumps { get; }
    }

    internal interface IGotoLabelInternal : IGotoLabel
    {
        void AddJump(SyntaxReference jump);
    }

    internal class GotoLabel : IGotoLabelInternal
    {
        private readonly IList<SyntaxReference> _jumps = new List<SyntaxReference>();

        public GotoLabel(string name, SyntaxReference location)
        {
            RoslynDebug.Assert(!string.IsNullOrEmpty(name));
            RoslynDebug.AssertNotNull(location);

            Name = name;
            Location = location;
            Jumps = SpecializedCollections.ReadOnlyEnumerable(_jumps);
        }

        public string Name { get; }

        public SyntaxReference Location { get; }

        public IEnumerable<SyntaxReference> Jumps { get; }

        public void AddJump(SyntaxReference jump)
        {
            RoslynDebug.AssertNotNull(jump);
            _jumps.Add(jump);
        }
    }
}
