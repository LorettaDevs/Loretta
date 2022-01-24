using Loretta.CodeAnalysis.Lua.Experimental;
using Loretta.CodeAnalysis.Lua.Experimental.Minifying;
using Loretta.CodeAnalysis.Lua.Test.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Experimental
{
    public class NamingStrategiesTests : LuaTestBase
    {
        private static SyntaxTree ParseAndValidate(
            string text,
            LuaSyntaxOptions? options = null)
        {
            var parsedTree = ParseWithRoundTripCheck(
                text,
                new(options ?? LuaSyntaxOptions.All));
            parsedTree.GetDiagnostics().Verify();
            return parsedTree;
        }

        [Fact]
        [Trait("Category", "Experimental/Minifying/NamingStrategies/Alphabetical")]
        // This test was added because I found out that the naming strategies were falling into
        // infinite loops when they found an existing variable in the scope that they were inserting.
        public void NamingStrategies_Alphabetic_DoesNotFallIntoAnInfiniteLoop()
        {
            // Here it'll try to rename 'c' to 'b' but will fail because there's already a global with
            // the same name so it should proceed to prefix it with '_'s.
            const string code = "local a, c = 1, 2\r\n" +
                                "print(a, b)";
            var tree = ParseAndValidate(code);

            var minified = AssertEx.RunsWithin(1000, () =>
                tree.Minify(NamingStrategies.Alphabetical));
            Assert.Equal("local a,_b=1,2 print(a,b)", minified.ToString());
        }
    }
}
