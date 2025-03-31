using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.Test.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Scoping;

public sealed class ScopeTests : ScriptTestsBase
{
    [Fact]
    [Trait("Category", "Script/FindScope")]
    public void CompilationUnit_HasFileScope()
    {
        var (tree, script) = ParseScript("print 'Hello'");

        var compilationUnit      = (CompilationUnitSyntax) tree.GetRoot(TestContext.Current.CancellationToken);
        var compilationUnitScope = script.GetScope(compilationUnit);

        Assert.NotNull(compilationUnitScope);
        Assert.Equal(ScopeKind.File, compilationUnitScope.Kind);
        Assert.Equal(script.RootScope, compilationUnitScope.ContainingScope);
    }

    [Fact]
    [Trait("Category", "Script/FindScope")]
    public void FindScope_OnRootElement_ReturnsRootScope()
    {
        var (tree, script) = ParseScript("print 'Hello'");

        var compilationUnit      = (CompilationUnitSyntax) tree.GetRoot(TestContext.Current.CancellationToken);
        var compilationUnitScope = script.GetScope(compilationUnit);
        var printExpression =
            (FunctionCallExpressionSyntax)
            ((ExpressionStatementSyntax) compilationUnit.Statements.Statements.Single()).Expression;
        var printExpressionScope = script.FindScope(printExpression.Expression);

        Assert.Equal(compilationUnitScope, printExpressionScope);
    }

    [Fact]
    [Trait("Category", "Script/FindScope")]
    [WorkItem(106, "https://github.com/LorettaDevs/Loretta/issues/106")]
    public void FindScope_LocalFunctionIsParsed()
    {
        var (tree, script) = ParseScript("local function a() end");
        var fileScope = script.GetScope(tree.GetRoot(TestContext.Current.CancellationToken));

        Assert.Equal(1, fileScope?.ContainedScopes.Count());
    }

    [Fact]
    [Trait("Category", "Script/FindScope")]
    [WorkItem(106, "https://github.com/LorettaDevs/Loretta/issues/106")]
    public void FindScope_AnonymousFunctionIsParsed()
    {
        var (tree, script) = ParseScript("(function(Variable) end)()");
        var fileScope = script.GetScope(tree.GetRoot(TestContext.Current.CancellationToken));

        Assert.Equal(1, fileScope?.ContainedScopes.Count());
    }
}