namespace Loretta.CodeAnalysis.Lua.Operations
{
    /// <summary>
    /// Kind of unary operator.
    /// </summary>
    internal enum UnaryOperatorKind
    {
        /// <summary>
        /// Represents unknown or error operator kind.
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents the Lua <c>#</c> operator.
        /// </summary>
        Length,

        /// <summary>
        /// Represents the Lua <c>not</c> operator.
        /// </summary>
        Not,

        /// <summary>
        /// Represents the Lua <c>-</c> operator.
        /// </summary>
        Negation,

        /// <summary>
        /// Represents the Lua <c>~</c> operator.
        /// </summary>
        BitwiseNot,
    }
}
