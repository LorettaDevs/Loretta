// See https://aka.ms/new-console-template for more information
using System.Runtime;
using System.Text.Json;
using Loretta.UnusedStuffFinder;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

ProfileOptimization.SetProfileRoot(".");
ProfileOptimization.StartProfile("Loretta.UnusedStuffFinder.pgo");

var instance = MSBuildLocator.RegisterDefaults();

using var workspace = MSBuildWorkspace.Create(new Dictionary<string, string>
{
    ["IsUnusedAnalysis"] = "true"
});
var solution = await workspace.OpenSolutionAsync("../../../Loretta.sln");
var publicApiFiles = Directory.EnumerateFiles("../../..", "PublicAPI.*.txt", new EnumerationOptions
{
    RecurseSubdirectories = true,
    MatchCasing = MatchCasing.CaseSensitive,
    IgnoreInaccessible = true
});
foreach (var diagnostic in workspace.Diagnostics)
{
    Console.WriteLine(diagnostic);
}
if (workspace.Diagnostics.Any(diag => diag.Kind == WorkspaceDiagnosticKind.Failure && !diag.Message.Contains("shproj", StringComparison.OrdinalIgnoreCase)))
{
    return;
}

//var publicApi = PublicApiContainer.LoadAll(publicApiFiles);
//var symbol = solution.Projects.Single(p => p.AssemblyName == "Loretta.CodeAnalysis")
//                              .GetCompilationAsync()
//                              .GetAwaiter()
//                              .GetResult()!
//                              .GetTypeByMetadataName("Loretta.CodeAnalysis.Boxes")!
//                              .GetMembers("BoxedFalse")
//                              .Single();
//var references = await ReferenceFinder.FindReferencingSymbolsAsync(solution, symbol);
//var references2 = await ReferenceFinder.FindReferencingSymbolsAsync(solution, references[1]);
//var references3 = await ReferenceFinder.FindReferencingSymbolsAsync(solution, references2[0]);
//var references4 = await ReferenceFinder.FindReferencingSymbolsAsync(solution, references3[0]);
//var isPublic = publicApi.IsPartOfPublicApi(references4[0]);
//return;

var finder = new UnusedFinder(
    solution,
    new[]
    {
        "Loretta.CodeAnalysis",
        "Loretta.CodeAnalysis.Test.Utilities",
        "Loretta.CodeAnalysis.Lua",
        "Loretta.CodeAnalysis.Lua.Experimental",
        "Loretta.CodeAnalysis.Lua.Test.Utilities",
    },
    publicApiFiles);
#if !DEBUG
try
#endif
{
    var unuseds = await finder.FindUnused();
    var names = unuseds.Select(s => s.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                       .Distinct(StringComparer.Ordinal)
                       .OrderBy(x => x);
    var writer = new StreamWriter("unused.txt");
    foreach (var name in names)
    {
        await writer.WriteLineAsync(name);
    }
    await writer.FlushAsync();
    await writer.DisposeAsync();
}
#if !DEBUG
finally
#endif
{
    Console.WriteLine("Dumping state...");
#if !DEBUG
    DesperatelyGcCollect();
#endif
    await DumpState(finder.Symbols);
}

#if !DEBUG
static void DesperatelyGcCollect()
{
    for (var idx = 0; idx < 100; idx++)
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
    }
}
#endif

static async Task DumpState(IEnumerable<SymbolEx> symbols)
{
    var stream = File.Open("state.json", FileMode.Create, FileAccess.Write, FileShare.Read);
    await JsonSerializer.SerializeAsync(stream, symbols, new JsonSerializerOptions
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
        WriteIndented = true,
        Converters =
    {
        new SymbolExConverter()
    }
    });
    await stream.FlushAsync();
    await stream.DisposeAsync();
}
