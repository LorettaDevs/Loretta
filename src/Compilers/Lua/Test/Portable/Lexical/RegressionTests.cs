using Loretta.Test.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Lexical
{
    public sealed class RegressionTests : LexicalTestsBase
    {
        [Fact]
        [Trait("Type", TestType.Regression)]
        [Trait("Category", "Lexer/Output")]
        public void Lexer_Lexes_LongStringWithoutLeadingNewLine()
        {
            const string RawText = """
                                   [[
                                   hi
                                   ]]
                                   """;
            const string Value = """
                                 hi

                                 """;
            var token = LexToken(RawText);

            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind());
            Assert.Equal(RawText, token.Text);
            Assert.Equal(Value, token.Value);
        }

        [Fact]
        [WorkItem(120, "https://github.com/LorettaDevs/Loretta/issues/120")]
        [Trait("Type", TestType.Regression)]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_Lexes_HexIntegersProperlyWhenPresetDoesntSupportIntegers()
        {
            const string RawText = "0X049bbe662";

            var token = LexToken(RawText, LuaSyntaxOptions.Lua51);

            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
            Assert.Equal(RawText, token.Text);
            Assert.Equal((double) 0x049bbe662, token.Value);
            Assert.False(token.ContainsDiagnostics);
        }

        [Fact]
        [WorkItem(120, "https://github.com/LorettaDevs/Loretta/issues/120")]
        [Trait("Type", TestType.Regression)]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_Warns_AboutHexFloatsProperlyWhenPresetDoesntSupportIntegers()
        {
            const string RawText = "0X049bbe662.ff";

            var token = LexToken(RawText, LuaSyntaxOptions.Lua51);

            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
            Assert.Equal(RawText, token.Text);
            Assert.True(token.ContainsDiagnostics);
        }
    }
}
