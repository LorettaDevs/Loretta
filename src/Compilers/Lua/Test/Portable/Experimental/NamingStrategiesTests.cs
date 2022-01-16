using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        // This test was added because I found out that the naming strategies were
        public void NamingStrategies_Alphabetic_DoesNotFallIntoAnInfiniteLoop()
        {
            const string code = @"local a, b = 1, 2";
            var tree = ParseAndValidate(code);

            var minified = AssertEx.RunsWithin(1000, () =>
                tree.Minify(NamingStrategies.Alphabetical));
            Assert.Equal("local _a,_a=1,2", minified.ToString());
        }
    }
}
