using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.Test.Utilities;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Scoping;

public sealed class ScopeTests : ScriptTestsBase
{
    [Test]
    public async Task CompilationUnit_HasFileScope()
    {
        var (tree, script) = await ParseScriptAsync("print 'Hello'");

        var compilationUnit      = (CompilationUnitSyntax) await tree.GetRootAsync();
        var compilationUnitScope = script.GetScope(compilationUnit);

        await Assert.That(compilationUnitScope).IsNotNull();
        await Assert.That(compilationUnitScope?.Kind).IsEqualTo(ScopeKind.File);
        await Assert.That(compilationUnitScope?.ContainingScope).IsEqualTo(script.RootScope);
    }

    [Test]
    public async Task FindScope_OnRootElement_ReturnsRootScope()
    {
        var (tree, script) = await ParseScriptAsync("print 'Hello'");

        var compilationUnit      = (CompilationUnitSyntax) await tree.GetRootAsync();
        var compilationUnitScope = script.GetScope(compilationUnit);
        var printExpression =
            (FunctionCallExpressionSyntax) ((ExpressionStatementSyntax) compilationUnit.Statements.Statements.Single())
            .Expression;
        var printExpressionScope = script.FindScope(printExpression.Expression);

        await Assert.That(printExpressionScope).IsEqualTo(compilationUnitScope);
    }

    [Test]
    [WorkItem(106, "https://github.com/LorettaDevs/Loretta/issues/106")]
    public async Task FindScope_LocalFunctionIsParsed()
    {
        var (tree, script) = await ParseScriptAsync("local function a() end");
        var fileScope = script.GetScope(await tree.GetRootAsync());

        await Assert.That(fileScope?.ContainedScopes).HasCount().EqualTo(1);
    }

    [Test]
    [WorkItem(106, "https://github.com/LorettaDevs/Loretta/issues/106")]
    public async Task FindScope_AnonymousFunctionIsParsed()
    {
        var (tree, script) = await ParseScriptAsync("(function(Variable) end)()");
        var fileScope = script.GetScope(await tree.GetRootAsync());

        await Assert.That(fileScope?.ContainedScopes).HasCount().EqualTo(1);
    }
}
