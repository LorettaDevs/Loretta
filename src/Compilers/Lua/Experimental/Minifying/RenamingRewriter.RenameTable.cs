using System.Collections.Generic;
using Loretta.Utilities;
using System.Linq;

namespace Loretta.CodeAnalysis.Lua.Experimental.Minifying
{
    internal partial class RenamingRewriter
    {
        private class RenameTable
        {
            private readonly object _lock = new();
            private readonly Script _script;
            private readonly StateStack _stateStack;
            private readonly NamingStrategy _namingStrategy;
            private readonly Dictionary<IVariable, SyntaxNode> _lastUseCache = new();
            private readonly Dictionary<IVariable, string> _variableRenameMap = new();

            public RenameTable(Script script, StateStack stateStack, NamingStrategy namingStrategy)
            {
                _script = script;
                _stateStack = stateStack;
                _namingStrategy = namingStrategy;
            }

            /// <summary>
            /// Gets the location the variable is last used.
            /// </summary>
            /// <param name="variable"></param>
            /// <returns></returns>
            public SyntaxNode GetLastUse(IVariable variable)
            {
                SyntaxNode? use;
                lock (_lock)
                {
                    if (!_lastUseCache.TryGetValue(variable, out use))
                    {
                        _lastUseCache[variable] = use =
                            variable.ReadLocations.Concat(variable.WriteLocations)
                                                  .OrderByDescending(node => node.Location.SourceSpan.Start)
                                                  .First();
                    }
                }

                return use;
            }

            /// <summary>
            /// Gets the new variable name to be used for this node.
            /// </summary>
            /// <param name="node"></param>
            /// <returns>
            /// If <see langword="null"/>, remains unchanged.
            /// </returns>
            public string? GetNewVariableName(SyntaxNode node)
            {
                var variable = _script.GetVariable(node);
                if (variable is null)
                    throw ExceptionUtilities.Unreachable;
                if (variable.Kind is VariableKind.Iteration or VariableKind.Local or VariableKind.Parameter)
                    return null;

                // Get or calculate the new name for the variable of the
                // provided node.
                if (!_variableRenameMap.TryGetValue(variable, out var name))
                {
                    var slot = _stateStack.Slot;
                    name = _namingStrategy(_stateStack.Scope, slot);
                    _variableRenameMap[variable] = name;
                    // Increment the slot so the slot we just used is not
                    // used by another variable.
                    _stateStack.IncrementSlot();
                }

                // If this the last use of this variable, then we won't be
                // needing it for the rest of the code so we can reuse the
                // number it was using.
                if (GetLastUse(variable) == node)
                    _stateStack.DecrementSlot();

                return name;
            }
        }
    }
}
