using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.StatisticsCollector
{
    internal record SourceFile(string FileName, SourceText Text);
}
