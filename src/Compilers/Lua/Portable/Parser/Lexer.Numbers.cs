using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Loretta.CodeAnalysis.Lua.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal sealed partial class Lexer
    {
        private void SkipDecimalDigits(StringBuilder builder)
        {
            char digit;
            while (CharUtils.IsDecimal(digit = _reader.Peek().GetValueOrDefault()) || digit == '_')
            {
                if (digit != '_')
                    builder.Append(digit);
                _reader.Position += 1;
            }
        }

        private void ParseBinaryNumber(ref TokenInfo info)
        {
            var num = 0L;
            var digits = 0;
            var hasUnderscores = false;
            var hasOverflown = false;
            char digit;
            while (CharUtils.IsInRange('0', digit = _reader.Peek().GetValueOrDefault(), '1') || digit == '_')
            {
                _reader.Position += 1;
                if (digit == '_')
                {
                    hasUnderscores = true;
                    continue;
                }
                // Next shift will overflow if 63rd bit is set
                if ((num & 0x4000000000000000) != 0)
                    hasOverflown = true;
                num = (num << 1) | (digit - (long) '0');
                digits++;
            }

            if (!Options.SyntaxOptions.AcceptBinaryNumbers)
                AddError(ErrorCode.ERR_BinaryNumericLiteralNotSupportedInVersion);
            if (!Options.SyntaxOptions.AcceptUnderscoreInNumberLiterals && hasUnderscores)
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

            info.Text = GetText(intern: true);
            switch (Options.SyntaxOptions.BinaryIntegerFormat)
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
                    throw ExceptionUtilities.UnexpectedValue(Options.SyntaxOptions.BinaryIntegerFormat);
            }
        }

        private void ParseOctalNumber(ref TokenInfo info)
        {
            var num = 0L;
            var digits = 0;
            var hasUnderscores = false;
            var hasOverflown = false;
            char digit;
            while (CharUtils.IsInRange('0', digit = _reader.Peek().GetValueOrDefault(), '7') || digit == '_')
            {
                _reader.Position += 1;
                if (digit == '_')
                {
                    hasUnderscores = true;
                    continue;
                }
                // If any of these bits are set, we'll overflow
                if ((num & 0x7000000000000000) != 0)
                    hasOverflown = true;
                num = (num << 3) | (digit - (long) '0');
                digits++;
            }

            if (!Options.SyntaxOptions.AcceptOctalNumbers)
                AddError(ErrorCode.ERR_OctalNumericLiteralNotSupportedInVersion);
            if (!Options.SyntaxOptions.AcceptUnderscoreInNumberLiterals && hasUnderscores)
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

            info.Text = GetText(intern: true);
            switch (Options.SyntaxOptions.OctalIntegerFormat)
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
                    throw ExceptionUtilities.UnexpectedValue(Options.SyntaxOptions.OctalIntegerFormat);
            }
        }

#pragma warning disable IDE0079 // Remove unnecessary suppression
        [SuppressMessage("Usage", "CA2249:Consider using 'string.Contains' instead of 'string.IndexOf'", Justification = "Not available in .NET Standard 2.0")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
        private void ParseDecimalNumber(ref TokenInfo info)
        {
            _builder.Clear();

            var isFloat = false;
            SkipDecimalDigits(_builder);
            if (_reader.IsNext('.'))
            {
                isFloat = true;
                _builder.Append(_reader.Read()!.Value);
                SkipDecimalDigits(_builder);
            }

            if (CharUtils.AsciiLowerCase(_reader.Peek().GetValueOrDefault()) == 'e')
            {
                isFloat = true;
                _builder.Append(_reader.Read()!.Value);
                if (_reader.IsNext('+') || _reader.IsNext('-'))
                    _builder.Append(_reader.Read()!.Value);
                SkipDecimalDigits(_builder);
            }

            info.Text = GetText(intern: true);
            if (!Options.SyntaxOptions.AcceptUnderscoreInNumberLiterals && info.Text.IndexOf('_') >= 0)
                AddError(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion);
            if (isFloat || Options.SyntaxOptions.DecimalIntegerFormat == IntegerFormats.NotSupported)
            {
                if (!RealParser.TryParseDouble(Intern(_builder), out var result))
                    AddError(ErrorCode.ERR_DoubleOverflow);
                info.ValueKind = ValueKind.Double;
                info.DoubleValue = result;
            }
            else
            {
                if (!long.TryParse(Intern(_builder), NumberStyles.None, CultureInfo.InvariantCulture, out var result))
                    AddError(ErrorCode.ERR_NumericLiteralTooLarge);

                switch (Options.SyntaxOptions.DecimalIntegerFormat)
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
                        throw ExceptionUtilities.UnexpectedValue(Options.SyntaxOptions.DecimalIntegerFormat);
                }
            }
        }

#pragma warning disable IDE0079 // Remove unnecessary suppression
        [SuppressMessage("Usage", "CA2249:Consider using 'string.Contains' instead of 'string.IndexOf'", Justification = "Not available in .NET Standard 2.0")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
        private void ParseHexadecimalNumber(ref TokenInfo info)
        {
            _builder.Clear();
            skipHexDigits();
            var isHexFloat = false;
            if (_reader.IsNext('.'))
            {
                isHexFloat = true;
                _builder.Append(_reader.Read()!.Value);
                skipHexDigits();
            }

            if (CharUtils.AsciiLowerCase(_reader.Peek().GetValueOrDefault()) == 'p')
            {
                isHexFloat = true;
                _builder.Append(_reader.Read()!.Value);
                if (_reader.IsNext('+') || _reader.IsNext('-'))
                    _builder.Append(_reader.Read()!.Value);
                SkipDecimalDigits(_builder);
            }

            info.Text = GetText(intern: true);
            if (!Options.SyntaxOptions.AcceptUnderscoreInNumberLiterals && info.Text.IndexOf('_') >= 0)
                AddError(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion);

            if (isHexFloat || Options.SyntaxOptions.HexIntegerFormat == IntegerFormats.NotSupported)
            {
                if (!Options.SyntaxOptions.AcceptHexFloatLiterals)
                    AddError(ErrorCode.ERR_HexFloatLiteralNotSupportedInVersion);

                var result = 0d;
                try
                {
                    result = HexFloat.DoubleFromHexString(Intern(_builder));
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
                if (!long.TryParse(Intern(_builder), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var result))
                    AddError(ErrorCode.ERR_NumericLiteralTooLarge);
                switch (Options.SyntaxOptions.HexIntegerFormat)
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
                        throw ExceptionUtilities.UnexpectedValue(Options.SyntaxOptions.HexIntegerFormat);
                }
            }

            void skipHexDigits()
            {
                char digit;
                while (CharUtils.IsHexadecimal(digit = _reader.Peek().GetValueOrDefault()) || digit == '_')
                {
                    if (digit != '_')
                        _builder.Append(digit);
                    _reader.Position += 1;
                }
            }
        }
    }
}
