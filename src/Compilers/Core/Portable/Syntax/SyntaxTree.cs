// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// The parsed representation of a source document.
    /// </summary>
    public abstract class SyntaxTree
    {
        /// <summary>
        /// The path of the source document file.
        /// </summary>
        /// <remarks>
        /// If this syntax tree is not associated with a file, this value can be empty.
        /// The path shall not be null.
        /// 
        /// The file doesn't need to exist on disk. The path is opaque to the compiler.
        /// </remarks>
        public abstract string FilePath { get; }

        /// <summary>
        /// Returns true if this syntax tree has a root with SyntaxKind "CompilationUnit".
        /// </summary>
        public abstract bool HasCompilationUnitRoot { get; }

        /// <summary>
        /// The options used by the parser to produce the syntax tree.
        /// </summary>
        public ParseOptions Options => OptionsCore;

        /// <summary>
        /// The options used by the parser to produce the syntax tree.
        /// </summary>
        protected abstract ParseOptions OptionsCore { get; }

        /// <summary>
        /// The length of the text of the syntax tree.
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Gets the syntax tree's text if it is available.
        /// </summary>
        public abstract bool TryGetText([NotNullWhen(true)] out SourceText? text);

        /// <summary>
        /// Gets the text of the source document.
        /// </summary>
        public abstract SourceText GetText(CancellationToken cancellationToken = default);

        /// <summary>
        /// The text encoding of the source document.
        /// </summary>
        public abstract Encoding? Encoding { get; }

        /// <summary>
        /// Gets the text of the source document asynchronously.
        /// </summary>
        /// <remarks>
        /// By default, the work associated with this method will be executed immediately on the current thread.
        /// Implementations that wish to schedule this work differently should override <see cref="GetTextAsync(CancellationToken)"/>.
        /// </remarks>
        public virtual Task<SourceText> GetTextAsync(CancellationToken cancellationToken = default) => Task.FromResult(TryGetText(out var text) ? text : GetText(cancellationToken));

        /// <summary>
        /// Gets the root of the syntax tree if it is available.
        /// </summary>
        public bool TryGetRoot([NotNullWhen(true)] out SyntaxNode? root) => TryGetRootCore(out root);

        /// <summary>
        /// Gets the root of the syntax tree if it is available.
        /// </summary>
        protected abstract bool TryGetRootCore([NotNullWhen(true)] out SyntaxNode? root);

        /// <summary>
        /// Gets the root node of the syntax tree, causing computation if necessary.
        /// </summary>
        public SyntaxNode GetRoot(CancellationToken cancellationToken = default) => GetRootCore(cancellationToken);

        /// <summary>
        /// Gets the root node of the syntax tree, causing computation if necessary.
        /// </summary>
        protected abstract SyntaxNode GetRootCore(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the root node of the syntax tree asynchronously.
        /// </summary>
        public Task<SyntaxNode> GetRootAsync(CancellationToken cancellationToken = default) => GetRootAsyncCore(cancellationToken);

        /// <summary>
        /// Gets the root node of the syntax tree asynchronously.
        /// </summary>
        protected abstract Task<SyntaxNode> GetRootAsyncCore(CancellationToken cancellationToken);

        /// <summary>
        /// Create a new syntax tree based off this tree using a new source text.
        /// 
        /// If the new source text is a minor change from the current source text an incremental
        /// parse will occur reusing most of the current syntax tree internal data.  Otherwise, a
        /// full parse will occur using the new source text.
        /// </summary>
        public abstract SyntaxTree WithChangedText(SourceText newText);

        /// <summary>
        /// Gets a list of all the diagnostics in the syntax tree.
        /// This method does not filter diagnostics based on #pragmas and compiler options
        /// like nowarn, warnaserror etc.
        /// </summary>
        public abstract IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a list of all the diagnostics in the sub tree that has the specified node as its root.
        /// This method does not filter diagnostics based on #pragmas and compiler options
        /// like nowarn, warnaserror etc.
        /// </summary>
        public abstract IEnumerable<Diagnostic> GetDiagnostics(SyntaxNode node);

        /// <summary>
        /// Gets a list of all the diagnostics associated with the token and any related trivia.
        /// This method does not filter diagnostics based on #pragmas and compiler options
        /// like nowarn, warnaserror etc.
        /// </summary>
        public abstract IEnumerable<Diagnostic> GetDiagnostics(SyntaxToken token);

        /// <summary>
        /// Gets a list of all the diagnostics associated with the trivia.
        /// This method does not filter diagnostics based on #pragmas and compiler options
        /// like nowarn, warnaserror etc.
        /// </summary>
        public abstract IEnumerable<Diagnostic> GetDiagnostics(SyntaxTrivia trivia);

        /// <summary>
        /// Gets a list of all the diagnostics in either the sub tree that has the specified node as its root or
        /// associated with the token and its related trivia. 
        /// This method does not filter diagnostics based on #pragmas and compiler options
        /// like nowarn, warnaserror etc.
        /// </summary>
        public abstract IEnumerable<Diagnostic> GetDiagnostics(SyntaxNodeOrToken nodeOrToken);

        /// <summary>
        /// Gets the location in terms of path, line and column for a given span.
        /// </summary>
        /// <param name="span">Span within the tree.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// A valid <see cref="FileLinePositionSpan"/> that contains path, line and column information.
        /// The values are not affected by line mapping directives (<c>#line</c>).
        /// </returns>
        public abstract FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a list of the changed regions between this tree and the specified tree. The list is conservative for
        /// performance reasons. It may return larger regions than what has actually changed.
        /// </summary>
        public abstract IList<TextSpan> GetChangedSpans(SyntaxTree syntaxTree);

        /// <summary>
        /// Gets a location for the specified text span.
        /// </summary>
        public abstract Location GetLocation(TextSpan span);

        /// <summary>
        /// Determines if two trees are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="tree">The tree to compare against.</param>
        /// <param name="topLevel"> If true then the trees are equivalent if the contained nodes and tokens declaring
        /// metadata visible symbolic information are equivalent, ignoring any differences of nodes inside method bodies
        /// or initializer expressions, otherwise all nodes and tokens must be equivalent. 
        /// </param>
        public abstract bool IsEquivalentTo(SyntaxTree tree, bool topLevel = false);

        /// <summary>
        /// Gets a SyntaxReference for a specified syntax node. SyntaxReferences can be used to
        /// regain access to a syntax node without keeping the entire tree and source text in
        /// memory.
        /// </summary>
        public abstract SyntaxReference GetReference(SyntaxNode node);

        /// <summary>
        /// Gets a list of text changes that when applied to the old tree produce this tree.
        /// </summary>
        /// <param name="oldTree">The old tree.</param>
        /// <remarks>The list of changes may be different than the original changes that produced
        /// this tree.</remarks>
        public abstract IList<TextChange> GetChanges(SyntaxTree oldTree);

        /// <summary>
        /// Returns a new tree whose root and options are as specified and other properties are copied from the current tree.
        /// </summary>
        public abstract SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options);

        /// <summary>
        /// Returns a new tree whose <see cref="FilePath"/> is the specified node and other properties are copied from the current tree.
        /// </summary>
        public abstract SyntaxTree WithFilePath(string path);

        /// <summary>
        /// Returns a <see cref="string" /> that represents the entire source text of this <see cref="SyntaxTree"/>.
        /// </summary>
        public override string ToString() => GetText(CancellationToken.None).ToString();

        internal virtual bool SupportsLocations => HasCompilationUnitRoot;
    }
}
