using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using GParse;
using GParse.IO;
using GParse.Lexing;
using GParse.Lexing.Modules;
using Loretta.ThirdParty.FParsec;
using Loretta.Utilities;

namespace Loretta.Lexing.Modules
{
    /// <summary>
    /// Parses numbers in the lexer.
    /// </summary>
    public class NumberLexerModule : ILexerModule<LuaTokenType>
    {
        /// <inheritdoc />
        public String Name => "Number Lexer Module";

        /// <inheritdoc />
        public String? Prefix => null;

        /// <summary>
        /// The options used by this module.
        /// </summary>
        public LuaOptions LuaOptions { get; }

        /// <summary>
        /// Initializes this module with the provided options.
        /// </summary>
        /// <param name="luaOptions">The options to be used by this module.</param>
        public NumberLexerModule ( LuaOptions luaOptions )
        {
            this.LuaOptions = luaOptions;
        }

        /// <summary>
        /// Checks whether the provided character is an underline and the options used by this
        /// module allows accepting underlines in numbers.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>
        /// Whether the provided character is an underline and the options used by this module
        /// allows accepting underlines in numbers.
        /// </returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        private Boolean IsUnderline ( Char ch ) =>
            this.LuaOptions.AcceptUnderlineInNumberLiterals && ch == '_';

        /// <summary>
        /// Checks whether the provided character is a hexadecimal character or an underline if the
        /// options used by this module allows accepting underlines in numbers.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>
        /// Whether the provided character is a hexadecimal character or an underline if the options
        /// used by this module allows accepting underlines in numbers.
        /// </returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        private Boolean IsHexadecimalChar ( Char ch ) =>
            CharUtils.IsHexadecimal ( ch ) || this.IsUnderline ( ch );

        /// <summary>
        /// Checks whether the provided character is a decimal character or an underline if the
        /// options used by this module allows accepting underlines in numbers.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>
        /// Whether the provided character is a decimal character or an underline if the options
        /// used by this module allows accepting underlines in numbers.
        /// </returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        private Boolean IsDecimalChar ( Char ch ) =>
            CharUtils.IsDecimal ( ch ) || this.IsUnderline ( ch );

        /// <summary>
        /// Checks whether the provided character is an octal character or an underline if the
        /// options used by this module allows accepting underlines in numbers.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>
        /// Whether the provided character is an octal character or an underline if the options used
        /// by this module allows accepting underlines in numbers.
        /// </returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        private Boolean IsOctalChar ( Char ch ) =>
            CharUtils.IsInRange ( '0', ch, '7' ) || this.IsUnderline ( ch );

        /// <summary>
        /// Checks whether the provided character is a binary character or an underline if the
        /// options used by this module allows accepting underlines in numbers.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>
        /// Whether the provided character is a binary character or an underline if the options used
        /// by this module allows accepting underlines in numbers.
        /// </returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        private Boolean IsBinaryChar ( Char ch ) =>
            ch == '0' || ch == '1' || this.IsUnderline ( ch );

        /// <inheritdoc />
        public Boolean CanConsumeNext ( IReadOnlyCodeReader reader )
        {
            // We use CharUtils' IsDecimal instead of ours because we don't accept underlines at the
            // start of a number nor right after the dot.
            return reader.Peek ( ) is Char peek
                   && ( CharUtils.IsDecimal ( peek )
                        || ( peek == '.' && reader.Peek ( 1 ) is Char peek2 && CharUtils.IsDecimal ( peek2 ) ) );
        }

