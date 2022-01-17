using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// Represents a non-terminal node in the syntax tree.
    /// </summary>
    //The fact that this type implements IMessageSerializable
    //enables it to be used as an argument to a diagnostic. This allows diagnostics
    //to defer the realization of strings. Often diagnostics generated while binding
    //in service of a SemanticModel API are never realized. So this
    //deferral can result in meaningful savings of strings.
    public abstract partial class LuaSyntaxNode : SyntaxNode, IFormattable
    {
        internal LuaSyntaxNode(GreenNode green, SyntaxNode? parent, int position)
            : base(green, parent, position)
        {
        }

        /// <summary>
        /// Used by structured trivia which has "parent == null", and therefore must know its
        /// SyntaxTree explicitly when created.
        /// </summary>
        internal LuaSyntaxNode(GreenNode green, int position, SyntaxTree? syntaxTree)
            : base(green, position, syntaxTree)
        {
        }

        /// <summary>
        /// Returns a non-null <see cref="SyntaxTree"/> that owns this node.
        /// If this node was created with an explicit non-null <see cref="SyntaxTree"/>, returns that tree.
        /// Otherwise, if this node has a non-null parent, then returns the parent's <see cref="SyntaxTree"/>.
        /// Otherwise, returns a newly created <see cref="SyntaxTree"/> rooted at this node, preserving this node's reference identity.
        /// </summary>
        internal new SyntaxTree SyntaxTree
        {
            get
            {
                var result = _syntaxTree ?? ComputeSyntaxTree(this);
                LorettaDebug.Assert(result != null);
                return result;
            }
        }

        private static SyntaxTree ComputeSyntaxTree(LuaSyntaxNode node)
        {
            ArrayBuilder<LuaSyntaxNode>? nodes = null;
            SyntaxTree? tree;

            // Find the nearest parent with a non-null syntax tree
            while (true)
            {
                tree = node._syntaxTree;
                if (tree != null)
                {
                    break;
                }

                var parent = node.Parent;
                if (parent == null)
                {
                    // set the tree on the root node atomically
                    Interlocked.CompareExchange(ref node._syntaxTree, LuaSyntaxTree.CreateWithoutClone(node), null);
                    tree = node._syntaxTree;
                    break;
                }

                tree = parent._syntaxTree;
                if (tree != null)
                {
                    node._syntaxTree = tree;
                    break;
                }

                (nodes ??= ArrayBuilder<LuaSyntaxNode>.GetInstance()).Add(node);
                node = parent;
            }

            // Propagate the syntax tree downwards if necessary
            if (nodes != null)
            {
                LorettaDebug.Assert(tree != null);

                foreach (var n in nodes)
                {
                    var existingTree = n._syntaxTree;
                    if (existingTree != null)
                    {
                        LorettaDebug.Assert(existingTree == tree, "how could this node belong to a different tree?");

                        // yield the race
                        break;
                    }
                    n._syntaxTree = tree;
                }

                nodes.Free();
            }

            return tree!;
        }

        /// <summary>
        /// Accepts a visitor into this node.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="visitor"></param>
        /// <returns></returns>
        public abstract TResult? Accept<TResult>(LuaSyntaxVisitor<TResult> visitor);

        /// <summary>
        /// Accepts a visitor into this node.
        /// </summary>
        /// <param name="visitor"></param>
        public abstract void Accept(LuaSyntaxVisitor visitor);

        /// <summary>
        /// The node that contains this node in its Children collection.
        /// </summary>
        internal new LuaSyntaxNode? Parent => (LuaSyntaxNode?) base.Parent;

        internal new LuaSyntaxNode? ParentOrStructuredTriviaParent => (LuaSyntaxNode?) base.ParentOrStructuredTriviaParent;

        // TODO: may be eventually not needed
        internal Syntax.InternalSyntax.LuaSyntaxNode CsGreen => (Syntax.InternalSyntax.LuaSyntaxNode) Green;

        /// <summary>
        /// Returns the <see cref="SyntaxKind"/> of the node.
        /// </summary>
        public SyntaxKind Kind() => (SyntaxKind) Green.RawKind;

        /// <summary>
        /// The language name that this node is syntax of.
        /// </summary>
        public override string Language => LanguageNames.Lua;

        /// <summary>
        /// The list of trivia that appears before this node in the source code.
        /// </summary>
        public new SyntaxTriviaList GetLeadingTrivia()
        {
            var firstToken = GetFirstToken(includeZeroWidth: true);
            return firstToken.LeadingTrivia;
        }

        /// <summary>
        /// The list of trivia that appears after this node in the source code.
        /// </summary>
        public new SyntaxTriviaList GetTrailingTrivia()
        {
            var lastToken = GetLastToken(includeZeroWidth: true);
            return lastToken.TrailingTrivia;
        }

        #region Serialization

        /// <summary>
        /// Deserialize a syntax node from the byte stream.
        /// </summary>
        public static SyntaxNode DeserializeFrom(Stream stream, CancellationToken cancellationToken = default)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new InvalidOperationException(CodeAnalysisResources.TheStreamCannotBeReadFrom);
            }

            using var reader = ObjectReader.TryGetReader(stream, leaveOpen: true, cancellationToken);

            if (reader == null)
            {
                throw new ArgumentException(CodeAnalysisResources.Stream_contains_invalid_data, nameof(stream));
            }

            var root = (Syntax.InternalSyntax.LuaSyntaxNode) reader.ReadValue();
            return root.CreateRed();
        }

        #endregion Serialization

        /// <summary>
        /// Gets a <see cref="Location"/> for this node.
        /// </summary>
        public new Location GetLocation() => new SourceLocation(this);

        /// <summary>
        /// Gets a SyntaxReference for this syntax node. SyntaxReferences can be used to
        /// regain access to a syntax node without keeping the entire tree and source text in
        /// memory.
        /// </summary>
        internal new SyntaxReference GetReference() => SyntaxTree.GetReference(this);

        /// <summary>
        /// Gets a list of all the diagnostics in the sub tree that has this node as its root.
        /// This method does not filter diagnostics based on #pragmas and compiler options
        /// like nowarn, warnaserror etc.
        /// </summary>
        public new IEnumerable<Diagnostic> GetDiagnostics() => SyntaxTree.GetDiagnostics(this);

        #region Token Lookup

        /// <summary>
        /// Gets the first token of the tree rooted by this node.
        /// </summary>
        /// <param name="includeZeroWidth">True if zero width tokens should be included, false by
        /// default.</param>
        /// <param name="includeSkipped">True if skipped tokens should be included, false by default.</param>
        /// <param name="includeDirectives">True if directives should be included, false by default.</param>
        /// <param name="includeDocumentationComments">True if documentation comments should be
        /// included, false by default.</param>
        /// <returns></returns>
        public new SyntaxToken GetFirstToken(bool includeZeroWidth = false, bool includeSkipped = false, bool includeDirectives = false, bool includeDocumentationComments = false)
            => base.GetFirstToken(includeZeroWidth, includeSkipped, includeDirectives, includeDocumentationComments);

        /// <summary>
        /// Gets the first token of the tree rooted by this node.
        /// </summary>
        /// <param name="predicate">Only tokens for which this predicate returns true are included.  Pass null to include
        /// all tokens.</param>
        /// <param name="stepInto">Steps into trivia if this is not null.  Only trivia for which this delegate returns
        /// true are included.</param> 
        /// <returns></returns>
        internal SyntaxToken GetFirstToken(Func<SyntaxToken, bool>? predicate, Func<SyntaxTrivia, bool>? stepInto = null)
            => SyntaxNavigator.Instance.GetFirstToken(this, predicate, stepInto);

        /// <summary>
        /// Gets the last non-zero-width token of the tree rooted by this node.
        /// </summary>
        /// <param name="includeZeroWidth">True if zero width tokens should be included, false by
        /// default.</param>
        /// <param name="includeSkipped">True if skipped tokens should be included, false by default.</param>
        /// <param name="includeDirectives">True if directives should be included, false by default.</param>
        /// <param name="includeDocumentationComments">True if documentation comments should be
        /// included, false by default.</param>
        /// <returns></returns>
        public new SyntaxToken GetLastToken(bool includeZeroWidth = false, bool includeSkipped = false, bool includeDirectives = false, bool includeDocumentationComments = false)
            => base.GetLastToken(includeZeroWidth, includeSkipped, includeDirectives, includeDocumentationComments);

        /// <summary>
        /// Finds a token according to the following rules:
        /// 1) If position matches the End of the node/s FullSpan and the node is CompilationUnit,
        ///    then EoF is returned. 
        /// 
        ///  2) If node.FullSpan.Contains(position) then the token that contains given position is
        ///     returned.
        /// 
        ///  3) Otherwise an ArgumentOutOfRangeException is thrown
        /// </summary>
        public new SyntaxToken FindToken(int position, bool findInsideTrivia = false)
            => base.FindToken(position, findInsideTrivia);

        #endregion Token Lookup

        #region Trivia Lookup

        /// <summary>
        /// Finds a descendant trivia of this node at the specified position, where the position is
        /// within the span of the node.
        /// </summary>
        /// <param name="position">The character position of the trivia relative to the beginning of
        /// the file.</param>
        /// <param name="stepInto">Specifies a function that determines per trivia node, whether to
        /// descend into structured trivia of that node.</param>
        /// <returns></returns>
        public new SyntaxTrivia FindTrivia(int position, Func<SyntaxTrivia, bool> stepInto) => base.FindTrivia(position, stepInto);

        /// <summary>
        /// Finds a descendant trivia of this node whose span includes the supplied position.
        /// </summary>
        /// <param name="position">The character position of the trivia relative to the beginning of
        /// the file.</param>
        /// <param name="findInsideTrivia">Whether to search inside structured trivia.</param>
        public new SyntaxTrivia FindTrivia(int position, bool findInsideTrivia = false) => base.FindTrivia(position, findInsideTrivia);

        #endregion Trivia Lookup

        #region SyntaxNode members

        /// <summary>
        /// Determine if this node is structurally equivalent to another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        protected override bool EquivalentToCore(SyntaxNode other) => throw ExceptionUtilities.Unreachable;

        /// <inheritdoc/>
        protected override SyntaxTree SyntaxTreeCore => SyntaxTree;

        /// <summary>
        /// Replaces child nodes.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="nodes"></param>
        /// <param name="computeReplacementNode"></param>
        /// <param name="tokens"></param>
        /// <param name="computeReplacementToken"></param>
        /// <param name="trivia"></param>
        /// <param name="computeReplacementTrivia"></param>
        /// <returns></returns>
        protected internal override SyntaxNode ReplaceCore<TNode>(
            IEnumerable<TNode>? nodes = null,
            Func<TNode, TNode, SyntaxNode>? computeReplacementNode = null,
            IEnumerable<SyntaxToken>? tokens = null,
            Func<SyntaxToken, SyntaxToken, SyntaxToken>? computeReplacementToken = null,
            IEnumerable<SyntaxTrivia>? trivia = null,
            Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia>? computeReplacementTrivia = null)
            =>
            SyntaxReplacer.Replace(this, nodes, computeReplacementNode, tokens, computeReplacementToken, trivia, computeReplacementTrivia).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);

        /// <summary>
        /// Replaces a node in a list.
        /// </summary>
        /// <param name="originalNode"></param>
        /// <param name="replacementNodes"></param>
        /// <returns></returns>
        protected internal override SyntaxNode ReplaceNodeInListCore(SyntaxNode originalNode, IEnumerable<SyntaxNode> replacementNodes)
            => SyntaxReplacer.ReplaceNodeInList(this, originalNode, replacementNodes).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);

        /// <summary>
        /// Inserts a node into a list.
        /// </summary>
        /// <param name="nodeInList"></param>
        /// <param name="nodesToInsert"></param>
        /// <param name="insertBefore"></param>
        /// <returns></returns>
        protected internal override SyntaxNode InsertNodesInListCore(SyntaxNode nodeInList, IEnumerable<SyntaxNode> nodesToInsert, bool insertBefore)
            => SyntaxReplacer.InsertNodeInList(this, nodeInList, nodesToInsert, insertBefore).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);

        /// <summary>
        /// Replaces a token in a list.
        /// </summary>
        /// <param name="originalToken"></param>
        /// <param name="newTokens"></param>
        /// <returns></returns>
        protected internal override SyntaxNode ReplaceTokenInListCore(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens)
            => SyntaxReplacer.ReplaceTokenInList(this, originalToken, newTokens).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);

        /// <summary>
        /// Inserts a token into a list.
        /// </summary>
        /// <param name="originalToken"></param>
        /// <param name="newTokens"></param>
        /// <param name="insertBefore"></param>
        /// <returns></returns>
        protected internal override SyntaxNode InsertTokensInListCore(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens, bool insertBefore)
            => SyntaxReplacer.InsertTokenInList(this, originalToken, newTokens, insertBefore).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);

        /// <summary>
        /// Replaces a trivia in a list.
        /// </summary>
        /// <param name="originalTrivia"></param>
        /// <param name="newTrivia"></param>
        /// <returns></returns>
        protected internal override SyntaxNode ReplaceTriviaInListCore(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia)
            => SyntaxReplacer.ReplaceTriviaInList(this, originalTrivia, newTrivia).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);

        /// <summary>
        /// Inserts a trivia into a list.
        /// </summary>
        /// <param name="originalTrivia"></param>
        /// <param name="newTrivia"></param>
        /// <param name="insertBefore"></param>
        /// <returns></returns>
        protected internal override SyntaxNode InsertTriviaInListCore(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia, bool insertBefore)
            => SyntaxReplacer.InsertTriviaInList(this, originalTrivia, newTrivia, insertBefore).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);

        /// <summary>
        /// Removes child nodes.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        protected internal override SyntaxNode? RemoveNodesCore(IEnumerable<SyntaxNode> nodes, SyntaxRemoveOptions options)
            => SyntaxNodeRemover.RemoveNodes(this, nodes.Cast<LuaSyntaxNode>(), options).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);

        /// <summary>
        /// Normalizes the whitespace in the node.
        /// </summary>
        /// <param name="indentation"></param>
        /// <param name="eol"></param>
        /// <param name="elasticTrivia"></param>
        /// <returns></returns>
        protected internal override SyntaxNode NormalizeWhitespaceCore(string indentation, string eol, bool elasticTrivia)
            => SyntaxNormalizer.Normalize(this, indentation, eol, elasticTrivia).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);

        /// <summary>
        /// Checks whether this node is equivalent to another.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="topLevel"></param>
        /// <returns></returns>
        protected override bool IsEquivalentToCore(SyntaxNode node, bool topLevel = false)
            => SyntaxFactory.AreEquivalent(this, (LuaSyntaxNode) node, topLevel);

        #endregion SyntaxNode members

        string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString();
    }
}
