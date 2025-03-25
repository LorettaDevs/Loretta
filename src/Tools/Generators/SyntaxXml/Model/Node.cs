// The code on this file is based on Roslyn which is distributed under the MIT license.

#nullable disable

using System.Xml.Serialization;

namespace Loretta.Generators.SyntaxXml
{
    public sealed class Node : TreeType
    {
        [XmlElement(ElementName = "Kind", Type = typeof(Kind))]
        public List<Kind> Kinds = [];

        public readonly List<Field> Fields = [];
    }
}
