using System.Collections.Generic;
using System.Collections.Immutable;
using Loretta.CodeAnalysis.Lua.Test.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.UnitTests.Scoping
{
    public class ScriptTestsBase : LuaTestBase
    {
        protected static (SyntaxTree, Script) ParseScript(string code, LuaSyntaxOptions? options = null)
        {
            var tree = ParseWithRoundTripCheck(code, options != null ? new LuaParseOptions(options) : null);
            tree.GetDiagnostics().Verify();
            var script = new Script(ImmutableArray.Create(tree));
            return (tree, script);
        }

        protected static Script ParseScript(params string[] codes) =>
            ParseScript(LuaSyntaxOptions.All, codes);

        protected static Script ParseScript(LuaSyntaxOptions options, params string[] codes)
        {
            var parseOptions = new LuaParseOptions(options);
            var trees = new List<SyntaxTree>();
            foreach (var code in codes)
            {
                var tree = ParseWithRoundTripCheck(code, parseOptions);
                tree.GetDiagnostics().Verify();
                trees.Add(tree);
            }
            var script = new Script(ImmutableArray.CreateRange(trees));
            return script;
        }
    }
}