#define LARGE_TESTS
//#define LARGE_TESTS_DEBUG
using Loretta.CodeAnalysis.Text;
using Tsu;

namespace Loretta.CodeAnalysis.Lua.Syntax.UnitTests.Lexical
{
    public readonly struct ShortToken
    {
        public readonly SyntaxKind Kind;
        public readonly string Text;
        public readonly Option<object?> Value;
        public readonly TextSpan Span;

        public ShortToken(SyntaxKind kind, string text, Option<object?> value = default)
        {
            Kind = kind;
            Text = text;
            Value = value;
            Span = new TextSpan(0, text.Length);
        }

        public ShortToken(SyntaxKind kind, string text, TextSpan span, Option<object?> value = default)
        {
            Kind = kind;
            Text = text;
            Value = value;
            Span = span;
        }

        public ShortToken(SyntaxToken token)
            : this(token.Kind(), token.Text, token.Span, token.Value)
        {
        }

        public ShortToken WithSpan(TextSpan span) =>
            new ShortToken(Kind, Text, span, Value);

        public override string ToString() => $"{Kind}<{Text}>";
    }
}
