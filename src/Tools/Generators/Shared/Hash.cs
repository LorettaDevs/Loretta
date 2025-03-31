namespace Loretta.Generators
{
    internal static class Hash
    {
        /// <summary>
        /// This is how VB Anonymous Types combine hash values for fields.
        /// </summary>
        internal static int Combine(int newKey, int currentKey) => unchecked((currentKey * (int) 0xA5555529) + newKey);

        internal static int Combine(bool newKeyPart, int currentKey) => Combine(currentKey, newKeyPart ? 1 : 0);

        /// <summary>
        /// This is how VB Anonymous Types combine hash values for fields.
        /// PERF: Do not use with enum types because that involves multiple
        /// unnecessary boxing operations.  Unfortunately, we can't constrain
        /// T to "non-enum", so we'll use a more restrictive constraint.
        /// </summary>
        internal static int Combine<T>(T newKeyPart, int currentKey) where T : class?
        {
            var hash = unchecked(currentKey * (int) 0xA5555529);

            if (newKeyPart != null) return unchecked(hash + newKeyPart.GetHashCode());

            return hash;
        }

        internal static int CombineValues<T>(IEnumerable<T>? values, int maxItemsToHash = int.MaxValue)
            => values is null
                   ? 0
                   : values.Take(maxItemsToHash).Where(static x => x is not null).Aggregate(
                       0,
                       static (current, value) => Combine(value!.GetHashCode(), current));

        internal static int CombineValues<T>(T[]? values, int maxItemsToHash = int.MaxValue)
        {
            if (values == null) return 0;

            var maxSize  = Math.Min(maxItemsToHash, values.Length);
            var hashCode = 0;

            for (var i = 0; i < maxSize; i++)
            {
                var value = values[i];

                // Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
                if (value is not null) hashCode = Combine(value.GetHashCode(), hashCode);
            }

            return hashCode;
        }

        internal static int CombineValues<T>(ImmutableArray<T> values, int maxItemsToHash = int.MaxValue)
            => values.IsDefaultOrEmpty
                   ? 0
                   : values.Take(maxItemsToHash).Where(static x => x is not null).Aggregate(
                       0,
                       static (current, value) => Combine(value!.GetHashCode(), current));

        internal static int CombineValues(
            IEnumerable<string?>? values,
            StringComparer        stringComparer,
            int                   maxItemsToHash = int.MaxValue)
            => values == null
                   ? 0
                   : values.Take(maxItemsToHash).OfType<string>().Aggregate(
                       0,
                       (current, value) => Combine(stringComparer.GetHashCode(value), current));
    }
}
