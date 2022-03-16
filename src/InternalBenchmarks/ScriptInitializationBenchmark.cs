using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;

#nullable disable

namespace Loretta.InternalBenchmarks
{
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.Net60)]
    [MedianColumn]
    [MemoryDiagnoser]
    public class ScriptInitializationBenchmark
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
                yield return TestFile.Load("samples/benchies/rustic-24mb.lua");
            }
        }

        private SyntaxTree _syntaxTree;
        private SyntaxNode _rootNode;

        [GlobalSetup]
        public void Setup()
        {
            _syntaxTree = LuaSyntaxTree.ParseText(File.Text, _parseOptions, File.Name);
            _rootNode = _syntaxTree.GetRoot();
        }

        [Benchmark]
        public IScope Initialization()
        {
            var script = new Script(ImmutableArray.Create(_syntaxTree));
            return script.GetScope(_rootNode);
        }
    }
}
