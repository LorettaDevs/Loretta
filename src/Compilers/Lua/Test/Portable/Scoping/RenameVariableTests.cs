namespace Loretta.CodeAnalysis.Lua.UnitTests.Scoping;

public sealed class RenameVariableTests : ScriptTestsBase
{
    [Test]
    public async Task Script_RenameVariable_ReturnsErrorForUnsupportedIdentifier()
    {
        var (tree, script) = await ParseScriptAsync("local a = 2", LuaSyntaxOptions.Lua51);

        var variable = script.GetVariable(
            (await tree.GetRootAsync()).ChildNodes().First().ChildNodes().First().ChildNodes().First());
        await Assert.That(variable).IsNotNull();
        var result = script.RenameVariable(variable!, "\uFEFF");
        await Assert.That(result.IsErr).IsTrue();
        var err = await Assert.That(result.Err.Value).HasSingleItem();
        await Assert.That(err).IsTypeOf<IdentifierNameNotSupportedError>().And.Satisfies(
            static err => err.TreeWithoutSupport,
            treeWithout => treeWithout.IsEqualTo(tree));
    }

    [Test]
    public async Task Script_RenameVariable_ReturnsErrorForConflictingVariable()
    {
        var (tree, script) = await ParseScriptAsync("local a, b = 2, 3", LuaSyntaxOptions.Lua51);

        var localDecl = (await tree.GetRootAsync()).ChildNodes().First().ChildNodes().First();
        var variableA = script.GetVariable(localDecl.ChildNodes().First());
        var variableB = script.GetVariable(localDecl.ChildNodes().ElementAt(1));
        await Assert.That(variableA).IsNotNull();
        await Assert.That(variableB).IsNotNull();
        var result = script.RenameVariable(variableA!, "b");
        await Assert.That(result.IsErr).IsTrue();
        var err = await Assert.That(result.Err.Value).HasSingleItem();
        await Assert.That(err).IsTypeOf<VariableConflictError>().And.Satisfies(
            static err => err.VariableBeingConflictedWith,
            var => var.IsEqualTo(variableB));
    }

    [Test]
    public async Task Script_RenameVariable_ReturnsCorrectlyRenamedScript()
    {
        var (tree, script) = await ParseScriptAsync("local a = 2\r\nlocal function a() end", LuaSyntaxOptions.Lua51);

        var variable = script.GetVariable(
            (await tree.GetRootAsync()).ChildNodes().First().ChildNodes().First().ChildNodes().First());
        await Assert.That(variable).IsNotNull();
        var result = script.RenameVariable(variable!, "b");
        await Assert.That(result.IsOk).IsTrue();
        var newTree = result.Ok.Value;
        await Assert.That(newTree).Satisfies(
            static script => script.SyntaxTrees,
            static trees => trees.HasSingleItem().And.Satisfies(
                static tree => tree[0].ToString(),
                static text => text.IsEqualTo("local b = 2\r\nlocal function a() end")!));
    }
}
