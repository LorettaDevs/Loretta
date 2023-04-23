using System.Text.RegularExpressions;
using LibGit2Sharp;
using NuGet.Versioning;

if (args.Length < 1 || !File.Exists(args[0]))
{
    WriteErrorLine("No output file for the parameters was provided.");
    return 1;
}

if (!File.Exists("./build/LibraryProject.props"))
{
    Console.WriteLine("::error title=Invalid Location::Could not find LibraryProject.props");
    WriteErrorLine("Could not find the ./build/LibraryProject.props file.");
    WriteErrorLine("Are you sure this tool is being ran from the project's root?");
    return 1;
}

var versionMatch = Regexes.PropsVersionRegex().Match(File.ReadAllText("./build/LibraryProject.props"));
if (!versionMatch.Success)
{
    Console.WriteLine("::error title=Invalid Version::Unable to read version from LibraryProject.props");
    WriteErrorLine("Could not read the <Version> tag in LibraryProject.props.");
    return 1;
}

var nextStable = 'v' + versionMatch.Groups[1].Value;

using var repo = new Repository(".");

var latestStable = repo.Tags.Where(tag => Regexes.TagVersionRegex().IsMatch(tag.FriendlyName))
    .OrderByDescending(tag => NuGetVersion.Parse(tag.FriendlyName[1..]), VersionComparer.VersionRelease)
    .First();

var nightlyPrefix = $"{nextStable}-nightly.";
var latestNightly = repo.Tags.Where(tag => tag.FriendlyName.StartsWith(nightlyPrefix))
    .OrderByDescending(tag => NuGetVersion.Parse(tag.FriendlyName[1..]), VersionComparer.VersionRelease)
    .FirstOrDefault();

WriteInfoLine($"Latest version: {latestStable.FriendlyName} ({latestStable.Target})");
WriteInfoLine($"Next version: {nextStable}");
WriteInfoLine($"Latest nightly: {latestNightly?.FriendlyName ?? "N/A"} ({latestNightly?.Target.ToString() ?? "N/A"})");

if (latestStable.FriendlyName == nextStable)
{
    Console.WriteLine("::error title=Invalid Version::Version in LibraryProject.props is the same as the last stable release");
    WriteErrorLine("Version in LibraryProject.props is the same as the last stable release");
    return 1;
}

var applicableCommitCount = 0;
Commit? mostRecentApplicableCommit = null;
var commitsSinceStable = repo.Commits.QueryBy(new CommitFilter { ExcludeReachableFrom = latestStable.Target })
    .OrderByDescending(commit => commit.Author.When);
WriteDebugLine($"Commits since last stable: {commitsSinceStable.Count()}");
foreach (var commit in commitsSinceStable)
{
    var applicable = true;
    if (Regexes.IgnoreTag().IsMatch(commit.Message))
        applicable = false;

    var changed = repo.Diff.Compare<Patch>(commit.Parents.First().Tree, commit.Tree);
    if (!changed.Any(entry => IsFileSignificant(entry.Path)))
        applicable = false;

    WriteDebugLine($"  {commit.Sha[0..7]} {commit.MessageShort}{(!applicable ? " (ignored)" : "")}");
    if (applicable)
    {
        applicableCommitCount++;
        mostRecentApplicableCommit ??= commit;
    }
}
if (applicableCommitCount < 1)
{
    Console.WriteLine("::notice title=No Changes Found::No significant changes were found since the last stable release");
    WriteInfoLine("No applicable commits were found since the latest stable release.");
    goto NO_CHANGES;
}

WriteInfoLine($"Applicable commits: {applicableCommitCount}");

if ((latestNightly?.Target as Commit)?.Author.When >= mostRecentApplicableCommit!.Author.When)
{
    Console.WriteLine("::notice title=No Changes::This nightly build is being skipped due to no changes having been made since the last nightly");
    WriteInfoLine("No changes since last nightly were detected.");
    goto NO_CHANGES;
}

var nextNightly = string.Concat(nightlyPrefix, applicableCommitCount);

WriteInfoLine($"Next nightly: {nextNightly}");

if (latestNightly is not null)
{
    var count = int.Parse(latestNightly.FriendlyName[(latestNightly.FriendlyName.LastIndexOf('.') + 1)..]);
    // Legacy nightly versioning scheme compatibility.
    if (count > applicableCommitCount)
    {
        WriteWarningLine("Latest nightly has a larger version number than ours. Adding one to the latest nightly instead.");
        nextNightly = string.Concat(nightlyPrefix, count + 1);
        WriteInfoLine($"New next nightly: {nextNightly}");
    }
}

File.AppendAllLines(args[0], new[]
{
    "HAS_NIGHTLY=yes",
    $"VERSION={nextNightly}",
    $"NO_PREFIX_VERSION={nextNightly[1..]}",
});

return 0;

NO_CHANGES:
File.AppendAllLines(args[0], new[] { "HAS_NIGHTLY::no" });
return 0;

static bool IsFileSignificant(string path)
{
    // The nightly build pipeline might change something in our builds.
    if (path.Equals(".github/workflows/nigthly-publish.yml", StringComparison.Ordinal))
        return true;
    // Any changes to the .props files in the build dir affects our builds.
    if (path.StartsWith("build/", StringComparison.OrdinalIgnoreCase) && path.EndsWith(".props", StringComparison.OrdinalIgnoreCase))
        return true;
    if (path.StartsWith("src/", StringComparison.OrdinalIgnoreCase))
    {
        // Internal benchmarks aren't significant to releases.
        if (path.StartsWith("src/InternalBenchmarks", StringComparison.OrdinalIgnoreCase))
            return false;

        // Changes to these tools do not matter.
        if (path.StartsWith("src/Tools/LuaStatisticsCollector", StringComparison.OrdinalIgnoreCase))
            return false;
        if (path.StartsWith("src/Tools/LuaStatisticsCollector", StringComparison.OrdinalIgnoreCase))
            return false;

        // Changes to markdown files aren't significant.
        if (path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            return false;

        // Text file changes other than public API ones shouldn't trigger releases.
        if (path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
            && !path.EndsWith("PublicAPI.Shipped.txt", StringComparison.OrdinalIgnoreCase)
            && !path.EndsWith("PublicAPI.Unshipped.txt", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // The g4 syntax file is auto-generated and doesn't influence on anything in our code.
        if (path.EndsWith(".g4", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
    // Any changes to the nuget config affects our releases.
    if (path.Equals("nuget.config", StringComparison.OrdinalIgnoreCase))
        return true;
    return false;
}

static void WriteErrorLine(string text)
{
    var color = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(text);
    Console.ForegroundColor = color;
}

static void WriteWarningLine(string text)
{
    var color = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Error.WriteLine(text);
    Console.ForegroundColor = color;
}

static void WriteInfoLine(string text)
{
    var color = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.Error.WriteLine(text);
    Console.ForegroundColor = color;
}

static void WriteDebugLine(string text)
{
    var color = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Error.WriteLine(text);
    Console.ForegroundColor = color;
}

internal partial class Regexes
{
    [GeneratedRegex(@"^v\d+\.\d+\.\d+$")]
    public static partial Regex TagVersionRegex();

    [GeneratedRegex(@"<Version>(\d+\.\d+\.\d+)(-[\w.+]+)?</Version>")]
    public static partial Regex PropsVersionRegex();

    [GeneratedRegex(@"\[(?:nightly (?:ignore|skip)|(?:ignore|skip) nightly)\]")]
    public static partial Regex IgnoreTag();
}
