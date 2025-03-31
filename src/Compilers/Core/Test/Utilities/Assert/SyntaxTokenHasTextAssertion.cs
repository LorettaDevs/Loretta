using System.Runtime.CompilerServices;
using TUnit.Assertions.AssertConditions;
using TUnit.Assertions.AssertConditions.Interfaces;
using TUnit.Assertions.AssertionBuilders;

namespace Loretta.CodeAnalysis.Test.Utilities;

sealed file class SyntaxTokenHasTextAssertion(string expected) : ExpectedValueAssertCondition<SyntaxToken, string>(expected)
{
    /// <inheritdoc />
    protected override string GetExpectation() => $"to have text {ExpectedValue}";

    /// <inheritdoc />
    protected override ValueTask<AssertionResult> GetResult(SyntaxToken actualValue, string? expectedValue)
    {
        return AssertionResult.FailIf(
            !string.Equals(expectedValue, actualValue.Text, StringComparison.Ordinal),
            $"it was {actualValue.Text}.");
    }
}

internal static partial class ValueSourceExtensions
{
    public static InvokableValueAssertionBuilder<SyntaxToken> HasText(
        this IValueSource<SyntaxToken>                      valueSource,
        string                                              expected,
        [CallerArgumentExpression(nameof(expected))] string doNotPopulateThisValue1 = "")
        => valueSource.RegisterAssertion(new SyntaxTokenHasTextAssertion(expected), [doNotPopulateThisValue1]);
}
