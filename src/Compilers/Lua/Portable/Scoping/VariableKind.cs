namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The kind of varibles available.
    /// </summary>
    public enum VariableKind
    {
        /// <summary>
        /// A local variable.
        /// </summary>
        Local,
        /// <summary>
        /// A global variable.
        /// </summary>
        Global,
        /// <summary>
        /// A function parameter.
        /// </summary>
        Parameter,
        /// <summary>
        /// A loop iteration variable.
        /// </summary>
        Iteration,
    }
}
