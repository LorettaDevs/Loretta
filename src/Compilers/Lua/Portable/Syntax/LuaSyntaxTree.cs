// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;
using InternalSyntax = Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The parsed representation of a Lua source document.
    /// </summary>
    public abstract partial class LuaSyntaxTree : SyntaxTree
    {
        internal static readonly SyntaxTree Dummy = new DummySyntaxTree();

        /// <summary>
        /// The options used by the parser to produce the syntax tree.
        /// </summary>
        public new abstract LuaParseOptions Options { get; }

        // REVIEW: I would prefer to not expose CloneAsRoot and make the functionality
        // internal to CaaS layer, to ensure that for a given SyntaxTree there can not
        // be multiple trees claiming to be its children.
        //
        // However, as long as we provide GetRoot extensibility point on SyntaxTree
        // the guarantee above cannot be implemented and we have to provide some way for
        // creating root nodes.
        //
        // Therefore I place CloneAsRoot API on SyntaxTree and make it protected to
        // at least limit its visibility to SyntaxTree extenders.

        /// <summary>
        /// Produces a clone of a <see cref="LuaSyntaxNode"/> which will have current syntax tree as its parent.
        ///
        /// Caller must guarantee that if the same instance of <see cref="LuaSyntaxNode"/> makes multiple calls
        /// to this function, only one result is observable.
        /// </summary>
        /// <typeparam name="T">Type of the syntax node.</typeparam>
        /// <param name="node">The original syntax node.</param>
        /// <returns>A clone of the original syntax node that has current <see cref="LuaSyntaxTree"/> as its parent.</returns>
        protected T CloneNodeAsRoot<T>(T node) where T : LuaSyntaxNode =>
            SyntaxNode.CloneNodeAsRoot(node, this);

        /// <summary>
        /// Gets the root node of the syntax tree.
        /// </summary>
        public new abstract LuaSyntaxNode GetRoot(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the root node of the syntax tree if it is already available.
        /// </summary>
        public abstract bool TryGetRoot([NotNullWhen(true)] out LuaSyntaxNode? root);

        /// <summary>
        /// Gets the root node of the syntax tree asynchronously.
        /// </summary>
        /// <remarks>
        /// By default, the work associated with this method will be executed immediately on the current thread.
        /// Implementations that wish to schedule this work differently should override <see cref="GetRootAsync(CancellationToken)"/>.
        /// </remarks>
        public new virtual Task<LuaSyntaxNode> GetRootAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(TryGetRoot(out var node) ? node : GetRoot(cancellationToken));

        /// <summary>
        /// Gets the root of the syntax tree statically typed as <see cref="CompilationUnitSyntax"/>.
        /// </summary>
        /// <remarks>
        /// Ensure that <see cref="SyntaxTree.HasCompilationUnitRoot"/> is true for this tree prior to invoking this method.
        /// </remarks>
        /// <exception cref="InvalidCastException">Throws this exception if <see cref="SyntaxTree.HasCompilationUnitRoot"/> is false.</exception>
        public CompilationUnitSyntax GetCompilationUnitRoot(CancellationToken cancellationToken = default) =>
            (CompilationUnitSyntax) GetRoot(cancellationToken);

        /// <summary>
        /// Determines if two trees are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="tree">The tree to compare against.</param>
        /// <param name="topLevel">
        /// If true then the trees are equivalent if the contained nodes and tokens declaring metadata visible symbolic information are equivalent,
        /// ignoring any differences of nodes inside method bodies or initializer expressions, otherwise all nodes and tokens must be equivalent.
        /// </param>
        public override bool IsEquivalentTo(SyntaxTree tree, bool topLevel = false) =>
            SyntaxFactory.AreEquivalent(this, tree, topLevel);

        #region Factories

        /// <summary>
        /// Creates a new syntax tree from a syntax node.
        /// </summary>
        public static SyntaxTree Create(LuaSyntaxNode root, LuaParseOptions? options = null, string path = "", Encoding? encoding = null)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            return new ParsedSyntaxTree(
                textOpt: null,
                encodingOpt: encoding,
                checksumAlgorithm: SourceHashAlgorithm.Sha1,
                path: path,
                options: options ?? LuaParseOptions.Default,
                root: root,
                cloneRoot: true);
        }

        /// <summary>
        /// Creates a new syntax tree from a syntax node with text that should correspond to the syntax node.
        /// </summary>
        /// <remarks>This is used by the ExpressionEvaluator.</remarks>
        internal static SyntaxTree CreateForDebugger(LuaSyntaxNode root, SourceText text, LuaParseOptions options)
        {
            LorettaDebug.Assert(root != null);

            return new DebuggerSyntaxTree(root, text, options);
        }

        /// <summary>
        /// <para>
        /// Internal helper for <see cref="LuaSyntaxNode"/> class to create a new syntax tree rooted at the given root node.
        /// This method does not create a clone of the given root, but instead preserves it's reference identity.
        /// </para>
        /// <para>NOTE: This method is only intended to be used from <see cref="LuaSyntaxNode.SyntaxTree"/> property.</para>
        /// <para>NOTE: Do not use this method elsewhere, instead use <see cref="Create(LuaSyntaxNode, LuaParseOptions, string, Encoding)"/> method for creating a syntax tree.</para>
        /// </summary>
        internal static SyntaxTree CreateWithoutClone(LuaSyntaxNode root)
        {
            LorettaDebug.Assert(root != null);

            return new ParsedSyntaxTree(
                textOpt: null,
                encodingOpt: null,
                checksumAlgorithm: SourceHashAlgorithm.Sha1,
                path: "",
                options: LuaParseOptions.Default,
                root: root,
                cloneRoot: false);
        }

        /// <summary>
        /// Produces a syntax tree by parsing the source text.
        /// </summary>
        public static SyntaxTree ParseText(
            string text,
            LuaParseOptions? options = null,
            string path = "",
            Encoding? encoding = null,
            CancellationToken cancellationToken = default) =>
            ParseText(SourceText.From(text, encoding), options, path, cancellationToken);

        /// <summary>
        /// Produces a syntax tree by parsing the source text.
        /// </summary>
        public static SyntaxTree ParseText(
            SourceText text,
            LuaParseOptions? options = null,
            string path = "",
            CancellationToken cancellationToken = default)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            options ??= LuaParseOptions.Default;

            using var lexer = new InternalSyntax.Lexer(text, options);
            using var parser = new InternalSyntax.LanguageParser(lexer, oldTree: null, changes: null, cancellationToken: cancellationToken);
            var compilationUnit = (CompilationUnitSyntax) parser.ParseCompilationUnit().CreateRed();
            var tree = new ParsedSyntaxTree(
                text,
                text.Encoding,
                text.ChecksumAlgorithm,
                path,
                options,
                compilationUnit,
                cloneRoot: true);
            tree.VerifySource();
            return tree;
        }

        #endregion Factories

        #region Changes

        /// <summary>
        /// Creates a new syntax based off this tree using a new source text.
        /// </summary>
        /// <remarks>
        /// If the new source text is a minor change from the current source text an incremental parse will occur
        /// reusing most of the current syntax tree internal data.  Otherwise, a full parse will occur using the new
        /// source text.
        /// </remarks>
        public override SyntaxTree WithChangedText(SourceText newText)
        {
            // try to find the changes between the old text and the new text.
            if (TryGetText(out var oldText))
            {
                var changes = newText.GetChangeRanges(oldText);

                if (changes.Count == 0 && newText == oldText)
                {
                    return this;
                }

                return WithChanges(newText, changes);
            }

            // if we do not easily know the old text, then specify entire text as changed so we do a full reparse.
            return WithChanges(newText, new[] { new TextChangeRange(new TextSpan(0, Length), newText.Length) });
        }

        private SyntaxTree WithChanges(SourceText newText, IReadOnlyList<TextChangeRange> changes)
        {
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }

            var workingChanges = changes;
            var oldTree = this;

            // if changes is entire text do a full reparse
            if (workingChanges.Count == 1 && workingChanges[0].Span == new TextSpan(0, Length) && workingChanges[0].NewLength == newText.Length)
            {
                // parser will do a full parse if we give it no changes
                workingChanges = null;
                oldTree = null;
            }

            using var lexer = new InternalSyntax.Lexer(newText, Options);
            using var parser = new InternalSyntax.LanguageParser(lexer, oldTree?.GetRoot(), workingChanges);

            var compilationUnit = (CompilationUnitSyntax) parser.ParseCompilationUnit().CreateRed();
            var tree = new ParsedSyntaxTree(
                newText,
                newText.Encoding,
                newText.ChecksumAlgorithm,
                FilePath,
                Options,
                compilationUnit,
                cloneRoot: true);
            tree.VerifySource(changes);
            return tree;
        }

        /// <summary>
        /// Produces a pessimistic list of spans that denote the regions of text in this tree that
        /// are changed from the text of the old tree.
        /// </summary>
        /// <param name="oldTree">The old tree. Cannot be <c>null</c>.</param>
        /// <remarks>The list is pessimistic because it may claim more or larger regions than actually changed.</remarks>
        public override IList<TextSpan> GetChangedSpans(SyntaxTree oldTree)
        {
            if (oldTree == null)
            {
                throw new ArgumentNullException(nameof(oldTree));
            }

            return SyntaxDiffer.GetPossiblyDifferentTextSpans(oldTree, this);
        }

        /// <summary>
        /// Gets a list of text changes that when applied to the old tree produce this tree.
        /// </summary>
        /// <param name="oldTree">The old tree. Cannot be <c>null</c>.</param>
        /// <remarks>The list of changes may be different than the original changes that produced this tree.</remarks>
        public override IList<TextChange> GetChanges(SyntaxTree oldTree)
        {
            if (oldTree == null)
            {
                throw new ArgumentNullException(nameof(oldTree));
            }

            return SyntaxDiffer.GetTextChanges(oldTree, this);
        }

        #endregion Changes

        #region LinePositions and Locations

        /// <summary>
        /// Gets the location in terms of path, line and column for a given span.
        /// </summary>
        /// <param name="span">Span within the tree.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="FileLinePositionSpan"/> that contains path, line and column information.
        /// </returns>
        /// <remarks>The values are not affected by line mapping directives (<c>#line</c>).</remarks>
        public override FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default)
        {
            var text = GetText(cancellationToken);
            return new FileLinePositionSpan(FilePath, text.Lines.GetLinePositionSpan(span));
        }

        /// <summary>
        /// Gets a <see cref="Location"/> for the specified text <paramref name="span"/>.
        /// </summary>
        public override Location GetLocation(TextSpan span) => new SourceLocation(this, span);

        #endregion LinePositions and Locations

        #region Diagnostics

        /// <summary>
        /// Gets a list of all the diagnostics in the sub tree that has the specified node as its root.
        /// </summary>
        /// <remarks>
        /// This method does not filter diagnostics based on <c>#pragma</c>s and compiler options
        /// like /nowarn, /warnaserror etc.
        /// </remarks>
        public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return GetDiagnostics(node.Green, node.Position);
        }

        private IEnumerable<Diagnostic> GetDiagnostics(GreenNode greenNode, int position)
        {
            if (greenNode == null)
            {
                throw new InvalidOperationException();
            }

            if (greenNode.ContainsDiagnostics)
            {
                return EnumerateDiagnostics(greenNode, position);
            }

            return SpecializedCollections.EmptyEnumerable<Diagnostic>();
        }

        private IEnumerable<Diagnostic> EnumerateDiagnostics(GreenNode node, int position)
        {
            var enumerator = new SyntaxTreeDiagnosticEnumerator(this, node, position);
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        /// <summary>
        /// Gets a list of all the diagnostics associated with the token and any related trivia.
        /// </summary>
        /// <remarks>
        /// This method does not filter diagnostics based on <c>#pragma</c>s and compiler options
        /// like /nowarn, /warnaserror etc.
        /// </remarks>
        public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxToken token)
        {
            if (token.Node == null)
            {
                throw new InvalidOperationException();
            }
            return GetDiagnostics(token.Node, token.Position);
        }

        /// <summary>
        /// Gets a list of all the diagnostics associated with the trivia.
        /// </summary>
        /// <remarks>
        /// This method does not filter diagnostics based on <c>#pragma</c>s and compiler options
        /// like /nowarn, /warnaserror etc.
        /// </remarks>
        public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxTrivia trivia)
        {
            if (trivia.UnderlyingNode == null)
            {
                throw new InvalidOperationException();
            }
            return GetDiagnostics(trivia.UnderlyingNode, trivia.Position);
        }

        /// <summary>
        /// Gets a list of all the diagnostics in either the sub tree that has the specified node as its root or
        /// associated with the token and its related trivia.
        /// </summary>
        /// <remarks>
        /// This method does not filter diagnostics based on <c>#pragma</c>s and compiler options
        /// like /nowarn, /warnaserror etc.
        /// </remarks>
        public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.UnderlyingNode == null)
            {
                throw new InvalidOperationException();
            }
            return GetDiagnostics(nodeOrToken.UnderlyingNode, nodeOrToken.Position);
        }

        /// <summary>
        /// Gets a list of all the diagnostics in the syntax tree.
        /// </summary>
        /// <remarks>
        /// This method does not filter diagnostics based on <c>#pragma</c>s and compiler options
        /// like /nowarn, /warnaserror etc.
        /// </remarks>
        public override IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default) =>
            GetDiagnostics(GetRoot(cancellationToken));

        #endregion Diagnostics

        #region SyntaxTree

        /// <inheritdoc/>
        protected override SyntaxNode GetRootCore(CancellationToken cancellationToken) =>
            GetRoot(cancellationToken);

        /// <inheritdoc/>
        protected override async Task<SyntaxNode> GetRootAsyncCore(CancellationToken cancellationToken) =>
            await GetRootAsync(cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        protected override bool TryGetRootCore([NotNullWhen(true)] out SyntaxNode? root) =>
            TryGetRoot(out root);

        /// <inheritdoc/>
        protected override ParseOptions OptionsCore => Options;

        #endregion SyntaxTree
    }
}
