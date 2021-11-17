using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.Syntax.UnitTests.Scoping
{
    public class ScopeTests
    {
        private static (SyntaxTree, Script) ParseScript(string code)
        {
            var tree = LuaSyntaxTree.ParseText("print 'Hello'");
            var script = new Script(ImmutableArray.Create(tree));
            return (tree, script);
        }

        [Fact]
        public void CompilationUnit_HasFileScope()
        {
            var (tree, script) = ParseScript("print 'Hello'");

            var compilationUnit = (CompilationUnitSyntax) tree.GetRoot();
            var compilationUnitScope = script.GetScope(compilationUnit);

            Assert.NotNull(compilationUnitScope);
            Assert.Equal(ScopeKind.File, compilationUnitScope.Kind);
            Assert.Equal(script.RootScope, compilationUnitScope.Parent);
        }

        [Fact]
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
