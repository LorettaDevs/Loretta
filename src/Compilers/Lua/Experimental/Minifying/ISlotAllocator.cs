using System;
using System.Collections.Generic;
using System.Text;

namespace Loretta.CodeAnalysis.Lua.Experimental.Minifying
{
    /// <summary>
    /// The slot allocator to use for renaming.
    /// </summary>
    public interface ISlotAllocator
    {
        /// <summary>
        /// Allocates a slot for the provided variable.
        /// </summary>
        /// <returns>The slot that was allocated to the variable.</returns>
        int AllocateSlot();

        /// <summary>
        /// Releases a slot for usage by other variables.
        /// </summary>
        /// <param name="slot">The slot the variable is located in.</param>
        void ReleaseSlot(int slot);
    }
}
