using System.Reflection;
using Xunit.Sdk;

namespace Loretta.CodeAnalysis.Lua.Test.Utilities
{
    public class RandomSpaceInserterDataAttribute : DataAttribute
    {
        private readonly string[] _parts;

        public RandomSpaceInserterDataAttribute(params string[] parts)
        {
            _parts = parts;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod) =>
            RandomSpaceInserter.MemberDataEnumerate(_parts);
    }
}
