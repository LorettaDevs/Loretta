using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Experimental.Minifying
{
    /// <summary>
    /// A class with helper methods for minifying.
    /// </summary>
    public static class MinifyingUtils
    {
        /// <summary>
        /// Returns whether this is a variable we are able to rename or not.
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public static bool CanRename(IVariable variable)
        {
            if (variable.Kind is not (VariableKind.Iteration or VariableKind.Local or VariableKind.Parameter))
                return false;
            if (variable.Declaration is null)
                return false;
            return true;
        }

        /// <summary>
        /// Returns the list of variables names that are <b>not</b> avaiable in the provided scopes.
        /// </summary>
        public static IImmutableSet<string> GetUnavailableNames(IEnumerable<IScope> scopes)
        {
            var set = ImmutableHashSet.CreateBuilder(StringOrdinalComparer.Instance);
            foreach (var scope in scopes)
            {
                set.UnionWith(GetUnavailableNames(scope));
            }
            return set.ToImmutable();
        }

        /// <summary>
        /// Returns the list of variables names that are <b>not</b> avaiable in the provided scope.
        /// </summary>
        public static IImmutableSet<string> GetUnavailableNames(IScope scope)
        {
            var result = ImmutableHashSet.CreateBuilder(StringOrdinalComparer.Instance);
            while (scope is not null)
            {
                foreach (var variable in scope.DeclaredVariables)
                {
                    if (CanRename(variable))
                        continue;
                    result.Add(variable.Name);
                }
                if (scope.ContainingScope is null)
                    break;
                scope = scope.ContainingScope;
            }
            return result.ToImmutable();
        }
    }
}
