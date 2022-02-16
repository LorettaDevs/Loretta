using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Text;

#nullable disable

namespace Loretta.InternalBenchmarks
{
    [SimpleJob(RuntimeMoniker.Net60)]
    [MemoryDiagnoser]
    public class ParseTimeBenchmark
    {
        private static readonly LuaParseOptions _parseOptions = new(LuaSyntaxOptions.All);
        private SourceText _fileContents;

        [Params("anim.lua", "rustic.lua")]
        public string FileName { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            using var stream = File.OpenRead(Path.Combine("samples", "benchies", FileName));
            _fileContents = SourceText.From(stream);
        }

        [Benchmark]
        public SyntaxTree Parse() =>
            LuaSyntaxTree.ParseText(_fileContents, _parseOptions, FileName);
    }
}
