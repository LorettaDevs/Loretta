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
    }
}
