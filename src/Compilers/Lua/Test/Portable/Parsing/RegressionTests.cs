using Loretta.Test.Utilities;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing;

public sealed class RegressionTests : ParsingTestsBase
{
    [Test]
    public async Task IncrementalParsing_DoesNotBreak_WithInvalidCastException()
    {
        SyntaxNode firstIdent, secondIdent;

        var initial = await UsingTreeAsync(
                          """
                          local a = b
                          local b = c
                          """);
        await N(SyntaxKind.CompilationUnit);
        {
            await N(SyntaxKind.StatementList);
            {
                await N(SyntaxKind.LocalVariableDeclarationStatement);
                {
                    await N(SyntaxKind.LocalKeyword);
                    await N(SyntaxKind.LocalDeclarationName);
                    {
                        firstIdent = (await N(SyntaxKind.IdentifierName)).AsNode()!;
                        {
                            await N(SyntaxKind.IdentifierToken, "a");
                        }
                    }
                    await N(SyntaxKind.EqualsValuesClause);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.IdentifierName);
                        {
                            await N(SyntaxKind.IdentifierToken, "b");
                        }
                    }
                }
                await N(SyntaxKind.LocalVariableDeclarationStatement);
                {
                    await N(SyntaxKind.LocalKeyword);
                    await N(SyntaxKind.LocalDeclarationName);
                    {
                        await N(SyntaxKind.IdentifierName);
                        {
                            await N(SyntaxKind.IdentifierToken, "b");
                        }
                    }
                    await N(SyntaxKind.EqualsValuesClause);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.IdentifierName);
                        {
                            await N(SyntaxKind.IdentifierToken, "c");
                        }
                    }
                }
            }
            await N(SyntaxKind.EndOfFileToken);
        }
        EOF();

        var newTree = initial.WithReplace(11, 0, " :: T");
        UsingNode((LuaSyntaxNode) await newTree.GetRootAsync());
        await N(SyntaxKind.CompilationUnit);
        {
            await N(SyntaxKind.StatementList);
            {
                await N(SyntaxKind.LocalVariableDeclarationStatement);
                {
                    await N(SyntaxKind.LocalKeyword);
                    await N(SyntaxKind.LocalDeclarationName);
                    {
                        secondIdent = (await N(SyntaxKind.IdentifierName)).AsNode()!;
                        {
                            await N(SyntaxKind.IdentifierToken, "a");
                        }
                    }
                    await N(SyntaxKind.EqualsValuesClause);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.TypeCastExpression);
                        {
                            await N(SyntaxKind.IdentifierName);
                            {
                                await N(SyntaxKind.IdentifierToken, "b");
                            }
                            await N(SyntaxKind.ColonColonToken);
                            await N(SyntaxKind.SimpleTypeName);
                            {
                                await N(SyntaxKind.IdentifierToken, "T");
                            }
                        }
                    }
                }
                await N(SyntaxKind.LocalVariableDeclarationStatement);
                {
                    await N(SyntaxKind.LocalKeyword);
                    await N(SyntaxKind.LocalDeclarationName);
                    {
                        await N(SyntaxKind.IdentifierName);
                        {
                            await N(SyntaxKind.IdentifierToken, "b");
                        }
                    }
                    await N(SyntaxKind.EqualsValuesClause);
                    {
                        await N(SyntaxKind.EqualsToken);
                        await N(SyntaxKind.IdentifierName);
                        {
                            await N(SyntaxKind.IdentifierToken, "c");
                        }
                    }
                }
            }
            await N(SyntaxKind.EndOfFileToken);
        }
        EOF();

        await Assert.That(firstIdent.IsEquivalentTo(secondIdent)).IsTrue();
    }

    [Test, WorkItem(100, "https://github.com/LorettaDevs/Loretta/issues/100")]
    public async Task LanguageParser_WhenParsingIntersectionTypes_DoNotGenerateBitwiseOperatorNotSupportedErrors()
        => await ParseAndValidateAsync("type T = A & B", LuaSyntaxOptions.Luau);

    [Test, WorkItem(100, "https://github.com/LorettaDevs/Loretta/issues/100")]
    public async Task LanguageParser_WhenParsingUnionTypes_DoNotGenerateBitwiseOperatorNotSupportedErrors()
        => await ParseAndValidateAsync("type T = A | B", LuaSyntaxOptions.Luau);

    [Test, WorkItem(100, "https://github.com/LorettaDevs/Loretta/issues/100")]
    public async Task LanguageParser_WhenParsingBitwiseAndExpressions_GeneratesBitwiseOperatorNotSupportedErrors()
        => await ParseAndValidateAsync(
               "local x = y & z",
               LuaSyntaxOptions.Luau,
               // (1,13): error LUA0021: Bitwise operators are not supported in this lua version
               // local x = y & z
               Diagnostic(ErrorCode.ERR_BitwiseOperatorsNotSupportedInVersion, "&").WithLocation(1, 13));

    [Test, WorkItem(100, "https://github.com/LorettaDevs/Loretta/issues/100")]
    public async Task LanguageParser_WhenParsingBitwiseOrExpressions_GeneratesBitwiseOperatorNotSupportedErrors()
        => await ParseAndValidateAsync(
               "local x = y | z",
               LuaSyntaxOptions.Luau,
               // (1,13): error LUA0021: Bitwise operators are not supported in this lua version
               // local x = y | z
               Diagnostic(ErrorCode.ERR_BitwiseOperatorsNotSupportedInVersion, "|").WithLocation(1, 13));

    [Test, WorkItem(126, "https://github.com/LorettaDevs/Loretta/issues/126")]
    public async Task LanguageParser_DoesNotGenerateOutOfRangeDiagnostics()
        => await ParseAndValidateAsync(
               "\n\"hello\"\n",
               LuaSyntaxOptions.Lua51,
               // (2,1): error LUA1012: Invalid statement
               // "hello"
               Diagnostic(ErrorCode.ERR_InvalidStatement, @"""hello""").WithLocation(2, 1));

    [Test, WorkItem(127, "https://github.com/LorettaDevs/Loretta/issues/127")]
    public async Task LanguageParser_ProperlyTreatsContinueAsNormalIdentifierWhenContinueTypeIsNone()
    {
        await UsingTreeAsync(
            """
            local continue = true

            if continue then
                continue = false
            end
            """,
            new LuaParseOptions(LuaSyntaxOptions.Lua51));
        await N(SyntaxKind.CompilationUnit);
        {
            await N(SyntaxKind.StatementList);
            {
                await N(SyntaxKind.LocalVariableDeclarationStatement);
                {
                    await N(SyntaxKind.LocalKeyword);
                    await N(SyntaxKind.LocalDeclarationName);
                    {
                        await N(SyntaxKind.IdentifierName);
                        {
                            await N(SyntaxKind.IdentifierToken, "continue");
                        }
                        await N(SyntaxKind.EqualsValuesClause);
                        {
                            await N(SyntaxKind.EqualsToken);
                            await N(SyntaxKind.TrueLiteralExpression);
                            {
                                await N(SyntaxKind.TrueKeyword);
                            }
                        }
                    }
                }

                await N(SyntaxKind.IfStatement);
                {
                    await N(SyntaxKind.IfKeyword);
                    await N(SyntaxKind.IdentifierName);
                    {
                        await N(SyntaxKind.IdentifierToken, "continue");
                    }
                    await N(SyntaxKind.ThenKeyword);

                    await N(SyntaxKind.StatementList);
                    {
                        await N(SyntaxKind.AssignmentStatement);
                        {
                            await N(SyntaxKind.IdentifierName);
                            {
                                await N(SyntaxKind.IdentifierToken, "continue");
                            }

                            await N(SyntaxKind.EqualsValuesClause);
                            {
                                await N(SyntaxKind.EqualsToken);
                                await N(SyntaxKind.FalseLiteralExpression);
                                {
                                    await N(SyntaxKind.FalseKeyword);
                                }
                            }
                        }
                    }

                    await N(SyntaxKind.EndKeyword);
                }
            }
            await N(SyntaxKind.EndOfFileToken);
        }
        EOF();
    }

    [Test, WorkItem(127, "https://github.com/LorettaDevs/Loretta/issues/127")]
    public async Task LanguageParser_DoesNotFindGotosNorGotoLabelsWhenAcceptGotoIsNotTrue()
        => await ParseAndValidateAsync(
               "::label:: goto label",
               LuaSyntaxOptions.Lua51,
               // (1,1): error LUA1012: Invalid statement
               // ::label:: goto label
               Diagnostic(ErrorCode.ERR_InvalidStatement, ":").WithLocation(1, 1),
               // (1,2): error LUA1012: Invalid statement
               // ::label:: goto label
               Diagnostic(ErrorCode.ERR_InvalidStatement, ":").WithLocation(1, 2),
               // (1,9): error LUA1001: Identifier expected
               // ::label:: goto label
               Diagnostic(ErrorCode.ERR_IdentifierExpected, ":").WithLocation(1, 9),
               // (1,9): error LUA1006: Syntax error, '(' expected
               // ::label:: goto label
               Diagnostic(ErrorCode.ERR_SyntaxError, ":").WithArguments("(", ":").WithLocation(1, 9),
               // (1,9): error LUA1011: Invalid expression part ':'
               // ::label:: goto label
               Diagnostic(ErrorCode.ERR_InvalidExpressionPart, ":").WithArguments(":").WithLocation(1, 9),
               // (1,9): error LUA1003: ) expected
               // ::label:: goto label
               Diagnostic(ErrorCode.ERR_CloseParenExpected, ":").WithLocation(1, 9),
               // (1,16): error LUA1006: Syntax error, '(' expected
               // ::label:: goto label
               Diagnostic(ErrorCode.ERR_SyntaxError, "label").WithArguments("(", "").WithLocation(1, 16),
               // (1,21): error LUA1003: ) expected
               // ::label:: goto label
               Diagnostic(ErrorCode.ERR_CloseParenExpected, "").WithLocation(1, 21));
    
    [Test, WorkItem(147, "https://github.com/LorettaDevs/Loretta/issues/147")]
    public async Task LanguageParser_WhenParsingEmptyReturnAtEndOfFile_DoNotGenerateErrors()
        => await ParseAndValidateAsync("return", LuaSyntaxOptions.Lua51);

    [Test, WorkItem(160, "https://github.com/LorettaDevs/Loretta/issues/160")]
    public async Task LanguageParser_ConcatIsRightAssociative()
    {
        await UsingExpressionAsync("a .. b .. c", new LuaParseOptions(LuaSyntaxOptions.All));
        await N(SyntaxKind.ConcatExpression);
        {
            await N(SyntaxKind.IdentifierName);
            {
                await N(SyntaxKind.IdentifierToken, "a");
            }
            await N(SyntaxKind.DotDotToken, "..");
            await N(SyntaxKind.ConcatExpression);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "b");
                }
                await N(SyntaxKind.DotDotToken, "..");
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "c");
                }
            }
        }
        EOF();
    }
}
