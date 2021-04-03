namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// A file's scope.
    /// </summary>
    public interface IFileScope : IScope
    {
        /// <summary>
        /// The implicit <c>args</c> that's available in all files.
        /// </summary>
        public IVariable ArgsVariable { get; }

        /// <summary>
        /// The implicit vararg that's available in all files
        /// </summary>
        public IVariable VarArgParameter { get; }
    }

    internal class FileScope : Scope, IFileScope
    {
        public FileScope(ScopeKind kind, SyntaxReference node, IScopeInternal? parent) : base(kind, node, parent)
        {
            ArgsVariable = CreateVariable(VariableKind.Parameter, "args");
            VarArgParameter = CreateVariable(VariableKind.Parameter, "...");
        }

        public IVariable ArgsVariable { get; }

        public IVariable VarArgParameter { get; }
    }
}
