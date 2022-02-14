using System;
using System.Collections.Generic;
using System.Text;

namespace Loretta.CodeAnalysis.Lua.Test.Utilities
{
    public static class RandomSpaceInserter
    {
        public static IEnumerable<string> Enumerate(params string[] parts)
        {
            if (parts.Length is < 1 or > 64)
                throw new ArgumentOutOfRangeException(nameof(parts));

            var spaceLocations = parts.Length - 1;
            var builder = new StringBuilder();

            var lastCase = (1UL << spaceLocations) - 1;

            for (var spaces = 0UL; spaces <= lastCase; spaces++)
            {
                builder.Clear();

                for (var partIdx = 0; partIdx < parts.Length - 1; partIdx++)
                {
                    builder.Append(parts[partIdx]);
                    if (((1UL << partIdx) & spaces) != 0)
                        builder.Append(' ');
                }
                builder.Append(parts[^1]);

                yield return builder.ToString();
            }
        }

        public static IEnumerable<object[]> MemberDataEnumerate(params string[] parts)
        {
            foreach (var result in Enumerate(parts))
            {
                yield return new object[] { result };
            }
        }
    }
}
