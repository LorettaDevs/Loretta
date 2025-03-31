using Loretta.Test.Utilities;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing;

public sealed class TypeParsingRegressionTests : ParsingTestsBase
{
    [Test, WorkItem(125, "https://github.com/LorettaDevs/Loretta/issues/125")]
    public async Task LanguageParser_ProperlyParsesType_AsAContextualKeyword()
        => await ParseAndValidateAsync(
               """
               local type
               type = 2
               print(type)
               """,
               LuaSyntaxOptions.Luau);

    [Test, WorkItem(125, "https://github.com/LorettaDevs/Loretta/issues/125")]
    public async Task LanguageParser_ProperlyParsesExport_AsAContextualKeyword()
        => await ParseAndValidateAsync(
               """
               local export
               export = 2
               print(export)
               """,
               LuaSyntaxOptions.Luau);

    [Test, WorkItem(119, "https://github.com/LorettaDevs/Loretta/issues/119")]
    public async Task LanguageParser_ParsesFunctionTypes_WithParameterNamesCorrectly()
        => await ParseAndValidateAsync("export type a = (p1: any) -> any", LuaSyntaxOptions.Luau);

    [Test, WorkItem(119, "https://github.com/LorettaDevs/Loretta/issues/119")]
    public async Task LanguageParser_ParsesVariadicFunctionReturnTypes_Correctly()
        => await ParseAndValidateAsync(
               """
               function sample(a): ...any
                   print "hi"
               end
               """,
               LuaSyntaxOptions.Luau);

    [Test, WorkItem(119, "https://github.com/LorettaDevs/Loretta/issues/119")]
    public async Task LanguageParser_ParsesVariadicFunctionTypeReturnTypes_Correctly()
        => await ParseAndValidateTypeAsync("((Player, ...any) -> ...any)?", LuaSyntaxOptions.Luau);
}
