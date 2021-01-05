using System.Linq;
using GParse.Collections;
using Loretta.Parsing;
using Loretta.Parsing.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Loretta.Tests.Parsing.Issues
{
    [TestClass]
    public class ParserVariableTests : ParserTestClass
    {
        [TestMethod]
        // Regression test for https://github.com/GGG-KILLER/Loretta/issues/17
        public void Issue_17 ( )
        {
            foreach ( LuaOptions preset in new[] { LuaOptions.Lua51, LuaOptions.Lua52, LuaOptions.LuaJIT, LuaOptions.GMod, LuaOptions.Roblox, LuaOptions.All } )
            {
                (StatementList statements, DiagnosticList diagnostics) = Parse ( preset, @"local a = 1
for a = 1, 4 do end
a = 2" );
                Assert.AreEqual ( 0, diagnostics.Count );
                var localDeclaration = ( LocalVariableDeclarationStatement ) statements.Body[0];
                var forLoop = ( NumericForLoopStatement ) statements.Body[1];
                var assignment = ( AssignmentStatement ) statements.Body[2];

                Variable localVariable = localDeclaration.Identifiers.Single ( ).Variable;
                Variable assignmentVariable = ( ( IdentifierExpression ) assignment.Variables.Single ( ) ).Variable;
                Variable forLoopVariable = forLoop.Variable.Variable;

                Assert.AreEqual ( localVariable, assignmentVariable );
                Assert.AreNotEqual ( forLoopVariable, localVariable );
                Assert.AreNotEqual ( forLoopVariable, assignmentVariable );
            }
        }

        [TestMethod]
        // Regression test for https://github.com/GGG-KILLER/Loretta/issues/18
        public void Issue_18 ( )
        {
            foreach ( LuaOptions preset in new[] { LuaOptions.Lua51, LuaOptions.Lua52, LuaOptions.LuaJIT, LuaOptions.GMod, LuaOptions.Roblox, LuaOptions.All } )
            {
                (StatementList statements, DiagnosticList diagnostics) = Parse ( preset, @"local a = b[a]" );
                Assert.AreEqual ( 0, diagnostics.Count );

                var localDeclaration = ( LocalVariableDeclarationStatement ) statements.Body[0];

                Variable localDeclarationVariable = localDeclaration.Identifiers.Single ( ).Variable;
                var indexExpression = ( IndexExpression ) localDeclaration.Values.Single ( );
                Variable indexerVariable = ( ( IdentifierExpression ) indexExpression.Indexer ).Variable;

                Assert.AreNotEqual ( localDeclarationVariable, indexerVariable );
            }
        }

        [TestMethod]
        // Regression test for https://github.com/GGG-KILLER/Loretta/issues/19
        public void Issue_19 ( )
        {
            foreach ( LuaOptions preset in new[] { LuaOptions.Lua51, LuaOptions.Lua52, LuaOptions.LuaJIT, LuaOptions.GMod, LuaOptions.Roblox, LuaOptions.All } )
            {
                (StatementList statements, DiagnosticList diagnostics) = Parse ( preset, @"local a = 1
local a = 2" );
                Assert.AreEqual ( 0, diagnostics.Count );
                var localDeclaration1 = ( LocalVariableDeclarationStatement ) statements.Body[0];
                var localDeclaration2 = ( LocalVariableDeclarationStatement ) statements.Body[1];

                Variable localDeclaration1Variable = localDeclaration1.Identifiers.Single ( ).Variable;
                Variable localDeclaration2Variable = localDeclaration2.Identifiers.Single ( ).Variable;

                Assert.AreNotEqual ( localDeclaration1Variable, localDeclaration2Variable );
            }
        }
    }
}
