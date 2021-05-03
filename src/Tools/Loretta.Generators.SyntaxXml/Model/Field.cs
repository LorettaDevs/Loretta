// The code on this file is based on Roslyn which is distributed under the MIT license.

#nullable disable

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Loretta.Generators.SyntaxXml
{
    public abstract class TreeTypeChild
    {
        public abstract T Accept<T>(TreeVisitor<T> visitor);
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

        public override T Accept<T>(TreeVisitor<T> visitor) => visitor.VisitChoice(this);
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

        public override T Accept<T>(TreeVisitor<T> visitor) => visitor.VisitSequence(this);
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

        [XmlElement(ElementName = "Kind", Type = typeof(Kind))]
        public List<Kind> Kinds = new List<Kind>();

        [XmlElement]
        public Comment PropertyComment;

        public bool IsToken => Type == "SyntaxToken";

        public Field WithOptionality(bool isOptional) =>
            Optional == isOptional ? this : new Field
            {
                AllowTrailingSeparator = AllowTrailingSeparator,
                Kinds = Kinds,
                MinCount = MinCount,
                Name = Name,
                New = New,
                Optional = isOptional,
                Override = Override,
                PropertyComment = PropertyComment,
                Type = Type
            };

        public override T Accept<T>(TreeVisitor<T> visitor) => visitor.VisitField(this);
    }
}
