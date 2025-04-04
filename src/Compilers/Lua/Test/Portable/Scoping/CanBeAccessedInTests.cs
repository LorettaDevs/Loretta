using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Scoping;

public sealed class CanBeAccessedInTests : ScriptTestsBase
{
    [Test]
    public async Task Script_CanBeAccessedIn_ReturnsTrueWhenSameScope()
    {
        var (tree, script) = await ParseScriptAsync("local a = 1 print(a)");
        var root = await Assert.That(await tree.GetRootAsync()).IsTypeOf<CompilationUnitSyntax>();
        var assignment = await Assert.That(root?.Statements.Statements[0])
                                     .IsTypeOf<LocalVariableDeclarationStatementSyntax>();
        var name = await Assert.That(assignment?.Names[0]).IsTypeOf<LocalDeclarationNameSyntax>();

        var variable = script.GetVariable(name!);

        await Assert.That(variable).IsNotNull();
        await Assert.That(variable!.CanBeAccessedIn(variable.ContainingScope)).IsTrue();
    }

    [Test]
    public async Task Script_CanBeAccessedIn_ReturnsTrueWhenScopeIsChild()
    {
        var (tree, script) = await ParseScriptAsync("local a = 1\r\n" + "do\r\n" + "    print(a)\r\n" + "end");
        var root        = await Assert.That(await tree.GetRootAsync()).IsTypeOf<CompilationUnitSyntax>();
        var assignment  = await Assert.That(root?.Statements.Statements[0]).IsTypeOf<LocalVariableDeclarationStatementSyntax>();
        var name        = await Assert.That(assignment?.Names[0]).IsTypeOf<LocalDeclarationNameSyntax>();
        var doStatement = await Assert.That(root?.Statements.Statements[1]).IsTypeOf<DoStatementSyntax>();

        var variable = script.GetVariable(name!);
        var doScope  = script.GetScope(doStatement!);

        await Assert.That(variable).IsNotNull();
        await Assert.That(doScope).IsNotNull();
        await Assert.That(variable!.CanBeAccessedIn(doScope!)).IsTrue();
    }

    [Test]
    public async Task Script_CanBeAccessedIn_ReturnsFalseWhenScopeIsParentOfParent()
    {
        var (tree, script) = await ParseScriptAsync("do\r\n" + "    local a = 1\r\n" + "end");
        var root        = await Assert.That(await tree.GetRootAsync()).IsTypeOf<CompilationUnitSyntax>();
        var doStatement = await Assert.That(root?.Statements.Statements[0]).IsTypeOf<DoStatementSyntax>();
        var assignment = await Assert.That(doStatement?.Body.Statements[0])
                                     .IsTypeOf<LocalVariableDeclarationStatementSyntax>();
        var name = await Assert.That(assignment?.Names[0]).IsTypeOf<LocalDeclarationNameSyntax>();

        var variable  = script.GetVariable(name!);
        var rootScope = script.GetScope(root!);

        await Assert.That(variable).IsNotNull();
        await Assert.That(rootScope).IsNotNull();
        await Assert.That(variable!.CanBeAccessedIn(rootScope!)).IsFalse();
    }
}
