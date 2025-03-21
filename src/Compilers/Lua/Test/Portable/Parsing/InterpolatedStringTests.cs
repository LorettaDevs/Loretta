using Xunit;
using Xunit.Abstractions;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing
{
    public class InterpolatedStringTests(ITestOutputHelper output) : ParsingTestsBase(output)
    {
        [Fact]
        public void LanguageParser_ProperlyReadsStringsInsideInterpolatedStrings()
        {
            UsingExpression(
                """`some\tthing {"a very\nlong string"} some\nthing`""",
                new LuaParseOptions(LuaSyntaxOptions.Luau));

            N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.BacktickToken);
                N(SyntaxKind.InterpolatedStringText);
                {
                    Assert.Equal(
                        "some\tthing ",
                        N(SyntaxKind.InterpolatedStringTextToken, "some\\tthing ").AsToken().Value);
                }
                N(SyntaxKind.Interpolation);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.StringLiteralExpression);
                    {
                        Assert.Equal(
                            "a very\nlong string",
                            N(SyntaxKind.StringLiteralToken, "\"a very\\nlong string\"").AsToken().Value);
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterpolatedStringText);
                {
                    Assert.Equal(
                        " some\nthing",
                        N(SyntaxKind.InterpolatedStringTextToken, " some\\nthing").AsToken().Value);
                }
                N(SyntaxKind.BacktickToken);
            }
            EOF();
        }

        [Fact]
        public void LanguageParser_ProperlyReadsDeeplyNestedInterpolatedStrings()
        {
            UsingExpression(
                """`a {`very {`{`very {`{`very` .. ` ` .. `very`} very{(" very"):rep(100)}`}`} very`} nested`} string`""",
                new LuaParseOptions(LuaSyntaxOptions.Luau));

            N(SyntaxKind.InterpolatedStringExpression);
            {
                N(SyntaxKind.BacktickToken);
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken, "a ");
                }
                N(SyntaxKind.Interpolation);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.InterpolatedStringExpression);
                    {
                        N(SyntaxKind.BacktickToken);
                        N(SyntaxKind.InterpolatedStringText);
                        {
                            N(SyntaxKind.InterpolatedStringTextToken, "very ");
                        }
                        N(SyntaxKind.Interpolation);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.InterpolatedStringExpression);
                            {
                                N(SyntaxKind.BacktickToken);
                                N(SyntaxKind.Interpolation);
                                {
                                    N(SyntaxKind.OpenBraceToken);
                                    N(SyntaxKind.InterpolatedStringExpression);
                                    {
                                        N(SyntaxKind.BacktickToken);
                                        N(SyntaxKind.InterpolatedStringText);
                                        {
                                            N(SyntaxKind.InterpolatedStringTextToken, "very ");
                                        }
                                        N(SyntaxKind.Interpolation);
                                        {
                                            N(SyntaxKind.OpenBraceToken);
                                            N(SyntaxKind.InterpolatedStringExpression);
                                            {
                                                N(SyntaxKind.BacktickToken);
                                                N(SyntaxKind.Interpolation);
                                                {
                                                    N(SyntaxKind.OpenBraceToken);
                                                    N(SyntaxKind.ConcatExpression);
                                                    {
                                                        N(SyntaxKind.ConcatExpression);
                                                        {
                                                            N(SyntaxKind.InterpolatedStringExpression);
                                                            {
                                                                N(SyntaxKind.BacktickToken);
                                                                N(SyntaxKind.InterpolatedStringText);
                                                                {
                                                                    N(SyntaxKind.InterpolatedStringTextToken, "very");
                                                                }
                                                                N(SyntaxKind.BacktickToken);
                                                            }
                                                            N(SyntaxKind.DotDotToken);
                                                            N(SyntaxKind.InterpolatedStringExpression);
                                                            {
                                                                N(SyntaxKind.BacktickToken);
                                                                N(SyntaxKind.InterpolatedStringText);
                                                                {
                                                                    N(SyntaxKind.InterpolatedStringTextToken, " ");
                                                                }
                                                                N(SyntaxKind.BacktickToken);
                                                            }
                                                        }
                                                        N(SyntaxKind.DotDotToken);
                                                        N(SyntaxKind.InterpolatedStringExpression);
                                                        {
                                                            N(SyntaxKind.BacktickToken);
                                                            N(SyntaxKind.InterpolatedStringText);
                                                            {
                                                                N(SyntaxKind.InterpolatedStringTextToken, "very");
                                                            }
                                                            N(SyntaxKind.BacktickToken);
                                                        }
                                                    }
                                                    N(SyntaxKind.CloseBraceToken);
                                                }
                                                N(SyntaxKind.InterpolatedStringText);
                                                {
                                                    N(SyntaxKind.InterpolatedStringTextToken, " very");
                                                }
                                                N(SyntaxKind.Interpolation);
                                                {
                                                    N(SyntaxKind.OpenBraceToken);
                                                    N(SyntaxKind.MethodCallExpression);
                                                    {
                                                        N(SyntaxKind.ParenthesizedExpression);
                                                        {
                                                            N(SyntaxKind.OpenParenthesisToken);
                                                            N(SyntaxKind.StringLiteralExpression);
                                                            {
                                                                N(SyntaxKind.StringLiteralToken, "\" very\"");
                                                            }
                                                            N(SyntaxKind.CloseParenthesisToken);
                                                        }
                                                        N(SyntaxKind.ColonToken);
                                                        N(SyntaxKind.IdentifierToken, "rep");
                                                        N(SyntaxKind.ExpressionListFunctionArgument);
                                                        {
                                                            N(SyntaxKind.OpenParenthesisToken);
                                                            N(SyntaxKind.NumericalLiteralExpression);
                                                            {
                                                                N(SyntaxKind.NumericLiteralToken, "100");
                                                            }
                                                            N(SyntaxKind.CloseParenthesisToken);
                                                        }
                                                    }
                                                    N(SyntaxKind.CloseBraceToken);
                                                }
                                                N(SyntaxKind.BacktickToken);
                                            }
                                            N(SyntaxKind.CloseBraceToken);
                                        }
                                        N(SyntaxKind.BacktickToken);
                                    }
                                    N(SyntaxKind.CloseBraceToken);
                                }
                                N(SyntaxKind.InterpolatedStringText);
                                {
                                    N(SyntaxKind.InterpolatedStringTextToken, " very");
                                }
                                N(SyntaxKind.BacktickToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                        N(SyntaxKind.InterpolatedStringText);
                        {
                            N(SyntaxKind.InterpolatedStringTextToken, " nested");
                        }
                        N(SyntaxKind.BacktickToken);
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterpolatedStringText);
                {
                    N(SyntaxKind.InterpolatedStringTextToken, " string");
                }
                N(SyntaxKind.BacktickToken);
            }
            EOF();
        }

        [Fact]
        public void LanguageParser_ProperlyReadsInterpolatedStringsWithComplexExpressions()
        {
            UsingExpression(
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

            N(SyntaxKind.FunctionCallExpression);
            {
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "print");
                }
                N(SyntaxKind.ExpressionListFunctionArgument);
                {
                    N(SyntaxKind.OpenParenthesisToken);
                    N(SyntaxKind.InterpolatedStringExpression);
                    {
                        N(SyntaxKind.BacktickToken);
                        N(SyntaxKind.InterpolatedStringText);
                        {
                            N(SyntaxKind.InterpolatedStringTextToken, "some ");
                        }
                        N(SyntaxKind.Interpolation);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.AnonymousFunctionExpression);
                            {
                                N(SyntaxKind.FunctionKeyword);
                                N(SyntaxKind.ParameterList);
                                {
                                    N(SyntaxKind.OpenParenthesisToken);
                                    N(SyntaxKind.CloseParenthesisToken);
                                }
                                N(SyntaxKind.StatementList);
                                {
                                    N(SyntaxKind.ExpressionStatement);
                                    {
                                        N(SyntaxKind.FunctionCallExpression);
                                        {
                                            N(SyntaxKind.IdentifierName);
                                            {
                                                N(SyntaxKind.IdentifierToken, "print");
                                            }
                                            N(SyntaxKind.ExpressionListFunctionArgument);
                                            {
                                                N(SyntaxKind.OpenParenthesisToken);
                                                N(SyntaxKind.InterpolatedStringExpression);
                                                {
                                                    N(SyntaxKind.BacktickToken);
                                                    N(SyntaxKind.InterpolatedStringText);
                                                    {
                                                        N(SyntaxKind.InterpolatedStringTextToken, "other ");
                                                    }
                                                    N(SyntaxKind.Interpolation);
                                                    {
                                                        N(SyntaxKind.OpenBraceToken);
                                                        N(SyntaxKind.AnonymousFunctionExpression);
                                                        {
                                                            N(SyntaxKind.FunctionKeyword);
                                                            N(SyntaxKind.ParameterList);
                                                            {
                                                                N(SyntaxKind.OpenParenthesisToken);
                                                                N(SyntaxKind.CloseParenthesisToken);
                                                            }
                                                            N(SyntaxKind.StatementList);
                                                            {
                                                                N(SyntaxKind.ExpressionStatement);
                                                                {
                                                                    N(SyntaxKind.FunctionCallExpression);
                                                                    {
                                                                        N(SyntaxKind.IdentifierName);
                                                                        {
                                                                            N(SyntaxKind.IdentifierToken, "print");
                                                                        }
                                                                        N(SyntaxKind.ExpressionListFunctionArgument);
                                                                        {
                                                                            N(SyntaxKind.OpenParenthesisToken);
                                                                            N(SyntaxKind.InterpolatedStringExpression);
                                                                            {
                                                                                N(SyntaxKind.BacktickToken);
                                                                                N(SyntaxKind.InterpolatedStringText);
                                                                                {
                                                                                    N(
                                                                                        SyntaxKind
                                                                                            .InterpolatedStringTextToken,
                                                                                        "some ");
                                                                                }
                                                                                N(SyntaxKind.Interpolation);
                                                                                {
                                                                                    N(SyntaxKind.OpenBraceToken);
                                                                                    N(SyntaxKind.IfExpression);
                                                                                    {
                                                                                        N(SyntaxKind.IfKeyword);
                                                                                        N(
                                                                                            SyntaxKind
                                                                                                .TrueLiteralExpression);
                                                                                        {
                                                                                            N(SyntaxKind.TrueKeyword);
                                                                                        }
                                                                                        N(SyntaxKind.ThenKeyword);
                                                                                        N(
                                                                                            SyntaxKind
                                                                                                .AnonymousFunctionExpression);
                                                                                        {
                                                                                            N(
                                                                                                SyntaxKind
                                                                                                    .FunctionKeyword);
                                                                                            N(SyntaxKind.ParameterList);
                                                                                            {
                                                                                                N(
                                                                                                    SyntaxKind
                                                                                                        .OpenParenthesisToken);
                                                                                                N(
                                                                                                    SyntaxKind
                                                                                                        .CloseParenthesisToken);
                                                                                            }
                                                                                            N(SyntaxKind.StatementList);
                                                                                            {
                                                                                                N(
                                                                                                    SyntaxKind
                                                                                                        .ExpressionStatement);
                                                                                                {
                                                                                                    N(
                                                                                                        SyntaxKind
                                                                                                            .FunctionCallExpression);
                                                                                                    {
                                                                                                        N(
                                                                                                            SyntaxKind
                                                                                                                .IdentifierName);
                                                                                                        {
                                                                                                            N(
                                                                                                                SyntaxKind
                                                                                                                    .IdentifierToken,
                                                                                                                "print");
                                                                                                        }
                                                                                                        N(
                                                                                                            SyntaxKind
                                                                                                                .ExpressionListFunctionArgument);
                                                                                                        {
                                                                                                            N(
                                                                                                                SyntaxKind
                                                                                                                    .OpenParenthesisToken);
                                                                                                            N(
                                                                                                                SyntaxKind
                                                                                                                    .InterpolatedStringExpression);
                                                                                                            {
                                                                                                                N(
                                                                                                                    SyntaxKind
                                                                                                                        .BacktickToken);
                                                                                                                N(
                                                                                                                    SyntaxKind
                                                                                                                        .InterpolatedStringText);
                                                                                                                {
                                                                                                                    N(
                                                                                                                        SyntaxKind
                                                                                                                            .InterpolatedStringTextToken,
                                                                                                                        "fucked up ");
                                                                                                                }
                                                                                                                N(
                                                                                                                    SyntaxKind
                                                                                                                        .Interpolation);
                                                                                                                {
                                                                                                                    N(
                                                                                                                        SyntaxKind
                                                                                                                            .OpenBraceToken);
                                                                                                                    N(
                                                                                                                        SyntaxKind
                                                                                                                            .AddExpression);
                                                                                                                    {
                                                                                                                        N(
                                                                                                                            SyntaxKind
                                                                                                                                .NumericalLiteralExpression);
                                                                                                                        {
                                                                                                                            N(
                                                                                                                                SyntaxKind
                                                                                                                                    .NumericLiteralToken,
                                                                                                                                "1");
                                                                                                                        }
                                                                                                                        N(
                                                                                                                            SyntaxKind
                                                                                                                                .PlusToken);
                                                                                                                        N(
                                                                                                                            SyntaxKind
                                                                                                                                .ExponentiateExpression);
                                                                                                                        {
                                                                                                                            N(
                                                                                                                                SyntaxKind
                                                                                                                                    .NumericalLiteralExpression);
                                                                                                                            {
                                                                                                                                N(
                                                                                                                                    SyntaxKind
                                                                                                                                        .NumericLiteralToken,
                                                                                                                                    "2");
                                                                                                                            }
                                                                                                                            N(
                                                                                                                                SyntaxKind
                                                                                                                                    .HatToken);
                                                                                                                            N(
                                                                                                                                SyntaxKind
                                                                                                                                    .NumericalLiteralExpression);
                                                                                                                            {
                                                                                                                                N(
                                                                                                                                    SyntaxKind
                                                                                                                                        .NumericLiteralToken,
                                                                                                                                    "6");
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                    N(
                                                                                                                        SyntaxKind
                                                                                                                            .CloseBraceToken);
                                                                                                                }
                                                                                                                N(
                                                                                                                    SyntaxKind
                                                                                                                        .InterpolatedStringText);
                                                                                                                {
                                                                                                                    N(
                                                                                                                        SyntaxKind
                                                                                                                            .InterpolatedStringTextToken,
                                                                                                                        " shit");
                                                                                                                }
                                                                                                                N(
                                                                                                                    SyntaxKind
                                                                                                                        .BacktickToken);
                                                                                                            }
                                                                                                            N(
                                                                                                                SyntaxKind
                                                                                                                    .CloseParenthesisToken);
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                            N(SyntaxKind.EndKeyword);
                                                                                        }
                                                                                        N(SyntaxKind.ElseKeyword);
                                                                                        N(
                                                                                            SyntaxKind
                                                                                                .AnonymousFunctionExpression);
                                                                                        {
                                                                                            N(
                                                                                                SyntaxKind
                                                                                                    .FunctionKeyword);
                                                                                            N(SyntaxKind.ParameterList);
                                                                                            {
                                                                                                N(
                                                                                                    SyntaxKind
                                                                                                        .OpenParenthesisToken);
                                                                                                N(
                                                                                                    SyntaxKind
                                                                                                        .VarArgParameter);
                                                                                                {
                                                                                                    N(
                                                                                                        SyntaxKind
                                                                                                            .DotDotDotToken);
                                                                                                }
                                                                                                N(
                                                                                                    SyntaxKind
                                                                                                        .CloseParenthesisToken);
                                                                                            }
                                                                                            N(SyntaxKind.StatementList);
                                                                                            {
                                                                                                N(
                                                                                                    SyntaxKind
                                                                                                        .ExpressionStatement);
                                                                                                {
                                                                                                    N(
                                                                                                        SyntaxKind
                                                                                                            .FunctionCallExpression);
                                                                                                    {
                                                                                                        N(
                                                                                                            SyntaxKind
                                                                                                                .IdentifierName);
                                                                                                        {
                                                                                                            N(
                                                                                                                SyntaxKind
                                                                                                                    .IdentifierToken,
                                                                                                                "print");
                                                                                                        }
                                                                                                        N(
                                                                                                            SyntaxKind
                                                                                                                .ExpressionListFunctionArgument);
                                                                                                        {
                                                                                                            N(
                                                                                                                SyntaxKind
                                                                                                                    .OpenParenthesisToken);
                                                                                                            N(
                                                                                                                SyntaxKind
                                                                                                                    .InterpolatedStringExpression);
                                                                                                            {
                                                                                                                N(
                                                                                                                    SyntaxKind
                                                                                                                        .BacktickToken);
                                                                                                                N(
                                                                                                                    SyntaxKind
                                                                                                                        .InterpolatedStringText);
                                                                                                                {
                                                                                                                    N(
                                                                                                                        SyntaxKind
                                                                                                                            .InterpolatedStringTextToken,
                                                                                                                        "fucked up ");
                                                                                                                }
                                                                                                                N(
                                                                                                                    SyntaxKind
                                                                                                                        .Interpolation);
                                                                                                                {
                                                                                                                    N(
                                                                                                                        SyntaxKind
                                                                                                                            .OpenBraceToken);
                                                                                                                    N(
                                                                                                                        SyntaxKind
                                                                                                                            .VarArgExpression);
                                                                                                                    {
                                                                                                                        N(
                                                                                                                            SyntaxKind
                                                                                                                                .DotDotDotToken);
                                                                                                                    }
                                                                                                                    N(
                                                                                                                        SyntaxKind
                                                                                                                            .CloseBraceToken);
                                                                                                                }
                                                                                                                N(
                                                                                                                    SyntaxKind
                                                                                                                        .InterpolatedStringText);
                                                                                                                {
                                                                                                                    N(
                                                                                                                        SyntaxKind
                                                                                                                            .InterpolatedStringTextToken,
                                                                                                                        " shit");
                                                                                                                }
                                                                                                                N(
                                                                                                                    SyntaxKind
                                                                                                                        .BacktickToken);
                                                                                                            }
                                                                                                            N(
                                                                                                                SyntaxKind
                                                                                                                    .CloseParenthesisToken);
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                            N(SyntaxKind.EndKeyword);
                                                                                        }
                                                                                    }
                                                                                    N(SyntaxKind.CloseBraceToken);
                                                                                }
                                                                                N(SyntaxKind.InterpolatedStringText);
                                                                                {
                                                                                    N(
                                                                                        SyntaxKind
                                                                                            .InterpolatedStringTextToken,
                                                                                        " shit");
                                                                                }
                                                                                N(SyntaxKind.BacktickToken);
                                                                            }
                                                                            N(SyntaxKind.CloseParenthesisToken);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            N(SyntaxKind.EndKeyword);
                                                        }
                                                        N(SyntaxKind.CloseBraceToken);
                                                    }
                                                    N(SyntaxKind.InterpolatedStringText);
                                                    {
                                                        N(SyntaxKind.InterpolatedStringTextToken, " shit");
                                                    }
                                                    N(SyntaxKind.BacktickToken);
                                                }
                                                N(SyntaxKind.CloseParenthesisToken);
                                            }
                                        }
                                    }
                                }
                                N(SyntaxKind.EndKeyword);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                        N(SyntaxKind.InterpolatedStringText);
                        {
                            N(SyntaxKind.InterpolatedStringTextToken, " fucked up shit");
                        }
                        N(SyntaxKind.BacktickToken);
                    }
                    N(SyntaxKind.CloseParenthesisToken);
                }
            }
            EOF();
        }
    }
}
