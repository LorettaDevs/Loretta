// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Represents compilation options common to C# and VB.
    /// </summary>
    public abstract class CompilationOptions
    {
        /// <summary>
        /// Name of the primary module, or null if a default name should be used.
        /// </summary>
        /// <remarks>
        /// The name usually (but not necessarily) includes an extension, e.g. "MyModule.dll".
        /// 
        /// If <see cref="ModuleName"/> is null the actual name written to metadata  
        /// is derived from the name of the compilation (<see cref="Compilation.AssemblyName"/>)
        /// by appending a default extension for <see cref="OutputKind"/>.
        /// </remarks>
        public string? ModuleName { get; protected set; }

        /// <summary>
        /// Global warning report option
        /// </summary>
        public ReportDiagnostic GeneralDiagnosticOption { get; protected set; }

        /// <summary>
        /// Global warning level (a non-negative integer).
        /// </summary>
        public int WarningLevel { get; protected set; }

        /// <summary>
        /// Specifies whether building compilation may use multiple threads.
        /// </summary>
        public bool ConcurrentBuild { get; protected set; }

        /// <summary>
        /// Specifies whether the compilation should be deterministic.
        /// </summary>
        public bool Deterministic { get; protected set; }

        /// <summary>
        /// Used for time-based version generation when <see cref="System.Reflection.AssemblyVersionAttribute"/> contains a wildcard.
        /// If equal to default(<see cref="DateTime"/>) the actual current local time will be used.
        /// </summary>
        internal DateTime CurrentLocalTime { get; private protected set; }

        /// <summary>
        /// Emit mode that favors debuggability. 
        /// </summary>
        internal bool DebugPlusMode { get; set; }

        /// <summary>
        /// Modifies the incoming diagnostic, for example escalating its severity, or discarding it (returning null) based on the compilation options.
        /// </summary>
        /// <param name="diagnostic"></param>
        /// <returns>The modified diagnostic, or null</returns>
        internal abstract Diagnostic? FilterDiagnostic(Diagnostic diagnostic, CancellationToken cancellationToken);

        /// <summary>
        /// Warning report option for each warning.
        /// </summary>
        public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; protected set; }

        /// <summary>
        /// Provider to retrieve options for particular syntax trees.
        /// </summary>
        public SyntaxTreeOptionsProvider? SyntaxTreeOptionsProvider { get; protected set; }

        /// <summary>
        /// Whether diagnostics suppressed in source, i.e. <see cref="Diagnostic.IsSuppressed"/> is true, should be reported.
        /// </summary>
        public bool ReportSuppressedDiagnostics { get; protected set; }

        /// <summary>
        /// Gets the resolver for resolving source document references for the compilation.
        /// Null if the compilation is not allowed to contain source file references, such as #line pragmas and #load directives.
        /// </summary>
        public SourceReferenceResolver? SourceReferenceResolver { get; protected set; }

        private readonly Lazy<ImmutableArray<Diagnostic>> _lazyErrors;

        // Expects correct arguments.
        internal CompilationOptions(
            bool reportSuppressedDiagnostics,
            string? moduleName,
            ReportDiagnostic generalDiagnosticOption,
            int warningLevel,
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions,
            bool concurrentBuild,
            bool deterministic,
            DateTime currentLocalTime,
            bool debugPlusMode,
            SourceReferenceResolver? sourceReferenceResolver,
            SyntaxTreeOptionsProvider? syntaxTreeOptionsProvider)
        {
            ModuleName = moduleName;
            GeneralDiagnosticOption = generalDiagnosticOption;
            WarningLevel = warningLevel;
            SpecificDiagnosticOptions = specificDiagnosticOptions;
            ReportSuppressedDiagnostics = reportSuppressedDiagnostics;
            ConcurrentBuild = concurrentBuild;
            Deterministic = deterministic;
            CurrentLocalTime = currentLocalTime;
            DebugPlusMode = debugPlusMode;
            SourceReferenceResolver = sourceReferenceResolver;
            SyntaxTreeOptionsProvider = syntaxTreeOptionsProvider;

            _lazyErrors = new Lazy<ImmutableArray<Diagnostic>>(() =>
            {
                var builder = ArrayBuilder<Diagnostic>.GetInstance();
                ValidateOptions(builder);
                return builder.ToImmutableAndFree();
            });
        }

        internal bool CanReuseCompilationReferenceManager(CompilationOptions other) => true;

        /// <summary>
        /// Gets the source language ("Lua").
        /// </summary>
        public abstract string Language { get; }

        /// <summary>
        /// Creates a new options instance with the specified general diagnostic option.
        /// </summary>
        public CompilationOptions WithGeneralDiagnosticOption(ReportDiagnostic value) => CommonWithGeneralDiagnosticOption(value);

        /// <summary>
        /// Creates a new options instance with the specified diagnostic-specific options.
        /// </summary>
        public CompilationOptions WithSpecificDiagnosticOptions(ImmutableDictionary<string, ReportDiagnostic>? value) => CommonWithSpecificDiagnosticOptions(value);

        /// <summary>
        /// Creates a new options instance with the specified diagnostic-specific options.
        /// </summary>
        public CompilationOptions WithSpecificDiagnosticOptions(IEnumerable<KeyValuePair<string, ReportDiagnostic>> value) => CommonWithSpecificDiagnosticOptions(value);

        /// <summary>
        /// Creates a new options instance with the specified suppressed diagnostics reporting option.
        /// </summary>
        public CompilationOptions WithReportSuppressedDiagnostics(bool value) => CommonWithReportSuppressedDiagnostics(value);

        /// <summary>
        /// Creates a new options instance with the concurrent build property set accordingly.
        /// </summary>
        public CompilationOptions WithConcurrentBuild(bool concurrent) => CommonWithConcurrentBuild(concurrent);

        /// <summary>
        /// Creates a new options instance with the deterministic property set accordingly.
        /// </summary>
        public CompilationOptions WithDeterministic(bool deterministic) => CommonWithDeterministic(deterministic);

        public CompilationOptions WithSourceReferenceResolver(SourceReferenceResolver? resolver) => CommonWithSourceReferenceResolver(resolver);

        public CompilationOptions WithSyntaxTreeOptionsProvider(SyntaxTreeOptionsProvider? provider) => CommonWithSyntaxTreeOptionsProvider(provider);

        public CompilationOptions WithModuleName(string? moduleName) => CommonWithModuleName(moduleName);

        protected abstract CompilationOptions CommonWithConcurrentBuild(bool concurrent);
        protected abstract CompilationOptions CommonWithDeterministic(bool deterministic);
        protected abstract CompilationOptions CommonWithSourceReferenceResolver(SourceReferenceResolver? resolver);
        protected abstract CompilationOptions CommonWithSyntaxTreeOptionsProvider(SyntaxTreeOptionsProvider? resolver);
        protected abstract CompilationOptions CommonWithGeneralDiagnosticOption(ReportDiagnostic generalDiagnosticOption);
        protected abstract CompilationOptions CommonWithSpecificDiagnosticOptions(ImmutableDictionary<string, ReportDiagnostic>? specificDiagnosticOptions);
        protected abstract CompilationOptions CommonWithSpecificDiagnosticOptions(IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions);
        protected abstract CompilationOptions CommonWithReportSuppressedDiagnostics(bool reportSuppressedDiagnostics);
        protected abstract CompilationOptions CommonWithModuleName(string? moduleName);

        /// <summary>
        /// Performs validation of options compatibilities and generates diagnostics if needed
        /// </summary>
        internal abstract void ValidateOptions(ArrayBuilder<Diagnostic> builder);

        /// <summary>
        /// Errors collection related to an incompatible set of compilation options
        /// </summary>
        public ImmutableArray<Diagnostic> Errors => _lazyErrors.Value;

        public abstract override bool Equals(object? obj);

        protected bool EqualsHelper([NotNullWhen(true)] CompilationOptions? other)
        {
            if (other is null)
            {
                return false;
            }

            // NOTE: StringComparison.Ordinal is used for type name comparisons, even for VB.  That's because
            // a change in the canonical case should still change the option.
            var equal =
                   ConcurrentBuild == other.ConcurrentBuild &&
                   Deterministic == other.Deterministic &&
                   CurrentLocalTime == other.CurrentLocalTime &&
                   DebugPlusMode == other.DebugPlusMode &&
                   GeneralDiagnosticOption == other.GeneralDiagnosticOption &&
                   string.Equals(ModuleName, other.ModuleName, StringComparison.Ordinal) &&
                   ReportSuppressedDiagnostics == other.ReportSuppressedDiagnostics &&
                   SpecificDiagnosticOptions.SequenceEqual(other.SpecificDiagnosticOptions, (left, right) => (left.Key == right.Key) && (left.Value == right.Value)) &&
                   WarningLevel == other.WarningLevel &&
                   Equals(SourceReferenceResolver, other.SourceReferenceResolver) &&
                   Equals(SyntaxTreeOptionsProvider, other.SyntaxTreeOptionsProvider);

            return equal;
        }

        public abstract override int GetHashCode();

        protected int GetHashCodeHelper()
        {
            return Hash.Combine(ConcurrentBuild,
                   Hash.Combine(Deterministic,
                   Hash.Combine(CurrentLocalTime.GetHashCode(),
                   Hash.Combine(DebugPlusMode,
                   Hash.Combine((int) GeneralDiagnosticOption,
                   Hash.Combine(ModuleName != null ? StringComparer.Ordinal.GetHashCode(ModuleName) : 0,
                   Hash.Combine(ReportSuppressedDiagnostics,
                   Hash.Combine(Hash.CombineValues(SpecificDiagnosticOptions),
                   Hash.Combine(WarningLevel,
                   Hash.Combine(SourceReferenceResolver,
                   Hash.Combine(SyntaxTreeOptionsProvider, 0)))))))))));
        }

        public static bool operator ==(CompilationOptions? left, CompilationOptions? right) => Equals(left, right);

        public static bool operator !=(CompilationOptions? left, CompilationOptions? right) => !Equals(left, right);
    }
}
