namespace Loretta.CodeAnalysis.Lua.Experimental
{
    /// <summary>
    /// Settings to use when constant folding.
    /// </summary>
    public record ConstantFoldingOptions(
        bool ExtractNumbersFromStrings)
    {
        /// <summary>
        /// The default, most conservative, preset.
        /// </summary>
        public static readonly ConstantFoldingOptions Default = new(
            ExtractNumbersFromStrings: false);

        /// <summary>
        /// The preset with everything set to true.
        /// </summary>
        public static readonly ConstantFoldingOptions All = new(
            ExtractNumbersFromStrings: true);
    }
}
