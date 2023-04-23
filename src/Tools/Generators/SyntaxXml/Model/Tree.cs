// The code on this file is based on Roslyn which is distributed under the MIT license.

#nullable disable

using System.Xml.Serialization;

namespace Loretta.Generators.SyntaxXml
{
    [XmlRoot]
    public class Tree
    {
        [XmlAttribute]
        public string Root;

        [XmlElement(ElementName = "Node", Type = typeof(Node))]
        [XmlElement(ElementName = "AbstractNode", Type = typeof(AbstractNode))]
        [XmlElement(ElementName = "PredefinedNode", Type = typeof(PredefinedNode))]
        public List<TreeType> Types;
    }
}
