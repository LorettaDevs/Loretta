namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>Defines what they type of strings using <c>`</c> delimiters will be parsed as.</summary>
    public enum BacktickStringType
    {
        /// <summary>Strings with <c>`</c> delimiters have no meaning and will generate errors for unsupported interpolations.</summary>
        None,

        /// <summary>Strings with <c>`</c> delimiters will be parsed as FiveM hash string literals.</summary>
        HashLiteral,

        /// <summary>Strings with <c>`</c> delimiters will be parsed as interpolated string literals.</summary>
        InterpolatedStringLiteral,
    }
}
