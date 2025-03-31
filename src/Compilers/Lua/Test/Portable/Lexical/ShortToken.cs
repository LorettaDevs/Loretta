using Loretta.CodeAnalysis.Text;
using Tsu;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Lexical;

public readonly record struct ShortToken(
    SyntaxKind      Kind,
    string          Text,
    TextSpan        Span,
    Option<object?> Value = default
)
{
    public ShortToken(SyntaxKind kind, string text, Option<object?> value = default) : this(
        kind,
        text,
        new TextSpan(0, text.Length),
        value) { }

    public ShortToken(SyntaxToken token) : this(token.Kind(), token.Text, token.Span, token.Value) { }

    public ShortToken(SyntaxTrivia trivia) : this(trivia.Kind(), trivia.ToFullString(), trivia.Span) { }

    public ShortToken WithSpan(TextSpan span) => this with { Span = span };

    public override string ToString() => $"{Kind}<{Text}> ({Span}){(Value.IsSome ? $" = {Value.Value}" : "")}";
}