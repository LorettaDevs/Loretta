namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The type of continue the lua flavor being parsed has.
    /// </summary>
    public enum ContinueType
    {
        /// <summary>
        /// No continue.
        /// </summary>
        None,

        /// <summary>
        /// Continue is a keyword.
        /// </summary>
        Keyword,

        /// <summary>
        /// Continue is a contextual keyword (is only a keyword when used as a statement).
        /// </summary>
        ContextualKeyword
    }
}