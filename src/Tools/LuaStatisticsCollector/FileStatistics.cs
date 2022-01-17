namespace Loretta.CodeAnalysis.Lua.StatisticsCollector
{
    internal record FileStatistics(
        string FileName,
        ParseStatistics ParseStatistics,
        TokenStatistics? TokenStatistics,
        FileFeatureStatistics? FeatureStatistics,
        DiagnosticStatistics DiagnosticStatistics);
}
