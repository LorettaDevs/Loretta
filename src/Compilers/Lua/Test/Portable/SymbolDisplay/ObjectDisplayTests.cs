using Loretta.CodeAnalysis.Lua.SymbolDisplay;
using Loretta.Test.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.SymbolDisplay
{
    public sealed class ObjectDisplayTests
    {
        [Fact, WorkItem(89, "https://github.com/LorettaDevs/Loretta/issues/89")]
        public void ObjectDisplay_FormatLiteralString_DoesNotEscapeSpace()
        {
            const string input = "hello there";

            var output = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.UseQuotes | ObjectDisplayOptions.EscapeNonPrintableCharacters);

            Assert.Equal("\"hello there\"", output);
        }
    }
}
