namespace Loretta.CodeAnalysis.Lua.Operations
{
    /// <summary>
    /// Kind of binary operator.
    /// </summary>
    internal enum BinaryOperatorKind
    {
        /// <summary>
        /// Represents unknown or error operator kind.
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents the Lua <c>+</c> operator.
        /// </summary>
        Addition,

        /// <summary>
        /// Represents the Lua <c>-</c> operator.
        /// </summary>
        Subtraction,

        /// <summary>
        /// Represents the Lua <c>*</c> operator.
        /// </summary>
        Multiplication,

        /// <summary>
        /// Represents the Lua <c>/</c> operator.
        /// </summary>
        Division,

        /// <summary>
        /// Represents the Lua <c>%</c> operator.
        /// </summary>
        Modulus,

        /// <summary>
        /// Represents the Lua <c>^</c> operator.
        /// </summary>
        Exponentiation,

        /// <summary>
        /// Represents the Lua <c>..</c> operator.
        /// </summary>
        StringConcatenation,

        /// <summary>
        /// Represents the Lua <c>&amp;</c> operator.
        /// </summary>
        BitwiseAnd,

        /// <summary>
        /// Represents the Lua <c>|</c> operator.
        /// </summary>
        BitwiseOr,

        /// <summary>
        /// Represents the Lua <c>~</c> operator.
        /// </summary>
        ExclusiveOr,

        /// <summary>
        /// Represents the Lua <c>&lt;&lt;</c> operator.
        /// </summary>
        LeftShift,

        /// <summary>
        /// Represents the Lua <c>&gt;&gt;</c> operator.
        /// </summary>
        RightShift,

        /// <summary>
        /// Represents the Lua <c>==</c> operator.
        /// </summary>
        Equals,

        /// <summary>
        /// Represents the Lua <c>!=</c> operator.
        /// </summary>
        NotEquals,

        /// <summary>
        /// Represents the Lua <c>&gt;</c> operator.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Represents the Lua <c>&gt;=</c> operator.
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// Represents the Lua <c>&lt;</c> operator.
        /// </summary>
        LessThan,

        /// <summary>
        /// Represents the Lua <c>&lt;=</c> operator.
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// Represents the Lua <see langword="and"/> operator.
        /// </summary>
        And,

        /// <summary>
        /// Represents the Lua <see langword="or"/> operator.
        /// </summary>
        Or,
    }
}
