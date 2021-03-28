using System;
using System.Diagnostics.CodeAnalysis;
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
                _reader.Advance(1);
            }
        }

        private double ParseBinaryNumber()
        {
            // Skip leading 0s
            while (_reader.Peek().GetValueOrDefault() == '0')
                _reader.Advance(1);

            var num = 0L;
            var digits = 0;
            var hasUnderscores = false;
            char digit;
            while (CharUtils.IsInRange('0', digit = _reader.Peek().GetValueOrDefault(), '1') || digit == '_')
            {
                _reader.Advance(1);
                if (digit == '_')
                {
                    hasUnderscores = true;
                    continue;
                }
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
            if (digits > 64)
            {
                num = 0; // Safe default
                AddError(ErrorCode.ERR_NumericLiteralTooLarge);
            }

            return num;
        }

        private double ParseOctalNumber()
        {
            // Skip leading 0s
            while (_reader.Peek().GetValueOrDefault() == '0')
                _reader.Advance(1);

            var num = 0L;
            var digits = 0;
            var hasUnderscores = false;
            char digit;
            while (CharUtils.IsInRange('0', digit = _reader.Peek().GetValueOrDefault(), '7') || digit == '_')
            {
                _reader.Advance(1);
                if (digit == '_')
                {
                    hasUnderscores = true;
                    continue;
                }
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
            if (digits > 21)
            {
                num = 0; // Safe default
                AddError(ErrorCode.ERR_NumericLiteralTooLarge);
            }

            return num;
        }

        [SuppressMessage("Usage", "CA2249:Consider using 'string.Contains' instead of 'string.IndexOf'", Justification = "Not available in .NET Standard 2.0")]
        private void ParseDecimalNumber(ref TokenInfo info)
        {
            _builder.Clear();

            SkipDecimalDigits(_builder);
            if (_reader.IsNext('.'))
            {
                _builder.Append(_reader.Read()!.Value);
                SkipDecimalDigits(_builder);
            }

            if (CharUtils.AsciiLowerCase(_reader.Peek().GetValueOrDefault()) == 'e')
            {
                _builder.Append(_reader.Read()!.Value);
                if (_reader.IsNext('+') || _reader.IsNext('-'))
                    _builder.Append(_reader.Read()!.Value);
                SkipDecimalDigits(_builder);
            }

            info.Text = GetText(intern: true);
            if (!Options.SyntaxOptions.AcceptUnderscoreInNumberLiterals && info.Text.IndexOf('_') >= 0)
                AddError(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion);
            if (!RealParser.TryParseDouble(Intern(_builder), out var result))
                AddError(ErrorCode.ERR_DoubleOverflow);
            info.DoubleValue = result;
        }

        [SuppressMessage("Usage", "CA2249:Consider using 'string.Contains' instead of 'string.IndexOf'", Justification = "Not available in .NET Standard 2.0")]
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

            if (isHexFloat && !Options.SyntaxOptions.AcceptHexFloatLiterals)
                AddError(ErrorCode.ERR_HexFloatLiteralNotSupportedInVersion);

            info.Text = GetText(intern: true);
            if (!Options.SyntaxOptions.AcceptUnderscoreInNumberLiterals && info.Text.IndexOf('_') >= 0)
                AddError(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion);

            var result = 0d;
            try
            {
                result = HexFloat.DoubleFromHexString(Intern(_builder));
            }
            catch (OverflowException)
            {
                AddError(ErrorCode.ERR_DoubleOverflow);
            }
            info.DoubleValue = result;

            void skipHexDigits()
            {
                char digit;
                while (CharUtils.IsHexadecimal(digit = _reader.Peek().GetValueOrDefault()) || digit == '_')
                {
                    if (digit != '_')
                        _builder.Append(digit);
                    _reader.Advance(1);
                }
            }
        }
    }
}
