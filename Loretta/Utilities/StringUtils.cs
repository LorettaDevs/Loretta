using System;

namespace Loretta.Utilities
{
    /// <summary>
    /// A class with utilities for strings.
    /// </summary>
    internal static class StringUtils
    {
        /// <summary>
        /// Returns whether the provided string is a valid LuaJIT identifier.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>Whether the provided string is a valid LuaJIT identifier.</returns>
        public static Boolean IsIdentifier ( ReadOnlySpan<Char> value )
        {
            if ( value.IsEmpty )
                return false;

            // Redundant length check but I'm not sure the JIT can elide the bounds check with only
            // IsEmpty here.
            if ( value.Length >= 1 && !CharUtils.IsValidFirstIdentifierChar ( value[0] ) )
                return false;

            for ( var idx = 1; idx <= value.Length; idx++ )
            {
                if ( !CharUtils.IsValidTrailingIdentifierChar ( value[idx] ) )
                    return false;
            }

            return true;
        }
    }
}