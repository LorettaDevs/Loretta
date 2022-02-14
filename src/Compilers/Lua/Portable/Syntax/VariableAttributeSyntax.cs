namespace Loretta.CodeAnalysis.Lua.Syntax
{
    public sealed partial class VariableAttributeSyntax
    {
        /// <summary>
        /// The name of the attribute.
        /// </summary>
        public string Name => Identifier.Text;
    }
}
