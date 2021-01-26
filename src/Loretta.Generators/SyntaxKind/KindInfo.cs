using System;
using Microsoft.CodeAnalysis;

namespace Loretta.Generators.SyntaxKind
{
    internal readonly struct KindInfo
    {
        public KindInfo ( IFieldSymbol field, Boolean isTrivia, TokenInfo? tokenInfo, OperatorInfo? unaryOperatorInfo, OperatorInfo? binaryOperatorInfo, NodeInfo? nodeInfo )
        {
            this.Field = field ?? throw new ArgumentNullException ( nameof ( field ) );
            this.IsTrivia = isTrivia;
            this.TokenInfo = tokenInfo;
            this.UnaryOperatorInfo = unaryOperatorInfo;
            this.BinaryOperatorInfo = binaryOperatorInfo;
            this.NodeInfo = nodeInfo;
        }

        public IFieldSymbol Field { get; }
        public Boolean IsTrivia { get; }
        public TokenInfo? TokenInfo { get; }
        public OperatorInfo? UnaryOperatorInfo { get; }
        public OperatorInfo? BinaryOperatorInfo { get; }
        public NodeInfo? NodeInfo { get; }
    }
}
