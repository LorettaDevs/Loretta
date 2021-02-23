// The code on this file is based on Roslyn which is distributed under the MIT license.

#nullable disable

using System.Xml.Serialization;

namespace Loretta.Generators.SyntaxXml
{
    public class Kind
    {
        [XmlAttribute]
        public string Name;

        public override bool Equals(object obj)
            => obj is Kind kind &&
               Name == kind.Name;

        public override int GetHashCode()
            => Name.GetHashCode();
    }
}
