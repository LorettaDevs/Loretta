using Loretta.CodeAnalysis.Lua.Experimental;
using Loretta.CodeAnalysis.Lua.Experimental.Minifying;
using Loretta.CodeAnalysis.Lua.Test.Utilities;
using Loretta.Test.Utilities;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Experimental;

public sealed class RegressionTests : LuaTestBase
{
    [Test]
    // This test was added because I found out that the naming strategies were falling into
    // infinite loops when they found an existing variable in the scope that they were inserting.
    public async Task NamingStrategies_Alphabetic_DoesNotFallIntoAnInfiniteLoop()
    {
        // Here it'll try to rename 'c' to 'b' but will fail because there's already a global with
        // the same name so it should proceed to prefix it with '_'s.
        const string code = "local a, c = 1, 2\r\n" + "print(a, b)";
        var          tree = await ParseAndValidateAsync(code);

        var minified = tree.Minify(NamingStrategies.Alphabetical);
        await Assert.That(minified.ToString()).IsEqualTo("local a,_b=1,2 print(a,b)");
    }

    [Test]
    [WorkItem(55, "https://github.com/GGG-KILLER/Loretta/issues/55")]
    [Arguments(
        """
        local x = 0
        x = x + 1
        """,
        "local a=0 a=a+1")]
    [Arguments(
        """
        local x = 0
        x += x + 1
        """,
        "local a=0 a+=a+1")]
    public async Task Minifier_DoesNotDoubleFree_OnReadAndWriteEndingInTheSamePlace(string code, string expected)
    {
        var tree = await ParseAndValidateAsync(code);

        var minified = tree.Minify(NamingStrategies.Alphabetical);
        await Assert.That(minified.ToString()).IsEqualTo(expected);
    }
}
