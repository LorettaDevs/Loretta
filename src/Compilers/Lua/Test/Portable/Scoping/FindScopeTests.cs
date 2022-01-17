using System.Linq;
using Loretta.CodeAnalysis.Lua.Syntax;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Scoping
{
    public class ScopeTests : ScriptTestsBase
    {
        [Fact]
        [Trait("Category", "Script/FindScope")]
        public void CompilationUnit_HasFileScope()
        {
            var (tree, script) = ParseScript("print 'Hello'");

            var compilationUnit = (CompilationUnitSyntax) tree.GetRoot();
            var compilationUnitScope = script.GetScope(compilationUnit);

            Assert.NotNull(compilationUnitScope);
            Assert.Equal(ScopeKind.File, compilationUnitScope!.Kind);
            Assert.Equal(script.RootScope, compilationUnitScope.ContainingScope);
        }

        [Fact]
        [Trait("Category", "Script/FindScope")]
        public void FindScope_OnRootElement_ReturnsRootScope()
        {
            var (tree, script) = ParseScript("print 'Hello'");

            var compilationUnit = (CompilationUnitSyntax) tree.GetRoot();
            var compilationUnitScope = script.GetScope(compilationUnit);
            var printExpression = (FunctionCallExpressionSyntax) ((ExpressionStatementSyntax) compilationUnit.Statements.Statements.Single()).Expression;
            var printExpressionScope = script.FindScope(printExpression.Expression);

            Assert.Equal(compilationUnitScope, printExpressionScope);
        }
    }
}
