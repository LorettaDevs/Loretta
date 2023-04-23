// The code on this file is based on Roslyn which is distributed under the MIT license.

#nullable disable

using System.Xml;
using System.Xml.Serialization;

namespace Loretta.Generators.SyntaxXml
{
    public class Comment
    {
        [XmlAnyElement]
        public XmlElement[] Body;
    }
}
