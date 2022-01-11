using System.Collections.Immutable;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.Syntax.UnitTests.Scoping
{
    public class ScriptTestsBase
    {
        protected static (SyntaxTree, Script) ParseScript(string code)
        {
            var tree = LuaSyntaxTree.ParseText(code);
            Assert.Empty(tree.GetDiagnostics());
            var script = new Script(ImmutableArray.Create(tree));
            return (tree, script);
        }
    }
}