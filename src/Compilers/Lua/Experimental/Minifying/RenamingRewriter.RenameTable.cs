namespace Loretta.CodeAnalysis.Lua.Experimental.Minifying
{
    internal partial class RenamingRewriter
    {
        private class RenameTable
        {
            private readonly object _lock = new();
            private readonly Script _script;
            private readonly NamingStrategy _namingStrategy;
            private readonly Dictionary<IVariable, SyntaxNode?> _lastUseCache = new();
            private readonly Dictionary<IVariable, (int slot, string newName)> _variableMap = new();
            private readonly ISlotAllocator _slotAllocator;

            public RenameTable(Script script, NamingStrategy namingStrategy, ISlotAllocator slotAllocator)
            {
                _script = script;
                _namingStrategy = namingStrategy;
                _slotAllocator = slotAllocator;
            }

            /// <summary>
            /// Gets the location the variable is last used.
            /// </summary>
            /// <param name="variable"></param>
            /// <returns></returns>
            public SyntaxNode? GetLastUse(IVariable variable)
            {
                SyntaxNode? use;
                lock (_lock)
                {
                    if (!_lastUseCache.TryGetValue(variable, out use))
                    {
                        _lastUseCache[variable] = use =
                            variable.ReadLocations.Concat(variable.WriteLocations)
                                                  .OrderByDescending(node => node.Location.SourceSpan)
                                                  .FirstOrDefault() ?? variable.Declaration;
                    }
                }

                return use;
            }

            /// <summary>
            /// Gets the new variable name to be used for this node.
            /// </summary>
            /// <param name="node"></param>
            /// 
            /// <returns>
            /// If <see langword="null"/>, remains unchanged.
            /// </returns>
            public string? GetNewVariableName(SyntaxNode node)
            {
                var variable = _script.GetVariable(node);
                if (variable is null)
                    throw ExceptionUtilities.Unreachable;
                if (!MinifyingUtils.CanRename(variable))
                    return null;

                // Get or calculate the new name for the variable of the
                // provided node.
                if (!_variableMap.TryGetValue(variable, out var name))
                {
                    var slot = _slotAllocator.AllocateSlot();
                    var scopes = variable.ReadLocations.Concat(variable.WriteLocations)
                                                       .Concat(variable.Declaration)
                                                       .Where(n => n is not null)
                                                       .Select(n =>
                                                       {
                                                           var scope = _script.FindScope(n!);
                                                           LorettaDebug.AssertNotNull(scope);
                                                           return scope;
                                                       });
                    name = (slot, _namingStrategy(slot, scopes));
                    _variableMap[variable] = name;
                }

                // If this the last use of this variable, then we won't be
                // needing it for the rest of the code so we can reuse the
                // number it was using.
                var lastUse = GetLastUse(variable);
                if (lastUse != null && node.AncestorsAndSelf().Any(n => n == lastUse))
                    _slotAllocator.ReleaseSlot(name.slot);

                return name.newName;
            }
        }
    }
}
