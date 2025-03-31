using Loretta.CodeAnalysis.Test.Utilities;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing;

public sealed class InterpolatedStringTests : ParsingTestsBase
{
    [Test]
    public async Task LanguageParser_ProperlyReadsStringsInsideInterpolatedStrings()
    {
        await UsingExpressionAsync(
            """`some\tthing {"a very\nlong string"} some\nthing`""",
            new LuaParseOptions(LuaSyntaxOptions.Luau));

        await N(SyntaxKind.InterpolatedStringExpression);
        {
            await N(SyntaxKind.BacktickToken);
            await N(SyntaxKind.InterpolatedStringText);
            {
                var token = (await N(SyntaxKind.InterpolatedStringTextToken, "some\\tthing ")).AsToken();
                await Assert.That(token).HasValue("some\tthing ");
            }
            await N(SyntaxKind.Interpolation);
            {
                await N(SyntaxKind.OpenBraceToken);
                await N(SyntaxKind.StringLiteralExpression);
                {
                    var token = (await N(SyntaxKind.StringLiteralToken, "\"a very\\nlong string\"")).AsToken();
                    await Assert.That(token).HasValue("a very\nlong string");
                }
                await N(SyntaxKind.CloseBraceToken);
            }
            await N(SyntaxKind.InterpolatedStringText);
            {
                var token = (await N(SyntaxKind.InterpolatedStringTextToken, " some\\nthing")).AsToken();
                await Assert.That(token).HasValue(" some\nthing");
            }
            await N(SyntaxKind.BacktickToken);
        }
        EOF();
    }

