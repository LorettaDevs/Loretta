using System.Reflection;
using System.Text;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Loretta.CodeAnalysis.Lua.Test.Utilities
{
    public class RandomSpaceInserterDataAttribute : DataAttribute
    {
        private readonly string[] _parts;

        public RandomSpaceInserterDataAttribute(params string[] parts)
        {
            if (parts.Length is < 1 or > 64) throw new ArgumentOutOfRangeException(nameof(parts));
            _parts = parts;
        }

        /// <inheritdoc />
        public override bool SupportsDiscoveryEnumeration() => true;

        /// <inheritdoc />
        public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
            MethodInfo      testMethod,
            DisposalTracker disposalTracker)
        {
            var spaceLocations = _parts.Length - 1;
            var builder        = new StringBuilder();
            var rows           = new TheoryData<string>();
            var lastCase       = (1UL << spaceLocations) - 1;

            for (var spaces = 0UL; spaces <= lastCase; spaces++)
            {
                builder.Clear();
                for (var partIdx = 0; partIdx < _parts.Length - 1; partIdx++)
                {
                    builder.Append(_parts[partIdx]);
                    if (((1UL << partIdx) & spaces) != 0) builder.Append(' ');
                }
                builder.Append(_parts[^1]);

                rows.Add(builder.ToString());
            }

            return new ValueTask<IReadOnlyCollection<ITheoryDataRow>>(rows);
        }
    }
}
