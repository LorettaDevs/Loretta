// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// The compilation object is an immutable representation of a single invocation of the
    /// compiler. Although immutable, a compilation is also on-demand, and will realize and cache
    /// data as necessary. A compilation can produce a new compilation from existing compilation
    /// with the application of small deltas. In many cases, it is more efficient than creating a
    /// new compilation from scratch, as the new compilation can reuse information from the old
    /// compilation.
    /// </summary>
    public abstract partial class Compilation
    {
        // Protected for access in CSharpCompilation.WithAdditionalFeatures
        protected readonly IReadOnlyDictionary<string, string> _features;

        internal Compilation(
            IReadOnlyDictionary<string, string> features)
        {
            RoslynDebug.Assert(features != null);

            _features = features;
        }

        protected static IReadOnlyDictionary<string, string> SyntaxTreeCommonFeatures(IEnumerable<SyntaxTree> trees)
        {
            IReadOnlyDictionary<string, string>? set = null;

            foreach (var tree in trees)
            {
                var treeFeatures = tree.Options.Features;
                if (set == null)
                {
                    set = treeFeatures;
                }
                else
                {
                    if ((object) set != treeFeatures && !set.SetEquals(treeFeatures))
                    {
                        throw new ArgumentException(CodeAnalysisResources.InconsistentSyntaxTreeFeature, nameof(trees));
                    }
                }
            }

            if (set == null)
            {
                // Edge case where there are no syntax trees
                set = ImmutableDictionary<string, string>.Empty;
            }

            return set;
        }

        /// <summary>
        /// Gets the source language ("C#" or "Visual Basic").
        /// </summary>
        public abstract string Language { get; }

        /// <summary>
        /// Checks options passed to submission compilation constructor.
        /// Throws an exception if the options are not applicable to submissions.
        /// </summary>
        internal static void CheckSubmissionOptions(CompilationOptions? options)
        {
            if (options == null)
            {
                return;
            }
        }

        /// <summary>
        /// Creates a new compilation equivalent to this one with different symbol instances.
        /// </summary>
        public Compilation Clone() => CommonClone();

        protected abstract Compilation CommonClone();

        internal string? Feature(string p) => _features.TryGetValue(p, out var v) ? v : null;

        #region Options

        /// <summary>
        /// Gets the options the compilation was created with.
        /// </summary>
        public CompilationOptions Options => CommonOptions;

        protected abstract CompilationOptions CommonOptions { get; }

        /// <summary>
        /// Creates a new compilation with the specified compilation options.
        /// </summary>
        /// <param name="options">The new options.</param>
        /// <returns>A new compilation.</returns>
        public Compilation WithOptions(CompilationOptions options) => CommonWithOptions(options);

        protected abstract Compilation CommonWithOptions(CompilationOptions options);

        #endregion

        #region Syntax Trees

        /// <summary>
        /// Gets the syntax trees (parsed from source code) that this compilation was created with.
        /// </summary>
        public IEnumerable<SyntaxTree> SyntaxTrees => CommonSyntaxTrees;
        protected abstract IEnumerable<SyntaxTree> CommonSyntaxTrees { get; }

        /// <summary>
        /// Creates a new compilation with additional syntax trees.
        /// </summary>
        /// <param name="trees">The new syntax trees.</param>
        /// <returns>A new compilation.</returns>
        public Compilation AddSyntaxTrees(params SyntaxTree[] trees) => CommonAddSyntaxTrees(trees);

        /// <summary>
        /// Creates a new compilation with additional syntax trees.
        /// </summary>
        /// <param name="trees">The new syntax trees.</param>
        /// <returns>A new compilation.</returns>
        public Compilation AddSyntaxTrees(IEnumerable<SyntaxTree> trees) => CommonAddSyntaxTrees(trees);

        protected abstract Compilation CommonAddSyntaxTrees(IEnumerable<SyntaxTree> trees);

        /// <summary>
        /// Creates a new compilation without the specified syntax trees. Preserves metadata info for use with trees
        /// added later.
        /// </summary>
        /// <param name="trees">The new syntax trees.</param>
        /// <returns>A new compilation.</returns>
        public Compilation RemoveSyntaxTrees(params SyntaxTree[] trees) => CommonRemoveSyntaxTrees(trees);

        /// <summary>
        /// Creates a new compilation without the specified syntax trees. Preserves metadata info for use with trees
        /// added later.
        /// </summary>
        /// <param name="trees">The new syntax trees.</param>
        /// <returns>A new compilation.</returns>
        public Compilation RemoveSyntaxTrees(IEnumerable<SyntaxTree> trees) => CommonRemoveSyntaxTrees(trees);

        protected abstract Compilation CommonRemoveSyntaxTrees(IEnumerable<SyntaxTree> trees);

        /// <summary>
        /// Creates a new compilation without any syntax trees. Preserves metadata info for use with
        /// trees added later.
        /// </summary>
        public Compilation RemoveAllSyntaxTrees() => CommonRemoveAllSyntaxTrees();

        protected abstract Compilation CommonRemoveAllSyntaxTrees();

        /// <summary>
        /// Creates a new compilation with an old syntax tree replaced with a new syntax tree.
        /// Reuses metadata from old compilation object.
        /// </summary>
        /// <param name="newTree">The new tree.</param>
        /// <param name="oldTree">The old tree.</param>
        /// <returns>A new compilation.</returns>
        public Compilation ReplaceSyntaxTree(SyntaxTree oldTree, SyntaxTree newTree) => CommonReplaceSyntaxTree(oldTree, newTree);

        protected abstract Compilation CommonReplaceSyntaxTree(SyntaxTree oldTree, SyntaxTree newTree);

        /// <summary>
        /// Returns true if this compilation contains the specified tree. False otherwise.
        /// </summary>
        /// <param name="syntaxTree">A syntax tree.</param>
        public bool ContainsSyntaxTree(SyntaxTree syntaxTree) => CommonContainsSyntaxTree(syntaxTree);

        protected abstract bool CommonContainsSyntaxTree(SyntaxTree? syntaxTree);

        #endregion

        #region Diagnostics

        internal const CompilationStage DefaultDiagnosticsStage = CompilationStage.Compile;

        /// <summary>
        /// Gets the diagnostics produced during the parsing stage.
        /// </summary>
        public abstract ImmutableArray<Diagnostic> GetParseDiagnostics(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the diagnostics produced during symbol declaration.
        /// </summary>
        public abstract ImmutableArray<Diagnostic> GetDeclarationDiagnostics(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the diagnostics produced during the analysis of method bodies and field initializers.
        /// </summary>
        public abstract ImmutableArray<Diagnostic> GetMethodBodyDiagnostics(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all the diagnostics for the compilation, including syntax, declaration, and
        /// binding. Does not include any diagnostics that might be produced during emit, see
        /// <see cref="EmitResult"/>.
        /// </summary>
        public abstract ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default);

        internal abstract void GetDiagnostics(CompilationStage stage, bool includeEarlierStages, DiagnosticBag diagnostics, CancellationToken cancellationToken = default);

        internal abstract CommonMessageProvider MessageProvider { get; }

        /// <summary>
        /// Filter out warnings based on the compiler options (/nowarn, /warn and /warnaserror) and the pragma warning directives.
        /// 'incoming' is freed.
        /// </summary>
        /// <param name="accumulator">Bag to which filtered diagnostics will be added.</param>
        /// <param name="incoming">Diagnostics to be filtered.</param>
        /// <returns>True if there are no unsuppressed errors (i.e., no errors which fail compilation).</returns>
        internal bool FilterAndAppendAndFreeDiagnostics(DiagnosticBag accumulator, [DisallowNull] ref DiagnosticBag? incoming, CancellationToken cancellationToken)
        {
            RoslynDebug.Assert(incoming is object);
            var result = FilterAndAppendDiagnostics(accumulator, incoming.AsEnumerableWithoutResolution(), exclude: null, cancellationToken);
            incoming.Free();
            incoming = null;
            return result;
        }

        /// <summary>
        /// Filter out warnings based on the compiler options (/nowarn, /warn and /warnaserror) and the pragma warning directives.
        /// </summary>
        /// <returns>True if there are no unsuppressed errors (i.e., no errors which fail compilation).</returns>
        internal bool FilterAndAppendDiagnostics(DiagnosticBag accumulator, IEnumerable<Diagnostic> incoming, HashSet<int>? exclude, CancellationToken cancellationToken)
        {
            var hasError = false;
            var reportSuppressedDiagnostics = Options.ReportSuppressedDiagnostics;

            foreach (var d in incoming)
            {
                if (exclude?.Contains(d.Code) == true)
                {
                    continue;
                }

                var filtered = Options.FilterDiagnostic(d, cancellationToken);
                if (filtered == null ||
                    (!reportSuppressedDiagnostics && filtered.IsSuppressed))
                {
                    continue;
                }
                else if (filtered.IsUnsuppressableError())
                {
                    hasError = true;
                }

                accumulator.Add(filtered);
            }

            return !hasError;
        }

        #endregion

        /// <summary>
        /// The compiler needs to define an ordering among different partial class in different syntax trees
        /// in some cases, because emit order for fields in structures, for example, is semantically important.
        /// This function defines an ordering among syntax trees in this compilation.
        /// </summary>
        internal int CompareSyntaxTreeOrdering(SyntaxTree tree1, SyntaxTree tree2)
        {
            if (tree1 == tree2)
            {
                return 0;
            }

            Debug.Assert(ContainsSyntaxTree(tree1));
            Debug.Assert(ContainsSyntaxTree(tree2));

            return GetSyntaxTreeOrdinal(tree1) - GetSyntaxTreeOrdinal(tree2);
        }

        internal abstract int GetSyntaxTreeOrdinal(SyntaxTree tree);

        /// <summary>
        /// Compare two source locations, using their containing trees, and then by Span.First within a tree.
        /// Can be used to get a total ordering on declarations, for example.
        /// </summary>
        internal abstract int CompareSourceLocations(Location loc1, Location loc2);

        /// <summary>
        /// Compare two source locations, using their containing trees, and then by Span.First within a tree.
        /// Can be used to get a total ordering on declarations, for example.
        /// </summary>
        internal abstract int CompareSourceLocations(SyntaxReference loc1, SyntaxReference loc2);

        /// <summary>
        /// Return the lexically first of two locations.
        /// </summary>
        internal TLocation FirstSourceLocation<TLocation>(TLocation first, TLocation second)
            where TLocation : Location
        {
            if (CompareSourceLocations(first, second) <= 0)
            {
                return first;
            }
            else
            {
                return second;
            }
        }

        /// <summary>
        /// Return the lexically first of multiple locations.
        /// </summary>
        internal TLocation? FirstSourceLocation<TLocation>(ImmutableArray<TLocation> locations)
            where TLocation : Location
        {
            if (locations.IsEmpty)
            {
                return null;
            }

            var result = locations[0];

            for (var i = 1; i < locations.Length; i++)
            {
                result = FirstSourceLocation(result, locations[i]);
            }

            return result;
        }
    }
}