    [Test]
    public async Task LanguageParser_ProperlyReadsDeeplyNestedInterpolatedStrings()
    {
        await UsingExpressionAsync(
            """`a {`very {`{`very {`{`very` .. ` ` .. `very`} very{(" very"):rep(100)}`}`} very`} nested`} string`""",
            new LuaParseOptions(LuaSyntaxOptions.Luau));

        await N(SyntaxKind.InterpolatedStringExpression);
        {
            await N(SyntaxKind.BacktickToken);
            await N(SyntaxKind.InterpolatedStringText);
            {
                await N(SyntaxKind.InterpolatedStringTextToken, "a ");
            }
            await N(SyntaxKind.Interpolation);
            {
                await N(SyntaxKind.OpenBraceToken);
                await N(SyntaxKind.InterpolatedStringExpression);
                {
                    await N(SyntaxKind.BacktickToken);
                    await N(SyntaxKind.InterpolatedStringText);
                    {
                        await N(SyntaxKind.InterpolatedStringTextToken, "very ");
                    }
                    await N(SyntaxKind.Interpolation);
                    {
                        await N(SyntaxKind.OpenBraceToken);
                        await N(SyntaxKind.InterpolatedStringExpression);
                        {
                            await N(SyntaxKind.BacktickToken);
                            await N(SyntaxKind.Interpolation);
                            {
                                await N(SyntaxKind.OpenBraceToken);
                                await N(SyntaxKind.InterpolatedStringExpression);
                                {
                                    await N(SyntaxKind.BacktickToken);
                                    await N(SyntaxKind.InterpolatedStringText);
                                    {
                                        await N(SyntaxKind.InterpolatedStringTextToken, "very ");
                                    }
                                    await N(SyntaxKind.Interpolation);
                                    {
                                        await N(SyntaxKind.OpenBraceToken);
                                        await N(SyntaxKind.InterpolatedStringExpression);
                                        {
                                            await N(SyntaxKind.BacktickToken);
                                            await N(SyntaxKind.Interpolation);
                                            {
                                                await N(SyntaxKind.OpenBraceToken);
                                                await N(SyntaxKind.ConcatExpression);
                                                {
                                                    await N(SyntaxKind.ConcatExpression);
                                                    {
                                                        await N(SyntaxKind.InterpolatedStringExpression);
                                                        {
                                                            await N(SyntaxKind.BacktickToken);
                                                            await N(SyntaxKind.InterpolatedStringText);
                                                            {
                                                                await N(SyntaxKind.InterpolatedStringTextToken, "very");
                                                            }
                                                            await N(SyntaxKind.BacktickToken);
                                                        }
                                                        await N(SyntaxKind.DotDotToken);
                                                        await N(SyntaxKind.InterpolatedStringExpression);
                                                        {
                                                            await N(SyntaxKind.BacktickToken);
                                                            await N(SyntaxKind.InterpolatedStringText);
                                                            {
                                                                await N(SyntaxKind.InterpolatedStringTextToken, " ");
                                                            }
                                                            await N(SyntaxKind.BacktickToken);
                                                        }
                                                    }
                                                    await N(SyntaxKind.DotDotToken);
                                                    await N(SyntaxKind.InterpolatedStringExpression);
                                                    {
                                                        await N(SyntaxKind.BacktickToken);
                                                        await N(SyntaxKind.InterpolatedStringText);
                                                        {
                                                            await N(SyntaxKind.InterpolatedStringTextToken, "very");
                                                        }
                                                        await N(SyntaxKind.BacktickToken);
                                                    }
                                                }
                                                await N(SyntaxKind.CloseBraceToken);
                                            }
                                            await N(SyntaxKind.InterpolatedStringText);
                                            {
                                                await N(SyntaxKind.InterpolatedStringTextToken, " very");
                                            }
                                            await N(SyntaxKind.Interpolation);
                                            {
                                                await N(SyntaxKind.OpenBraceToken);
                                                await N(SyntaxKind.MethodCallExpression);
                                                {
                                                    await N(SyntaxKind.ParenthesizedExpression);
                                                    {
                                                        await N(SyntaxKind.OpenParenthesisToken);
                                                        await N(SyntaxKind.StringLiteralExpression);
                                                        {
                                                            await N(SyntaxKind.StringLiteralToken, "\" very\"");
                                                        }
                                                        await N(SyntaxKind.CloseParenthesisToken);
                                                    }
                                                    await N(SyntaxKind.ColonToken);
                                                    await N(SyntaxKind.IdentifierToken, "rep");
                                                    await N(SyntaxKind.ExpressionListFunctionArgument);
                                                    {
                                                        await N(SyntaxKind.OpenParenthesisToken);
                                                        await N(SyntaxKind.NumericalLiteralExpression);
                                                        {
                                                            await N(SyntaxKind.NumericLiteralToken, "100");
                                                        }
                                                        await N(SyntaxKind.CloseParenthesisToken);
                                                    }
                                                }
                                                await N(SyntaxKind.CloseBraceToken);
                                            }
                                            await N(SyntaxKind.BacktickToken);
                                        }
                                        await N(SyntaxKind.CloseBraceToken);
                                    }
                                    await N(SyntaxKind.BacktickToken);
                                }
                                await N(SyntaxKind.CloseBraceToken);
                            }
                            await N(SyntaxKind.InterpolatedStringText);
                            {
                                await N(SyntaxKind.InterpolatedStringTextToken, " very");
                            }
                            await N(SyntaxKind.BacktickToken);
                        }
                        await N(SyntaxKind.CloseBraceToken);
                    }
                    await N(SyntaxKind.InterpolatedStringText);
                    {
                        await N(SyntaxKind.InterpolatedStringTextToken, " nested");
                    }
                    await N(SyntaxKind.BacktickToken);
                }
                await N(SyntaxKind.CloseBraceToken);
            }
            await N(SyntaxKind.InterpolatedStringText);
            {
                await N(SyntaxKind.InterpolatedStringTextToken, " string");
            }
            await N(SyntaxKind.BacktickToken);
        }
        EOF();
    }

