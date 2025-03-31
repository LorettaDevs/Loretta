using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Scoping;

public sealed class FindVariableTests : ScriptTestsBase
{
    [Test]
    [TProperty("Category", "Script/FindVariable")]
    [Arguments(ScopeKind.File, "glob")]
    [Arguments(ScopeKind.Function, "a")]
    [Arguments(ScopeKind.Block, "b")]
    public async Task Script_FindVariable_ReturnsNullWhenNoVariableIsAvailable(ScopeKind scopeKind, string name)
    {
        var innerMostScope = await SetupScriptAsync();
        await Assert.That(innerMostScope.FindVariable(name, scopeKind)).IsNull();
    }

    [Test]
    [TProperty("Category", "Script/FindVariable")]
    [Arguments(ScopeKind.Global, "glob")]
    [Arguments(ScopeKind.Global, "a")]
    [Arguments(ScopeKind.File, "a")]
    [Arguments(ScopeKind.Global, "b")]
    [Arguments(ScopeKind.File, "b")]
    [Arguments(ScopeKind.Function, "b")]
    [Arguments(ScopeKind.Global, "c")]
    [Arguments(ScopeKind.File, "c")]
    [Arguments(ScopeKind.Function, "c")]
    [Arguments(ScopeKind.Block, "c")]
    public async Task Script_FindVariable_ReturnsVariableWhenVariableIsAvailable(ScopeKind scopeKind, string name)
    {
        var innerMostScope = await SetupScriptAsync();
        await Assert.That(innerMostScope.FindVariable(name, scopeKind)).IsNotNull();
    }

    private static async Task<IScope> SetupScriptAsync()
    {
        var script = await ParseScriptAsync(
                         """
                         local a = 1
                         function f(b)
                             print(b)
                             do
                                 local c = 3
                             end
                         end
                         """,
                         "glob = 2");

        var firstTree = script.SyntaxTrees.First();
        var root      = await Assert.That(await firstTree.GetRootAsync()).IsTypeOf<CompilationUnitSyntax>();
        var functionDecl = await Assert.That(root?.Statements.Statements[1])
                                       .IsTypeOf<FunctionDeclarationStatementSyntax>();
        var doStatement    = await Assert.That(functionDecl?.Body.Statements[1]).IsTypeOf<DoStatementSyntax>();
        var innerMostScope = script.GetScope(doStatement!)!;
        await Assert.That(innerMostScope).IsNotNull();
        return innerMostScope;
    }
}
