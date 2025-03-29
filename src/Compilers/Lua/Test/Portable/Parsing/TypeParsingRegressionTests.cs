using Loretta.Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing;

public sealed class TypeParsingRegressionTests(ITestOutputHelper output) : ParsingTestsBase(output)
{
    [Fact, WorkItem(125, "https://github.com/LorettaDevs/Loretta/issues/125")]
    public void LanguageParser_ProperlyParsesType_AsAContextualKeyword()
        => ParseAndValidate(
            """
            local type
            type = 2
            print(type)
            """,
            LuaSyntaxOptions.Luau);
    
    [Fact, WorkItem(125, "https://github.com/LorettaDevs/Loretta/issues/125")]
    public void LanguageParser_ProperlyParsesExport_AsAContextualKeyword()
        => ParseAndValidate(
            """
            local export
            export = 2
            print(export)
            """,
            LuaSyntaxOptions.Luau);
}
