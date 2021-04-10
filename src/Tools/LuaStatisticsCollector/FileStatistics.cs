namespace Loretta.CodeAnalysis.Lua.StatisticsCollector
{
    record FileStatistics(
        string FileName,
        ParseStatistics ParseStatistics,
        TokenStatistics? TokenStatistics,
        FileFeatureStatistics? FeatureStatistics,
        DiagnosticStatistics DiagnosticStatistics);
}
