namespace Loretta.CodeAnalysis.Lua.Syntax
{
    public sealed partial class NamedParameterSyntax
    {
        /// <summary>
        /// This parameter's name.
        /// </summary>
        public string Name => Identifier.Text;
    }
}
