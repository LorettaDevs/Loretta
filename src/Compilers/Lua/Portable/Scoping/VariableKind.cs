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
        LocalVariable,
        /// <summary>
        /// A global variable.
        /// </summary>
        GlobalVariable,
        /// <summary>
        /// A function parameter.
        /// </summary>
        Parameter
    }
}
