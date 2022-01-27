using Loretta.CodeAnalysis.Lua.Experimental;
using Loretta.CodeAnalysis.Lua.Experimental.Minifying;
using Loretta.CodeAnalysis.Lua.Test.Utilities;
using Loretta.Test.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Experimental
{
    public class RegressionTests : LuaTestBase
    {
        [Fact(Timeout = 1000)]
        [Trait("Type", TestType.Regression)]
        [Trait("Category", TestCategory.Experimental_Minifying_NamingStrategies)]
        // This test was added because I found out that the naming strategies were falling into
        // infinite loops when they found an existing variable in the scope that they were inserting.
        public void NamingStrategies_Alphabetic_DoesNotFallIntoAnInfiniteLoop()
        {
            // Here it'll try to rename 'c' to 'b' but will fail because there's already a global with
            // the same name so it should proceed to prefix it with '_'s.
            const string code = "local a, c = 1, 2\r\n" +
                                "print(a, b)";
            var tree = ParseAndValidate(code);

            var minified = tree.Minify(NamingStrategies.Alphabetical);
            Assert.Equal("local a,_b=1,2 print(a,b)", minified.ToString());
        }

        [Theory]
        [WorkItem(55, "https://github.com/GGG-KILLER/Loretta/issues/55")]
        [Trait("Type", TestType.Regression)]
        [Trait("Category", TestCategory.Experimental_Minifying)]
        [InlineData(
            "local x = 0\r\n" +
            "x = x + 1",
            "local a=0 a=a+1")]
        [InlineData(
            "local x = 0\r\n" +
            "x += x + 1",
            "local a=0 a+=a+1")]
        public void Minifier_DoesNotDoubleFree_OnReadAndWriteEndingInTheSamePlace(string code, string expected)
        {
            var tree = ParseAndValidate(code);

            var minified = tree.Minify(NamingStrategies.Alphabetical);
            Assert.Equal(expected, minified.ToString());
        }
    }
}
