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

        [Fact]
        [WorkItem(127, "https://github.com/LorettaDevs/Loretta/issues/127")]
        [Trait("Type", TestType.Regression)]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_DoesNotLexContinueAsKeywordWhenItHasBeenDisabled()
        {
            const string RawText = """
                                   local continue = true

                                   if continue then
                                       continue = false
                                   end
                                   """;

            var tokens = Lex(RawText, LuaSyntaxOptions.Lua51).Select(static t => (t.Kind(), t.ContextualKind()));
            
            Assert.Equal([
                // local continue = true
                (SyntaxKind.LocalKeyword, SyntaxKind.LocalKeyword),
                (SyntaxKind.IdentifierToken, SyntaxKind.None),
                (SyntaxKind.EqualsToken, SyntaxKind.EqualsToken),
                (SyntaxKind.TrueKeyword, SyntaxKind.TrueKeyword),
                
                // if continue then
                (SyntaxKind.IfKeyword, SyntaxKind.IfKeyword),
                (SyntaxKind.IdentifierToken, SyntaxKind.None),
                (SyntaxKind.ThenKeyword, SyntaxKind.ThenKeyword),
                
                //     continue = false
                (SyntaxKind.IdentifierToken, SyntaxKind.None),
                (SyntaxKind.EqualsToken, SyntaxKind.EqualsToken),
                (SyntaxKind.FalseKeyword, SyntaxKind.FalseKeyword),
                
                // end
                (SyntaxKind.EndKeyword, SyntaxKind.EndKeyword),
                (SyntaxKind.EndOfFileToken, SyntaxKind.EndOfFileToken),
            ], tokens);
        }

        // This didn't exactly come from this issue, but it was another keyword that didn't have this handling.
        [Fact]
        [WorkItem(127, "https://github.com/LorettaDevs/Loretta/issues/127")]
        [Trait("Type", TestType.Regression)]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_DoesNotLexGotoAsKeywordWhenItHasBeenDisabled()
        {
            const string RawText = """
                                   ::label::

                                   goto label
                                   """;

            var tokens = Lex(RawText, LuaSyntaxOptions.Lua51).Select(static t => (t.Kind(), t.ContextualKind()));
            
            Assert.Equal([
                // ::label::
                (SyntaxKind.ColonToken, SyntaxKind.ColonToken),
                (SyntaxKind.ColonToken, SyntaxKind.ColonToken),
                (SyntaxKind.IdentifierToken, SyntaxKind.None),
                (SyntaxKind.ColonToken, SyntaxKind.ColonToken),
                (SyntaxKind.ColonToken, SyntaxKind.ColonToken),
                
                // goto label
                (SyntaxKind.IdentifierToken, SyntaxKind.None),
                (SyntaxKind.IdentifierToken, SyntaxKind.None),
                (SyntaxKind.EndOfFileToken, SyntaxKind.EndOfFileToken),
            ], tokens);
        }
    }
}