    [Test]
    public async Task LanguageParser_ProperlyReadsInterpolatedStringsWithComplexExpressions()
    {
        await UsingExpressionAsync(
            """
            print(`some {function()
              print(`other {function()
                print(`some {if true then function()
                  print(`fucked up {1 + 2 ^ 6} shit`)
                end else function (...)
                  print(`fucked up {...} shit`)
                end} shit`)
              end} shit`)
            end} fucked up shit`)
            """,
            new LuaParseOptions(LuaSyntaxOptions.Luau));

        await N(SyntaxKind.FunctionCallExpression);
        {
            await N(SyntaxKind.IdentifierName);
            {
                await N(SyntaxKind.IdentifierToken, "print");
            }
            await N(SyntaxKind.ExpressionListFunctionArgument);
            {
                await N(SyntaxKind.OpenParenthesisToken);
                await N(SyntaxKind.InterpolatedStringExpression);
                {
                    await N(SyntaxKind.BacktickToken);
                    await N(SyntaxKind.InterpolatedStringText);
                    {
                        await N(SyntaxKind.InterpolatedStringTextToken, "some ");
                    }
                    await N(SyntaxKind.Interpolation);
                    {
                        await N(SyntaxKind.OpenBraceToken);
                        await N(SyntaxKind.AnonymousFunctionExpression);
                        {
                            await N(SyntaxKind.FunctionKeyword);
                            await N(SyntaxKind.ParameterList);
                            {
                                await N(SyntaxKind.OpenParenthesisToken);
                                await N(SyntaxKind.CloseParenthesisToken);
                            }
                            await N(SyntaxKind.StatementList);
                            {
                                await N(SyntaxKind.ExpressionStatement);
                                {
                                    await N(SyntaxKind.FunctionCallExpression);
                                    {
                                        await N(SyntaxKind.IdentifierName);
                                        {
                                            await N(SyntaxKind.IdentifierToken, "print");
                                        }
                                        await N(SyntaxKind.ExpressionListFunctionArgument);
                                        {
                                            await N(SyntaxKind.OpenParenthesisToken);
                                            await N(SyntaxKind.InterpolatedStringExpression);
                                            {
                                                await N(SyntaxKind.BacktickToken);
                                                await N(SyntaxKind.InterpolatedStringText);
                                                {
                                                    await N(SyntaxKind.InterpolatedStringTextToken, "other ");
                                                }
                                                await N(SyntaxKind.Interpolation);
                                                {
                                                    await N(SyntaxKind.OpenBraceToken);
                                                    await N(SyntaxKind.AnonymousFunctionExpression);
                                                    {
                                                        await N(SyntaxKind.FunctionKeyword);
                                                        await N(SyntaxKind.ParameterList);
                                                        {
                                                            await N(SyntaxKind.OpenParenthesisToken);
                                                            await N(SyntaxKind.CloseParenthesisToken);
                                                        }
                                                        await N(SyntaxKind.StatementList);
                                                        {
                                                            await N(SyntaxKind.ExpressionStatement);
                                                            {
                                                                await N(SyntaxKind.FunctionCallExpression);
                                                                {
                                                                    await N(SyntaxKind.IdentifierName);
                                                                    {
                                                                        await N(SyntaxKind.IdentifierToken, "print");
                                                                    }
                                                                    await N(SyntaxKind.ExpressionListFunctionArgument);
                                                                    {
                                                                        await N(SyntaxKind.OpenParenthesisToken);
                                                                        await N(
                                                                            SyntaxKind.InterpolatedStringExpression);
                                                                        {
                                                                            await N(SyntaxKind.BacktickToken);
                                                                            await N(SyntaxKind.InterpolatedStringText);
                                                                            {
                                                                                await N(
                                                                                    SyntaxKind
                                                                                        .InterpolatedStringTextToken,
                                                                                    "some ");
                                                                            }
                                                                            await N(SyntaxKind.Interpolation);
                                                                            {
                                                                                await N(SyntaxKind.OpenBraceToken);
                                                                                await N(SyntaxKind.IfExpression);
                                                                                {
                                                                                    await N(SyntaxKind.IfKeyword);
                                                                                    await N(
                                                                                        SyntaxKind
                                                                                            .TrueLiteralExpression);
                                                                                    {
                                                                                        await N(SyntaxKind.TrueKeyword);
                                                                                    }
                                                                                    await N(SyntaxKind.ThenKeyword);
                                                                                    await N(
                                                                                        SyntaxKind
                                                                                            .AnonymousFunctionExpression);
                                                                                    {
                                                                                        await N(
                                                                                            SyntaxKind.FunctionKeyword);
                                                                                        await N(
                                                                                            SyntaxKind.ParameterList);
                                                                                        {
                                                                                            await N(
                                                                                                SyntaxKind
                                                                                                    .OpenParenthesisToken);
                                                                                            await N(
                                                                                                SyntaxKind
                                                                                                    .CloseParenthesisToken);
                                                                                        }
                                                                                        await N(
                                                                                            SyntaxKind.StatementList);
                                                                                        {
                                                                                            await N(
                                                                                                SyntaxKind
                                                                                                    .ExpressionStatement);
                                                                                            {
                                                                                                await N(
                                                                                                    SyntaxKind
                                                                                                        .FunctionCallExpression);
                                                                                                {
                                                                                                    await N(
                                                                                                        SyntaxKind
                                                                                                            .IdentifierName);
                                                                                                    {
                                                                                                        await N(
                                                                                                            SyntaxKind
                                                                                                                .IdentifierToken,
                                                                                                            "print");
                                                                                                    }
                                                                                                    await N(
                                                                                                        SyntaxKind
                                                                                                            .ExpressionListFunctionArgument);
                                                                                                    {
                                                                                                        await N(
                                                                                                            SyntaxKind
                                                                                                                .OpenParenthesisToken);
                                                                                                        await N(
                                                                                                            SyntaxKind
                                                                                                                .InterpolatedStringExpression);
                                                                                                        {
                                                                                                            await N(
                                                                                                                SyntaxKind
                                                                                                                    .BacktickToken);
                                                                                                            await N(
                                                                                                                SyntaxKind
                                                                                                                    .InterpolatedStringText);
                                                                                                            {
                                                                                                                await N(
                                                                                                                    SyntaxKind
                                                                                                                        .InterpolatedStringTextToken,
                                                                                                                    "fucked up ");
                                                                                                            }
                                                                                                            await N(
                                                                                                                SyntaxKind
                                                                                                                    .Interpolation);
                                                                                                            {
                                                                                                                await N(
                                                                                                                    SyntaxKind
                                                                                                                        .OpenBraceToken);
                                                                                                                await N(
                                                                                                                    SyntaxKind
                                                                                                                        .AddExpression);
                                                                                                                {
                                                                                                                    await
                                                                                                                        N(
                                                                                                                            SyntaxKind
                                                                                                                                .NumericalLiteralExpression);
                                                                                                                    {
                                                                                                                        await
                                                                                                                            N(
                                                                                                                                SyntaxKind
                                                                                                                                    .NumericLiteralToken,
                                                                                                                                "1");
                                                                                                                    }
                                                                                                                    await
                                                                                                                        N(
                                                                                                                            SyntaxKind
                                                                                                                                .PlusToken);
                                                                                                                    await
                                                                                                                        N(
                                                                                                                            SyntaxKind
                                                                                                                                .ExponentiateExpression);
                                                                                                                    {
                                                                                                                        await
                                                                                                                            N(
                                                                                                                                SyntaxKind
                                                                                                                                    .NumericalLiteralExpression);
                                                                                                                        {
                                                                                                                            await
                                                                                                                                N(
                                                                                                                                    SyntaxKind
                                                                                                                                        .NumericLiteralToken,
                                                                                                                                    "2");
                                                                                                                        }
                                                                                                                        await
                                                                                                                            N(
                                                                                                                                SyntaxKind
                                                                                                                                    .HatToken);
                                                                                                                        await
                                                                                                                            N(
                                                                                                                                SyntaxKind
                                                                                                                                    .NumericalLiteralExpression);
                                                                                                                        {
                                                                                                                            await
                                                                                                                                N(
                                                                                                                                    SyntaxKind
                                                                                                                                        .NumericLiteralToken,
                                                                                                                                    "6");
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                                await N(
                                                                                                                    SyntaxKind
                                                                                                                        .CloseBraceToken);
                                                                                                            }
                                                                                                            await N(
                                                                                                                SyntaxKind
                                                                                                                    .InterpolatedStringText);
                                                                                                            {
                                                                                                                await N(
                                                                                                                    SyntaxKind
                                                                                                                        .InterpolatedStringTextToken,
                                                                                                                    " shit");
                                                                                                            }
                                                                                                            await N(
                                                                                                                SyntaxKind
                                                                                                                    .BacktickToken);
                                                                                                        }
                                                                                                        await N(
                                                                                                            SyntaxKind
                                                                                                                .CloseParenthesisToken);
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                        await N(SyntaxKind.EndKeyword);
                                                                                    }
                                                                                    await N(SyntaxKind.ElseKeyword);
                                                                                    await N(
                                                                                        SyntaxKind
                                                                                            .AnonymousFunctionExpression);
                                                                                    {
                                                                                        await N(
                                                                                            SyntaxKind.FunctionKeyword);
                                                                                        await N(
                                                                                            SyntaxKind.ParameterList);
                                                                                        {
                                                                                            await N(
                                                                                                SyntaxKind
                                                                                                    .OpenParenthesisToken);
                                                                                            await N(
                                                                                                SyntaxKind
                                                                                                    .VarArgParameter);
                                                                                            {
                                                                                                await N(
                                                                                                    SyntaxKind
                                                                                                        .DotDotDotToken);
                                                                                            }
                                                                                            await N(
                                                                                                SyntaxKind
                                                                                                    .CloseParenthesisToken);
                                                                                        }
                                                                                        await N(
                                                                                            SyntaxKind.StatementList);
                                                                                        {
                                                                                            await N(
                                                                                                SyntaxKind
                                                                                                    .ExpressionStatement);
                                                                                            {
                                                                                                await N(
                                                                                                    SyntaxKind
                                                                                                        .FunctionCallExpression);
                                                                                                {
                                                                                                    await N(
                                                                                                        SyntaxKind
                                                                                                            .IdentifierName);
                                                                                                    {
                                                                                                        await N(
                                                                                                            SyntaxKind
                                                                                                                .IdentifierToken,
                                                                                                            "print");
                                                                                                    }
                                                                                                    await N(
                                                                                                        SyntaxKind
                                                                                                            .ExpressionListFunctionArgument);
                                                                                                    {
                                                                                                        await N(
                                                                                                            SyntaxKind
                                                                                                                .OpenParenthesisToken);
                                                                                                        await N(
                                                                                                            SyntaxKind
                                                                                                                .InterpolatedStringExpression);
                                                                                                        {
                                                                                                            await N(
                                                                                                                SyntaxKind
                                                                                                                    .BacktickToken);
                                                                                                            await N(
                                                                                                                SyntaxKind
                                                                                                                    .InterpolatedStringText);
                                                                                                            {
                                                                                                                await N(
                                                                                                                    SyntaxKind
                                                                                                                        .InterpolatedStringTextToken,
                                                                                                                    "fucked up ");
                                                                                                            }
                                                                                                            await N(
                                                                                                                SyntaxKind
                                                                                                                    .Interpolation);
                                                                                                            {
                                                                                                                await N(
                                                                                                                    SyntaxKind
                                                                                                                        .OpenBraceToken);
                                                                                                                await N(
                                                                                                                    SyntaxKind
                                                                                                                        .VarArgExpression);
                                                                                                                {
                                                                                                                    await
                                                                                                                        N(
                                                                                                                            SyntaxKind
                                                                                                                                .DotDotDotToken);
                                                                                                                }
                                                                                                                await N(
                                                                                                                    SyntaxKind
                                                                                                                        .CloseBraceToken);
                                                                                                            }
                                                                                                            await N(
                                                                                                                SyntaxKind
                                                                                                                    .InterpolatedStringText);
                                                                                                            {
                                                                                                                await N(
                                                                                                                    SyntaxKind
                                                                                                                        .InterpolatedStringTextToken,
                                                                                                                    " shit");
                                                                                                            }
                                                                                                            await N(
                                                                                                                SyntaxKind
                                                                                                                    .BacktickToken);
                                                                                                        }
                                                                                                        await N(
                                                                                                            SyntaxKind
                                                                                                                .CloseParenthesisToken);
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                        await N(SyntaxKind.EndKeyword);
                                                                                    }
                                                                                }
                                                                                await N(SyntaxKind.CloseBraceToken);
                                                                            }
                                                                            await N(SyntaxKind.InterpolatedStringText);
                                                                            {
                                                                                await N(
                                                                                    SyntaxKind
                                                                                        .InterpolatedStringTextToken,
                                                                                    " shit");
                                                                            }
                                                                            await N(SyntaxKind.BacktickToken);
                                                                        }
                                                                        await N(SyntaxKind.CloseParenthesisToken);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        await N(SyntaxKind.EndKeyword);
                                                    }
                                                    await N(SyntaxKind.CloseBraceToken);
                                                }
                                                await N(SyntaxKind.InterpolatedStringText);
                                                {
                                                    await N(SyntaxKind.InterpolatedStringTextToken, " shit");
                                                }
                                                await N(SyntaxKind.BacktickToken);
                                            }
                                            await N(SyntaxKind.CloseParenthesisToken);
                                        }
                                    }
                                }
                            }
                            await N(SyntaxKind.EndKeyword);
                        }
                        await N(SyntaxKind.CloseBraceToken);
                    }
                    await N(SyntaxKind.InterpolatedStringText);
                    {
                        await N(SyntaxKind.InterpolatedStringTextToken, " fucked up shit");
                    }
                    await N(SyntaxKind.BacktickToken);
                }
                await N(SyntaxKind.CloseParenthesisToken);
            }
        }
        EOF();
    }
}
