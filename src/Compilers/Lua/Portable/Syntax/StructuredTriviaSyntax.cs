using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax
{
    /// <summary>
    /// It's a non terminal Trivia CSharpSyntaxNode that has a tree underneath it.
    /// </summary>
    public abstract partial class StructuredTriviaSyntax : LuaSyntaxNode, IStructuredTriviaSyntax
    {
        private SyntaxTrivia _parent;

        internal StructuredTriviaSyntax(InternalSyntax.LuaSyntaxNode green, SyntaxNode? parent, int position)
            : base(green, position, parent?.SyntaxTree)
        {
            LorettaDebug.Assert(parent == null || position >= 0);
        }

        internal static StructuredTriviaSyntax Create(SyntaxTrivia trivia)
        {
            var node = trivia.UnderlyingNode;
            LorettaDebug.Assert(node is not null);
            var parent = trivia.Token.Parent;
            var position = trivia.Position;
            var red = (StructuredTriviaSyntax) node.CreateRed(parent, position);
            red._parent = trivia;
            return red;
        }

        /// <summary>
        /// Get parent trivia.
        /// </summary>
        public override SyntaxTrivia ParentTrivia => _parent;
    }
}
