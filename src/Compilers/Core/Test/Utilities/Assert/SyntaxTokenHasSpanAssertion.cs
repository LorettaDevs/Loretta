using System.Runtime.CompilerServices;
using Loretta.CodeAnalysis.Text;
using TUnit.Assertions.AssertConditions;
using TUnit.Assertions.AssertConditions.Interfaces;
using TUnit.Assertions.AssertionBuilders;

namespace Loretta.CodeAnalysis.Test.Utilities;

sealed file class SyntaxTokenHasSpanAssertion(TextSpan expected)
    : ExpectedValueAssertCondition<SyntaxToken, TextSpan>(expected)
{
    /// <inheritdoc />
    protected override string GetExpectation() => $"to have span {ExpectedValue}";

    /// <inheritdoc />
    protected override ValueTask<AssertionResult> GetResult(SyntaxToken actualValue, TextSpan expectedValue)
        => AssertionResult.FailIf(expectedValue != actualValue.Span, $"it was {actualValue.Span}");
}

internal static partial class ValueSourceExtensions
{
    public static InvokableValueAssertionBuilder<SyntaxToken> HasSpan(
        this IValueSource<SyntaxToken>                      valueSource,
        TextSpan                                            expected,
        [CallerArgumentExpression(nameof(expected))] string doNotPopulateThisValue1 = "")
        => valueSource.RegisterAssertion(new SyntaxTokenHasSpanAssertion(expected), [doNotPopulateThisValue1]);
}
