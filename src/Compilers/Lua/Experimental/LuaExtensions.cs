using System.Collections.Immutable;

namespace Loretta.CodeAnalysis.Lua.Experimental
{
    /// <summary>
    /// The extension methods for 
    /// </summary>
    public static partial class LuaExtensions
    {
        /// <summary>
        /// Runs constant folding on the tree rooted by the provided node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static SyntaxNode ConstantFold(this SyntaxNode node) =>
            ConstantFolder.Fold(node);

        /// <summary>
        /// Minifies the provided tree using the provided naming strategy.
        /// </summary>
        /// <param name="tree">The tree to minify.</param>
        /// <param name="namingStrategy">
        /// The naming strategy to use.
        /// See <see cref="Minifying.NamingStrategies"/> for common ones.
        /// </param>
        /// <returns>
        /// The tree with the new minified root.
        /// </returns>
        public static SyntaxTree Minify(this SyntaxTree tree, Minifying.NamingStrategy namingStrategy)
        {
            var renamingRewriter = new Minifying.RenamingRewriter(new Script(ImmutableArray.Create(tree)), namingStrategy);
            var root = tree.GetRoot();
            root = renamingRewriter.Visit(root)!;
            root = Minifying.TriviaRewriter.Instance.Visit(root)!;
            return tree.WithRootAndOptions(root, tree.Options);
        }
    }
}
