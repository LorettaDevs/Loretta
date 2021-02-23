using System;
using System.Globalization;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.Syntax
{
    internal sealed partial class Lexer
    {
        private void SkipDecimalDigits()
        {
            char digit;
            while (CharUtils.IsDecimal(digit = this._reader.Peek().GetValueOrDefault()) || digit == '_')
                this._reader.Advance(1);
        }

        private double ParseBinaryNumber()
        {
            // Skip leading 0s
            while (this._reader.Peek().GetValueOrDefault() == '0')
                this._reader.Advance(1);

            var num = 0L;
            var digits = 0;
            var hasUnderscores = false;
            char digit;
            while (CharUtils.IsInRange('0', digit = this._reader.Peek().GetValueOrDefault(), '1') || digit == '_')
            {
                this._reader.Advance(1);
                if (digit == '_')
                {
                    hasUnderscores = true;
                    continue;
                }
                num = (num << 1) | (digit - (long) '0');
                digits++;
            }

            var span = TextSpan.FromBounds(this._start, this._reader.Position);
            var location = new TextLocation(this._text, span);
            if (!this._luaOptions.AcceptBinaryNumbers)
                this.Diagnostics.ReportBinaryLiteralNotSupportedInVersion(location);
            if (!this._luaOptions.AcceptUnderscoreInNumberLiterals && hasUnderscores)
                this.Diagnostics.ReportUnderscoreInNumberLiteralNotSupportedInVersion(location);
            if (digits < 1)
            {
                num = 0; // Safe default
                this.Diagnostics.ReportInvalidNumber(location);
            }
            if (digits > 64)
            {
                num = 0; // Safe default
                this.Diagnostics.ReportNumericLiteralTooLarge(location);
            }

            return num;
        }

        private double ParseOctalNumber()
        {
            // Skip leading 0s
            while (this._reader.Peek().GetValueOrDefault() == '0')
                this._reader.Advance(1);

            var num = 0L;
            var digits = 0;
            var hasUnderscores = false;
            char digit;
            while (CharUtils.IsInRange('0', digit = this._reader.Peek().GetValueOrDefault(), '7') || digit == '_')
            {
                this._reader.Advance(1);
                if (digit == '_')
                {
                    hasUnderscores = true;
                    continue;
                }
                num = (num << 3) | (digit - (long) '0');
                digits++;
            }

            var span = TextSpan.FromBounds(this._start, this._reader.Position);
            var location = new TextLocation(this._text, span);
            if (!this._luaOptions.AcceptOctalNumbers)
                this.Diagnostics.ReportOctalLiteralNotSupportedInVersion(location);
            if (!this._luaOptions.AcceptUnderscoreInNumberLiterals && hasUnderscores)
                this.Diagnostics.ReportUnderscoreInNumberLiteralNotSupportedInVersion(location);
            if (digits < 1)
            {
                num = 0; // Safe default
                this.Diagnostics.ReportInvalidNumber(location);
            }
            if (digits > 21)
            {
                num = 0; // Safe default
                this.Diagnostics.ReportNumericLiteralTooLarge(location);
            }

            return num;
        }

        private double ParseDecimalNumber()
        {
            SkipDecimalDigits();
            if (this._reader.IsNext('.'))
            {
                this._reader.Advance(1);
                SkipDecimalDigits();
                if (CharUtils.AsciiLowerCase(this._reader.Peek().GetValueOrDefault()) == 'e')
                {
                    this._reader.Advance(1);
                    if (this._reader.IsNext('+') || this._reader.IsNext('-'))
                        this._reader.Advance(1);
                    SkipDecimalDigits();
                }
            }

            var numEnd = this._reader.Position;
            var numLength = numEnd - this._start;

            this._reader.Restore(this._start);
            if (numLength < 255)
            {
                ReadOnlySpan<char> rawNum = this._reader.ReadSpan(numLength);
                Span<char> buff = stackalloc char[numLength];

                if (!this._luaOptions.AcceptUnderscoreInNumberLiterals
                     && rawNum.IndexOf('_') >= 0)
                {
                    var span = TextSpan.FromBounds(this._start, this._reader.Position);
                    var location = new TextLocation(this._text, span);
                    this.Diagnostics.ReportUnderscoreInNumberLiteralNotSupportedInVersion(location);
                }

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
                var rawNum = this._reader.ReadString(numLength)!;

                if (!this._luaOptions.AcceptUnderscoreInNumberLiterals
                     && rawNum.Contains('_'))
                {
                    var span = TextSpan.FromBounds(this._start, this._reader.Position);
                    var location = new TextLocation(this._text, span);
                    this.Diagnostics.ReportUnderscoreInNumberLiteralNotSupportedInVersion(location);
                }

                return double.Parse(
                    rawNum.Replace("_", ""),
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
                    CultureInfo.InvariantCulture);
            }
        }

        private double ParseHexadecimalNumber()
        {
            var numStart = this._reader.Position;

            skipHexDigits();
            var isHexFloat = false;
            if (this._reader.IsNext('.'))
            {
                isHexFloat = true;
                this._reader.Advance(1);
                skipHexDigits();
            }

            if (CharUtils.AsciiLowerCase(this._reader.Peek().GetValueOrDefault()) == 'p')
            {
                isHexFloat = true;
                this._reader.Advance(1);
                if (this._reader.IsNext('+') || this._reader.IsNext('-'))
                    this._reader.Advance(1);
                SkipDecimalDigits();
            }

            if (isHexFloat && !this._luaOptions.AcceptHexFloatLiterals)
            {
                var span = TextSpan.FromBounds(this._start, this._reader.Position);
                var location = new TextLocation(this._text, span);
                this.Diagnostics.ReportHexFloatLiteralNotSupportedInVersion(location);
            }

            var numEnd = this._reader.Position;
            var numLength = numEnd - numStart;

            this._reader.Restore(numStart);
            if (numLength < 255)
            {
                ReadOnlySpan<char> rawNum = this._reader.ReadSpan(numLength);
                Span<char> buff = stackalloc char[numLength];

                if (!this._luaOptions.AcceptUnderscoreInNumberLiterals
                     && rawNum.IndexOf('_') >= 0)
                {
                    var span = TextSpan.FromBounds(this._start, this._reader.Position);
                    var location = new TextLocation(this._text, span);
                    this.Diagnostics.ReportUnderscoreInNumberLiteralNotSupportedInVersion(location);
                }

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
                var rawNum = this._reader.ReadString(numLength)!;

                if (!this._luaOptions.AcceptUnderscoreInNumberLiterals
                     && rawNum.Contains('_'))
                {
                    var span = TextSpan.FromBounds(this._start, this._reader.Position);
                    var location = new TextLocation(this._text, span);
                    this.Diagnostics.ReportUnderscoreInNumberLiteralNotSupportedInVersion(location);
                }

                return HexFloat.DoubleFromHexString(rawNum.Replace("_", ""));
            }

            void skipHexDigits()
            {
                char digit;
                while (CharUtils.IsHexadecimal(digit = this._reader.Peek().GetValueOrDefault()) || digit == '_')
                    this._reader.Advance(1);
            }
        }
    }
}
