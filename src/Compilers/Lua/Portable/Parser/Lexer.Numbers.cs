using System;
using System.Globalization;
using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal sealed partial class Lexer
    {
        private void SkipDecimalDigits()
        {
            char digit;
            while (CharUtils.IsDecimal(digit = _reader.Peek().GetValueOrDefault()) || digit == '_')
                _reader.Advance(1);
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

        private double ParseDecimalNumber()
        {
            SkipDecimalDigits();
            if (_reader.IsNext('.'))
            {
                _reader.Advance(1);
                SkipDecimalDigits();
                if (CharUtils.AsciiLowerCase(_reader.Peek().GetValueOrDefault()) == 'e')
                {
                    _reader.Advance(1);
                    if (_reader.IsNext('+') || _reader.IsNext('-'))
                        _reader.Advance(1);
                    SkipDecimalDigits();
                }
            }

            var numEnd = _reader.Position;
            var numLength = numEnd - _start;

            _reader.Restore(_start);
            if (numLength < 255)
            {
                var rawNum = _reader.ReadSpan(numLength);
                Span<char> buff = stackalloc char[numLength];

                if (!Options.SyntaxOptions.AcceptUnderscoreInNumberLiterals && rawNum.IndexOf('_') >= 0)
                    AddError(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion);

                var buffIdx = 0;
                for (var rawNumIdx = 0; rawNumIdx < numLength; rawNumIdx++)
                {
                    var ch = rawNum[rawNumIdx];
                    if (ch == '_') continue;
                    buff[buffIdx] = ch;
                    buffIdx++;
                }

                return double.Parse(
                    buff.Slice(0, buffIdx),
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
                    CultureInfo.InvariantCulture);
            }
            else
            {
                var rawNum = _reader.ReadString(numLength)!;

                if (!Options.SyntaxOptions.AcceptUnderscoreInNumberLiterals && rawNum.Contains('_'))
                    AddError(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion);

                return double.Parse(
                    rawNum.Replace("_", ""),
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
                    CultureInfo.InvariantCulture);
            }
        }

        private double ParseHexadecimalNumber()
        {
            var numStart = _reader.Position;

            skipHexDigits();
            var isHexFloat = false;
            if (_reader.IsNext('.'))
            {
                isHexFloat = true;
                _reader.Advance(1);
                skipHexDigits();
            }

            if (CharUtils.AsciiLowerCase(_reader.Peek().GetValueOrDefault()) == 'p')
            {
                isHexFloat = true;
                _reader.Advance(1);
                if (_reader.IsNext('+') || _reader.IsNext('-'))
                    _reader.Advance(1);
                SkipDecimalDigits();
            }

            if (isHexFloat && !Options.SyntaxOptions.AcceptHexFloatLiterals)
                AddError(ErrorCode.ERR_HexFloatLiteralNotSupportedInVersion);

            var numEnd = _reader.Position;
            var numLength = numEnd - numStart;

            _reader.Restore(numStart);
            if (numLength < 255)
            {
                var rawNum = _reader.ReadSpan(numLength);
                Span<char> buff = stackalloc char[numLength];

                if (!Options.SyntaxOptions.AcceptUnderscoreInNumberLiterals && rawNum.IndexOf('_') >= 0)
                    AddError(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion);

                var buffIdx = 0;
                for (var rawNumIdx = 0; rawNumIdx < numLength; rawNumIdx++)
                {
                    var ch = rawNum[rawNumIdx];
                    if (ch == '_') continue;
                    buff[buffIdx] = ch;
                    buffIdx++;
                }

                return HexFloat.DoubleFromHexString(buff.Slice(0, buffIdx));
            }
            else
            {
                var rawNum = _reader.ReadString(numLength)!;

                if (!Options.SyntaxOptions.AcceptUnderscoreInNumberLiterals && rawNum.Contains('_'))
                    AddError(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion);

                return HexFloat.DoubleFromHexString(rawNum.Replace("_", ""));
            }

            void skipHexDigits()
            {
                char digit;
                while (CharUtils.IsHexadecimal(digit = _reader.Peek().GetValueOrDefault()) || digit == '_')
                    _reader.Advance(1);
            }
        }
    }
}
