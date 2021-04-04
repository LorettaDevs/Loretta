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
        /// A local function's scope.
        /// </summary>
        Function,
        /// <summary>
        /// A block's scope.
        /// </summary>
        Block,
    }
}
