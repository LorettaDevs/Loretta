using System;

namespace Loretta.Generators.SyntaxKind
{
    internal readonly struct OperatorInfo
    {
        public OperatorInfo ( Int32 precedence )
        {
            this.Precedence = precedence;
        }

        public Int32 Precedence { get; }

        public override String ToString ( ) =>
            $"{{ Precedence = {this.Precedence} }}";
    }
}
