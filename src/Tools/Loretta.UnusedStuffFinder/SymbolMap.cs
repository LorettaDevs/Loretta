using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Loretta.UnusedStuffFinder
{
    internal class SymbolExComparer : IEqualityComparer<SymbolEx>
    {
        public static readonly SymbolExComparer Instance = new();
        public bool Equals(SymbolEx? x, SymbolEx? y) =>
            SymbolEqualityComparer.Default.Equals(x?.Symbol, y?.Symbol);
        public int GetHashCode([DisallowNull] SymbolEx obj) =>
            SymbolEqualityComparer.Default.GetHashCode(obj?.Symbol);
    }

    internal class SymbolMap : IEnumerable<SymbolEx>
    {
#pragma warning disable RS1024 // Compare symbols correctly
        private readonly ConcurrentDictionary<ISymbol, SymbolEx> _symbolMap = new(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly

        public SymbolEx Get(ISymbol symbol) =>
            _symbolMap.GetOrAdd(symbol, symbol => new SymbolEx(symbol));

        public IEnumerator<SymbolEx> GetEnumerator() => _symbolMap.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
