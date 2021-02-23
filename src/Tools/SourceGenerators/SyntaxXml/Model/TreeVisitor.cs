namespace Loretta.Generators.SyntaxXml
{
    public class TreeVisitor<T>
    {
        public virtual T? VisitPredefinedNode(PredefinedNode predefinedNode) => default;
        public virtual T? VisitAbstractNode(AbstractNode abstractNode) => default;
        public virtual T? VisitNode(Node node) => default;
        public virtual T? VisitChoice(Choice choice) => default;
        public virtual T? VisitSequence(Sequence sequence) => default;
        public virtual T? VisitField(Field field) => default;

        public virtual T? Visit(TreeType type) => type.Accept(this);
        public virtual T? Visit(TreeTypeChild typeChild) => typeChild.Accept(this);
    }
}
