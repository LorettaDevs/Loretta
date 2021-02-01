using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;
using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// A syntax token.
    /// </summary>
    public sealed class SyntaxToken : SyntaxNode
    {
        /// <summary>
        /// Initializes a new syntax token.
        /// </summary>
        /// <param name="kind"><inheritdoc cref="Kind" path="/summary"/></param>
        /// <param name="position"><inheritdoc cref="Position" path="/summary"/></param>
        /// <param name="text"><inheritdoc cref="Text" path="/summary"/></param>
        /// <param name="value"><inheritdoc cref="Value" path="/summary"/></param>
        /// <param name="leadingTrivia"><inheritdoc cref="LeadingTrivia" path="/summary"/></param>
        /// <param name="trailingTrivia"><inheritdoc cref="TrailingTrivia" path="/summary"/></param>
        /// <param name="isMissing"><inheritdoc cref="IsMissing" path="/summary"/></param>
        internal SyntaxToken (
            SyntaxKind kind,
            Int32 position,
            Option<ReadOnlyMemory<Char>> text,
            Option<Object?> value,
            ImmutableArray<SyntaxTrivia> leadingTrivia,
            ImmutableArray<SyntaxTrivia> trailingTrivia,
            Boolean isMissing )
        {
            this.Kind = kind;
            this.Position = position;
            this.Text = text;
            this.Value = value;
            this.LeadingTrivia = leadingTrivia;
            this.TrailingTrivia = trailingTrivia;
            this.IsMissing = isMissing;
        }

        /// <summary>
        /// This token's kind.
        /// </summary>
        public override SyntaxKind Kind { get; }

        /// <summary>
        /// This token's position.
        /// </summary>
        public Int32 Position { get; }

        /// <summary>
        /// This token's raw text.
        /// </summary>
        public Option<ReadOnlyMemory<Char>> Text { get; }

        /// <summary>
        /// This token's value.
        /// </summary>
        public Option<Object?> Value { get; }

        /// <summary>
        /// This token's position range.
        /// </summary>
        public override TextSpan Span => new ( this.Position, this.Text.Map ( static r => r.Length ).UnwrapOr ( 0 ) );

        /// <summary>
        /// This token's full range including trivia.
        /// </summary>
        public override TextSpan FullSpan
        {
            get
            {
                var start = this.LeadingTrivia.Length == 0
                            ? this.Span.Start
                            : this.LeadingTrivia.First ( ).Span.Start;
                var end = this.TrailingTrivia.Length == 0
                          ? this.Span.End
                          : this.TrailingTrivia.Last ( ).Span.End;
                return TextSpan.FromBounds ( start, end );
            }
        }

        /// <summary>
        /// This token's leading trivia.
        /// </summary>
        public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }

        /// <summary>
        /// This token's trailing trivia.
        /// </summary>
        public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }

        /// <summary>
        /// Whether this token is missing (inserted by the parser and doesn't appear in source).
        /// </summary>
        public Boolean IsMissing { get; }

        /// <inheritdoc/>
        public override void Accept ( SyntaxVisitor syntaxVisitor ) =>
            syntaxVisitor.VisitToken ( this );

        /// <inheritdoc/>
        public override TReturn? Accept<TReturn> ( SyntaxVisitor<TReturn> syntaxVisitor ) where TReturn : default =>
            syntaxVisitor.VisitToken ( this );

        /// <inheritdoc/>
        public override IEnumerable<SyntaxNode> GetChildren ( ) => Enumerable.Empty<SyntaxNode> ( );

        /// <summary>
        /// Updates this token with the provided values.
        /// Returns the token itself if nothing changed.
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="position"></param>
        /// <param name="text"></param>
        /// <param name="value"></param>
        /// <param name="leadingTrivia"></param>
        /// <param name="trailingTrivia"></param>
        /// <param name="isMissing"></param>
        /// <returns></returns>
        public SyntaxToken Update (
            SyntaxKind kind,
            Int32 position,
            Option<ReadOnlyMemory<Char>> text,
            Option<Object?> value,
            ImmutableArray<SyntaxTrivia> leadingTrivia,
            ImmutableArray<SyntaxTrivia> trailingTrivia,
            Boolean isMissing )
        {
            if ( kind != this.Kind
                 || position != this.Position
                 || text != this.Text
                 || value != this.Value
                 || leadingTrivia != this.LeadingTrivia
                 || trailingTrivia != this.TrailingTrivia
                 || isMissing != this.IsMissing )
            {
                return new SyntaxToken ( kind, position, text, value, leadingTrivia, trailingTrivia, isMissing );
            }

            return this;
        }

        /// <summary>
        /// Copies this token replacing its kind with the provided one.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public SyntaxToken WithKind ( SyntaxKind kind ) =>
            this.Update (
                kind,
                this.Position,
                this.Text,
                this.Value,
                this.LeadingTrivia,
                this.TrailingTrivia,
                this.IsMissing );

        /// <summary>
        /// Copies this token replacing its position with the provided one.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public SyntaxToken WithPosition ( Int32 position ) =>
            this.Update (
                this.Kind,
                position,
                this.Text,
                this.Value,
                this.LeadingTrivia,
                this.TrailingTrivia,
                this.IsMissing );

        /// <summary>
        /// Copies this token replacing its text with the provided one.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public SyntaxToken WithText ( Option<ReadOnlyMemory<Char>> text ) =>
            this.Update (
                this.Kind,
                this.Position,
                text,
                this.Value,
                this.LeadingTrivia,
                this.TrailingTrivia,
                this.IsMissing );

        /// <summary>
        /// Copies this token replacing its value with the provided one.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public SyntaxToken WithValue ( Option<Object?> value ) =>
            this.Update (
                this.Kind,
                this.Position,
                this.Text,
                value,
                this.LeadingTrivia,
                this.TrailingTrivia,
                this.IsMissing );

        /// <summary>
        /// Generates a new token with the provided leading trivia.
        /// </summary>
        /// <param name="leadingTrivia"></param>
        /// <returns></returns>
        public SyntaxToken WithLeadingTrivia ( ImmutableArray<SyntaxTrivia> leadingTrivia ) =>
            this.Update (
                this.Kind,
                this.Position,
                this.Text,
                this.Value,
                leadingTrivia,
                this.TrailingTrivia,
                this.IsMissing );

        /// <summary>
        /// Generates a new token but with the provided trailing trivia.
        /// </summary>
        /// <param name="trailingTrivia"></param>
        /// <returns></returns>
        public SyntaxToken WithTrailingTrivia ( ImmutableArray<SyntaxTrivia> trailingTrivia ) =>
            this.Update (
                this.Kind,
                this.Position,
                this.Text,
                this.Value,
                this.LeadingTrivia,
                trailingTrivia,
                this.IsMissing );
    }

    public static partial class SyntaxFactory
    {
        /// <summary>
        /// Creates a token of the provided <paramref name="kind"/>.
        /// </summary>
        /// <param name="kind">The token's kind.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Token ( SyntaxKind kind, Boolean isMissing = false )
        {
            if ( !kind.IsToken ( ) || SyntaxFacts.GetText ( kind ) is null )
                throw new ArgumentException ( "Kind must be a token's with predefined text.", nameof ( kind ) );
            return Token ( kind, SyntaxFacts.GetText ( kind )!, isMissing );
        }

        /// <summary>
        /// Creates a new token with the provided <paramref name="leadingTrivia"/>, <paramref name="kind"/> and <paramref name="trailingTrivia"/>.
        /// </summary>
        /// <param name="leadingTrivia">The token's leading <see cref="SyntaxTrivia"/>.</param>
        /// <param name="kind">The token's kind.</param>
        /// <param name="trailingTrivia">The token's trailing <see cref="SyntaxTrivia"/>.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Token ( ImmutableArray<SyntaxTrivia> leadingTrivia, SyntaxKind kind, ImmutableArray<SyntaxTrivia> trailingTrivia, Boolean isMissing = false )
        {
            if ( leadingTrivia.IsDefault )
                throw new ArgumentException ( $"'{nameof ( leadingTrivia )}' must not be a default array.", nameof ( leadingTrivia ) );
            if ( trailingTrivia.IsDefault )
                throw new ArgumentException ( $"'{nameof ( trailingTrivia )}' must not be a default array.", nameof ( trailingTrivia ) );
            if ( !kind.IsToken ( ) || SyntaxFacts.GetText ( kind ) is null )
                throw new ArgumentException ( "Kind must be a token's with predefined text.", nameof ( kind ) );
            return Token ( leadingTrivia, kind, SyntaxFacts.GetText ( kind )!, trailingTrivia, isMissing );
        }

        /// <summary>
        /// Creates a new token with the provided <paramref name="kind"/> and <paramref name="text"/>.
        /// </summary>
        /// <param name="kind">The token's kind.</param>
        /// <param name="text">The token's text.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Token ( SyntaxKind kind, String text, Boolean isMissing = false )
        {
            if ( !kind.IsToken ( ) )
                throw new ArgumentException ( "Kind must be a token's.", nameof ( kind ) );
            return new SyntaxToken (
                kind,
                0,
                text.AsMemory ( ),
                default,
                SyntaxTrivia.Empty,
                SyntaxTrivia.Empty,
                isMissing );
        }

        /// <summary>
        /// Creates a new token with the provided <paramref name="kind"/> and <paramref name="text"/>.
        /// </summary>
        /// <param name="leadingTrivia">The token's leading <see cref="SyntaxTrivia"/>.</param>
        /// <param name="kind">The token's kind.</param>
        /// <param name="text">The token's text.</param>
        /// <param name="trailingTrivia">The token's trailing <see cref="SyntaxTrivia"/>.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Token ( ImmutableArray<SyntaxTrivia> leadingTrivia, SyntaxKind kind, String text, ImmutableArray<SyntaxTrivia> trailingTrivia, Boolean isMissing = false )
        {
            if ( leadingTrivia.IsDefault )
                throw new ArgumentException ( $"'{nameof ( leadingTrivia )}' must not be a default array.", nameof ( leadingTrivia ) );
            if ( trailingTrivia.IsDefault )
                throw new ArgumentException ( $"'{nameof ( trailingTrivia )}' must not be a default array.", nameof ( trailingTrivia ) );
            if ( !kind.IsToken ( ) )
                throw new ArgumentException ( "Kind must be a token's.", nameof ( kind ) );
            return new SyntaxToken (
                kind,
                0,
                text.AsMemory ( ),
                default,
                leadingTrivia,
                trailingTrivia,
                isMissing );
        }

        /// <summary>
        /// Creates a new token with the provided <paramref name="kind"/>, <paramref name="text"/>
        /// and <paramref name="value"/>.
        /// </summary>
        /// <param name="kind">The token's kind.</param>
        /// <param name="text">The token's text.</param>
        /// <param name="value">The token' value.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Token ( SyntaxKind kind, String text, Object value, Boolean isMissing = false )
        {
            if ( !kind.IsToken ( ) )
                throw new ArgumentException ( "Kind must be a token's.", nameof ( kind ) );
            return new SyntaxToken (
                kind,
                0,
                text.AsMemory ( ),
                value,
                SyntaxTrivia.Empty,
                SyntaxTrivia.Empty,
                isMissing );
        }

        /// <summary>
        /// Creates a new token with the provided <paramref name="kind"/>, <paramref name="text"/>
        /// and <paramref name="value"/>.
        /// </summary>
        /// <param name="leadingTrivia">The token's leading <see cref="SyntaxTrivia"/>.</param>
        /// <param name="kind">The token's kind.</param>
        /// <param name="text">The token's text.</param>
        /// <param name="value">The token' value.</param>
        /// <param name="trailingTrivia">The token's trailing <see cref="SyntaxTrivia"/>.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Token ( ImmutableArray<SyntaxTrivia> leadingTrivia, SyntaxKind kind, String text, Object value, ImmutableArray<SyntaxTrivia> trailingTrivia, Boolean isMissing = false )
        {
            if ( leadingTrivia.IsDefault )
                throw new ArgumentException ( $"'{nameof ( leadingTrivia )}' must not be a default array.", nameof ( leadingTrivia ) );
            if ( trailingTrivia.IsDefault )
                throw new ArgumentException ( $"'{nameof ( trailingTrivia )}' must not be a default array.", nameof ( trailingTrivia ) );
            if ( !kind.IsToken ( ) )
                throw new ArgumentException ( "Kind must be a token's.", nameof ( kind ) );
            return new SyntaxToken (
                kind,
                0,
                text.AsMemory ( ),
                value,
                leadingTrivia,
                trailingTrivia,
                isMissing );
        }

        /// <summary>
        /// Creates an identifier token.
        /// </summary>
        /// <param name="identifier">The identifier's text.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Identifier ( String identifier, Boolean isMissing = false )
        {
            if ( String.IsNullOrWhiteSpace ( identifier ) )
                throw new ArgumentException ( $"'{nameof ( identifier )}' cannot be null or whitespace.", nameof ( identifier ) );
            if ( !StringUtils.IsIdentifier ( identifier ) )
                throw new ArgumentException ( $"'{nameof ( identifier )}' must be a valid identifier.", nameof ( identifier ) );

            return Token ( SyntaxKind.IdentifierToken, identifier, isMissing );
        }

        /// <summary>
        /// Creates an identifier token.
        /// </summary>
        /// <param name="leadingTrivia">The token's leading <see cref="SyntaxTrivia"/>.</param>
        /// <param name="identifier">The identifier's text.</param>
        /// <param name="trailingTrivia">The token's trailing <see cref="SyntaxTrivia"/>.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Identifier ( ImmutableArray<SyntaxTrivia> leadingTrivia, String identifier, ImmutableArray<SyntaxTrivia> trailingTrivia, Boolean isMissing = false )
        {
            if ( leadingTrivia.IsDefault )
                throw new ArgumentException ( $"'{nameof ( leadingTrivia )}' must not be a default array.", nameof ( leadingTrivia ) );
            if ( trailingTrivia.IsDefault )
                throw new ArgumentException ( $"'{nameof ( trailingTrivia )}' must not be a default array.", nameof ( trailingTrivia ) );
            if ( String.IsNullOrWhiteSpace ( identifier ) )
                throw new ArgumentException ( $"'{nameof ( identifier )}' cannot be null or whitespace.", nameof ( identifier ) );
            if ( !StringUtils.IsIdentifier ( identifier ) )
                throw new ArgumentException ( $"'{nameof ( identifier )}' must be a valid identifier.", nameof ( identifier ) );

            return Token ( leadingTrivia, SyntaxKind.IdentifierToken, identifier, trailingTrivia, isMissing );
        }

        /// <summary>
        /// Creates a string literal token.
        /// </summary>
        /// <param name="value">The string value of the token.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Literal ( String value, Boolean isMissing = false )
        {
            var delimiter = value.Contains ( '\'' ) ? '"' : '\'';
            var escaped = "\\" + delimiter;
            var raw = String.Concat ( delimiter, String.Concat ( value.Select ( c => c switch
            {
                '\'' when delimiter == c => escaped,
                '"' when delimiter == c => escaped,
                _ => LoCharUtils.ToReadableString ( c )
            } ) ), delimiter );

            return Literal ( raw, value, isMissing );
        }

        /// <summary>
        /// Creates a string literal token.
        /// </summary>
        /// <param name="leadingTrivia">The token's leading <see cref="SyntaxTrivia"/>.</param>
        /// <param name="value">The string value of the token.</param>
        /// <param name="trailingTrivia">The token's trailing <see cref="SyntaxTrivia"/>.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Literal ( ImmutableArray<SyntaxTrivia> leadingTrivia, String value, ImmutableArray<SyntaxTrivia> trailingTrivia, Boolean isMissing = false )
        {
            var delimiter = value.Contains ( '\'' ) ? '"' : '\'';
            var escaped = "\\" + delimiter;
            var raw = String.Concat ( delimiter, String.Concat ( value.Select ( c => c switch
            {
                '\'' when delimiter == c => escaped,
                '"' when delimiter == c => escaped,
                _ => LoCharUtils.ToReadableString ( c )
            } ) ), delimiter );

            return Literal ( leadingTrivia, raw, value, trailingTrivia, isMissing );
        }

        /// <summary>
        /// Creates a new string literal token.
        /// </summary>
        /// <param name="text">The token's raw text.</param>
        /// <param name="value">The token's value.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Literal ( String text, String value, Boolean isMissing = false ) =>
            Token (
                text.StartsWith ( "[" ) ? SyntaxKind.LongStringToken : SyntaxKind.ShortStringToken,
                text,
                value,
                isMissing );

        /// <summary>
        /// Creates a new string literal token.
        /// </summary>
        /// <param name="leadingTrivia">The token's leading <see cref="SyntaxTrivia"/>.</param>
        /// <param name="text">The token's raw text.</param>
        /// <param name="value">The token's value.</param>
        /// <param name="trailingTrivia">The token's trailing <see cref="SyntaxTrivia"/>.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Literal ( ImmutableArray<SyntaxTrivia> leadingTrivia, String text, String value, ImmutableArray<SyntaxTrivia> trailingTrivia, Boolean isMissing = false ) =>
            Token (
                leadingTrivia,
                text.StartsWith ( "[" ) ? SyntaxKind.LongStringToken : SyntaxKind.ShortStringToken,
                text,
                value,
                trailingTrivia,
                isMissing );

        /// <summary>
        /// Creates a new numeric literal token.
        /// </summary>
        /// <param name="value">The token's value.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Literal ( Double value, Boolean isMissing = false ) =>
            Literal ( value.ToString ( ), value, isMissing );

        /// <summary>
        /// Creates a new numeric literal token.
        /// </summary>
        /// <param name="leadingTrivia">The token's leading <see cref="SyntaxTrivia"/>.</param>
        /// <param name="value">The token's value.</param>
        /// <param name="trailingTrivia">The token's trailing <see cref="SyntaxTrivia"/>.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Literal ( ImmutableArray<SyntaxTrivia> leadingTrivia, Double value, ImmutableArray<SyntaxTrivia> trailingTrivia, Boolean isMissing = false ) =>
            Literal ( leadingTrivia, value.ToString ( ), value, trailingTrivia, isMissing );

        /// <summary>
        /// Creates a new numeric literal token.
        /// </summary>
        /// <param name="text">The token's raw text.</param>
        /// <param name="value">The token's value.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Literal ( String text, Double value, Boolean isMissing = false ) =>
            Token ( SyntaxKind.NumberToken, text, ( Double ) value, isMissing );

        /// <summary>
        /// Creates a new numeric literal token.
        /// </summary>
        /// <param name="leadingTrivia">The token's leading <see cref="SyntaxTrivia"/>.</param>
        /// <param name="text">The token's raw text.</param>
        /// <param name="value">The token's value.</param>
        /// <param name="trailingTrivia">The token's trailing <see cref="SyntaxTrivia"/>.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Literal ( ImmutableArray<SyntaxTrivia> leadingTrivia, String text, Double value, ImmutableArray<SyntaxTrivia> trailingTrivia, Boolean isMissing = false ) =>
            Token ( leadingTrivia, SyntaxKind.NumberToken, text, ( Double ) value, trailingTrivia, isMissing );

        /// <summary>
        /// Creates a new boolean keyword token.
        /// </summary>
        /// <param name="value">The token's value.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Literal ( Boolean value, Boolean isMissing = false ) =>
            Token ( value ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword, isMissing );

        /// <summary>
        /// Creates a new boolean keyword token.
        /// </summary>
        /// <param name="leadingTrivia">The token's leading <see cref="SyntaxTrivia"/>.</param>
        /// <param name="value">The token's value.</param>
        /// <param name="trailingTrivia">The token's trailing <see cref="SyntaxTrivia"/>.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Literal ( ImmutableArray<SyntaxTrivia> leadingTrivia, Boolean value, ImmutableArray<SyntaxTrivia> trailingTrivia, Boolean isMissing = false ) =>
            Token ( leadingTrivia, value ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword, trailingTrivia, isMissing );

        /// <summary>
        /// Creates a new nil keyword token.
        /// </summary>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Nil ( Boolean isMissing = false ) =>
            Token ( SyntaxKind.NilKeyword, isMissing );

        /// <summary>
        /// Creates a new nil keyword token.
        /// </summary>
        /// <param name="leadingTrivia">The token's leading <see cref="SyntaxTrivia"/>.</param>
        /// <param name="trailingTrivia">The token's trailing <see cref="SyntaxTrivia"/>.</param>
        /// <param name="isMissing">Whether the token should be considered missing from the source text.</param>
        /// <returns></returns>
        public static SyntaxToken Nil ( ImmutableArray<SyntaxTrivia> leadingTrivia, ImmutableArray<SyntaxTrivia> trailingTrivia, Boolean isMissing = false ) =>
            Token ( leadingTrivia, SyntaxKind.NilKeyword, trailingTrivia, isMissing );
    }
}
