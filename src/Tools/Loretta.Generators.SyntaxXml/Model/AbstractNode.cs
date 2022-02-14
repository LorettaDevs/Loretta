// The code on this file is based on Roslyn which is distributed under the MIT license.

#nullable disable

namespace Loretta.Generators.SyntaxXml
{
    public class AbstractNode : TreeType
    {
        public readonly List<Field> Fields = new();

        public override T Accept<T>(TreeVisitor<T> visitor) => visitor.VisitAbstractNode(this);
    }
}
