using System.Runtime.CompilerServices;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// A file's scope.
    /// </summary>
    [InternalImplementationOnly]
    public interface IFileScope : IScope
    {
        /// <summary>
        /// The implicit <c>arg</c> that's available in all files.
        /// </summary>
        IVariable ArgVariable { get; }

        /// <summary>
        /// The implicit vararg that's available in all files
        /// </summary>
        IVariable VarArgParameter { get; }
    }

    internal interface IFileScopeInternal : IScopeInternal, IFileScope
    {
    }

    internal class FileScope : Scope, IFileScopeInternal
    {
        public FileScope(SyntaxNode node, IScopeInternal? parent) : base(ScopeKind.File, node, parent)
        {
            ArgVariable = CreateVariable(VariableKind.Parameter, "arg");
            VarArgParameter = CreateVariable(VariableKind.Parameter, "...");
        }

        public IVariableInternal ArgVariable { get; }

        IVariable IFileScope.ArgVariable => ArgVariable;

        public IVariableInternal VarArgParameter { get; }

        IVariable IFileScope.VarArgParameter => VarArgParameter;
    }
}
