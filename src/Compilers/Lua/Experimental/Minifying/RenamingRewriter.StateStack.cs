using System.Collections.Generic;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Experimental.Minifying
{
    internal partial class RenamingRewriter
    {
        private class StateStack
        {
            private readonly object _lock = new();
            private readonly Stack<(IScope scope, int slot)> _stack = new();

            public int Slot => _stack.Peek().slot;
            public IScope Scope => _stack.Peek().scope;

            public int IncrementSlot()
            {
                lock (_lock)
                {
                    var state = _stack.Pop();
                    state.slot++;
                    _stack.Push(state);
                    return state.slot;
                }
            }

            public int DecrementSlot()
            {
                lock (_lock)
                {
                    var state = _stack.Pop();
                    state.slot--;
                    _stack.Push(state);
                    return state.slot;
                }
            }

            public void EnterScope(IScope scope)
            {
                lock (_lock)
                {
                    _stack.Push((scope, Slot));
                }
            }

            public void ExitScope(IScope scope)
            {
                lock (_lock)
                {
                    var popped = _stack.Pop();
                    RoslynDebug.Assert(popped.scope == scope);
                    RoslynDebug.Assert(popped.slot == Slot);
                }
            }
        }
    }
}
