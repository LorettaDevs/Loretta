using Loretta.CodeAnalysis.Collections;
using Loretta.CodeAnalysis.PooledObjects;

namespace Loretta.CodeAnalysis.Lua.StatisticsCollector
{
    internal class Pools
    {
        public const int BigListPoolSize = 1 << 25;
        public static ObjectPool<SegmentedList<double>> BigDoubleListPool { get; } = new ObjectPool<SegmentedList<double>>(() => new SegmentedList<double>(BigListPoolSize));
    }
}
