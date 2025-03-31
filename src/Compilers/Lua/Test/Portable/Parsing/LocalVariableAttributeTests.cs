using Loretta.CodeAnalysis.Lua.Test.Utilities;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing;

public sealed class LocalVariableAttributeTests : ParsingTestsBase
{
    [Test]
    public async Task Parser_GeneratesAnErrorDiagnosticWhen_IdentifierIsMissing()
    {
        await UsingStatementAsync(
            "local a <>",
            // (1,10): error LUA1001: Identifier expected
            // local a <>
            Diagnostic(ErrorCode.ERR_IdentifierExpected, ">").WithLocation(1, 10));

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
            }
            await N(SyntaxKind.VariableAttribute);
            {
                await N(SyntaxKind.LessThanToken);
                await M(SyntaxKind.IdentifierToken);
                await N(SyntaxKind.GreaterThanToken);
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_GeneratesAnErrorDiagnosticWhen_ClosingTokenIsMissing()
    {
        await UsingStatementAsync(
            "local a<const",
            // (1,14): error LUA1006: Syntax error, '>' expected
            // local a<const
            Diagnostic(ErrorCode.ERR_SyntaxError, "").WithArguments(">", "").WithLocation(1, 14));

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
            }
            await N(SyntaxKind.VariableAttribute);
            {
                await N(SyntaxKind.LessThanToken);
                await N(SyntaxKind.IdentifierToken, "const");
                await M(SyntaxKind.GreaterThanToken);
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesLocalDeclaration_WithSingleVariableAndNoValue()
    {
        await UsingStatementAsync("local a<const>");

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
                await N(SyntaxKind.VariableAttribute);
                {
                    await N(SyntaxKind.LessThanToken);
                    await N(SyntaxKind.IdentifierToken, "const");
                    await N(SyntaxKind.GreaterThanToken);
                }
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesLocalDeclaration_WithSingleVariableAndValue()
    {
        await UsingStatementAsync("local a<const> = 1");

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
                await N(SyntaxKind.VariableAttribute);
                {
                    await N(SyntaxKind.LessThanToken);
                    await N(SyntaxKind.IdentifierToken, "const");
                    await N(SyntaxKind.GreaterThanToken);
                }
            }
            await N(SyntaxKind.EqualsValuesClause);
            {
                await N(SyntaxKind.EqualsToken);
                await N(SyntaxKind.NumericalLiteralExpression);
                {
                    await N(SyntaxKind.NumericLiteralToken, "1");
                }
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesLocalDeclaration_WithMultipleVariablesAndNoValue()
    {
        await UsingStatementAsync("local a<const>, b<const>");

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
                await N(SyntaxKind.VariableAttribute);
                {
                    await N(SyntaxKind.LessThanToken);
                    await N(SyntaxKind.IdentifierToken, "const");
                    await N(SyntaxKind.GreaterThanToken);
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "b");
                }
                await N(SyntaxKind.VariableAttribute);
                {
                    await N(SyntaxKind.LessThanToken);
                    await N(SyntaxKind.IdentifierToken, "const");
                    await N(SyntaxKind.GreaterThanToken);
                }
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_ParsesLocalDeclaration_WithMultipleVariablesAndValues()
    {
        await UsingStatementAsync("local a<const>, b<const> = 1, 2");

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
                await N(SyntaxKind.VariableAttribute);
                {
                    await N(SyntaxKind.LessThanToken);
                    await N(SyntaxKind.IdentifierToken, "const");
                    await N(SyntaxKind.GreaterThanToken);
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "b");
                }
                await N(SyntaxKind.VariableAttribute);
                {
                    await N(SyntaxKind.LessThanToken);
                    await N(SyntaxKind.IdentifierToken, "const");
                    await N(SyntaxKind.GreaterThanToken);
                }
            }
            await N(SyntaxKind.EqualsValuesClause);
            {
                await N(SyntaxKind.EqualsToken);
                await N(SyntaxKind.NumericalLiteralExpression);
                {
                    await N(SyntaxKind.NumericLiteralToken, "1");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.NumericalLiteralExpression);
                {
                    await N(SyntaxKind.NumericLiteralToken, "2");
                }
            }
        }
        EOF();
    }

    [Test]
    [MethodDataSource(
        typeof(RandomSpaceInserter),
        nameof(RandomSpaceInserter.GetTokenPairs),
        Arguments = [new[] { "local a", "<", "const", ">, b", "<", "const", "> = 1, 2" }])]
    public async Task Parser_WorksWithSpacesInsideTheAttribute(string code)
    {
        await UsingStatementAsync(code);

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
                await N(SyntaxKind.VariableAttribute);
                {
                    await N(SyntaxKind.LessThanToken);
                    await N(SyntaxKind.IdentifierToken, "const");
                    await N(SyntaxKind.GreaterThanToken);
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "b");
                }
                await N(SyntaxKind.VariableAttribute);
                {
                    await N(SyntaxKind.LessThanToken);
                    await N(SyntaxKind.IdentifierToken, "const");
                    await N(SyntaxKind.GreaterThanToken);
                }
            }
            await N(SyntaxKind.EqualsValuesClause);
            {
                await N(SyntaxKind.EqualsToken);
                await N(SyntaxKind.NumericalLiteralExpression);
                {
                    await N(SyntaxKind.NumericLiteralToken, "1");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.NumericalLiteralExpression);
                {
                    await N(SyntaxKind.NumericLiteralToken, "2");
                }
            }
        }
        EOF();
    }

    [Test]
    public async Task Parser_AllowsMixingOfAttributedAndUnattributedVariables()
    {
        await UsingStatementAsync("local a, b<const>, c, d<const>, e<const>, f, g = 1, 2, 3, 4, 5, 6");

        await N(SyntaxKind.LocalVariableDeclarationStatement);
        {
            await N(SyntaxKind.LocalKeyword);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "b");
                }
                await N(SyntaxKind.VariableAttribute);
                {
                    await N(SyntaxKind.LessThanToken);
                    await N(SyntaxKind.IdentifierToken, "const");
                    await N(SyntaxKind.GreaterThanToken);
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "c");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "d");
                }
                await N(SyntaxKind.VariableAttribute);
                {
                    await N(SyntaxKind.LessThanToken);
                    await N(SyntaxKind.IdentifierToken, "const");
                    await N(SyntaxKind.GreaterThanToken);
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "e");
                }
                await N(SyntaxKind.VariableAttribute);
                {
                    await N(SyntaxKind.LessThanToken);
                    await N(SyntaxKind.IdentifierToken, "const");
                    await N(SyntaxKind.GreaterThanToken);
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "f");
                }
            }
            await N(SyntaxKind.CommaToken);
            await N(SyntaxKind.LocalDeclarationName);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "g");
                }
            }
            await N(SyntaxKind.EqualsValuesClause);
            {
                await N(SyntaxKind.EqualsToken);
                await N(SyntaxKind.NumericalLiteralExpression);
                {
                    await N(SyntaxKind.NumericLiteralToken, "1");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.NumericalLiteralExpression);
                {
                    await N(SyntaxKind.NumericLiteralToken, "2");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.NumericalLiteralExpression);
                {
                    await N(SyntaxKind.NumericLiteralToken, "3");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.NumericalLiteralExpression);
                {
                    await N(SyntaxKind.NumericLiteralToken, "4");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.NumericalLiteralExpression);
                {
                    await N(SyntaxKind.NumericLiteralToken, "5");
                }
                await N(SyntaxKind.CommaToken);
                await N(SyntaxKind.NumericalLiteralExpression);
                {
                    await N(SyntaxKind.NumericLiteralToken, "6");
                }
            }
        }
        EOF();
    }
}
