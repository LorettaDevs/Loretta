using Loretta.CodeAnalysis.Lua.SymbolDisplay;

namespace Loretta.CodeAnalysis.Lua.Syntax
{
    internal class LuaTreeDumperConverter : LuaSyntaxWalker
    {
        private readonly Stack<List<TreeDumperNode?>> _childrenStack = new();
        private readonly bool _includeDiagnostics;

        private LuaTreeDumperConverter(bool includeDiagnostics = false) : base(SyntaxWalkerDepth.StructuredTrivia)
        {
            _includeDiagnostics = includeDiagnostics;
            _childrenStack.Push(new List<TreeDumperNode?>());
        }

        public static TreeDumperNode Convert(SyntaxNode node, bool includeDiagnostics = false)
        {
            var visitor = new LuaTreeDumperConverter(includeDiagnostics);
            visitor.Visit(node);
            return visitor.GetRoot();
        }

        public static TreeDumperNode Convert(SyntaxToken token, bool includeDiagnostics = false)
        {
            var visitor = new LuaTreeDumperConverter(includeDiagnostics);
            visitor.VisitToken(token);
            return visitor._childrenStack.Pop().Single()!;
        }

        public static TreeDumperNode Convert(SyntaxTrivia trivia, bool includeDiagnostics = false)
        {
            var visitor = new LuaTreeDumperConverter(includeDiagnostics);
            visitor.VisitTrivia(trivia);
            return visitor._childrenStack.Pop().Single()!;
        }

        public TreeDumperNode GetRoot()
        {
            var children = ToEnumerable(_childrenStack.Pop());
            if (_childrenStack.Count > 0)
                throw new InvalidCastException("Extra list remaining in stack.");
            return new TreeDumperNode("Root", null, children);
        }

        public override void DefaultVisit(SyntaxNode node)
        {
            var children = WithNewList(base.DefaultVisit, node);
            if (_includeDiagnostics)
                children = Concat(children, GetDiagnosticsNode(node.GetDiagnostics()));
            Add(new TreeDumperNode(node.Kind().ToString(), null, children));
        }

        public override void VisitToken(SyntaxToken token)
        {
            var children = WithNewList(base.VisitToken, token);
            if (_includeDiagnostics)
                children = Concat(children, GetDiagnosticsNode(token.GetDiagnostics()));
            Add(new TreeDumperNode(token.Kind().ToString(), ObjectDisplay.FormatPrimitive(token.ValueText, ObjectDisplayOptions.EscapeNonPrintableCharacters), children));
        }

        public override void VisitLeadingTrivia(SyntaxToken token)
        {
            var children = WithNewList(base.VisitLeadingTrivia, token);
            Add(new TreeDumperNode("Leading Trivia", null, children));
        }

        public override void VisitTrailingTrivia(SyntaxToken token)
        {
            var children = WithNewList(base.VisitTrailingTrivia, token);
            Add(new TreeDumperNode("Trailing Trivia", null, children));
        }

        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            IEnumerable<TreeDumperNode>? children = null;
            if (_includeDiagnostics)
            {
                var diagnosticNode = GetDiagnosticsNode(trivia.GetDiagnostics());
                children = diagnosticNode is null ? null : SpecializedCollections.SingletonEnumerable(diagnosticNode);
            }
            Add(new TreeDumperNode(trivia.Kind().ToString(), ObjectDisplay.FormatPrimitive(trivia.ToFullString(), ObjectDisplayOptions.EscapeNonPrintableCharacters), children));
        }

        private IEnumerable<TreeDumperNode>? WithNewList<TArg>(Action<TArg> action, TArg arg)
        {
            _childrenStack.Push(new List<TreeDumperNode?>());
            action(arg);
            return ToEnumerable(_childrenStack.Pop());
        }

        private void Add(TreeDumperNode? node) => _childrenStack.Peek().Add(node);

        private static IEnumerable<T>? ToEnumerable<T>(IEnumerable<T?> ts)
        {
            ts = ts.Where(t => t is not null);
            if (!ts.Any())
                return null;
            return ts!;
        }

        private static IEnumerable<T>? Concat<T>(IEnumerable<T>? enumerable, T? value) =>
            value is null ? enumerable : (enumerable?.Concat(value) ?? SpecializedCollections.SingletonEnumerable(value));

        private static TreeDumperNode? GetDiagnosticsNode(IEnumerable<Diagnostic> diagnostics)
        {
            if (diagnostics.Any())
                return null;
            return new TreeDumperNode("Diagnostics", null, diagnostics.Select(d => new TreeDumperNode(d.ToString())));
        }
    }
}
