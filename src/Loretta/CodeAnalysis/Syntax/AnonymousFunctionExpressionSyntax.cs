using System;
using System.Collections.Immutable;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents an anonymous function expression.
    /// </summary>
    public sealed partial class AnonymousFunctionExpressionSyntax : ExpressionSyntax
    {
        internal AnonymousFunctionExpressionSyntax (
            SyntaxToken functionKeyword,
            ParameterListSyntax parameters,
            ImmutableArray<StatementSyntax> body,
            SyntaxToken endKeyword )
        {
            this.FunctionKeyword = functionKeyword;
            this.Parameters = parameters;
            this.Body = body;
            this.EndKeyword = endKeyword;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.AnonymousFunctionExpression;

        /// <summary>
        /// The 'function' keyword.
        /// </summary>
        public SyntaxToken FunctionKeyword { get; }

        /// <summary>
        /// The list of parameters.
        /// </summary>
        public ParameterListSyntax Parameters { get; }

        /// <summary>
        /// The function's body.
        /// </summary>
        public ImmutableArray<StatementSyntax> Body { get; }

        /// <summary>
        /// The 'end' keyword.
        /// </summary>
        public SyntaxToken EndKeyword { get; }
    }

    public static partial class SyntaxFactory
    {
        /// <summary>
        /// Creates a new <see cref="AnonymousFunctionExpressionSyntax"/>.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static AnonymousFunctionExpressionSyntax AnonymousFunctionExpression (
            ParameterListSyntax parameters,
            ImmutableArray<StatementSyntax> body )
        {
            if ( parameters is null )
                throw new ArgumentNullException ( nameof ( parameters ) );
            if ( body.IsDefault )
                throw new ArgumentException ( $"'{nameof ( body )}' must not be a default array." );

            return AnonymousFunctionExpression (
                Token ( SyntaxKind.FunctionKeyword ),
                parameters,
                body,
                Token ( SyntaxKind.EndKeyword ) );
        }

        /// <summary>
        /// Creates a new <see cref="AnonymousFunctionExpressionSyntax"/>.
        /// </summary>
        /// <param name="functionKeyword"></param>
        /// <param name="parameters"></param>
        /// <param name="body"></param>
        /// <param name="endKeyword"></param>
        /// <returns></returns>
        public static AnonymousFunctionExpressionSyntax AnonymousFunctionExpression (
            SyntaxToken functionKeyword,
            ParameterListSyntax parameters,
            ImmutableArray<StatementSyntax> body,
            SyntaxToken endKeyword )
        {
            if ( functionKeyword is null )
                throw new ArgumentNullException ( nameof ( functionKeyword ) );
            if ( functionKeyword.Kind != SyntaxKind.FunctionKeyword )
                throw new ArgumentException ( $"'{nameof ( functionKeyword )}' must be a FunctionKeyword." );
            if ( parameters is null )
                throw new ArgumentNullException ( nameof ( parameters ) );
            if ( body.IsDefault )
                throw new ArgumentException ( $"'{nameof ( body )}' must not be a default array." );
            if ( endKeyword is null )
                throw new ArgumentNullException ( nameof ( endKeyword ) );
            if ( endKeyword.Kind != SyntaxKind.EndKeyword )
                throw new ArgumentException ( $"'{nameof ( endKeyword )}' must be an EndKeyword." );

            return new AnonymousFunctionExpressionSyntax ( functionKeyword, parameters, body, endKeyword );
        }
    }
}