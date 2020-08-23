using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Loretta.Tests.Parsing.Visitor
{
    [TestClass]
    public class ConstantFolderTests
    {
        private static readonly ConstantFolder constantFolder = new ConstantFolder ( );

        // Regression test for https://github.com/GGG-KILLER/Loretta/issues/3
        [TestMethod]
        public void ConstantFolder_FoldsNotExpressionsCorrectly ( )
        {
            IdentifierExpression identifier = ASTNodeFactory.Identifier ( "var" );
            Token<LuaTokenType> not = TokenFactory.Token ( "not", LuaTokenType.Keyword, "not", "not" );
            UnaryOperationExpression unary = ASTNodeFactory.UnaryOperation ( not, identifier );

            LuaASTNode folded = constantFolder.VisitUnaryOperation ( unary );

            Assert.AreSame ( unary, folded, "Constant folder has folded a not expression with an identifier." );
        }
    }
}