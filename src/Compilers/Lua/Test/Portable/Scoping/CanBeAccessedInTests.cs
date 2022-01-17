using Loretta.CodeAnalysis.Lua.Syntax;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Scoping
{
    public class CanBeAccessedInTests : ScriptTestsBase
    {
        [Fact]
        [Trait("Category", "Script/CanBeAccessedIn")]
        public void Script_CanBeAccessedIn_ReturnsTrueWhenSameScope()
        {
            var (tree, script) = ParseScript("local a = 1 print(a)");
            var root = Assert.IsType<CompilationUnitSyntax>(tree.GetRoot());
            var assignment = Assert.IsType<LocalVariableDeclarationStatementSyntax>(root.Statements.Statements[0]);
            var name = Assert.IsType<IdentifierNameSyntax>(assignment.Names[0]);

            var variable = script.GetVariable(name);

            Assert.NotNull(variable);
            Assert.True(variable!.CanBeAccessedIn(variable.ContainingScope));
        }

        [Fact]
        [Trait("Category", "Script/CanBeAccessedIn")]
        public void Script_CanBeAccessedIn_ReturnsTrueWhenScopeIsChild()
        {
            var (tree, script) = ParseScript("local a = 1\r\n" +
                                             "do\r\n" +
                                             "    print(a)\r\n" +
                                             "end");
            var root = Assert.IsType<CompilationUnitSyntax>(tree.GetRoot());
            var assignment = Assert.IsType<LocalVariableDeclarationStatementSyntax>(root.Statements.Statements[0]);
            var name = Assert.IsType<IdentifierNameSyntax>(assignment.Names[0]);
            var doStatement = Assert.IsType<DoStatementSyntax>(root.Statements.Statements[1]);

            var variable = script.GetVariable(name);
            var doScope = script.GetScope(doStatement);

            Assert.NotNull(variable);
            Assert.NotNull(doScope);
            Assert.True(variable!.CanBeAccessedIn(doScope!));
        }

        [Fact]
        [Trait("Category", "Script/CanBeAccessedIn")]
        public void Script_CanBeAccessedIn_ReturnsFalseWhenScopeIsParentOfParent()
        {
            var (tree, script) = ParseScript("do\r\n" +
                                             "    local a = 1\r\n" +
                                             "end");
            var root = Assert.IsType<CompilationUnitSyntax>(tree.GetRoot());
            var doStatement = Assert.IsType<DoStatementSyntax>(root.Statements.Statements[0]);
            var assignment = Assert.IsType<LocalVariableDeclarationStatementSyntax>(doStatement.Body.Statements[0]);
            var name = Assert.IsType<IdentifierNameSyntax>(assignment.Names[0]);

            var variable = script.GetVariable(name);
            var rootScope = script.GetScope(root);

            Assert.NotNull(variable);
            Assert.NotNull(rootScope);
            Assert.False(variable!.CanBeAccessedIn(rootScope!));
        }
    }
}
