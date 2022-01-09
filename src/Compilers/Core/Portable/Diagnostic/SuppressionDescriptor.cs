// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Provides a description about a programmatic suppression of a <see cref="Diagnostic"/> by a <see cref="DiagnosticSuppressor"/>.
    /// </summary>
    public sealed class SuppressionDescriptor : IEquatable<SuppressionDescriptor?>
    {
        /// <summary>
        /// An unique identifier for the suppression.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Identifier of the suppressed diagnostic, i.e. <see cref="Diagnostic.Id"/>.
        /// </summary>
        public string SuppressedDiagnosticId { get; }

        /// <summary>
        /// A localizable justification about the suppression.
        /// </summary>
        public LocalizableString Justification { get; }

        /// <summary>
        /// Create a SuppressionDescriptor, which provides a justification about a programmatic suppression of a <see cref="Diagnostic"/>.
        /// NOTE: For localizable <paramref name="justification"/>,
        /// use constructor overload <see cref="SuppressionDescriptor(string, string, LocalizableString)"/>.
        /// </summary>
        /// <param name="id">A unique identifier for the suppression. For example, suppression ID "SP1001".</param>
        /// <param name="suppressedDiagnosticId">Identifier of the suppressed diagnostic, i.e. <see cref="Diagnostic.Id"/>. For example, compiler warning Id "CS0649".</param>
        /// <param name="justification">Justification for the suppression. For example: "Suppress CS0649 on fields marked with YYY attribute as they are implicitly assigned.".</param>
        public SuppressionDescriptor(
            string id,
            string suppressedDiagnosticId,
            string justification)
            : this(id, suppressedDiagnosticId, (LocalizableString) justification)
        {
        }

        /// <summary>
        /// Create a SuppressionDescriptor, which provides a localizable justification about a programmatic suppression of a <see cref="Diagnostic"/>.
        /// </summary>
        /// <param name="id">A unique identifier for the suppression. For example, suppression ID "SP1001".</param>
        /// <param name="suppressedDiagnosticId">Identifier of the suppressed diagnostic, i.e. <see cref="Diagnostic.Id"/>. For example, compiler warning Id "CS0649".</param>
        /// <param name="justification">Justification for the suppression. For example: "Suppress CS0649 on fields marked with YYY attribute as they are implicitly assigned.".</param>
        public SuppressionDescriptor(
            string id,
            string suppressedDiagnosticId,
            LocalizableString justification)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(CodeAnalysisResources.SuppressionIdCantBeNullOrWhitespace, nameof(id));
            }

            if (string.IsNullOrWhiteSpace(suppressedDiagnosticId))
            {
                throw new ArgumentException(CodeAnalysisResources.DiagnosticIdCantBeNullOrWhitespace, nameof(suppressedDiagnosticId));
            }

            Id = id;
            SuppressedDiagnosticId = suppressedDiagnosticId;
            Justification = justification ?? throw new ArgumentNullException(nameof(justification));
        }

        /// <inheritdoc/>
        public bool Equals(SuppressionDescriptor? other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return
                other != null &&
                Id == other.Id &&
                SuppressedDiagnosticId == other.SuppressedDiagnosticId &&
                Justification.Equals(other.Justification);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as SuppressionDescriptor);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Hash.Combine(Id.GetHashCode(),
                   Hash.Combine(SuppressedDiagnosticId.GetHashCode(), Justification.GetHashCode()));
        }
    }
}
