using System;
using System.Globalization;
using GParse;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Lexing
{
    public static class TokenFactory
    {
        public static LuaToken ChangeTokenType ( LuaToken token, LuaTokenType type ) =>
            new LuaToken ( token.Id, token.Raw, token.Value, type, token.Range );

        public static LuaToken Token ( String ID, LuaTokenType type, String? raw = null, Object? value = null, SourceRange? range = null ) =>
            new LuaToken ( ID, raw ?? ID, value, type, range ?? SourceRange.Zero );

        public static LuaToken Boolean ( Boolean value, SourceRange? range = null )
        {
            var strValue = value ? "true" : "false";
            return Token ( strValue, LuaTokenType.Boolean, strValue, value, range );
        }

        public static LuaToken Number ( Double value, String? rawValue = null, SourceRange? range = null )
        {
            if ( Double.IsNaN ( value ) || Double.IsInfinity ( value ) )
            {
                throw new InvalidOperationException ( "Can't create a number token from NaN or Infinity." );
            }

            rawValue ??= value.ToString ( CultureInfo.InvariantCulture );
            return Token ( "number", LuaTokenType.Number, rawValue, value, range );
        }

        public static LuaToken ShortString ( String value, String rawValue, SourceRange? range = null )
        {
            if ( value is null )
                throw new ArgumentNullException ( nameof ( value ) );
            if ( rawValue is null )
                throw new ArgumentNullException ( nameof ( rawValue ) );

            return Token ( "string", LuaTokenType.String, rawValue, value, range );
        }

        public static LuaToken LongString ( String value, String rawValue, SourceRange? range = null )
        {
            if ( value is null )
                throw new ArgumentNullException ( nameof ( value ) );
            if ( rawValue is null )
                throw new ArgumentNullException ( nameof ( rawValue ) );

            return Token ( "long-string", LuaTokenType.LongString, rawValue, value, range );
        }
    }
}