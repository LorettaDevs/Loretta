using System;

namespace Loretta.Generators.SyntaxKind
{
    internal readonly struct TokenInfo
    {
        public TokenInfo ( String? text, Boolean isKeyword )
        {
            this.Text = text;
            this.IsKeyword = isKeyword;
        }

        public String? Text { get; }
        public Boolean IsKeyword { get; }

        public override String ToString ( ) =>
            $"{{ Text = \"{this.Text}\", IsKeyword = {this.IsKeyword} }}";
    }
}
