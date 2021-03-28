// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Represents parse options common to C# and VB.
    /// </summary>
    public abstract class ParseOptions
    {
        private readonly Lazy<ImmutableArray<Diagnostic>> _lazyErrors;

        /// <summary>
        /// Gets a value indicating whether the documentation comments are parsed.
        /// </summary>
        /// <value><c>true</c> if documentation comments are parsed, <c>false</c> otherwise.</value>
        public DocumentationMode DocumentationMode { get; protected set; }

        internal ParseOptions(DocumentationMode documentationMode)
        {
            DocumentationMode = documentationMode;

            _lazyErrors = new Lazy<ImmutableArray<Diagnostic>>(() =>
            {
                var builder = ArrayBuilder<Diagnostic>.GetInstance();
                ValidateOptions(builder);
                return builder.ToImmutableAndFree();
            });
        }

        /// <summary>
        /// Gets the source language ("Lua").
        /// </summary>
        public abstract string Language { get; }

        /// <summary>
        /// Errors collection related to an incompatible set of parse options
        /// </summary>
        public ImmutableArray<Diagnostic> Errors => _lazyErrors.Value;

        /// <summary>
        /// Performs validation of options compatibilities and generates diagnostics if needed
        /// </summary>
        internal abstract void ValidateOptions(ArrayBuilder<Diagnostic> builder);

        internal void ValidateOptions(ArrayBuilder<Diagnostic> builder, CommonMessageProvider messageProvider)
        {
            if (!DocumentationMode.IsValid())
            {
                builder.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_BadDocumentationMode, Location.None, DocumentationMode.ToString()));
            }
        }

        /// <summary>
        /// Creates a new options instance with the specified documentation mode.
        /// </summary>
        public ParseOptions WithDocumentationMode(DocumentationMode documentationMode) => CommonWithDocumentationMode(documentationMode);

        protected abstract ParseOptions CommonWithDocumentationMode(DocumentationMode documentationMode);

        /// <summary>
        /// Enable some experimental language features for testing.
        /// </summary>
        public ParseOptions WithFeatures(IEnumerable<KeyValuePair<string, string>> features) => CommonWithFeatures(features);

        protected abstract ParseOptions CommonWithFeatures(IEnumerable<KeyValuePair<string, string>> features);

        /// <summary>
        /// Returns the experimental features.
        /// </summary>
        public abstract IReadOnlyDictionary<string, string> Features
        {
            get;
        }

        /// <summary>
        /// Names of defined preprocessor symbols.
        /// </summary>
        public abstract IEnumerable<string> PreprocessorSymbolNames { get; }

        public abstract override bool Equals(object? obj);

        protected bool EqualsHelper([NotNullWhen(true)] ParseOptions? other)
        {
            if (other is null)
            {
                return false;
            }

            return
                DocumentationMode == other.DocumentationMode &&
                Features.SequenceEqual(other.Features) &&
                (PreprocessorSymbolNames == null ? other.PreprocessorSymbolNames == null : PreprocessorSymbolNames.SequenceEqual(other.PreprocessorSymbolNames, StringComparer.Ordinal));
        }

        public abstract override int GetHashCode();

        protected int GetHashCodeHelper()
        {
            return HashCode.Combine(
                DocumentationMode,
                HashFeatures(Features),
                Hash.CombineValues(PreprocessorSymbolNames, StringComparer.Ordinal));
        }

        private static int HashFeatures(IReadOnlyDictionary<string, string> features)
        {
            var hash = new HashCode();
            foreach (var kv in features)
            {
                hash.Add(kv.Key); hash.Add(kv.Value);
            }
            return hash.ToHashCode();
        }

        public static bool operator ==(ParseOptions? left, ParseOptions? right) => object.Equals(left, right);

        public static bool operator !=(ParseOptions? left, ParseOptions? right) => !object.Equals(left, right);
    }
}
