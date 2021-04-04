namespace Loretta.CodeAnalysis.Lua.Syntax
{
    public sealed partial class IdentifierNameSyntax
    {
        /// <summary>
        /// This identifier's name.
        /// </summary>
        public string Name => Identifier.Text;
    }
}
