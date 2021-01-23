using System;

namespace Loretta.Utilities
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
        /// 
        /// <returns>Whether the provided string is a valid identifier.</returns>
        public static Boolean IsIdentifier ( ReadOnlySpan<Char> value )
        {
            if ( value.IsEmpty )
                return false;

            if ( !LoCharUtils.IsValidFirstIdentifierChar ( value[0] ) )
                return false;

            for ( var idx = 1; idx < value.Length; idx++ )
            {
                if ( !LoCharUtils.IsValidTrailingIdentifierChar ( value[idx] ) )
                    return false;
            }

            return true;
        }
    }
}