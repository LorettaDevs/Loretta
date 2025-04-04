namespace Loretta.Generators
{
    internal static class Hash
    {
        /// <summary>
        /// The offset bias value used in the FNV-1a algorithm
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        internal const int FnvOffsetBias = unchecked((int) 0x811C9DC5);

        /// <summary>
        /// The generative factor used in the FNV-1a algorithm
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        internal const int FnvPrime = 0x1000193;

        /// <summary>
        /// Compute the hashcode of a string using FNV-1a
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The FNV-1a hash code of <paramref name="text"/></returns>
        internal static int GetFnvHashCode(string text) => CombineFnvHash(FnvOffsetBias, text);

        internal static int GetJoinedFnvHashCode(IEnumerable<string> values, string separator)
        {
            var first = true;
            var hash  = FnvOffsetBias;
            foreach (var value in values)
            {
                if (!first) hash = CombineFnvHash(hash, separator);
                first = false;
                if (value is not null) hash = CombineFnvHash(hash, value);
            }
            return hash;
        }

        /// <summary>
        /// Combine a string with an existing FNV-1a hash code
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="hashCode">The accumulated hash code</param>
        /// <param name="text">The string to combine</param>
        /// <returns>The result of combining <paramref name="hashCode"/> with <paramref name="text"/> using the FNV-1a algorithm</returns>
        internal static int CombineFnvHash(int hashCode, string text)
            => text.Aggregate(hashCode, static (current, ch) => unchecked((current ^ ch) * FnvPrime));

        /// <summary>
        /// Combine a char with an existing FNV-1a hash code
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="hashCode">The accumulated hash code</param>
        /// <param name="ch">The new character to combine</param>
        /// <returns>The result of combining <paramref name="hashCode"/> with <paramref name="ch"/> using the FNV-1a algorithm</returns>
        internal static int CombineFnvHash(int hashCode, char ch) => unchecked((hashCode ^ ch) * FnvPrime);
    }
}
