using System;
using System.Collections.Generic;
using System.Text;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// An error found while renaming a variable.
    /// </summary>
    public record RenameError;
    /// <summary>
    /// An error that represents the provided identifier not being supported
    /// in a provided tree.
    /// </summary>
    /// <param name="SyntaxTree">
    /// The <see cref="SyntaxTree"/> the identifier name is not supported on.
    /// </param>
    public record IdentifierNameNotSupportedError(SyntaxTree SyntaxTree) : RenameError;
    /// <summary>
    /// Represents a conflict with an existing variable.
    /// </summary>
    /// <param name="Variable">The variable that is conflicted with.</param>
    public record VariableConflictError(IVariable Variable) : RenameError;
}
