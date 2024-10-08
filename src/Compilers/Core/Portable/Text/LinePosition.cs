﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace Loretta.CodeAnalysis.Text
{
    /// <summary>
    /// Immutable representation of a line number and position within a SourceText instance.
    /// </summary>
    [DataContract]
    public readonly struct LinePosition : IEquatable<LinePosition>, IComparable<LinePosition>
    {
        /// <summary>
        /// A <see cref="LinePosition"/> that represents position 0 at line 0.
        /// </summary>
        public static LinePosition Zero => default;

        [DataMember(Order = 0)]
        private readonly int _line;

        [DataMember(Order = 1)]
        private readonly int _character;

        /// <summary>
        /// Initializes a new instance of a <see cref="LinePosition"/> with the given line and character.
        /// </summary>
        /// <param name="line">
        /// The line of the line position. The first line in a file is defined as line 0 (zero based line numbering).
        /// </param>
        /// <param name="character">
        /// The character position in the line.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="line"/> or <paramref name="character"/> is less than zero. </exception>
        public LinePosition(int line, int character)
        {
            if (line < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(line));
            }

            if (character < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(character));
            }

            _line = line;
            _character = character;
        }

        // internal constructor that supports a line number == -1.
        // VB allows users to specify a 1-based line number of 0 when processing
        // externalsource directives, which get decremented during conversion to 0-based line numbers.
        // in this case the line number can be -1.
        internal LinePosition(int character)
        {
            if (character < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(character));
            }

            _line = -1;
            _character = character;
        }

        /// <summary>
        /// The line number. The first line in a file is defined as line 0 (zero based line numbering).
        /// </summary>
        public int Line => _line;

        /// <summary>
        /// The character position within the line.
        /// </summary>
        public int Character => _character;

        /// <summary>
        /// Determines whether two <see cref="LinePosition"/> are the same.
        /// </summary>
        public static bool operator ==(LinePosition left, LinePosition right) =>
            left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="LinePosition"/> are different.
        /// </summary>
        public static bool operator !=(LinePosition left, LinePosition right) =>
            !left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="LinePosition"/> are the same.
        /// </summary>
        /// <param name="other">The object to compare.</param>
        public bool Equals(LinePosition other) =>
            other.Line == Line && other.Character == Character;

        /// <summary>
        /// Determines whether two <see cref="LinePosition"/> are the same.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        public override bool Equals(object? obj) =>
            obj is LinePosition position && Equals(position);

        /// <summary>
        /// Provides a hash function for <see cref="LinePosition"/>.
        /// </summary>
        public override int GetHashCode() => Hash.Combine(Line, Character);

        /// <summary>
        /// Provides a string representation for <see cref="LinePosition"/>.
        /// </summary>
        /// <example>0,10</example>
        public override string ToString() => Line + "," + Character;

        /// <inheritdoc/>
        public int CompareTo(LinePosition other)
        {
            int result = _line.CompareTo(other._line);
            return (result != 0) ? result : _character.CompareTo(other.Character);
        }

        /// <summary>
        /// Checks whether one position is located after another.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >(LinePosition left, LinePosition right) =>
            left.CompareTo(right) > 0;

        /// <summary>
        /// Checks whether one position is located after or at the same location as another.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >=(LinePosition left, LinePosition right) =>
            left.CompareTo(right) >= 0;

        /// <summary>
        /// Checks whether one position is located before another.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <(LinePosition left, LinePosition right) =>
            left.CompareTo(right) < 0;

        /// <summary>
        /// Checks whether one position is located before or at the same location as another.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <=(LinePosition left, LinePosition right) =>
            left.CompareTo(right) <= 0;
    }
}
