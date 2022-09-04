using Microsoft.CodeAnalysis;

namespace Loretta.Generators.SyntaxKindGenerator
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
}
