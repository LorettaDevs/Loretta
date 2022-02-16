namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The format integers should be stored as.
    /// </summary>
    public enum IntegerFormats
    {
        /// <summary>
        /// No integer support at all and numbers are parsed
        /// as <see cref="double"/>s without overflow behavior.
        /// </summary>
        NotSupported = 0,
        /// <summary>
        /// Integers are stored as a <see cref="double"/>.
        /// </summary>
        Double = 1,
        /// <summary>
        /// Integers are stored as a <see cref="long"/>.
        /// </summary>
        Int64 = 2,
    }
}
