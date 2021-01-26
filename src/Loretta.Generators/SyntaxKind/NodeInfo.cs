using Microsoft.CodeAnalysis;

namespace Loretta.Generators.SyntaxKind
{
    internal readonly struct NodeInfo
    {
        public NodeInfo ( INamedTypeSymbol nodeType )
        {
            this.NodeType = nodeType;
        }

        public INamedTypeSymbol NodeType { get; }
    }
}
