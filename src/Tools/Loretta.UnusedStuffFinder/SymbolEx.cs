using Microsoft.CodeAnalysis;

namespace Loretta.UnusedStuffFinder
{
    class SymbolEx
    {
        public SymbolEx(ISymbol symbol)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        }

        public ISymbol Symbol { get; }
        public bool RequiredsListed { get; set; }
        public bool IsUsed { get; set; }
        public ISet<SymbolEx> RequiredBy { get; } = new HashSet<SymbolEx>(SymbolExComparer.Instance);
    }
}