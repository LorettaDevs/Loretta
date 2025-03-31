using System.Runtime.CompilerServices;
using TUnit.Assertions.AssertConditions;
using TUnit.Assertions.AssertConditions.Interfaces;
using TUnit.Assertions.AssertionBuilders;

namespace Loretta.CodeAnalysis.Test.Utilities;

sealed file class SyntaxTokenHasValueAssertion(object? expected)
    : ExpectedValueAssertCondition<SyntaxToken, object?>(expected)
{
    /// <inheritdoc />
    protected override string GetExpectation() => $"to have value {ExpectedValue}";

    /// <inheritdoc />
    protected override ValueTask<AssertionResult> GetResult(SyntaxToken actualValue, object? expectedValue)
    {
        return AssertionResult.FailIf(
            !expectedValue?.Equals(actualValue.Value) ?? actualValue.Value is not null,
            $"it was {actualValue.Value}.");
    }
}

internal static partial class ValueSourceExtensions
{
    public static InvokableValueAssertionBuilder<SyntaxToken> HasValue(
        this IValueSource<SyntaxToken>                      valueSource,
        object?                                             expected,
        [CallerArgumentExpression(nameof(expected))] string doNotPopulateThisValue1 = "")
        => valueSource.RegisterAssertion(new SyntaxTokenHasValueAssertion(expected), [doNotPopulateThisValue1]);
}
