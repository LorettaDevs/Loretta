using System.Runtime.CompilerServices;
using TUnit.Assertions.AssertConditions;
using TUnit.Assertions.AssertConditions.Interfaces;
using TUnit.Assertions.AssertionBuilders;

namespace Loretta.CodeAnalysis.Test.Utilities;

sealed file class SyntaxNodeOrTokenHasTextAssertion(string expected)
    : ExpectedValueAssertCondition<SyntaxNodeOrToken, string>(expected)
{
    /// <inheritdoc />
    protected override string GetExpectation() => $"to have text {ExpectedValue}";

    /// <inheritdoc />
    protected override ValueTask<AssertionResult> GetResult(SyntaxNodeOrToken actualValue, string? expectedValue)
    {
        return AssertionResult.FailIf(
            !string.Equals(expectedValue, actualValue.ToString(), StringComparison.Ordinal),
            $"it was {actualValue.ToString()}.");
    }
}

internal static partial class ValueSourceExtensions
{
    public static InvokableValueAssertionBuilder<SyntaxNodeOrToken> HasText(
        this IValueSource<SyntaxNodeOrToken>                valueSource,
        string                                              expected,
        [CallerArgumentExpression(nameof(expected))] string doNotPopulateThisValue1 = "")
        => valueSource.RegisterAssertion(new SyntaxNodeOrTokenHasTextAssertion(expected), [doNotPopulateThisValue1]);
}
