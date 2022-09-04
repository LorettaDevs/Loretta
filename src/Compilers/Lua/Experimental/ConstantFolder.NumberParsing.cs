using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Loretta.CodeAnalysis.Lua.Utilities;

namespace Loretta.CodeAnalysis.Lua.Experimental
{
    internal partial class ConstantFolder
    {
        private const RegexOptions RegexFlags = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture;
        private static readonly Regex s_hexIntegerRegex = new(@"^[+\-]?0[xX][\da-fA-F]+$", RegexFlags);
        private static readonly Regex s_hexFloatRegex = new(
            @"[+\-]?0x(\.[\da-fA-F]+|[\da-fA-F]+(\.[\da-fA-F]+)?)([pP][+\-]?\d+)?",
            RegexFlags);
        private static readonly Regex s_decIntegerRegex = new(@"^[+\-]?\d+$", RegexFlags);
        private static readonly Regex s_decFloatRegex = new(
            @"[+\-]?(\.\d+|\d+(\.\d+)?)([eE][+\-]?\d+)?",
            RegexFlags);

        private static bool TryParseNumberInString(
            string value,
            [NotNullWhen(true)] out dynamic? parsedValue)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            long i64;
            double f64;

            value = StringUtils.Trim(value);
            if (s_decIntegerRegex.IsMatch(value)
                && long.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out i64))
            {
                parsedValue = i64;
                return true;
            }
            else if (s_hexIntegerRegex.IsMatch(value)
                && long.TryParse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out i64))
            {
                parsedValue = i64;
                return true;
            }
            else if (s_decFloatRegex.IsMatch(value) && RealParser.TryParseDouble(value, out f64))
            {
                parsedValue = f64;
                return true;
            }
            else if (s_hexFloatRegex.IsMatch(value))
            {
                try
                {
                    parsedValue = HexFloat.DoubleFromHexString(value);
                    return true;
                }
                catch
                {
                    // Nothing
                }
            }

            parsedValue = null;
            return false;
        }
    }
}
