using System.Runtime.CompilerServices;
using TUnit.Assertions.AssertConditions;
using TUnit.Assertions.AssertConditions.Interfaces;
using TUnit.Assertions.AssertionBuilders;

namespace Loretta.CodeAnalysis.Test.Utilities;

sealed file class SyntaxTriviaHasTextAssertion(string expected)
    : ExpectedValueAssertCondition<SyntaxTrivia, string>(expected)
{
    /// <inheritdoc />
    protected override string GetExpectation() => $"to have full string to be {ExpectedValue}";

    /// <inheritdoc />
    protected override ValueTask<AssertionResult> GetResult(SyntaxTrivia actualValue, string? expectedValue)
    {
        return AssertionResult.FailIf(
            !string.Equals(expectedValue, actualValue.ToFullString(), StringComparison.Ordinal),
            $"it was {actualValue.ToFullString()}");
    }
}

internal static partial class ValueSourceExtensions
{
    public static InvokableValueAssertionBuilder<SyntaxTrivia> HasText(
        this IValueSource<SyntaxTrivia>                     valueSource,
        string                                              expected,
        [CallerArgumentExpression(nameof(expected))] string doNotPopulateThisValue1 = "")
        => valueSource.RegisterAssertion(new SyntaxTriviaHasTextAssertion(expected), [doNotPopulateThisValue1]);
}
