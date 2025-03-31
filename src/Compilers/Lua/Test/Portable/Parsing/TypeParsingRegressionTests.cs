using Loretta.Test.Utilities;
using Xunit;

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

    [Fact, WorkItem(119, "https://github.com/LorettaDevs/Loretta/issues/119")]
    public void LanguageParser_ParsesFunctionTypes_WithParameterNamesCorrectly()
        => ParseAndValidate("export type a = (p1: any) -> any", LuaSyntaxOptions.Luau);

    [Fact, WorkItem(119, "https://github.com/LorettaDevs/Loretta/issues/119")]
    public void LanguageParser_ParsesVariadicFunctionReturnTypes_Correctly()
        => ParseAndValidate(
            """
            function sample(a): ...any
                print "hi"
            end
            """,
            LuaSyntaxOptions.Luau);

    [Fact, WorkItem(119, "https://github.com/LorettaDevs/Loretta/issues/119")]
    public void LanguageParser_ParsesVariadicFunctionTypeReturnTypes_Correctly()
        => ParseAndValidateType("((Player, ...any) -> ...any)?", LuaSyntaxOptions.Luau);
}
