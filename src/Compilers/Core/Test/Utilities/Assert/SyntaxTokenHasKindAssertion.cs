using System.Runtime.CompilerServices;
using Loretta.CodeAnalysis.Lua;
using TUnit.Assertions.AssertConditions;
using TUnit.Assertions.AssertConditions.Interfaces;
using TUnit.Assertions.AssertionBuilders;

namespace Loretta.CodeAnalysis.Test.Utilities;

sealed file class SyntaxTokenHasKindAssertion(SyntaxKind expected)
    : ExpectedValueAssertCondition<SyntaxToken, SyntaxKind>(expected)
{
    /// <inheritdoc />
    protected override string GetExpectation() => $"to have kind SyntaxKind.{ExpectedValue}";

    /// <inheritdoc />
    protected override ValueTask<AssertionResult> GetResult(SyntaxToken actualValue, SyntaxKind expectedValue)
        => AssertionResult.FailIf(expectedValue != actualValue.Kind(), $"it was SyntaxKind.{actualValue.Kind()}");
}

internal static partial class ValueSourceExtensions
{
    public static InvokableValueAssertionBuilder<SyntaxToken> HasKind(
        this IValueSource<SyntaxToken>                      valueSource,
        SyntaxKind                                          expected,
        [CallerArgumentExpression(nameof(expected))] string doNotPopulateThisValue1 = "")
        => valueSource.RegisterAssertion(new SyntaxTokenHasKindAssertion(expected), [doNotPopulateThisValue1]);
}
