using System.Text;

namespace Loretta.CodeAnalysis.Lua.Test.Utilities
{
    public static class RandomSpaceInserter
    {
        public static IEnumerable<string> GetTokenPairs(string[] parts)
        {
            var spaceLocations = parts.Length - 1;
            var builder        = new StringBuilder();
            var lastCase       = (1UL << spaceLocations) - 1;

            for (var spaces = 0UL; spaces <= lastCase; spaces++)
            {
                builder.Clear();
                for (var partIdx = 0; partIdx < parts.Length - 1; partIdx++)
                {
                    builder.Append(parts[partIdx]);
                    if (((1UL << partIdx) & spaces) != 0) builder.Append(' ');
                }
                builder.Append(parts[^1]);

                yield return builder.ToString();
            }
        }
    }
}
