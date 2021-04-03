using System;
using System.Collections.Generic;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The base interface for variables
    /// </summary>
    public interface IVariable
    {
        /// <summary>
        /// The kind of this variable.
        /// </summary>
        VariableKind Kind { get; }

        /// <summary>
        /// The containing scope.
        /// </summary>
        IScope ContainingScope { get; }

        /// <summary>
        /// The variable's name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The node where this variable is declared.
        /// </summary>
        /// <remarks>
        /// <see langword="null"/> if it is a global or implicit variable.
        /// </remarks>
        SyntaxReference? Declaration { get; }

        /// <summary>
        /// All scopes that reference this variable.
        /// </summary>
        IEnumerable<IScope> ReferencingScopes { get; }

        /// <summary>
        /// All locations this variable is read from.
        /// </summary>
        IEnumerable<SyntaxReference> ReadLocations { get; }

        /// <summary>
        /// All locations this variable is written to.
        /// </summary>
        IEnumerable<SyntaxReference> WriteLocations { get; }
    }

    internal class Variable : IVariable
    {
        private readonly IList<IScope> _referencingScopes = new List<IScope>();
        private readonly IList<SyntaxReference> _readLocations = new List<SyntaxReference>();
        private readonly IList<SyntaxReference> _writeLocations = new List<SyntaxReference>();

        public Variable(VariableKind kind, IScopeInternal containingScope, string name, SyntaxReference? declaration)
        {
            RoslynDebug.AssertNotNull(containingScope);
            RoslynDebug.AssertNotNull(name);

            Kind = kind;
            ContainingScope = containingScope;
            Name = name;
            Declaration = declaration;
            ReferencingScopes = SpecializedCollections.ReadOnlyEnumerable(_referencingScopes);
            ReadLocations = SpecializedCollections.ReadOnlyEnumerable(_readLocations);
            WriteLocations = SpecializedCollections.ReadOnlyEnumerable(_writeLocations);
        }

        public VariableKind Kind { get; }

        public IScopeInternal ContainingScope { get; }

        IScope IVariable.ContainingScope => ContainingScope;

        public string Name { get; }

        public SyntaxReference? Declaration { get; }

        public IEnumerable<IScope> ReferencingScopes { get; }

        public IEnumerable<SyntaxReference> ReadLocations { get; }

        public IEnumerable<SyntaxReference> WriteLocations { get; }
    }
}
