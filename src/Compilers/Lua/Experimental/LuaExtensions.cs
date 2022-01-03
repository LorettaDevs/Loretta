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
    }
}
