using System.Linq;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.Syntax.UnitTests.Scoping
{
    public class FindVariableTests : ScriptTestsBase
    {
        [Theory]
        [Trait("Category", "Script/FindVariable")]
        [InlineData(ScopeKind.File, "glob")]
        [InlineData(ScopeKind.Function, "a")]
        [InlineData(ScopeKind.Block, "b")]
        public void Script_FindVariable_ReturnsNullWhenNoVariableIsAvailable(ScopeKind scopeKind, string name)
        {
            SetupScript(out var innerMostScope);
            Assert.Null(innerMostScope.FindVariable(name, scopeKind));
        }

        [Theory]
        [Trait("Category", "Script/FindVariable")]
        [InlineData(ScopeKind.Global, "glob")]
        [InlineData(ScopeKind.Global, "a")]
        [InlineData(ScopeKind.File, "a")]
        [InlineData(ScopeKind.Global, "b")]
        [InlineData(ScopeKind.File, "b")]
        [InlineData(ScopeKind.Function, "b")]
        [InlineData(ScopeKind.Global, "c")]
        [InlineData(ScopeKind.File, "c")]
        [InlineData(ScopeKind.Function, "c")]
        [InlineData(ScopeKind.Block, "c")]
        public void Script_FindVariable_ReturnsVariableWhenVariableIsAvailable(ScopeKind scopeKind, string name)
        {
            SetupScript(out var innerMostScope);
            Assert.NotNull(innerMostScope.FindVariable(name, scopeKind));
        }

        private static void SetupScript(out IScope innerMostScope)
        {
            var script = ParseScript("local a = 1\r\n" +
                                     "function f(b)\r\n" +
                                     "    print(b)\r\n" +
                                     "    do\r\n" +
                                     "        local c = 3\r\n" +
                                     "    end\r\n" +
                                     "end",
                                     "glob = 2");

            var firstTree = script.SyntaxTrees.First();
            var root = Assert.IsType<CompilationUnitSyntax>(firstTree.GetRoot());
            var functionDecl = Assert.IsType<FunctionDeclarationStatementSyntax>(root.Statements.Statements[1]);
            var doStatement = Assert.IsType<DoStatementSyntax>(functionDecl.Body.Statements[1]);
            innerMostScope = script.GetScope(doStatement)!;
            Assert.NotNull(innerMostScope);
        }
    }
}
