// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Text
{
    /// <summary>
    /// Describes a single change when a particular span is replaced with a new text.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    public readonly struct TextChange : IEquatable<TextChange>
    {
        /// <summary>
        /// The original span of the changed text. 
        /// </summary>
        [DataMember(Order = 0)]
        public TextSpan Span { get; }

        /// <summary>
        /// The new text.
        /// </summary>
        [DataMember(Order = 1)]
        public string? NewText { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="TextChange"/>
        /// </summary>
        /// <param name="span">The original span of the changed text.</param>
        /// <param name="newText">The new text.</param>
        public TextChange(TextSpan span, string newText)
            : this()
        {
            if (newText == null)
            {
                throw new ArgumentNullException(nameof(newText));
            }

            Span = span;
            NewText = newText;
        }

        /// <summary>
        /// Provides a string representation for <see cref="TextChange"/>.
        /// </summary>
        public override string ToString() =>
            string.Format("{0}: {{ {1}, \"{2}\" }}", GetType().Name, Span, NewText);

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is TextChange change && Equals(change);

        /// <inheritdoc/>
        public bool Equals(TextChange other)
        {
            return
                EqualityComparer<TextSpan>.Default.Equals(Span, other.Span) &&
                EqualityComparer<string?>.Default.Equals(NewText, other.NewText);
        }

        /// <inheritdoc/>
        public override int GetHashCode() =>
            Hash.Combine(Span.GetHashCode(), NewText?.GetHashCode() ?? 0);

        /// <summary>
        /// Checks whether two text changes are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(TextChange left, TextChange right) =>
            left.Equals(right);

        /// <summary>
        /// Checks whether two text changes are not equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(TextChange left, TextChange right) =>
            !(left == right);

        /// <summary>
        /// Converts a <see cref="TextChange"/> to a <see cref="TextChangeRange"/>.
        /// </summary>
        /// <param name="change"></param>
        public static implicit operator TextChangeRange(TextChange change)
        {
            LorettaDebug.Assert(change.NewText is not null);
            return new TextChangeRange(change.Span, change.NewText.Length);
        }

        /// <summary>
        /// An empty set of changes.
        /// </summary>
        public static IReadOnlyList<TextChange> NoChanges => SpecializedCollections.EmptyReadOnlyList<TextChange>();

        internal string GetDebuggerDisplay()
        {
            var newTextDisplay = NewText switch
            {
                null => "null",
                { Length: < 10 } => $"\"{NewText}\"",
                { Length: var length } => $"(NewLength = {length})"
            };
            return $"new TextChange(new TextSpan({Span.Start}, {Span.Length}), {newTextDisplay})";
        }
    }
}
