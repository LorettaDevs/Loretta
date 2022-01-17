using System.Threading;

namespace Loretta.CodeAnalysis.Lua.Experimental.Minifying
{
    /// <summary>
    /// A sequential slot allocator.
    /// Never returns previously used slots and is the fastest one.
    /// </summary>
    public sealed class SequentialSlotAllocator : ISlotAllocator
    {
        private int _slot = -1;

        /// <inheritdoc/>
        public int AllocateSlot() => Interlocked.Increment(ref _slot);

        /// <inheritdoc/>
        public void ReleaseSlot(int slot)
        {
            // Do nothing.
        }
    }
}
