using Loretta.CodeAnalysis.Text;
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
            SyntaxToken firstIdentToken, secondIdentToken;
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
                                firstIdentToken = N(SyntaxKind.IdentifierToken, "a").AsToken();
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
                                secondIdentToken = N(SyntaxKind.IdentifierToken, "a").AsToken();
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
    }
}
