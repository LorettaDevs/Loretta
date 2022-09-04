using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text;
using Loretta.CodeAnalysis.Lua.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal sealed partial class Lexer
    {
        private int ConsumeDecimalDigits(StringBuilder? builder)
        {
            var start = TextWindow.Position;
            char digit;
            while (CharUtils.IsDecimal(digit = TextWindow.PeekChar()) || digit == '_')
            {
                if (digit != '_')
                    builder?.Append(digit);
                TextWindow.AdvanceChar();
            }
            return TextWindow.Position - start;
        }

        private int ConsumeHexDigits()
        {
            var start = TextWindow.Position;
            char digit;
            while (CharUtils.IsHexadecimal(digit = TextWindow.PeekChar()) || digit == '_')
            {
                if (digit != '_')
                    _builder.Append(digit);
                TextWindow.AdvanceChar();
            }
            return TextWindow.Position - start;
        }

        private void ParseBinaryNumber(ref TokenInfo info)
        {
            var num = 0UL;
            var digits = 0;
            var hasUnderscores = false;
            var hasOverflown = false;
            char digit;
            while (CharUtils.IsBinary(digit = TextWindow.PeekChar()) || digit == '_')
            {
                TextWindow.AdvanceChar();
                if (digit == '_')
                {
                    hasUnderscores = true;
                    continue;
                }
                // Next shift will overflow if 63rd bit is set
                if ((num & 0x8000_0000_0000_0000) != 0)
                    hasOverflown = true;
                num = (num << 1) | (ulong) CharUtils.DecimalValue(digit);
                digits++;
            }

            var (isUnsignedLong, isSignedLong, isComplex) = (false, false, false);

            if (TextWindow.AdvanceIfMatches("ull", true))
            {
                isUnsignedLong = true;
            }
            else if (TextWindow.AdvanceIfMatches("ll", true))
            {
                isSignedLong = true;
            }
            else if (TextWindow.AdvanceIfMatches("i", true))
            {
                isComplex = true;
            }

            if (!_options.SyntaxOptions.AcceptBinaryNumbers)
                AddError(ErrorCode.ERR_BinaryNumericLiteralNotSupportedInVersion);
            if (!_options.SyntaxOptions.AcceptUnderscoreInNumberLiterals && hasUnderscores)
                AddError(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion);
            if (!Options.SyntaxOptions.AcceptLuaJITNumberSuffixes && (isUnsignedLong || isSignedLong || isComplex))
                AddError(ErrorCode.ERR_NumberSuffixNotSupportedInVersion);

            if (digits < 1)
            {
                num = 0; // Safe default
                AddError(ErrorCode.ERR_InvalidNumber);
            }
            if (hasOverflown || (num > long.MaxValue && !isUnsignedLong))
            {
                num = 0; // Safe default
                AddError(ErrorCode.ERR_NumericLiteralTooLarge);
            }

            info.Text = TextWindow.GetText(intern: true);
            if (isUnsignedLong)
            {
                info.ValueKind = ValueKind.ULong;
                info.ULongValue = num;
            }
            else if (isSignedLong)
            {
                info.ValueKind = ValueKind.Long;
                info.LongValue = unchecked((long) num);
            }
            else if (isComplex)
            {
                info.ValueKind = ValueKind.Complex;
                info.ComplexValue = new Complex(0, num);
            }
            else
            {
                switch (_options.SyntaxOptions.BinaryIntegerFormat)
                {
                    case IntegerFormats.NotSupported:
                    case IntegerFormats.Double:
                        info.ValueKind = ValueKind.Double;
                        info.DoubleValue = num;
                        break;

                    case IntegerFormats.Int64:
                        info.ValueKind = ValueKind.Long;
                        info.LongValue = unchecked((long) num);
                        break;

                    default:
                        throw ExceptionUtilities.UnexpectedValue(_options.SyntaxOptions.BinaryIntegerFormat);
                }
            }
        }

        private void ParseOctalNumber(ref TokenInfo info)
        {
            var num = 0L;
            var digits = 0;
            var hasUnderscores = false;
            var hasOverflown = false;
            char digit;
            while (CharUtils.IsOctal(digit = TextWindow.PeekChar()) || digit == '_')
            {
                TextWindow.AdvanceChar();
                if (digit == '_')
                {
                    hasUnderscores = true;
                    continue;
                }
                // If any of these bits are set, we'll overflow
                if ((num & 0x7000_0000_0000_0000) != 0)
                    hasOverflown = true;
                num = (num << 3) | CharUtils.DecimalValue(digit);
                digits++;
            }

            if (!_options.SyntaxOptions.AcceptOctalNumbers)
                AddError(ErrorCode.ERR_OctalNumericLiteralNotSupportedInVersion);
            if (!_options.SyntaxOptions.AcceptUnderscoreInNumberLiterals && hasUnderscores)
                AddError(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion);

            if (digits < 1)
            {
                num = 0; // Safe default
                AddError(ErrorCode.ERR_InvalidNumber);
            }
            if (hasOverflown)
            {
                num = 0; // Safe default
                AddError(ErrorCode.ERR_NumericLiteralTooLarge);
            }

            info.Text = TextWindow.GetText(intern: true);
            switch (_options.SyntaxOptions.OctalIntegerFormat)
            {
                case IntegerFormats.NotSupported:
                case IntegerFormats.Double:
                    info.ValueKind = ValueKind.Double;
                    info.DoubleValue = num;
                    break;

                case IntegerFormats.Int64:
                    info.ValueKind = ValueKind.Long;
                    info.LongValue = num;
                    break;

                default:
                    throw ExceptionUtilities.UnexpectedValue(_options.SyntaxOptions.OctalIntegerFormat);
            }
        }

#pragma warning disable IDE0079 // Remove unnecessary suppression
        [SuppressMessage("Usage", "CA2249:Consider using 'string.Contains' instead of 'string.IndexOf'", Justification = "Not available in .NET Standard 2.0")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
        private void ParseDecimalNumber(ref TokenInfo info)
        {
            _builder.Clear();

            var isFloat = false;
            ConsumeDecimalDigits(_builder);
            if (TextWindow.PeekChar() == '.')
            {
                TextWindow.AdvanceChar();
                isFloat = true;
                _builder.Append('.');
                ConsumeDecimalDigits(_builder);
            }

            if (CharUtils.AsciiLowerCase(TextWindow.PeekChar()) == 'e')
            {
                TextWindow.AdvanceChar();
                isFloat = true;
                _builder.Append('e');
                if (TextWindow.PeekChar() is '+' or '-')
                    _builder.Append(TextWindow.NextChar());
                ConsumeDecimalDigits(_builder);
            }

            var (isUnsignedLong, isSignedLong, isComplex) = (false, false, false);

            if (TextWindow.AdvanceIfMatches("ull", true))
            {
                if (isFloat)
                {
                    AddError(ErrorCode.ERR_LuajitSuffixInFloat);
                }
                else
                {
                    isUnsignedLong = true;
                }
            }
            else if (TextWindow.AdvanceIfMatches("ll", true))
            {
                if (isFloat)
                {
                    AddError(ErrorCode.ERR_LuajitSuffixInFloat);
                }
                else
                {
                    isSignedLong = true;
                }
            }
            else if (TextWindow.AdvanceIfMatches("i", true))
            {
                isComplex = true;
            }

            info.Text = TextWindow.GetText(intern: true);
            if (!_options.SyntaxOptions.AcceptUnderscoreInNumberLiterals && info.Text.IndexOf('_') >= 0)
                AddError(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion);
            if (!Options.SyntaxOptions.AcceptLuaJITNumberSuffixes && (isUnsignedLong || isSignedLong || isComplex))
                AddError(ErrorCode.ERR_NumberSuffixNotSupportedInVersion);

            if (isUnsignedLong)
            {
                if (!ulong.TryParse(TextWindow.Intern(_builder), NumberStyles.None, CultureInfo.InvariantCulture, out var result))
                    AddError(ErrorCode.ERR_NumericLiteralTooLarge);

                info.ValueKind = ValueKind.ULong;
                info.ULongValue = result;
            }
            else if (isSignedLong)
            {
                if (!long.TryParse(TextWindow.Intern(_builder), NumberStyles.None, CultureInfo.InvariantCulture, out var result))
                    AddError(ErrorCode.ERR_NumericLiteralTooLarge);

                info.ValueKind = ValueKind.Long;
                info.LongValue = result;
            }
            else if (isComplex)
            {
                if (!RealParser.TryParseDouble(TextWindow.Intern(_builder), out var result))
                    AddError(ErrorCode.ERR_DoubleOverflow);

                info.ValueKind = ValueKind.Complex;
                info.ComplexValue = new Complex(0, result);
            }
            else if (isFloat || _options.SyntaxOptions.DecimalIntegerFormat == IntegerFormats.NotSupported)
            {
                if (!RealParser.TryParseDouble(TextWindow.Intern(_builder), out var result))
                    AddError(ErrorCode.ERR_DoubleOverflow);

                info.ValueKind = ValueKind.Double;
                info.DoubleValue = result;
            }
            else
            {
                if (!long.TryParse(TextWindow.Intern(_builder), NumberStyles.None, CultureInfo.InvariantCulture, out var result))
                    AddError(ErrorCode.ERR_NumericLiteralTooLarge);

                switch (_options.SyntaxOptions.DecimalIntegerFormat)
                {
                    case IntegerFormats.Double:
                        info.ValueKind = ValueKind.Double;
                        info.DoubleValue = result;
                        break;

                    case IntegerFormats.Int64:
                        info.ValueKind = ValueKind.Long;
                        info.LongValue = result;
                        break;

                    default:
                        throw ExceptionUtilities.UnexpectedValue(_options.SyntaxOptions.DecimalIntegerFormat);
                }
            }
        }

#pragma warning disable IDE0079 // Remove unnecessary suppression
        [SuppressMessage("Usage", "CA2249:Consider using 'string.Contains' instead of 'string.IndexOf'", Justification = "Not available in .NET Standard 2.0")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
        private void ParseHexadecimalNumber(ref TokenInfo info)
        {
            _builder.Clear();
            ConsumeHexDigits();
            var isHexFloat = false;
            if (TextWindow.PeekChar() == '.')
            {
                TextWindow.AdvanceChar();
                isHexFloat = true;
                _builder.Append('.');
                ConsumeHexDigits();
            }

            if (CharUtils.AsciiLowerCase(TextWindow.PeekChar()) == 'p')
            {
                TextWindow.AdvanceChar();
                isHexFloat = true;
                _builder.Append('p');
                if (TextWindow.PeekChar() is '+' or '-')
                    _builder.Append(TextWindow.NextChar());
                ConsumeDecimalDigits(_builder);
            }

            var (isUnsignedLong, isSignedLong, isComplex) = (false, false, false);

            if (TextWindow.AdvanceIfMatches("ull", true))
            {
                if (isHexFloat)
                {
                    AddError(ErrorCode.ERR_LuajitSuffixInFloat);
                }
                else
                {
                    isUnsignedLong = true;
                }
            }
            else if (TextWindow.AdvanceIfMatches("ll", true))
            {
                if (isHexFloat)
                {
                    AddError(ErrorCode.ERR_LuajitSuffixInFloat);
                }
                else
                {
                    isSignedLong = true;
                }
            }
            else if (TextWindow.AdvanceIfMatches("i", true))
            {
                isComplex = true;
            }

            info.Text = TextWindow.GetText(intern: true);
            if (!_options.SyntaxOptions.AcceptUnderscoreInNumberLiterals && info.Text.IndexOf('_') >= 0)
                AddError(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion);
            if (!Options.SyntaxOptions.AcceptLuaJITNumberSuffixes && (isUnsignedLong || isSignedLong || isComplex))
                AddError(ErrorCode.ERR_NumberSuffixNotSupportedInVersion);

            if (isUnsignedLong)
            {
                if (!ulong.TryParse(TextWindow.Intern(_builder), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var result))
                    AddError(ErrorCode.ERR_NumericLiteralTooLarge);

                info.ValueKind = ValueKind.ULong;
                info.ULongValue = result;
            }
            else if (isSignedLong)
            {
                if (!long.TryParse(TextWindow.Intern(_builder), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var result))
                    AddError(ErrorCode.ERR_NumericLiteralTooLarge);

                info.ValueKind = ValueKind.Long;
                info.LongValue = result;
            }
            else if (isComplex)
            {
                var result = 0d;
                try
                {
                    result = HexFloat.DoubleFromHexString(TextWindow.Intern(_builder));
                }
                catch (OverflowException)
                {
                    AddError(ErrorCode.ERR_DoubleOverflow);
                }

                info.ValueKind = ValueKind.Complex;
                info.ComplexValue = new Complex(0, result);
            }
            else if (isHexFloat || _options.SyntaxOptions.HexIntegerFormat == IntegerFormats.NotSupported)
            {
                if (!_options.SyntaxOptions.AcceptHexFloatLiterals)
                    AddError(ErrorCode.ERR_HexFloatLiteralNotSupportedInVersion);

                var result = 0d;
                try
                {
                    result = HexFloat.DoubleFromHexString(TextWindow.Intern(_builder));
                }
                catch (OverflowException)
                {
                    AddError(ErrorCode.ERR_DoubleOverflow);
                }
                info.ValueKind = ValueKind.Double;
                info.DoubleValue = result;
            }
            else
            {
                if (!long.TryParse(TextWindow.Intern(_builder), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var result))
                    AddError(ErrorCode.ERR_NumericLiteralTooLarge);
                switch (_options.SyntaxOptions.HexIntegerFormat)
                {
                    case IntegerFormats.Double:
                        info.ValueKind = ValueKind.Double;
                        info.DoubleValue = result;
                        break;

                    case IntegerFormats.Int64:
                        info.ValueKind = ValueKind.Long;
                        info.LongValue = result;
                        break;

                    default:
                        throw ExceptionUtilities.UnexpectedValue(_options.SyntaxOptions.HexIntegerFormat);
                }
            }
        }
    }
}
