using Loretta.CodeAnalysis.Lua.Test.Utilities;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Scoping;

public class ScriptTestsBase : LuaTestBase
{
    protected static async Task<(SyntaxTree, Script)> ParseScriptAsync(string code, LuaSyntaxOptions? options = null)
    {
        var tree = await ParseWithRoundTripCheckAsync(code, options != null ? new LuaParseOptions(options) : null);
        tree.GetDiagnostics().Verify();
        var script = new Script([tree]);
        return (tree, script);
    }

    protected static async Task<Script> ParseScriptAsync(params string[] codes) =>
        await ParseScriptAsync(LuaSyntaxOptions.All, codes);

    protected static async Task<Script> ParseScriptAsync(LuaSyntaxOptions options, params string[] codes)
    {
        var parseOptions = new LuaParseOptions(options);
        var trees        = new List<SyntaxTree>();
        foreach (var code in codes)
        {
            var tree = await ParseWithRoundTripCheckAsync(code, parseOptions);
            tree.GetDiagnostics().Verify();
            trees.Add(tree);
        }
        var script = new Script([..trees]);
        return script;
    }
}
