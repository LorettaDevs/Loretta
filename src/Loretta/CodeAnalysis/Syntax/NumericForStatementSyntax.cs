using System.Collections.Immutable;
using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a numeric for statement.
    /// </summary>
    public sealed partial class NumericForStatementSyntax : StatementSyntax
    {
        internal NumericForStatementSyntax (
            SyntaxToken forKeyword,
            SyntaxToken identifier,
            SyntaxToken equalsToken,
            ExpressionSyntax initialValue,
            SyntaxToken finalValueCommaToken,
            ExpressionSyntax finalValue,
            Option<SyntaxToken> stepValueCommaToken,
            Option<ExpressionSyntax> stepValue,
            SyntaxToken doKeyword,
            ImmutableArray<StatementSyntax> body,
            SyntaxToken endKeyword,
            Option<SyntaxToken> semicolonToken )
            : base ( semicolonToken )
        {
            this.ForKeyword = forKeyword;
            this.Identifier = identifier;
            this.EqualsToken = equalsToken;
            this.InitialValue = initialValue;
            this.FinalValueCommaToken = finalValueCommaToken;
            this.FinalValue = finalValue;
            this.StepValueCommaToken = stepValueCommaToken;
            this.StepValue = stepValue;
            this.DoKeyword = doKeyword;
            this.Body = body;
            this.EndKeyword = endKeyword;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.NumericForStatement;

        /// <summary>
        /// The 'for' keyword.
        /// </summary>
        public SyntaxToken ForKeyword { get; }

        /// <summary>
        /// The loop variable identifier.
        /// </summary>
        public SyntaxToken Identifier { get; }

        /// <summary>
        /// The equals token.
        /// </summary>
        public SyntaxToken EqualsToken { get; }
        
        /// <summary>
        /// The expression defining the initial value of the loop variable.
        /// </summary>
        public ExpressionSyntax InitialValue { get; }

        /// <summary>
        /// The comma separating the initial value from the final value.
        /// </summary>
        public SyntaxToken FinalValueCommaToken { get; }

        /// <summary>
        /// The expression defining the final value of the loop variable.
        /// </summary>
        public ExpressionSyntax FinalValue { get; }

        /// <summary>
        /// The comma separating the final value from the step value.
        /// May be None if there is no step.
        /// </summary>
        public Option<SyntaxToken> StepValueCommaToken { get; }

        /// <summary>
        /// The expression defining the step value of the loop variable.
        /// May be None if there is no step.
        /// </summary>
        public Option<ExpressionSyntax> StepValue { get; }

        /// <summary>
        /// The 'do' keyword.
        /// </summary>
        public SyntaxToken DoKeyword { get; }

        /// <summary>
        /// The loop's body.
        /// </summary>
        public ImmutableArray<StatementSyntax> Body { get; }

        /// <summary>
        /// The end keyword.
        /// </summary>
        public SyntaxToken EndKeyword { get; }
    }
}