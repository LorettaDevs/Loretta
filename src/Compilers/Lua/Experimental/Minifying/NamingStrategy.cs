using System.Collections.Generic;

namespace Loretta.CodeAnalysis.Lua.Experimental.Minifying
{
    /// <summary>
    /// The naming strategy to use to convert a slot into a variable
    /// name.
    /// Uses the provided scope to check if the variable name is not being
    /// used already.
    /// </summary>
    /// <param name="scopes">The scopes the slot will be used in.</param>
    /// <param name="slot">The slot to convert to a variable name.</param>
    /// <returns></returns>
    public delegate string NamingStrategy(IEnumerable<IScope> scopes, int slot);
}
