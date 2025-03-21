using Loretta.CodeAnalysis.Text;
using Loretta.Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing
{
    public class RegressionTests(ITestOutputHelper output) : ParsingTestsBase(output)
    {
        [Fact]
        public void IncrementalParsing_DoesNotBreak_WithInvalidCastException()
        {
            SyntaxNode firstIdent, secondIdent;
            
            var initial = UsingTree(
                """
                local a = b
                local b = c
                """);
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.StatementList);
                {
                    N(SyntaxKind.LocalVariableDeclarationStatement);
                    {
                        N(SyntaxKind.LocalKeyword);
                        N(SyntaxKind.LocalDeclarationName);
                        {
                            firstIdent = N(SyntaxKind.IdentifierName).AsNode()!;
                            {
                                N(SyntaxKind.IdentifierToken, "a").AsToken();
                            }
                        }
                        N(SyntaxKind.EqualsValuesClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "b");
                            }
                        }
                    }
                    N(SyntaxKind.LocalVariableDeclarationStatement);
                    {
                        N(SyntaxKind.LocalKeyword);
                        N(SyntaxKind.LocalDeclarationName);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "b");
                            }
                        }
                        N(SyntaxKind.EqualsValuesClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "c");
                            }
                        }
                    }
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();

            var newText = initial.GetText()
                                 .WithChanges(new TextChange(new TextSpan(11, 0), " :: T"));

            var newTree = initial.WithChangedText(newText);
            UsingNode((LuaSyntaxNode) newTree.GetRoot());
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.StatementList);
                {
                    N(SyntaxKind.LocalVariableDeclarationStatement);
                    {
                        N(SyntaxKind.LocalKeyword);
                        N(SyntaxKind.LocalDeclarationName);
                        {
                            secondIdent = N(SyntaxKind.IdentifierName).AsNode()!;
                            {
                                N(SyntaxKind.IdentifierToken, "a").AsToken();
                            }
                        }
                        N(SyntaxKind.EqualsValuesClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.TypeCastExpression);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken, "b");
                                }
                                N(SyntaxKind.ColonColonToken);
                                N(SyntaxKind.SimpleTypeName);
                                {
                                    N(SyntaxKind.IdentifierToken, "T");
                                }
                            }
                        }
                    }
                    N(SyntaxKind.LocalVariableDeclarationStatement);
                    {
                        N(SyntaxKind.LocalKeyword);
                        N(SyntaxKind.LocalDeclarationName);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "b");
                            }
                        }
                        N(SyntaxKind.EqualsValuesClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "c");
                            }
                        }
                    }
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();

            Assert.True(firstIdent.IsEquivalentTo(secondIdent));
        }

        [Fact, WorkItem(100, "https://github.com/LorettaDevs/Loretta/issues/100")]
        public void LanguageParser_WhenParsingIntersectionTypes_DoNotGenerateBitwiseOperatorNotSupportedErrors() =>
            ParseAndValidate("type T = A & B", LuaSyntaxOptions.Luau);

        [Fact, WorkItem(100, "https://github.com/LorettaDevs/Loretta/issues/100")]
        public void LanguageParser_WhenParsingUnionTypes_DoNotGenerateBitwiseOperatorNotSupportedErrors() =>
            ParseAndValidate("type T = A | B", LuaSyntaxOptions.Luau);

        [Fact, WorkItem(100, "https://github.com/LorettaDevs/Loretta/issues/100")]
        public void LanguageParser_WhenParsingBitwiseAndExpressions_GeneratesBitwiseOperatorNotSupportedErrors() =>
            ParseAndValidate("local x = y & z", LuaSyntaxOptions.Luau,
                // (1,13): error LUA0021: Bitwise operators are not supported in this lua version
                // local x = y & z
                Diagnostic(ErrorCode.ERR_BitwiseOperatorsNotSupportedInVersion, "&").WithLocation(1, 13));

        [Fact, WorkItem(100, "https://github.com/LorettaDevs/Loretta/issues/100")]
        public void LanguageParser_WhenParsingBitwiseOrExpressions_GeneratesBitwiseOperatorNotSupportedErrors() =>
            ParseAndValidate("local x = y | z", LuaSyntaxOptions.Luau,
                // (1,13): error LUA0021: Bitwise operators are not supported in this lua version
                // local x = y | z
                Diagnostic(ErrorCode.ERR_BitwiseOperatorsNotSupportedInVersion, "|").WithLocation(1, 13));

        [Fact, WorkItem(126, "https://github.com/LorettaDevs/Loretta/issues/126")]
        public void LanguageParser_DoesNotGenerateOutOfRangeDiagnostics()
            => ParseAndValidate("\n\"hello\"\n", LuaSyntaxOptions.Lua51,
                                // (2,1): error LUA1012: Invalid statement
                                // "hello"
                                Diagnostic(ErrorCode.ERR_InvalidStatement, @"""hello""").WithLocation(2, 1));

        [Fact, WorkItem(127, "https://github.com/LorettaDevs/Loretta/issues/127")]
        public void LanguageParser_ProperlyTreatsContinueAsNormalIdentifierWhenContinueTypeIsNone()
        {
            var initial = ParseWithRoundTripCheck(
                """
                local continue = true

                if continue then
                    continue = false
                end
                """,
                new LuaParseOptions(LuaSyntaxOptions.Lua51));

            UsingNode((LuaSyntaxNode) initial.GetRoot());
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.StatementList);
                {
                    N(SyntaxKind.LocalVariableDeclarationStatement);
                    {
                        N(SyntaxKind.LocalKeyword);
                        N(SyntaxKind.LocalDeclarationName);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "continue");
                            }
                            N(SyntaxKind.EqualsValuesClause);
                            {
                                N(SyntaxKind.EqualsToken);
                                N(SyntaxKind.TrueLiteralExpression);
                                {
                                    N(SyntaxKind.TrueKeyword);
                                }
                            }
                        }
                    }

                    N(SyntaxKind.IfStatement);
                    {
                        N(SyntaxKind.IfKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "continue");
                        }
                        N(SyntaxKind.ThenKeyword);

                        N(SyntaxKind.StatementList);
                        {
                            N(SyntaxKind.AssignmentStatement);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken, "continue");
                                }
                                
                                N(SyntaxKind.EqualsValuesClause);
                                {
                                    N(SyntaxKind.EqualsToken);
                                    N(SyntaxKind.FalseLiteralExpression);
                                    {
                                        N(SyntaxKind.FalseKeyword);
                                    }
                                }
                            }
                        }

                        N(SyntaxKind.EndKeyword);
                    }
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(127, "https://github.com/LorettaDevs/Loretta/issues/127")]
        public void LanguageParser_DoesNotFindGotosNorGotoLabelsWhenAcceptGotoIsNotTrue() =>
            ParseAndValidate("::label:: goto label", LuaSyntaxOptions.Lua51,
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
    }
}
