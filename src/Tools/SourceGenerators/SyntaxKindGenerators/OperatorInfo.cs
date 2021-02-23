using Microsoft.CodeAnalysis;

namespace Loretta.Generators.SyntaxKindGenerators
{
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
