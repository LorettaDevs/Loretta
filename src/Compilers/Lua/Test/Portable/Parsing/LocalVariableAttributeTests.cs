using Loretta.CodeAnalysis.Lua.Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing
{
    public class LocalVariableAttributeTests : ParsingTestsBase
    {
        public LocalVariableAttributeTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Parser_GeneratesAnErrorDiagnosticWhen_IdentifierIsMissing()
        {
            UsingStatement(
                "local a <>",
                // (1,10): error LUA1001: Identifier expected
                // local a <>
                Diagnostic(ErrorCode.ERR_IdentifierExpected, ">").WithLocation(1, 10));

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                }
                N(SyntaxKind.VariableAttribute);
                {
                    N(SyntaxKind.LessThanToken);
                    M(SyntaxKind.IdentifierToken);
                    N(SyntaxKind.GreaterThanToken);
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_GeneratesAnErrorDiagnosticWhen_ClosingTokenIsMissing()
        {
            UsingStatement(
                "local a<const",
                // (1,14): error LUA1006: Syntax error, '>' expected
                // local a<const
                Diagnostic(ErrorCode.ERR_SyntaxError, "").WithArguments(">", "").WithLocation(1, 14));

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                }
                N(SyntaxKind.VariableAttribute);
                {
                    N(SyntaxKind.LessThanToken);
                    N(SyntaxKind.IdentifierToken, "const");
                    M(SyntaxKind.GreaterThanToken);
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesLocalDeclaration_WithSingleVariableAndNoValue()
        {
            UsingStatement("local a<const>");

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                    N(SyntaxKind.VariableAttribute);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.IdentifierToken, "const");
                        N(SyntaxKind.GreaterThanToken);
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesLocalDeclaration_WithSingleVariableAndValue()
        {
            UsingStatement("local a<const> = 1");

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                    N(SyntaxKind.VariableAttribute);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.IdentifierToken, "const");
                        N(SyntaxKind.GreaterThanToken);
                    }
                }
                N(SyntaxKind.EqualsValuesClause);
                {
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.NumericalLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "1");
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesLocalDeclaration_WithMultipleVariablesAndNoValue()
        {
            UsingStatement("local a<const>, b<const>");

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                    N(SyntaxKind.VariableAttribute);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.IdentifierToken, "const");
                        N(SyntaxKind.GreaterThanToken);
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                    }
                    N(SyntaxKind.VariableAttribute);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.IdentifierToken, "const");
                        N(SyntaxKind.GreaterThanToken);
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_ParsesLocalDeclaration_WithMultipleVariablesAndValues()
        {
            UsingStatement("local a<const>, b<const> = 1, 2");

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                    N(SyntaxKind.VariableAttribute);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.IdentifierToken, "const");
                        N(SyntaxKind.GreaterThanToken);
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                    }
                    N(SyntaxKind.VariableAttribute);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.IdentifierToken, "const");
                        N(SyntaxKind.GreaterThanToken);
                    }
                }
                N(SyntaxKind.EqualsValuesClause);
                {
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.NumericalLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "1");
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.NumericalLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "2");
                    }
                }
            }
            EOF();
        }

        [Theory]
        [RandomSpaceInserterData("local a", "<", "const", ">, b", "<", "const", "> = 1, 2")]
        public void Parser_WorksWithSpacesInsideTheAttribute(string code)
        {
            UsingStatement(code);

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                    N(SyntaxKind.VariableAttribute);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.IdentifierToken, "const");
                        N(SyntaxKind.GreaterThanToken);
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                    }
                    N(SyntaxKind.VariableAttribute);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.IdentifierToken, "const");
                        N(SyntaxKind.GreaterThanToken);
                    }
                }
                N(SyntaxKind.EqualsValuesClause);
                {
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.NumericalLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "1");
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.NumericalLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "2");
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void Parser_AllowsMixingOfAttributedAndUnattributedVariables()
        {
            UsingStatement("local a, b<const>, c, d<const>, e<const>, f, g = 1, 2, 3, 4, 5, 6");

            N(SyntaxKind.LocalVariableDeclarationStatement);
            {
                N(SyntaxKind.LocalKeyword);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "b");
                    }
                    N(SyntaxKind.VariableAttribute);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.IdentifierToken, "const");
                        N(SyntaxKind.GreaterThanToken);
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "c");
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "d");
                    }
                    N(SyntaxKind.VariableAttribute);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.IdentifierToken, "const");
                        N(SyntaxKind.GreaterThanToken);
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "e");
                    }
                    N(SyntaxKind.VariableAttribute);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.IdentifierToken, "const");
                        N(SyntaxKind.GreaterThanToken);
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "f");
                    }
                }
                N(SyntaxKind.CommaToken);
                N(SyntaxKind.LocalDeclarationName);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "g");
                    }
                }
                N(SyntaxKind.EqualsValuesClause);
                {
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.NumericalLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "1");
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.NumericalLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "2");
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.NumericalLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "3");
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.NumericalLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "4");
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.NumericalLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "5");
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.NumericalLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "6");
                    }
                }
            }
            EOF();
        }
    }
}
