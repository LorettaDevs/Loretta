// The code on this file is based on Roslyn which is distributed under the MIT license.

#nullable disable

using System;
using System.Collections.Generic;
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
        public List<Kind> Kinds = new List<Kind>();

        public readonly List<Field> Fields = new List<Field>();

        public bool HasErrors => StringComparer.Ordinal.Equals(Errors, "true");

        public override T Accept<T>(TreeVisitor<T> visitor) => visitor.VisitNode(this);
    }
}
