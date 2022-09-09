using Microsoft.CodeAnalysis;

namespace Loretta.Generators.SyntaxFactsGenerator
{
    internal readonly struct KindInfo
    {
        public KindInfo(
            IFieldSymbol field,
            bool isTrivia,
            TokenInfo? tokenInfo,
            OperatorInfo? unaryOperatorInfo,
            OperatorInfo? binaryOperatorInfo,
            ImmutableArray<string> extraCategories,
            ImmutableDictionary<string, TypedConstant> properties)
        {
            Field = field ?? throw new ArgumentNullException(nameof(field));
            IsTrivia = isTrivia;
            TokenInfo = tokenInfo;
            UnaryOperatorInfo = unaryOperatorInfo;
            BinaryOperatorInfo = binaryOperatorInfo;
            ExtraCategories = extraCategories;
            Properties = properties;
        }

        public IFieldSymbol Field { get; }
        public bool IsTrivia { get; }
        public TokenInfo? TokenInfo { get; }
        public OperatorInfo? UnaryOperatorInfo { get; }
        public OperatorInfo? BinaryOperatorInfo { get; }
        public ImmutableArray<string> ExtraCategories { get; }
        public ImmutableDictionary<string, TypedConstant> Properties { get; }
    }

    internal readonly struct TokenInfo
    {
        public TokenInfo(string? text, bool isKeyword)
        {
            Text = text;
            IsKeyword = isKeyword;
        }

        public string? Text { get; }
        public bool IsKeyword { get; }

        public override string ToString() =>
            $"{{ Text = \"{Text}\", IsKeyword = {IsKeyword} }}";
    }

    internal readonly struct OperatorInfo
    {
        public OperatorInfo(int precedence, TypedConstant expression)
        {
            Precedence = precedence;
            Expression = expression;
        }

        public int Precedence { get; }
        public TypedConstant Expression { get; }

        public override string ToString() =>
            $"{{ Precedence = {Precedence}, Expression = {Expression} }}";
    }
}
