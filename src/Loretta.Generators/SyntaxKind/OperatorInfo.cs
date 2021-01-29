using System;
using Microsoft.CodeAnalysis;

namespace Loretta.Generators.SyntaxKind
{
    internal readonly struct OperatorInfo
    {
        public OperatorInfo ( Int32 precedence, TypedConstant expression )
        {
            this.Precedence = precedence;
            this.Expression = expression;
        }

        public Int32 Precedence { get; }
        public TypedConstant Expression { get; }

        public override String ToString ( ) =>
            $"{{ Precedence = {this.Precedence}, Expression = {this.Expression} }}";
    }
}
