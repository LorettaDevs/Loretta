namespace Loretta.CodeAnalysis.Lua
{
    internal static class SyntaxNodeExtensions
    {
        public static TNode WithAnnotations<TNode>(this TNode node, params SyntaxAnnotation[] annotations) where TNode : LuaSyntaxNode =>
            (TNode) node.Green.SetAnnotations(annotations).CreateRed(node.Parent, node.Position);
    }
}
