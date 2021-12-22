using Loretta.CodeAnalysis.Lua.Syntax.UnitTests.Parsing;
using Loretta.CodeAnalysis.Lua.Test.Utilities;
using Loretta.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.Syntax.UnitTests.Lexical
{
    public class LexicalErrorTests : LuaTestBase
    {
        private static void ParseAndValidate(string text, LuaSyntaxOptions? options = null, params DiagnosticDescription[] expectedErrors) =>
            ParsingTests.ParseAndValidate(text, options, expectedErrors);

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticsOn_InvalidEscapes()
        {
            const string source = @"
local str = ""some\ltext""
local str = 'some\ltext'
local str = ""some\xGtext""
local str = 'some\xGtext'
local str = ""some\300text""
local str = 'some\300text'
";
            ParseAndValidate(source, null,
                // (2,18): error LUA0001: Invalid string escape
                // local str = "some\ltext"
                Diagnostic(ErrorCode.ERR_InvalidStringEscape, @"\l").WithLocation(2, 18),
                // (3,18): error LUA0001: Invalid string escape
                // local str = 'some\ltext'
                Diagnostic(ErrorCode.ERR_InvalidStringEscape, @"\l").WithLocation(3, 18),
                // (4,18): error LUA0001: Invalid string escape
                // local str = "some\xGtext"
                Diagnostic(ErrorCode.ERR_InvalidStringEscape, @"\x").WithLocation(4, 18),
                // (5,18): error LUA0001: Invalid string escape
                // local str = 'some\xGtext'
                Diagnostic(ErrorCode.ERR_InvalidStringEscape, @"\x").WithLocation(5, 18),
                // (6,18): error LUA0001: Invalid string escape
                // local str = "some\300text"
                Diagnostic(ErrorCode.ERR_InvalidStringEscape, @"\300").WithLocation(6, 18),
                // (7,18): error LUA0001: Invalid string escape
                // local str = 'some\300text'
                Diagnostic(ErrorCode.ERR_InvalidStringEscape, @"\300").WithLocation(7, 18));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]

        public void Lexer_EmitsDiagnosticsOn_StringWithLineBreak()
        {
            const string source = @"
local str1 = ""some" + "\n" + @"text""
local str2 = 'some" + "\n" + @"text'
local str3 = ""some" + "\r" + @"text""
local str4 = 'some" + "\r" + @"text'
local str5 = ""some" + "\r\n" + @"text""
local str6 = 'some" + "\r\n" + @"text'
";
            ParseAndValidate(source, null,
                // (2,19): error LUA0002: Unescaped line break in string
                // local str1 = "some\ntext"
                Diagnostic(ErrorCode.ERR_UnescapedLineBreakInString, "\n").WithLocation(2, 19),
                // (4,19): error LUA0002: Unescaped line break in string
                // local str2 = 'some\ntext'
                Diagnostic(ErrorCode.ERR_UnescapedLineBreakInString, "\n").WithLocation(4, 19),
                // (6,19): error LUA0002: Unescaped line break in string
                // local str3 = "some\rtext"
                Diagnostic(ErrorCode.ERR_UnescapedLineBreakInString, "\r").WithLocation(6, 19),
                // (8,19): error LUA0002: Unescaped line break in string
                // local str4 = 'some\rtext'
                Diagnostic(ErrorCode.ERR_UnescapedLineBreakInString, "\r").WithLocation(8, 19),
                // (10,19): error LUA0002: Unescaped line break in string
                // local str5 = "some\r\ntext"
                Diagnostic(ErrorCode.ERR_UnescapedLineBreakInString, "\r\n").WithLocation(10, 19),
                // (12,19): error LUA0002: Unescaped line break in string
                // local str6 = 'some\r\ntext'
                Diagnostic(ErrorCode.ERR_UnescapedLineBreakInString, "\r\n").WithLocation(12, 19));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [InlineData("\"text")]
        [InlineData("'text")]
        [InlineData("\"text'")]
        [InlineData("'text\"")]
        public void Lexer_EmitsDiagnosticsOn_UnterminatedShortString(string text)
        {
            var srcText = "local str = " + text;
            ParseAndValidate(srcText, null,
                Diagnostic(ErrorCode.ERR_UnfinishedString, text).WithLocation(1, 13));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticsOn_InvalidNumbers()
        {
            const string srcText = @"
local num1 = 0b
local num2 = 0b_
local num3 = 0o
local num4 = 0o_
";
            ParseAndValidate(srcText, null,
                // (2,14): error LUA0004: Invalid number
                // local num1 = 0b
                Diagnostic(ErrorCode.ERR_InvalidNumber, "0b").WithLocation(2, 14),
                // (3,14): error LUA0004: Invalid number
                // local num2 = 0b_
                Diagnostic(ErrorCode.ERR_InvalidNumber, "0b_").WithLocation(3, 14),
                // (4,14): error LUA0004: Invalid number
                // local num3 = 0o
                Diagnostic(ErrorCode.ERR_InvalidNumber, "0o").WithLocation(4, 14),
                // (5,14): error LUA0004: Invalid number
                // local num4 = 0o_
                Diagnostic(ErrorCode.ERR_InvalidNumber, "0o_").WithLocation(5, 14));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticsOn_LargeNumbersAndOverflows()
        {
            const string srcText = @"
local num1 = 0b10000000000000000000000000000000000000000000000000000000000000000
local num2 = 0o1000000000000000000000
local num3 = 1e999999
local num4 = 0x1p999999
";
            ParseAndValidate(srcText, null,
                // (2,14): error LUA0005: Numeric literal is too large
                // local num1 = 0b10000000000000000000000000000000000000000000000000000000000000000
                Diagnostic(ErrorCode.ERR_NumericLiteralTooLarge, "0b10000000000000000000000000000000000000000000000000000000000000000").WithLocation(2, 14),
                // (3,14): error LUA0005: Numeric literal is too large
                // local num2 = 0o1000000000000000000000
                Diagnostic(ErrorCode.ERR_NumericLiteralTooLarge, "0o1000000000000000000000").WithLocation(3, 14),
                // (4,14): error LUA0020: Constant represents a value either too large or too small for a double precision floating-point number.
                // local num3 = 1e999999
                Diagnostic(ErrorCode.ERR_DoubleOverflow, "1e999999").WithLocation(4, 14),
                // (5,14): error LUA0020: Constant represents a value either too large or too small for a double precision floating-point number.
                // local num4 = 0x1p999999
                Diagnostic(ErrorCode.ERR_DoubleOverflow, "0x1p999999").WithLocation(5, 14));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [InlineData("/* hi")]
        [InlineData("--[[ hi")]
        [InlineData("--[=[ hi")]
        public void Lexer_EmitsDiagnosticOn_UnfinishedLongComment(string text)
        {
            ParseAndValidate(text, null,
                // (1,1): error LUA0006: Unfinished multi-line comment
                // ...
                Diagnostic(ErrorCode.ERR_UnfinishedLongComment, text).WithLocation(1, 1));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticWhen_ShebangIsFound_And_LuaSyntaxOptionsAcceptShebangIsFalse()
        {
            const string srcText = "#!/bin/bash";
            ParseAndValidate(srcText, LuaSyntaxOptions.All.With(acceptShebang: false),
                // (1,1): error LUA0007: Shebangs are not supported in this lua version
                // #!/bin/bash
                Diagnostic(ErrorCode.ERR_ShebangNotSupportedInLuaVersion, srcText).WithLocation(1, 1));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticWhen_BinaryNumberIsFound_And_LuaSyntaxOptionsAcceptBinaryNumbersIsFalse()
        {
            const string srcText = "local num = 0b1010";
            ParseAndValidate(srcText, LuaSyntaxOptions.All.With(acceptBinaryNumbers: false),
                // (1,13): error LUA0008: Binary numeric literals are not supported in this lua version
                // local num = 0b1010
                Diagnostic(ErrorCode.ERR_BinaryNumericLiteralNotSupportedInVersion, "0b1010").WithLocation(1, 13));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticWhen_OctalNumberIsFound_And_LuaSyntaxOptionsAcceptOctalNumbersIsFalse()
        {
            const string srcText = "local num = 0o77";
            ParseAndValidate(srcText, LuaSyntaxOptions.All.With(acceptOctalNumbers: false),
                // (1,13): error LUA0009: Octal numeric literals are not supported in this lua version
                // local num = 0o77
                Diagnostic(ErrorCode.ERR_OctalNumericLiteralNotSupportedInVersion, "0o77").WithLocation(1, 13));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticWhen_HexFloatIsFound_And_LuaSyntaxOptionsAcceptHexFloatIsFalse()
        {
            const string srcText = @"
local num1 = 0xff.ff
local num2 = 0xffp10
local num3 = 0xff.ffp10
";
            ParseAndValidate(srcText, LuaSyntaxOptions.All.With(acceptHexFloatLiterals: false),
                // (2,14): error LUA0010: Hexadecimal floating point numeric literals are not supported in this lua version
                // local num1 = 0xff.ff
                Diagnostic(ErrorCode.ERR_HexFloatLiteralNotSupportedInVersion, "0xff.ff").WithLocation(2, 14),
                // (3,14): error LUA0010: Hexadecimal floating point numeric literals are not supported in this lua version
                // local num2 = 0xffp10
                Diagnostic(ErrorCode.ERR_HexFloatLiteralNotSupportedInVersion, "0xffp10").WithLocation(3, 14),
                // (4,14): error LUA0010: Hexadecimal floating point numeric literals are not supported in this lua version
                // local num3 = 0xff.ffp10
                Diagnostic(ErrorCode.ERR_HexFloatLiteralNotSupportedInVersion, "0xff.ffp10").WithLocation(4, 14));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticWhen_UnderscoreInNumberIsFound_And_LuaSyntaxOptionsAcceptUnderscoresInNumbersIsFalse()
        {
            const string srcText = @"
local num1 = 0b1010_1010
local num2 = 0o7070_7070
local num3 = 10_10.10_10
local num4 = 0xf_f
";
            ParseAndValidate(srcText, LuaSyntaxOptions.All.With(acceptUnderscoreInNumberLiterals: false),
                // (2,14): error LUA0011: Underscores in numeric literals are not supported in this lua version
                // local num1 = 0b1010_1010
                Diagnostic(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion, "0b1010_1010").WithLocation(2, 14),
                // (3,14): error LUA0011: Underscores in numeric literals are not supported in this lua version
                // local num2 = 0o7070_7070
                Diagnostic(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion, "0o7070_7070").WithLocation(3, 14),
                // (4,14): error LUA0011: Underscores in numeric literals are not supported in this lua version
                // local num3 = 10_10.10_10
                Diagnostic(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion, "10_10.10_10").WithLocation(4, 14),
                // (5,14): error LUA0011: Underscores in numeric literals are not supported in this lua version
                // local num4 = 0xf_f
                Diagnostic(ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion, "0xf_f").WithLocation(5, 14));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticWhen_CCommentIsFound_And_LuaSyntaxOptionsAcceptCCommentsIsFalse()
        {
            const string source = @"// hi
/* hi */";
            ParseAndValidate(source, LuaSyntaxOptions.All.With(acceptCCommentSyntax: false),
                // (1,1): error LUA0012: C comments are not supported in this lua version
                // // hi
                Diagnostic(ErrorCode.ERR_CCommentsNotSupportedInVersion, "// hi").WithLocation(1, 1),
                // (2,1): error LUA0012: C comments are not supported in this lua version
                // /* hi */
                Diagnostic(ErrorCode.ERR_CCommentsNotSupportedInVersion, "/* hi */").WithLocation(2, 1));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticWhen_IdentifiersWithCharactersAbove0x7FAreFound_And_LuaSyntaxOptionsUseLuajitIdentifierRulesIsFalse()
        {
            const string srcText = "local 🅱 = 1\r\n"
                                   + "local \ufeff = 1 -- ZERO WIDTH NO-BREAK SPACE\r\n"
                                   + "local \u206b = 1 -- ACTIVATE SYMMETRIC SWAPPING\r\n"
                                   + "local \u202a = 1 -- LEFT-TO-RIGHT EMBEDDING\r\n"
                                   + "local \u206a = 1 -- INHIBIT SYMMETRIC SWAPPING\r\n"
                                   + "local \u200e = 1 -- LEFT-TO-RIGHT MARK\r\n"
                                   + "local \u200c = 1 -- ZERO WIDTH NON-JOINER";
            ParseAndValidate(srcText, LuaSyntaxOptions.All.With(useLuaJitIdentifierRules: false),
                // (1,7): error LUA0013: Identifiers containing characters with value above 0x7F are not supported in this lua version
                // local 🅱 = 1
                Diagnostic(ErrorCode.ERR_LuajitIdentifierRulesNotSupportedInVersion, "🅱").WithLocation(1, 7),
                // (2,7): error LUA0013: Identifiers containing characters with value above 0x7F are not supported in this lua version
                // local ? = 1 -- ZERO WIDTH NO-BREAK SPACE
                Diagnostic(ErrorCode.ERR_LuajitIdentifierRulesNotSupportedInVersion, "\ufeff").WithLocation(2, 7),
                // (3,7): error LUA0013: Identifiers containing characters with value above 0x7F are not supported in this lua version
                // local ? = 1 -- ACTIVATE SYMMETRIC SWAPPING
                Diagnostic(ErrorCode.ERR_LuajitIdentifierRulesNotSupportedInVersion, "\u206b").WithLocation(3, 7),
                // (4,7): error LUA0013: Identifiers containing characters with value above 0x7F are not supported in this lua version
                // local ? = 1 -- LEFT-TO-RIGHT EMBEDDING
                Diagnostic(ErrorCode.ERR_LuajitIdentifierRulesNotSupportedInVersion, "\u202a").WithLocation(4, 7),
                // (5,7): error LUA0013: Identifiers containing characters with value above 0x7F are not supported in this lua version
                // local ? = 1 -- INHIBIT SYMMETRIC SWAPPING
                Diagnostic(ErrorCode.ERR_LuajitIdentifierRulesNotSupportedInVersion, "\u206a").WithLocation(5, 7),
                // (6,7): error LUA0013: Identifiers containing characters with value above 0x7F are not supported in this lua version
                // local ? = 1 -- LEFT-TO-RIGHT MARK
                Diagnostic(ErrorCode.ERR_LuajitIdentifierRulesNotSupportedInVersion, "\u200e").WithLocation(6, 7),
                // (7,7): error LUA0013: Identifiers containing characters with value above 0x7F are not supported in this lua version
                // local ? = 1 -- ZERO WIDTH NON-JOINER
                Diagnostic(ErrorCode.ERR_LuajitIdentifierRulesNotSupportedInVersion, "\u200c").WithLocation(7, 7));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticWhen_BadCharactersAreFound()
        {
            const string source = @"$\?";
            ParseAndValidate(source, null,
                // (1,1): error LUA1012: Invalid statement
                // $\?
                Diagnostic(ErrorCode.ERR_InvalidStatement, "$").WithLocation(1, 1),
                // (1,1): error LUA0014: Bad character input: '$'
                // $\?
                Diagnostic(ErrorCode.ERR_BadCharacter, "$").WithArguments("$").WithLocation(1, 1),
                // (1,2): error LUA1012: Invalid statement
                // $\?
                Diagnostic(ErrorCode.ERR_InvalidStatement, @"\").WithLocation(1, 2),
                // (1,2): error LUA0014: Bad character input: '\'
                // $\?
                Diagnostic(ErrorCode.ERR_BadCharacter, @"\").WithArguments("\\").WithLocation(1, 2),
                // (1,3): error LUA1012: Invalid statement
                // $\?
                Diagnostic(ErrorCode.ERR_InvalidStatement, "").WithLocation(1, 3),
                // (1,3): error LUA0014: Bad character input: '?'
                // $\?
                Diagnostic(ErrorCode.ERR_BadCharacter, "?").WithArguments("?").WithLocation(1, 3));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticsWhen_HexEscapesAreFound_And_LuaSyntaxOptionsAcceptHexEscapesIsFalse()
        {
            const string srcText = @"
local str1 = ""hello\xAthere""
local str2 = 'hello\xAthere'
local str3 = ""hello\xFFthere""
local str4 = 'hello\xFFthere'
";
            var options = LuaSyntaxOptions.All.With(acceptHexEscapesInStrings: false);
            ParseAndValidate(srcText, options,
                // (2,20): error LUA0016: Hexadecimal string escapes are not supported in this lua version
                // local str1 = "hello\xAthere"
                Diagnostic(ErrorCode.ERR_HexStringEscapesNotSupportedInVersion, @"\xA").WithLocation(2, 20),
                // (3,20): error LUA0016: Hexadecimal string escapes are not supported in this lua version
                // local str2 = 'hello\xAthere'
                Diagnostic(ErrorCode.ERR_HexStringEscapesNotSupportedInVersion, @"\xA").WithLocation(3, 20),
                // (4,20): error LUA0016: Hexadecimal string escapes are not supported in this lua version
                // local str3 = "hello\xFFthere"
                Diagnostic(ErrorCode.ERR_HexStringEscapesNotSupportedInVersion, @"\xFF").WithLocation(4, 20),
                // (5,20): error LUA0016: Hexadecimal string escapes are not supported in this lua version
                // local str4 = 'hello\xFFthere'
                Diagnostic(ErrorCode.ERR_HexStringEscapesNotSupportedInVersion, @"\xFF").WithLocation(5, 20));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsMultipleDiagnosticsWhen_MultipleHexEscapesAreFound_And_LuaSyntaxOptionsAcceptHexEscapesIsFalse()
        {
            const string source = @"local str = 'hello\xAFthere\xBFgood\xCFfriend'";
            var options = LuaSyntaxOptions.All.With(acceptHexEscapesInStrings: false);
            ParseAndValidate(source, options,
                // (1,19): error LUA0016: Hexadecimal string escapes are not supported in this lua version
                // local str = 'hello\xAFthere\xBFgood\xCFfriend'
                Diagnostic(ErrorCode.ERR_HexStringEscapesNotSupportedInVersion, @"\xAF").WithLocation(1, 19),
                // (1,28): error LUA0016: Hexadecimal string escapes are not supported in this lua version
                // local str = 'hello\xAFthere\xBFgood\xCFfriend'
                Diagnostic(ErrorCode.ERR_HexStringEscapesNotSupportedInVersion, @"\xBF").WithLocation(1, 28),
                // (1,36): error LUA0016: Hexadecimal string escapes are not supported in this lua version
                // local str = 'hello\xAFthere\xBFgood\xCFfriend'
                Diagnostic(ErrorCode.ERR_HexStringEscapesNotSupportedInVersion, @"\xCF").WithLocation(1, 36));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsWarning_ForExoticLineBreak()
        {
            const string source = "local a = 1\n\rlocal b = 2\n\rlocal c = 3";
            ParseAndValidate(source, null,
                // (1,12): warning LUA0022: This line break (\n\r) may affect error reporting between the editor and lua
                // local a = 1
                Diagnostic(ErrorCode.WRN_LineBreakMayAffectErrorReporting, "\n\r").WithLocation(1, 12),
                // (3,12): warning LUA0022: This line break (\n\r) may affect error reporting between the editor and lua
                // local b = 2
                Diagnostic(ErrorCode.WRN_LineBreakMayAffectErrorReporting, "\n\r").WithLocation(3, 12));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticsWhen_WhitespaceEscapesAreFound_And_LuaSyntaxOptionsAcceptWhitespaceEscapeIsFalse()
        {
            const string source = "local a = \"aaa\\z    aaaa\"\r\n" +
                "local b = 'aaa\\z    aaaa'";
            var options = LuaSyntaxOptions.All.With(acceptWhitespaceEscape: false);
            ParseAndValidate(source, options,
                // (1,15): error LUA0023: The whitespace escape ('\z') is not supported in this lua version.
                // local a = "aaa\z    aaaa"
                Diagnostic(ErrorCode.ERR_WhitespaceEscapeNotSupportedInVersion, @"\z    ").WithLocation(1, 15),
                // (2,15): error LUA0023: The whitespace escape ('\z') is not supported in this lua version.
                // local b = 'aaa\z    aaaa'
                Diagnostic(ErrorCode.ERR_WhitespaceEscapeNotSupportedInVersion, @"\z    ").WithLocation(2, 15));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticsWhen_InvalidUnicodeEscapesAreFound()
        {
            const string source = "local a = '\\u{}'\r\nlocal b = '\\uFEBF}'\r\nlocal c = '\\u{FEBF'\r\nlocal d = '\\uFEBF'\r\nlocal e = '\\u{1100000}'";
            ParseAndValidate(source, null,
                // (1,12): error LUA0027: Hexadecimal digit expected
                // local a = '\u{}'
                Diagnostic(ErrorCode.ERR_HexDigitExpected, @"\u{").WithLocation(1, 12),
                // (2,12): error LUA0024: Unicode escape must have an opening brace ('{') after '\u'
                // local b = '\uFEBF}'
                Diagnostic(ErrorCode.ERR_UnicodeEscapeMissingOpenBrace, @"\uFEBF}").WithLocation(2, 12),
                // (3,12): error LUA0025: Unicode escape must have a closing brace ('}') after the hexadecimal number
                // local c = '\u{FEBF'
                Diagnostic(ErrorCode.ERR_UnicodeEscapeMissingCloseBrace, @"\u{FEBF").WithLocation(3, 12),
                // (4,12): error LUA0024: Unicode escape must have an opening brace ('{') after '\u'
                // local d = '\uFEBF'
                Diagnostic(ErrorCode.ERR_UnicodeEscapeMissingOpenBrace, @"\uFEBF").WithLocation(4, 12),
                // (4,12): error LUA0025: Unicode escape must have a closing brace ('}') after the hexadecimal number
                // local d = '\uFEBF'
                Diagnostic(ErrorCode.ERR_UnicodeEscapeMissingCloseBrace, @"\uFEBF").WithLocation(4, 12),
                // (5,12): error LUA0026: Escape is too large, the limit is 10FFFF
                // local e = '\u{1100000}'
                Diagnostic(ErrorCode.ERR_EscapeTooLarge, @"\u{1100000}").WithArguments("10FFFF").WithLocation(5, 12));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticsWhen_UnicodeEscapesAreFound_And_LuaSyntaxOptionsAcceptUnicodeEscapeIsFalse()
        {
            const string source = "local a = \"\\u{FEBE}\"\r\n" +
                "local b = '\\u{FEBE}'";
            var options = LuaSyntaxOptions.All.With(acceptUnicodeEscape: false);
            ParseAndValidate(source, options,
                // (1,12): error LUA0028: Unicode escapes are not supported in this lua version
                // local a = "\u{FEBE}"
                Diagnostic(ErrorCode.ERR_UnicodeEscapesNotSupportedLuaInVersion, @"\u{FEBE}").WithLocation(1, 12),
                // (2,12): error LUA0028: Unicode escapes are not supported in this lua version
                // local b = '\u{FEBE}'
                Diagnostic(ErrorCode.ERR_UnicodeEscapesNotSupportedLuaInVersion, @"\u{FEBE}").WithLocation(2, 12));
        }
    }
}