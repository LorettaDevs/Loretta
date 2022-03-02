using Loretta.CodeAnalysis.Test.Utilities;
using Loretta.Test.Utilities;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Lexical
{
    public class NoLongTestsCondition : ExecutionCondition
    {
        public override bool ShouldSkip =>
            RuntimeUtilities.IsDesktopRuntime || Environment.GetEnvironmentVariable("NO_LONG_TESTS") is ("1" or "true" or "TRUE" or "True");

        public override string SkipReason =>
            "NO_LONG_TESTS environment variable was set or runtime is .NET Framework.";
    }
}
