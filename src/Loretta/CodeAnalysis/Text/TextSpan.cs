using System;

namespace Loretta.CodeAnalysis.Text
{
    /// <summary>
    /// Represents a span of the source text.
    /// </summary>
    public readonly struct TextSpan : IEquatable<TextSpan>
    {
        /// <summary>
        /// Creates a <see cref="TextSpan"/> from its bounds.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when:
        /// <paramref name="start"/> is negative;
        /// <para>-or-</para>
        /// <paramref name="end"/> is less than <paramref name="start"/>.
        /// </exception>
        public static TextSpan FromBounds ( Int32 start, Int32 end )
        {
            if ( start < 0 )
                throw new ArgumentOutOfRangeException ( nameof ( start ), "Start must be positive." );
            if ( end < start )
                throw new ArgumentOutOfRangeException ( nameof ( end ), "End must be greater than start." );

            var length = end - start;
            return new TextSpan ( start, length );
        }

        /// <summary>
        /// Initializes a new text span.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when:
        /// <paramref name="start"/> is negative;
        /// <para>-or-</para>
        /// <paramref name="length"/> is negative.
        /// </exception>
        public TextSpan ( Int32 start, Int32 length )
        {
            if ( start < 0 )
                throw new ArgumentOutOfRangeException ( nameof ( start ), "Start must be positive." );
            if ( length < 0 )
                throw new ArgumentOutOfRangeException ( nameof ( length ), "Length must be positive." );
            this.Start = start;
            this.Length = length;
        }

        /// <summary>
        /// The Span's starting position.
        /// </summary>
        public Int32 Start { get; }

        /// <summary>
        /// The Span's length.
        /// </summary>
        public Int32 Length { get; }

        /// <summary>
        /// The Span's ending position.
        /// </summary>
        public Int32 End => this.Start + this.Length;

        /// <summary>
        /// Checks whether this span overlaps with another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Boolean OverlapsWith ( TextSpan other ) =>
            this.Start < other.End && other.Start < this.End;

        /// <inheritdoc/>
        public override Boolean Equals ( Object? obj ) =>
            obj is TextSpan span && this.Equals ( span );

        /// <inheritdoc/>
        public Boolean Equals ( TextSpan other ) =>
            this.Start == other.Start
            && this.Length == other.Length;

        /// <inheritdoc/>
        public override Int32 GetHashCode ( ) =>
            HashCode.Combine ( this.Start, this.Length );

        /// <inheritdoc/>
        public override String ToString ( ) => $"{this.Start}..{this.End}";

        /// <summary>
        /// Checks whether two text spans are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Boolean operator == ( TextSpan left, TextSpan right ) =>
            left.Equals ( right );

        /// <summary>
        /// Checks whether two text spans are not equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Boolean operator != ( TextSpan left, TextSpan right ) =>
            !( left == right );
    }
}
