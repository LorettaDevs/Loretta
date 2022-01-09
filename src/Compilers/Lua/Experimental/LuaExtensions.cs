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

        /// <inheritdoc cref="Minify(SyntaxTree, Minifying.NamingStrategy, Minifying.ISlotAllocator)"/>
        public static SyntaxTree Minify(this SyntaxTree tree) =>
            Minify(tree, Minifying.NamingStrategies.Alphabetical);

        /// <inheritdoc cref="Minify(SyntaxTree, Minifying.NamingStrategy, Minifying.ISlotAllocator)"/>
        public static SyntaxTree Minify(this SyntaxTree tree, Minifying.NamingStrategy namingStrategy) =>
            Minify(tree, namingStrategy, new Minifying.SortedSlotAllocator());

        /// <summary>
        /// Minifies the provided tree using the provided naming strategy.
        /// </summary>
        /// <param name="tree">The tree to minify.</param>
        /// <param name="namingStrategy">
        /// The naming strategy to use.
        /// See <see cref="Minifying.NamingStrategies"/> for common ones.
        /// </param>
        /// <param name="slotAllocator">
        /// The slot allocator to use.
        /// There are two builtin ones:
        /// <list type="bullet">
        ///   <item><see cref="Minifying.SequentialSlotAllocator"/> - fast but doesn't reuse variable names.</item>
        ///   <item><see cref="Minifying.SortedSlotAllocator"/> - reuses variable names as much as possible.</item>
        /// </list>
        /// </param>
        /// <returns>The tree with the new minified root.</returns>
        public static SyntaxTree Minify(this SyntaxTree tree, Minifying.NamingStrategy namingStrategy, Minifying.ISlotAllocator slotAllocator)
        {
            var renamingRewriter = new Minifying.RenamingRewriter(new Script(ImmutableArray.Create(tree)), namingStrategy, slotAllocator);
            var root = tree.GetRoot();
            root = renamingRewriter.Visit(root)!;
            root = Minifying.TriviaRewriter.Instance.Visit(root)!;
            return tree.WithRootAndOptions(root, tree.Options);
        }
    }
}
