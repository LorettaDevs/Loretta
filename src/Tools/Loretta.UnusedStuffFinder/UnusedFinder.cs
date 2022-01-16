using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Tsu.Numerics;

namespace Loretta.UnusedStuffFinder
{
    internal class UnusedFinder
    {
        private const int ReportCount = 1000;

        public static async Task<(ImmutableArray<ISymbol>, IEnumerable<SymbolEx>)> FindUnused(
            Solution solution,
            IEnumerable<string> assembliesToAnalyze,
            IEnumerable<string> publicApiFiles)
        {
            var finder = new UnusedFinder(solution, assembliesToAnalyze, publicApiFiles);
            return (await finder.FindUnused(), finder.Symbols);
        }

        private readonly SymbolMap _symbols = new();
        private readonly HashSet<string> _toAnalyze;
        private readonly PublicApiContainer _publicApi;
        private readonly Solution _solution;

        public UnusedFinder(
            Solution solution,
            IEnumerable<string> assembliesToAnalyze,
            IEnumerable<string> publicApiFiles)
        {
            _solution = solution;
            _toAnalyze = new HashSet<string>(assembliesToAnalyze, StringComparer.Ordinal);
            _publicApi = PublicApiContainer.LoadAll(publicApiFiles);
        }

        public IEnumerable<SymbolEx> Symbols => _symbols;

        private bool ShouldAnalyze(ISymbol symbol) =>
            _toAnalyze.Contains(symbol.ContainingAssembly.Identity.Name, StringComparer.Ordinal);

        private bool CanCheckSymbol(SymbolKind kind) =>
            kind is SymbolKind.Event
                 or SymbolKind.Field
                 or SymbolKind.Method
                 or SymbolKind.NamedType
                 or SymbolKind.Property;

        private async Task ListSymbolsAsync()
        {
            foreach (var project in _solution.Projects)
            {
                if (!_toAnalyze.Contains(project.AssemblyName))
                    continue;

                var compilation = await project.GetCompilationAsync() ?? throw new Exception("null compilation");

                var queue = new Queue<ISymbol>();
                queue.Enqueue(compilation.GlobalNamespace);
                while (queue.TryDequeue(out var symbol))
                {
                    if (symbol is INamespaceOrTypeSymbol namespaceOrType)
                    {
                        foreach (var member in namespaceOrType.GetMembers())
                        {
                            if (CanCheckSymbol(member.Kind)
                                && ShouldAnalyze(member))
                            {
                                var memberEx = _symbols.Get(member);
                                if (symbol.Kind == SymbolKind.NamedType)
                                    memberEx.RequiredBy.Add(_symbols.Get(symbol));
                                if (member.Kind is SymbolKind.Event)
                                {
                                    var @event = (IEventSymbol) member;
                                    if (@event.AddMethod is { } addMethod)
                                        _symbols.Get(addMethod).RequiredBy.Add(memberEx);
                                    if (@event.RemoveMethod is { } removeMethod)
                                        _symbols.Get(removeMethod).RequiredBy.Remove(memberEx);
                                }
                                else if (member.Kind is SymbolKind.Property)
                                {
                                    var property = (IPropertySymbol) member;
                                    if (property.GetMethod is { } getMethod)
                                        _symbols.Get(getMethod).RequiredBy.Add(memberEx);
                                    if (property.SetMethod is { } setMethod)
                                        _symbols.Get(setMethod).RequiredBy.Add(memberEx);
                                }
                            }
                            queue.Enqueue(member);
                        }
                    }
                }
            }
        }

        private static int GetSymbolOrder(ISymbol symbol)
        {
            return symbol switch
            {
                { Kind: SymbolKind.NamedType } => 1,
                { DeclaredAccessibility: Accessibility.Private } => 2,
                { Kind: SymbolKind.Method or SymbolKind.Property } => 4,
                _ => 3,
            };
        }

        private async Task ListDependenciesAsync()
        {
            var handled = 0;
            var started = Stopwatch.GetTimestamp();
            var symbols = _symbols.Where(s => ShouldAnalyze(s.Symbol) && !s.RequiredsListed)
                                  .OrderBy(s => GetSymbolOrder(s.Symbol))
                                  .ToArray();
            await Parallel.ForEachAsync(symbols, worker);

            async ValueTask worker(SymbolEx symbol, CancellationToken token = default)
            {
                symbol.RequiredBy.UnionWith((await ReferenceFinder.FindReferencingSymbolsAsync(_solution, symbol.Symbol, token))
                                                                        .Select(_symbols.Get)
                                                                        .ToArray());
                symbol.RequiredsListed = true;

                var tmp = Interlocked.Increment(ref handled);
                if (tmp == symbols.Length || tmp % ReportCount == 0)
                {
                    var ticksPerSymbol = (Stopwatch.GetTimestamp() - started) / tmp;

                    var remainingTicks = (symbols!.Length - tmp) * ticksPerSymbol;
                    var endTime = DateTime.Now.AddTicks(remainingTicks);
                    var endEta = TimeSpan.FromTicks(remainingTicks);

                    var reportTicks = ReportCount * ticksPerSymbol;
                    var reportEta = TimeSpan.FromTicks(reportTicks);
                    Console.WriteLine($"Found references for {tmp}/{symbols.Length} symbols. "
                        + $"Finding references at an average of {Duration.Format(ticksPerSymbol)}/symbol with an ETA of {endTime:HH:mm:ss} "
                        + $"({endEta:mm'm'ss's'}). "
                        + $"Next report in {reportEta:ss'.'fff}s.");
                }
            }
        }

        private void MarkUsed()
        {
            foreach (var symbol in _symbols)
            {
                if (!ShouldAnalyze(symbol.Symbol) || _publicApi.IsPartOfPublicApi(symbol.Symbol))
                {
                    symbol.IsUsed = true;
                }
            }

            var marked = 0;
            do
            {
                marked = 0;
                var unused = new CompactList<SymbolEx>(_symbols.Where(symbol => ShouldAnalyze(symbol.Symbol) && !symbol.IsUsed));
                foreach (var symbol in unused)
                {
                    if (symbol.RequiredBy.Any(s => s.IsUsed))
                    {
                        symbol.IsUsed = true;
                        marked++;
                    }
                }
            }
            while (marked > 0);
        }

        public async Task<ImmutableArray<ISymbol>> FindUnused()
        {
            Console.WriteLine("Listing symbols...");
            await ListSymbolsAsync();
            Console.WriteLine("Listing dependencies...");
            await ListDependenciesAsync();
            Console.WriteLine("Marking used symbols...");
            MarkUsed();
            Console.WriteLine("Done.");
            return _symbols.Where(s => ShouldAnalyze(s.Symbol) && !s.IsUsed)
                           .Select(s => s.Symbol)
                           .ToImmutableArray();
        }
    }
}
