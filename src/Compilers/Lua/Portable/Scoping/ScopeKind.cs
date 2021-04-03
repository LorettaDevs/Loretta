namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The type of scope.
    /// </summary>
    public enum ScopeKind
    {
        /// <summary>
        /// The global scope.
        /// </summary>
        Global,
        /// <summary>
        /// A file's scope.
        /// </summary>
        File,
        /// <summary>
        /// A global function's scope.
        /// </summary>
        GlobalFunction,
        /// <summary>
        /// A local function's scope.
        /// </summary>
        LocalFunction,
        /// <summary>
        /// A block's scope.
        /// </summary>
        Block,
    }
}
