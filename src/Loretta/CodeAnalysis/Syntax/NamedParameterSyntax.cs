using System;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a named function parameter.
    /// </summary>
    public sealed partial class NamedParameterSyntax : ParameterSyntax
    {
        internal NamedParameterSyntax ( SyntaxTree syntaxTree, SyntaxToken identifier )
            : base ( syntaxTree )
        {
            this.Identifier = identifier;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.NamedParameter;

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public SyntaxToken Identifier { get; }
    }
}