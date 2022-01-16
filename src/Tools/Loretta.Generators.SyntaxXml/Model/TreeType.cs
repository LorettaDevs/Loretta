// The code on this file is based on Roslyn which is distributed under the MIT license.

#nullable disable

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Loretta.Generators.SyntaxXml
{
    public abstract class TreeType
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Base;

        [XmlAttribute]
        public bool SkipConvenienceFactories;

        [XmlElement]
        public Comment TypeComment;

        [XmlElement]
        public Comment FactoryComment;

        [XmlElement(ElementName = "Field", Type = typeof(Field))]
        [XmlElement(ElementName = "Choice", Type = typeof(Choice))]
        [XmlElement(ElementName = "Sequence", Type = typeof(Sequence))]
        public List<TreeTypeChild> Children = new();

        public abstract T Accept<T>(TreeVisitor<T> visitor);
    }
}