        /// <inheritdoc />
        public Token<LuaTokenType> ConsumeNext ( ICodeReader reader, IProgress<Diagnostic> diagnosticReporter )
        {
            var buffer = new StringBuilder ( );
            SourceLocation start = reader.Location;
            Double value = 0;

            buffer.Append ( reader.Read ( ) ?? throw new InvalidOperationException ( "Cannot parse a number from an empty reader." ) );

            switch ( reader.Peek ( ) )
            {
                case 'x':
                {
                    #region Hexadecimal number parsing

                    // Read the integral part
                    reader.Advance ( 1 );
                    buffer.Append ( 'x' );
                    buffer.Append ( reader.ReadStringWhile ( this.IsHexadecimalChar ) );

                    if ( this.LuaOptions.AcceptHexFloatLiterals )
                    {
                        // Read the fractional part
                        if ( reader.IsNext ( '.' ) )
                        {
                            reader.Advance ( 1 );
                            buffer.Append ( '.' );
                            buffer.Append ( reader.ReadStringWhile ( this.IsHexadecimalChar ) );
                        }

                        // Read the exponent
                        if ( reader.IsNext ( 'p' ) || reader.IsNext ( 'P' ) )
                        {
                            buffer.Append ( ( Char ) reader.Read ( )! );

                            // Acommodate optional exponent sign
                            if ( reader.IsNext ( '+' ) || reader.IsNext ( '-' ) )
                                buffer.Append ( ( Char ) reader.Read ( )! );

                            buffer.Append ( reader.ReadStringWhile ( this.IsDecimalChar ) );
                        }

                        try
                        {
                            value = HexFloat.DoubleFromHexString ( buffer.ToString ( )[2..].Replace ( "_", "" ) );
                        }
                        catch ( OverflowException )
                        {
                            goto numberTooLarge;
                        }
                        catch ( FormatException )
                        {
                            goto invalidNumber;
                        }
                    }
                    else
                    {
                        try
                        {
                            value = Double.Parse ( buffer.ToString ( ).Replace ( "_", "" ), NumberStyles.AllowHexSpecifier );
                        }
                        catch ( OverflowException )
                        {
                            goto numberTooLarge;
                        }
                        catch ( FormatException )
                        {
                            goto invalidNumber;
                        }
                    }
                    break;

                    #endregion Hexadecimal number parsing
                }

                case 'o' when this.LuaOptions.AcceptOctalNumbers:
                {
                    #region Octal number parsing

                    reader.Advance ( 1 );
                    buffer.Append ( 'o' );

                    var num = 0;
                    var digs = 0;
                    while ( reader.Peek ( ) is Char peek && this.IsOctalChar ( peek ) )
                    {
                        buffer.Append ( peek );
                        // Skip digit separators
                        if ( peek == '_' )
                        {
                            reader.Advance ( 1 );
                            continue;
                        }

                        /*
                         * x * 2ⁿ ≡ x << n
                         * and
                         * 8 ≡ 2³
                         * so
                         * x * 8 ≡ x << 3
                         */
                        num = ( num << 3 ) | ( reader.Read ( )!.Value - '0' );
                        digs++;
                    }

                    if ( digs < 1 )
                        goto invalidNumber;
                    if ( digs > 21 )
                        goto numberTooLarge;

                    value = num;
                    break;

                    #endregion Octal number parsing
                }

                case 'b' when this.LuaOptions.AcceptBinaryNumbers:
                {
                    #region Binary number parsing

                    reader.Advance ( 1 );
                    buffer.Append ( 'b' );

                    var num = 0;
                    var digs = 0;
                    while ( reader.Peek ( ) is Char peek && this.IsBinaryChar ( peek ) )
                    {
                        buffer.Append ( peek );
                        // Skip digit separators
                        if ( peek == '_' )
                        {
                            reader.Advance ( 1 );
                            continue;
                        }

                        num = ( num << 1 ) | ( reader.Read ( )!.Value & 1 );
                        digs++;
                    }

                    if ( digs < 1 )
                        goto invalidNumber;
                    if ( digs > 32 )
                        goto numberTooLarge;

                    value = num;
                    break;

                    #endregion Binary number parsing
                }

                default:
                {
                    #region Decimal number parsing

                    // Read the rest of the integral part
                    if ( reader.Peek ( ) is Char peek && this.IsDecimalChar ( peek ) )
                        buffer.Append ( reader.ReadStringWhile ( this.IsDecimalChar ) );

                    // Read the fractionary part
                    if ( reader.IsNext ( '.' ) )
                    {
                        reader.Advance ( 1 );
                        buffer.Append ( '.' );
                        buffer.Append ( reader.ReadStringWhile ( this.IsDecimalChar ) );
                    }

                    // Read the exponent
                    if ( reader.IsNext ( 'e' ) || reader.IsNext ( 'E' ) )
                    {
                        buffer.Append ( ( Char ) reader.Read ( )! );

                        // Acommodate optional exponent sign
                        if ( reader.IsNext ( '+' ) || reader.IsNext ( '-' ) )
                            buffer.Append ( ( Char ) reader.Read ( )! );

                        buffer.Append ( reader.ReadStringWhile ( this.IsDecimalChar ) );
                    }

                    try
                    {
                        value = Double.Parse ( buffer.ToString ( ).Replace ( "_", "" ), NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture );
                    }
                    catch ( OverflowException )
                    {
                        goto numberTooLarge;
                    }
                    catch ( FormatException )
                    {
                        goto invalidNumber;
                    }
                    break;

                    #endregion Decimal number parsing
                }
            }

            return new Token<LuaTokenType> ( "number", buffer.ToString ( ), value, LuaTokenType.Number, start.To ( reader.Location ) );

        invalidNumber:
            diagnosticReporter.Report ( LuaDiagnostics.SyntaxError.InvalidNumber ( start.To ( reader.Location ), buffer.ToString ( ) ) );
            goto returnFallbackToken;

        numberTooLarge:
            diagnosticReporter.Report ( LuaDiagnostics.SyntaxError.NumberTooLarge ( start.To ( reader.Location ), buffer.ToString ( ) ) );
            goto returnFallbackToken;

        returnFallbackToken:
            return new Token<LuaTokenType> ( "number", "", 0d, LuaTokenType.Number, start.To ( reader.Location ) );
        }
    }
}