using Xunit;
using Xunit.Abstractions;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing
{
    public class BinaryOperatorPrecedenceTests(ITestOutputHelper output) : ParsingTestsBase(output)
    {
        public static IEnumerable<object[]> GetBinaryOperatorPairsData()
        {
            var untestedKinds = new[]
            {
                SyntaxKind.TypeCastExpression,
                SyntaxKind.FloorDivideExpression,
            };
            return from leftKind in SyntaxFacts.GetBinaryExpressionKinds()
                                               .Except(untestedKinds)
                   from rightKind in SyntaxFacts.GetBinaryExpressionKinds()
                                                .Except(untestedKinds)
                   select new object[]
                   {
                       leftKind,
                       rightKind
                   };
        }

        private static bool LeftBindsStrongerThanRight(SyntaxKind leftKind, SyntaxKind rightKind)
        {
            var leftPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(leftKind);
            var rightPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(rightKind);

            if (leftPrecedence > rightPrecedence)
                return true;
            if (leftPrecedence == rightPrecedence && !SyntaxFacts.IsRightAssociative(leftKind))
                return true;
            return false;
        }

        [Theory]
        [MemberData(nameof(GetBinaryOperatorPairsData))]
        public void Parser_DoesBinaryOperatorPrecedencesCorrectly(SyntaxKind leftExpressionKind, SyntaxKind rightExpressionKind)
        {
            var leftTokenKind = SyntaxFacts.GetOperatorTokenKind(leftExpressionKind).Value;
            var rightTokenKind = SyntaxFacts.GetOperatorTokenKind(rightExpressionKind).Value;

            var leftTokenText = SyntaxFacts.GetText(leftTokenKind);
            var rightTokenText = SyntaxFacts.GetText(rightTokenKind);

            var text = $"a {leftTokenText} b {rightTokenText} c";

            UsingExpression(text, new LuaParseOptions(LuaSyntaxOptions.All));
            if (LeftBindsStrongerThanRight(leftTokenKind, rightTokenKind))
            {
                // Format:
                //     +
                //    / \
                //   +   c
                //  / \
                // a   b
                N(rightExpressionKind);
                {
                    N(leftExpressionKind);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "a");
                        }
                        N(leftTokenKind, leftTokenText);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "b");
                        }
                    }
                    N(rightTokenKind, rightTokenText);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "c");
                    }
                }
                EOF();
            }
            else
            {
                // Format:
                //     ^
                //    / \
                //   a  ^
                //     / \
                //    b   c
                N(leftExpressionKind);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                    N(leftTokenKind, leftTokenText);
                    N(rightExpressionKind);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "b");
                        }
                        N(rightTokenKind, rightTokenText);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "c");
                        }
                    }
                }
                EOF();
            }
        }
    }
}
