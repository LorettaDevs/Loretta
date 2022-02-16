using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.InternalBenchmarks.Attributes;

#nullable disable

namespace Loretta.InternalBenchmarks
{
    [SimpleJob(RuntimeMoniker.Net60)]
    [MeanThroughputColumn("File"), MedianColumn, MedianThroughputColumn("File")]
    [MemoryDiagnoser]
    public class ParseTimeBenchmark
    {
        private static readonly LuaParseOptions _parseOptions = new(LuaSyntaxOptions.All);

        [ParamsSource(nameof(Files))]
        public TestFile File { get; set; }

        public static IEnumerable<TestFile> Files
        {
            get
            {
                yield return TestFile.Load("samples/benchies/anim.lua");
                yield return TestFile.Load("samples/benchies/rustic.lua");
            }
        }

        [Benchmark]
        public SyntaxTree Parse() =>
            LuaSyntaxTree.ParseText(File.Text, _parseOptions, File.Name);
    }
}
