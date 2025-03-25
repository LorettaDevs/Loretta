using Microsoft.CodeAnalysis;

namespace Loretta.Generators.SyntaxFactsGenerator
{
    internal readonly struct KindInfo(
        IFieldSymbol                               field,
        bool                                       isTrivia,
        TokenInfo?                                 tokenInfo,
        OperatorInfo?                              unaryOperatorInfo,
        OperatorInfo?                              binaryOperatorInfo,
        ImmutableArray<string>                     extraCategories,
        ImmutableDictionary<string, TypedConstant> properties
    )
    {
        public IFieldSymbol Field { get; } = field ?? throw new ArgumentNullException(nameof(field));

        public bool IsTrivia { get; } = isTrivia;

        public TokenInfo? TokenInfo { get; } = tokenInfo;

        public OperatorInfo? UnaryOperatorInfo { get; } = unaryOperatorInfo;

        public OperatorInfo? BinaryOperatorInfo { get; } = binaryOperatorInfo;

        public ImmutableArray<string> ExtraCategories { get; } = extraCategories;

        public ImmutableDictionary<string, TypedConstant> Properties { get; } = properties;
    }

    internal readonly struct TokenInfo(string? text, bool isKeyword)
    {
        public string? Text { get; } = text;

        public bool IsKeyword { get; } = isKeyword;

        public override string ToString() => $"{{ Text = \"{Text}\", IsKeyword = {IsKeyword} }}";
    }

    internal readonly struct OperatorInfo(int precedence, TypedConstant expression)
    {
        public int Precedence { get; } = precedence;

        public TypedConstant Expression { get; } = expression;

        public override string ToString() => $"{{ Precedence = {Precedence}, Expression = {Expression} }}";
    }
}
