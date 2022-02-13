namespace Loretta.CodeAnalysis.Lua.Syntax
{
    public sealed partial class LocalDeclarationNameSyntax
    {
        /// <summary>
        /// The variable name.
        /// </summary>
        public string Name => IdentifierName.Name;

        /// <summary>
        /// The name of the attribute.
        /// </summary>
        public string? AttributeName => Attribute?.Name;
    }
}
