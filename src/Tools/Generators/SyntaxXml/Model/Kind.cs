// The code on this file is based on Roslyn which is distributed under the MIT license.

using System.Xml.Serialization;

namespace Loretta.Generators.SyntaxXml
{
    public class Kind : IEquatable<Kind>
    {
        [XmlAttribute]
        public string? Name;

        public override bool Equals(object? obj)
            => Equals(obj as Kind);

        public bool Equals(Kind? other)
            => Name == other?.Name;

        public override int GetHashCode()
            => Name == null ? 0 : Name.GetHashCode();
    }
}
