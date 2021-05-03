namespace Loretta.Generators.SyntaxKindGenerator
{
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
}
