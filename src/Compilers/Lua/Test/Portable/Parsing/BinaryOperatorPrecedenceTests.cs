namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing;

public sealed class BinaryOperatorPrecedenceTests : ParsingTestsBase
{
    public static IEnumerable<(SyntaxKind, SyntaxKind)> GetBinaryOperatorPairsData()
    {
        var untestedKinds = new[] { SyntaxKind.TypeCastExpression, SyntaxKind.FloorDivideExpression, };
        return from leftKind in SyntaxFacts.GetBinaryExpressionKinds().Except(untestedKinds)
               from rightKind in SyntaxFacts.GetBinaryExpressionKinds().Except(untestedKinds)
               select (leftKind, rightKind);
    }

    private static bool LeftBindsStrongerThanRight(SyntaxKind leftKind, SyntaxKind rightKind)
    {
        var leftPrecedence  = SyntaxFacts.GetBinaryOperatorPrecedence(leftKind);
        var rightPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(rightKind);

        if (leftPrecedence > rightPrecedence) return true;
        return leftPrecedence == rightPrecedence && !SyntaxFacts.IsRightAssociative(leftKind);
    }

    [Test]
    [MethodDataSource(nameof(GetBinaryOperatorPairsData))]
    public async Task Parser_DoesBinaryOperatorPrecedencesCorrectly(
        SyntaxKind leftExpressionKind,
        SyntaxKind rightExpressionKind)
    {
        var leftTokenKind  = SyntaxFacts.GetOperatorTokenKind(leftExpressionKind).Value;
        var rightTokenKind = SyntaxFacts.GetOperatorTokenKind(rightExpressionKind).Value;

        var leftTokenText  = SyntaxFacts.GetText(leftTokenKind);
        var rightTokenText = SyntaxFacts.GetText(rightTokenKind);

        var text = $"a {leftTokenText} b {rightTokenText} c";

        await UsingExpressionAsync(text, new LuaParseOptions(LuaSyntaxOptions.All));
        if (LeftBindsStrongerThanRight(leftTokenKind, rightTokenKind))
        {
            // Format:
            //     +
            //    / \
            //   +   c
            //  / \
            // a   b
            await N(rightExpressionKind);
            {
                await N(leftExpressionKind);
                {
                    await N(SyntaxKind.IdentifierName);
                    {
                        await N(SyntaxKind.IdentifierToken, "a");
                    }
                    await N(leftTokenKind, leftTokenText);
                    await N(SyntaxKind.IdentifierName);
                    {
                        await N(SyntaxKind.IdentifierToken, "b");
                    }
                }
                await N(rightTokenKind, rightTokenText);
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "c");
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
            await N(leftExpressionKind);
            {
                await N(SyntaxKind.IdentifierName);
                {
                    await N(SyntaxKind.IdentifierToken, "a");
                }
                await N(leftTokenKind, leftTokenText);
                await N(rightExpressionKind);
                {
                    await N(SyntaxKind.IdentifierName);
                    {
                        await N(SyntaxKind.IdentifierToken, "b");
                    }
                    await N(rightTokenKind, rightTokenText);
                    await N(SyntaxKind.IdentifierName);
                    {
                        await N(SyntaxKind.IdentifierToken, "c");
                    }
                }
            }
            EOF();
        }
    }
}
