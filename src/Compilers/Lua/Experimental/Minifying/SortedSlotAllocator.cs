namespace Loretta.CodeAnalysis.Lua.Experimental.Minifying
{
    /// <summary>
    /// The sorted slot allocator.
    /// Will always use the lowest free slot.
    /// </summary>
    public sealed class SortedSlotAllocator : ISlotAllocator
    {
        private readonly SortedSet<int> _freeSlots = new();
        private int _currentSlot = 0;

        /// <inheritdoc/>
        public int AllocateSlot()
        {
            if (_freeSlots.Count > 0)
            {
                var slot = _freeSlots.First();
                _freeSlots.Remove(slot);
                return slot;
            }
            else
            {
                return _currentSlot++;
            }
        }

        /// <inheritdoc/>
        public void ReleaseSlot(int slot)
        {
            if (!_freeSlots.Add(slot))
                throw new InvalidOperationException($"Slot {slot} was released two times.");
        }
    }
}
