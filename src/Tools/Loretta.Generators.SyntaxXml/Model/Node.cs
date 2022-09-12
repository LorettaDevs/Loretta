// The code on this file is based on Roslyn which is distributed under the MIT license.

#nullable disable

using System.Xml.Serialization;

namespace Loretta.Generators.SyntaxXml
{
    public class Node : TreeType
    {
        [XmlAttribute]
        public string Root;

        [XmlAttribute]
        public string Errors;

        [XmlElement(ElementName = "Kind", Type = typeof(Kind))]
        public List<Kind> Kinds = new();

        public readonly List<Field> Fields = new();
    }
}
