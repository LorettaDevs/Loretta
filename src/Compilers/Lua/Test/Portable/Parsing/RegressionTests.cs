using Loretta.CodeAnalysis.Text;
using Loretta.Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing
{
    public class RegressionTests : ParsingTestsBase
    {
        public RegressionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void IncrementalParsing_DoesNotBreak_WithInvalidCastException()
        {
            SyntaxNode firstIdent, secondIdent;
            var initial = ParseWithRoundTripCheck("""
                local a = b
                local b = c
                """);

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
    }
}
