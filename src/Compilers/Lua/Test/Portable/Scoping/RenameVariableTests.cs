using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Scoping
{
    public class RenameVariableTests : ScriptTestsBase
    {
        [Fact]
        [Trait("Category", "Script/RenameVariable")]
        public void Script_RenameVariable_ReturnsErrorForUnsupportedIdentifier()
        {
            var (tree, script) = ParseScript("local a = 2", LuaSyntaxOptions.Lua51);

            var variable = script.GetVariable(tree.GetRoot().ChildNodes().First().ChildNodes().First().ChildNodes().First());
            Assert.NotNull(variable);
            var result = script.RenameVariable(variable!, "\uFEFF");
            Assert.True(result.IsErr);
            var error = Assert.IsType<IdentifierNameNotSupportedError>(Assert.Single(result.Err.Value));
            Assert.Equal(tree, error.TreeWithoutSupport);
        }

        [Fact]
        [Trait("Category", "Script/RenameVariable")]
        public void Script_RenameVariable_ReturnsErrorForConflictingVariable()
        {
            var (tree, script) = ParseScript("local a, b = 2, 3", LuaSyntaxOptions.Lua51);

            var localDecl = tree.GetRoot().ChildNodes().First().ChildNodes().First();
            var variableA = script.GetVariable(localDecl.ChildNodes().First());
            var variableB = script.GetVariable(localDecl.ChildNodes().ElementAt(1));
            Assert.NotNull(variableA);
            Assert.NotNull(variableB);
            var result = script.RenameVariable(variableA!, "b");
            Assert.True(result.IsErr);
            var error = Assert.IsType<VariableConflictError>(Assert.Single(result.Err.Value));
            Assert.Equal(variableB, error.VariableBeingConflictedWith);
        }

        [Fact]
        [Trait("Category", "Script/RenameVariable")]
        public void Script_RenameVariable_ReturnsCorrectlyRenamedScript()
        {
            var (tree, script) = ParseScript("local a = 2\r\nlocal function a() end", LuaSyntaxOptions.Lua51);

            var variable = script.GetVariable(tree.GetRoot().ChildNodes().First().ChildNodes().First().ChildNodes().First());
            Assert.NotNull(variable);
            var result = script.RenameVariable(variable!, "b");
            Assert.True(result.IsOk);
            var newTree = result.Ok.Value;
            Assert.Equal("local b = 2\r\nlocal function a() end", Assert.Single(newTree.SyntaxTrees).ToString());
        }
    }
}
