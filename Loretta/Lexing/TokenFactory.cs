using System;
using System.Globalization;
using System.Linq;
using GParse;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Lexing
{
    /// <summary>
    /// The utility class for generating tokens.
    /// </summary>
    public static class TokenFactory
    {
        /// <summary>
        /// Clones a token with a new type.
        /// </summary>
        /// <param name="token">The token's type to be changed.</param>
        /// <param name="type">The type to assign to the cloned token.</param>
        /// <returns>The token with the changed type.</returns>
        public static LuaToken ChangeTokenType ( LuaToken token, LuaTokenType type ) =>
            new LuaToken ( token.Id, token.Raw, token.Value, type, token.Range, token.IsTrivia, token.Trivia.ToArray ( ) );

        /// <summary>
        /// Creates a new token with the provided data.
        /// </summary>
        /// <param name="id">The token's ID.</param>
        /// <param name="type">The token's type.</param>
        /// <param name="raw">The token's raw value. If null uses the <paramref name="id" />.</param>
        /// <param name="value">The token's value. Defaults to null.</param>
        /// <param name="range">
        /// The location range of the token. If null uses <see cref="SourceRange.Zero" />.
        /// </param>
        /// <returns>The newly created token.</returns>
        public static LuaToken Token ( String id, LuaTokenType type, String? raw = null, Object? value = null, SourceRange? range = null, Boolean isTrivia = false ) =>
            new LuaToken ( id, raw ?? id, value, type, range ?? SourceRange.Zero, isTrivia );

        /// <summary>
        /// Creates an identifier token.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="range">
        /// <inheritdoc cref="Token(String, LuaTokenType, String?, Object?, SourceRange?, Boolean)" />
        /// </param>
        /// <returns>The identifier token.</returns>
        public static LuaToken Identifier ( String identifier, SourceRange? range = null ) =>
            Token ( identifier, LuaTokenType.Identifier, identifier, identifier, range );

        /// <summary>
        /// Creates a boolean literal token.
        /// </summary>
        /// <param name="value">The token's value.</param>
        /// <param name="range">
        /// <inheritdoc cref="Token(String, LuaTokenType, String?, Object?, SourceRange?, Boolean)" />
        /// </param>
        /// <returns>The boolean literal token.</returns>
        public static LuaToken Boolean ( Boolean value, SourceRange? range = null )
        {
            var strValue = value ? "true" : "false";
            return Token ( strValue, LuaTokenType.Boolean, strValue, value, range );
        }

        /// <summary>
        /// Creates a number literal token.
        /// </summary>
        /// <param name="value">The token's value.</param>
        /// <param name="rawValue">
        /// The number's raw (string) form. If null calls <see
        /// cref="Double.ToString(IFormatProvider)" /> on the <paramref name="value" /> with <see
        /// cref="CultureInfo.InvariantCulture" />.
        /// </param>
        /// <param name="range">
        /// <inheritdoc cref="Token(String, LuaTokenType, String?, Object?, SourceRange?, Boolean)" />
        /// </param>
        /// <returns>The number literal token.</returns>
        public static LuaToken Number ( Double value, String? rawValue = null, SourceRange? range = null )
        {
            if ( Double.IsNaN ( value ) || Double.IsInfinity ( value ) )
            {
                throw new InvalidOperationException ( "Can't create a number token from NaN or Infinity." );
            }

            rawValue ??= value.ToString ( CultureInfo.InvariantCulture );
            return Token ( "number", LuaTokenType.Number, rawValue, value, range );
        }

        /// <summary>
        /// Creates a short string token.
        /// </summary>
        /// <param name="value">The string's value.</param>
        /// <param name="rawValue">The string's raw (string) form.</param>
        /// <param name="range">
        /// <inheritdoc cref="Token(String, LuaTokenType, String?, Object?, SourceRange?, Boolean)" />
        /// </param>
        /// <returns>The string literal token.</returns>
        public static LuaToken ShortString ( String value, String rawValue, SourceRange? range = null )
        {
            if ( value is null )
                throw new ArgumentNullException ( nameof ( value ) );
            if ( rawValue is null )
                throw new ArgumentNullException ( nameof ( rawValue ) );

            return Token ( "string", LuaTokenType.String, rawValue, value, range );
        }

        /// <summary>
        /// Creates a long string literal token.
        /// </summary>
        /// <param name="value">The string's value.</param>
        /// <param name="rawValue">The string's raw (string) value.</param>
        /// <param name="range">
        /// <inheritdoc cref="Token(String, LuaTokenType, String?, Object?, SourceRange?, Boolean)" />
        /// </param>
        /// <returns>The long string literal token.</returns>
        public static LuaToken LongString ( String value, String rawValue, SourceRange? range = null )
        {
            if ( value is null )
                throw new ArgumentNullException ( nameof ( value ) );
            if ( rawValue is null )
                throw new ArgumentNullException ( nameof ( rawValue ) );

            return Token ( "long-string", LuaTokenType.LongString, rawValue, value, range );
        }

        /// <summary>
        /// Creates a long string literal token.
        /// </summary>
        /// <param name="value">The string's value.</param>
        /// <param name="rawValue">The string's raw (string) value.</param>
        /// <param name="start">The string's starting location.</param>
        /// <returns>The long string literal token.</returns>
        public static LuaToken LongString ( String value, String rawValue, SourceLocation? start = null )
        {
            if ( value is null )
                throw new ArgumentNullException ( nameof ( value ) );
            if ( rawValue is null )
                throw new ArgumentNullException ( nameof ( rawValue ) );
            SourceRange? range = null;
            if ( start is SourceLocation s )
            {
                var lines = rawValue.Split ( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );
                if ( lines.Length == 1 )
                {
                    range = s.To ( new SourceLocation ( s.Line, s.Column + lines[0].Length, s.Byte + rawValue.Length ) );
                }
                else
                {
                    range = s.To ( new SourceLocation ( s.Line + lines.Length - 1, 1 + lines[^1].Length, s.Byte + rawValue.Length ) );
                }
            }

            return Token ( "long-string", LuaTokenType.LongString, rawValue, value, range );
        }

        /// <summary>
        /// Creates a single-line comment.
        /// </summary>
        /// <param name="value">The comment's contents.</param>
        /// <param name="rawValue">The comment's raw form.</param>
        /// <param name="start">The comment's start location.</param>
        /// <returns></returns>
        public static LuaToken Comment ( String value, String? rawValue = null, SourceLocation? start = null )
        {
            if ( value is null )
                throw new ArgumentNullException ( nameof ( value ) );
            if ( value.Contains ( '\r', StringComparison.Ordinal ) || value.Contains ( '\n', StringComparison.Ordinal ) )
                throw new ArgumentException ( "A single-line comment cannot have line breaks.", nameof ( value ) );
            rawValue ??= ( "--" + value );
            SourceRange? range = null;
            if ( start is SourceLocation s )
                range = start.Value.To ( new SourceLocation ( s.Line, s.Column + rawValue.Length, s.Byte + rawValue.Length ) );

            return Token ( "comment", LuaTokenType.Comment, rawValue, value, range, true );
        }

        /// <summary>
        /// Creates a new <see cref="LuaTokenType.LongComment"/> comment.
        /// </summary>
        /// <param name="value">The comment's contents.</param>
        /// <param name="rawValue">The comment's raw form.</param>
        /// <param name="start">The starting location of the token.</param>
        /// <returns></returns>
        public static LuaToken LongComment ( String value, String rawValue, SourceLocation? start = null )
        {
            if ( value is null )
                throw new ArgumentNullException ( nameof ( value ) );
            if ( rawValue is null )
                throw new ArgumentNullException ( nameof ( rawValue ) );
            SourceRange? range = null;
            if ( start is SourceLocation s )
            {
                var lines = rawValue.Split ( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );
                if ( lines.Length == 1 )
                {
                    range = s.To ( new SourceLocation ( s.Line, s.Column + lines[0].Length, s.Byte + rawValue.Length ) );
                }
                else
                {
                    range = s.To ( new SourceLocation ( s.Line + lines.Length - 1, 1 + lines[^1].Length, s.Byte + rawValue.Length ) );
                }
            }

            return Token ( "long-comment", LuaTokenType.LongComment, rawValue, value, range, true );
        }
    }
}