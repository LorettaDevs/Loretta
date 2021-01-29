namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a function name which is a member.
    /// </summary>
    public sealed partial class MemberFunctionNameSyntax : FunctionNameSyntax
    {
        internal MemberFunctionNameSyntax (
            SyntaxTree syntaxTree,
            FunctionNameSyntax name,
            SyntaxToken dotToken,
            SyntaxToken identifier )
            : base ( syntaxTree )
        {
            this.Name = name;
            this.DotToken = dotToken;
            this.Identifier = identifier;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.MemberFunctionName;

        /// <summary>
        /// The base name.
        /// </summary>
        public FunctionNameSyntax Name { get; }

        /// <summary>
        /// The dot token.
        /// </summary>
        public SyntaxToken DotToken { get; }

        /// <summary>
        /// The member name.
        /// </summary>
        public SyntaxToken Identifier { get; }
    }
}