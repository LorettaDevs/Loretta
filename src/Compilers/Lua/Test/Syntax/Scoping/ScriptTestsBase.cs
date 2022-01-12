using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.CodeAnalysis.Lua.Test.Utilities;
using Xunit;

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
            var trees = new List<SyntaxTree>();
            foreach (var code in codes)
            {
                var tree = ParseWithRoundTripCheck(code);
                tree.GetDiagnostics().Verify();
            }
            Assert.Empty(trees.SelectMany(tree => tree.GetDiagnostics()));
            var script = new Script(ImmutableArray.CreateRange(trees));
            return script;
        }
    }
}