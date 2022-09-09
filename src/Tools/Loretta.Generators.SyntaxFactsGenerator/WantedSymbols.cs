using Microsoft.CodeAnalysis;

namespace Loretta.Generators.SyntaxFactsGenerator
{
    internal sealed class WantedSymbols : IEquatable<WantedSymbols?>
    {
        public WantedSymbols(
            INamedTypeSymbol? syntaxKindType,
            INamedTypeSymbol extraCategoriesAttributeType,
            INamedTypeSymbol triviaAttributeType,
            INamedTypeSymbol tokenAttributeType,
            INamedTypeSymbol keywordAttributeType,
            INamedTypeSymbol unaryOperatorAttributeType,
            INamedTypeSymbol binaryOperatorAttributeType,
            INamedTypeSymbol propertyAttributeType)
        {
            SyntaxKindType = syntaxKindType;
            ExtraCategoriesAttributeType = extraCategoriesAttributeType ?? throw new ArgumentNullException(nameof(extraCategoriesAttributeType));
            TriviaAttributeType = triviaAttributeType ?? throw new ArgumentNullException(nameof(triviaAttributeType));
            TokenAttributeType = tokenAttributeType ?? throw new ArgumentNullException(nameof(tokenAttributeType));
            KeywordAttributeType = keywordAttributeType ?? throw new ArgumentNullException(nameof(keywordAttributeType));
            UnaryOperatorAttributeType = unaryOperatorAttributeType ?? throw new ArgumentNullException(nameof(unaryOperatorAttributeType));
            BinaryOperatorAttributeType = binaryOperatorAttributeType ?? throw new ArgumentNullException(nameof(binaryOperatorAttributeType));
            PropertyAttributeType = propertyAttributeType ?? throw new ArgumentNullException(nameof(propertyAttributeType));
        }

        public INamedTypeSymbol? SyntaxKindType { get; }
        public INamedTypeSymbol ExtraCategoriesAttributeType { get; }
        public INamedTypeSymbol TriviaAttributeType { get; }
        public INamedTypeSymbol TokenAttributeType { get; }
        public INamedTypeSymbol KeywordAttributeType { get; }
        public INamedTypeSymbol UnaryOperatorAttributeType { get; }
        public INamedTypeSymbol BinaryOperatorAttributeType { get; }
        public INamedTypeSymbol PropertyAttributeType { get; }

        #region Equality and HashCode (required for proper caching)
        public override bool Equals(object? obj) => Equals(obj as WantedSymbols);

        public bool Equals(WantedSymbols? other) =>
            other is not null
            && SymbolEqualityComparer.Default.Equals(SyntaxKindType, other.SyntaxKindType)
            && SymbolEqualityComparer.Default.Equals(ExtraCategoriesAttributeType, other.ExtraCategoriesAttributeType)
            && SymbolEqualityComparer.Default.Equals(TriviaAttributeType, other.TriviaAttributeType)
            && SymbolEqualityComparer.Default.Equals(TokenAttributeType, other.TokenAttributeType)
            && SymbolEqualityComparer.Default.Equals(KeywordAttributeType, other.KeywordAttributeType)
            && SymbolEqualityComparer.Default.Equals(UnaryOperatorAttributeType, other.UnaryOperatorAttributeType)
            && SymbolEqualityComparer.Default.Equals(BinaryOperatorAttributeType, other.BinaryOperatorAttributeType)
            && SymbolEqualityComparer.Default.Equals(PropertyAttributeType, other.PropertyAttributeType);

        public override int GetHashCode()
        {
            var hashCode = -162528945;
            hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(SyntaxKindType);
            hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(ExtraCategoriesAttributeType);
            hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(TriviaAttributeType);
            hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(TokenAttributeType);
            hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(KeywordAttributeType);
            hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(UnaryOperatorAttributeType);
            hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(BinaryOperatorAttributeType);
            hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(PropertyAttributeType);
            return hashCode;
        }
        #endregion Equality and HashCode (required for proper caching)
    }
}
