// The code on this file is based on Roslyn which is distributed under the MIT license.

#nullable disable

using System.Xml.Serialization;

namespace Loretta.Generators.SyntaxXml
{
    public abstract class TreeTypeChild
    {
    }

    public class Choice : TreeTypeChild
    {
        // Note: 'Choice's should not be children of a 'Choice'.  It's not necessary, and the child
        // choice can just be inlined into the parent.
        [XmlElement(ElementName = "Field", Type = typeof(Field))]
        [XmlElement(ElementName = "Sequence", Type = typeof(Sequence))]
        public List<TreeTypeChild> Children;

        [XmlAttribute]
        public bool Optional;
    }

    public class Sequence : TreeTypeChild
    {
        // Note: 'Sequence's should not be children of a 'Sequence'.  It's not necessary, and the
        // child choice can just be inlined into the parent.
        [XmlElement(ElementName = "Field", Type = typeof(Field))]
        [XmlElement(ElementName = "Choice", Type = typeof(Choice))]
        public List<TreeTypeChild> Children;

        [XmlAttribute]
        public bool Optional;
    }

    public class Field : TreeTypeChild
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Type;

        [XmlAttribute]
        public bool Optional;

        [XmlAttribute]
        public bool Override;

        [XmlAttribute]
        public bool New;

        [XmlAttribute]
        public int MinCount;

        [XmlAttribute]
        public bool AllowTrailingSeparator;

        /// <summary>
        /// Basically tells whether this should always be required
        /// in the red factory even if it is a node that can be
        /// auto-constructed or a list that has no MinCount.
        /// </summary>
        [XmlAttribute]
        public bool FactoryRequired;

        [XmlElement(ElementName = "Kind", Type = typeof(Kind))]
        public List<Kind> Kinds = new();

        [XmlElement]
        public Comment PropertyComment;

        public bool IsToken => Type == "SyntaxToken";
    }
}
