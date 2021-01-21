using System;
using Microsoft.CodeAnalysis;

namespace Loretta.Generators.SyntaxFacts
{
    internal readonly struct KindInfo
    {
        public KindInfo ( IFieldSymbol field, Boolean isTrivia, TokenInfo? tokenInfo, OperatorInfo? unaryOperatorInfo, OperatorInfo? binaryOperatorInfo )
        {
            this.Field = field ?? throw new ArgumentNullException ( nameof ( field ) );
            this.IsTrivia = isTrivia;
            this.TokenInfo = tokenInfo;
            this.UnaryOperatorInfo = unaryOperatorInfo;
            this.BinaryOperatorInfo = binaryOperatorInfo;
        }

        public IFieldSymbol Field { get; }
        public Boolean IsTrivia { get; }
        public TokenInfo? TokenInfo { get; }
        public OperatorInfo? UnaryOperatorInfo { get; }
        public OperatorInfo? BinaryOperatorInfo { get; }
    }
}
