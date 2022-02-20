namespace Loretta.CodeAnalysis.Lua.Utilities
{
    /// <summary>
    /// A class with utilities for strings.
    /// </summary>
    internal static class StringUtils
    {
        /// <summary>
        /// Returns whether the provided string is a valid identifier.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>Whether the provided string is a valid identifier.</returns>
        public static bool IsIdentifier(ReadOnlySpan<char> value)
        {
            if (value.IsEmpty)
                return false;

            if (!CharUtils.IsValidFirstIdentifierChar(value[0]))
                return false;

            for (var idx = 1; idx < value.Length; idx++)
            {
                if (!CharUtils.IsValidTrailingIdentifierChar(value[idx]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the provided string is a valid identifier.
        /// </summary>
        /// <param name="value">The string to check.</param>
        ///
        /// <returns>Whether the provided string is a valid identifier.</returns>
        public static bool IsIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (!CharUtils.IsValidFirstIdentifierChar(value[0]))
                return false;

            for (var idx = 1; idx < value.Length; idx++)
            {
                if (!CharUtils.IsValidTrailingIdentifierChar(value[idx]))
                    return false;
            }

            return true;
        }

        public static string Trim(string value)
        {
            var startIdx = 0;
            var endIdx = value.Length - 1;
            while (CharUtils.IsWhitespace(value[startIdx]))
                startIdx++;
            while (CharUtils.IsWhitespace(value[endIdx]))
                endIdx--;
            return value[startIdx..(endIdx + 1)];
        }

        public static ReadOnlySpan<char> Trim(ReadOnlySpan<char> value)
        {
            var startIdx = 0;
            var endIdx = value.Length - 1;
            while (CharUtils.IsWhitespace(value[startIdx]))
                startIdx++;
            while (CharUtils.IsWhitespace(value[endIdx]))
                endIdx--;
            return value[startIdx..(endIdx + 1)];
        }
    }
}
