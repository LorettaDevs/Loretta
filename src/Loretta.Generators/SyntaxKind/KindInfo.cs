using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Loretta.Generators.SyntaxKind
{
    internal readonly struct KindInfo
    {
        public KindInfo (
            IFieldSymbol field,
            Boolean isTrivia,
            TokenInfo? tokenInfo,
            OperatorInfo? unaryOperatorInfo,
            OperatorInfo? binaryOperatorInfo,
            ImmutableArray<String> extraCategories,
            ImmutableDictionary<String, TypedConstant> properties )
        {
            this.Field = field ?? throw new ArgumentNullException ( nameof ( field ) );
            this.IsTrivia = isTrivia;
            this.TokenInfo = tokenInfo;
            this.UnaryOperatorInfo = unaryOperatorInfo;
            this.BinaryOperatorInfo = binaryOperatorInfo;
            this.ExtraCategories = extraCategories;
            this.Properties = properties;
        }

        public IFieldSymbol Field { get; }
        public Boolean IsTrivia { get; }
        public TokenInfo? TokenInfo { get; }
        public OperatorInfo? UnaryOperatorInfo { get; }
        public OperatorInfo? BinaryOperatorInfo { get; }
        public ImmutableArray<String> ExtraCategories { get; }
        public ImmutableDictionary<String, TypedConstant> Properties { get; }
    }
}
