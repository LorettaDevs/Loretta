// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Loretta.CodeAnalysis.Debugging;
using Loretta.CodeAnalysis.PooledObjects;

namespace Loretta.CodeAnalysis.Text
{
    /// <summary>
    /// An abstraction of source text.
    /// </summary>
    public abstract class SourceText
    {
        private const int CharBufferSize = 32 * 1024;
        private const int CharBufferCount = 5;
        internal const int LargeObjectHeapLimitInChars = 40 * 1024; // 40KB

        private static readonly ObjectPool<char[]> s_charArrayPool = new(() => new char[CharBufferSize], CharBufferCount);

        private readonly SourceHashAlgorithm _checksumAlgorithm;
        private SourceTextContainer? _lazyContainer;
        private TextLineCollection? _lazyLineInfo;
        private ImmutableArray<byte> _lazyChecksum;

        private static readonly Encoding s_utf8EncodingWithNoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);

        /// <summary>internal</summary>
        protected SourceText(ImmutableArray<byte> checksum = default, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1, SourceTextContainer? container = null)
        {
            ValidateChecksumAlgorithm(checksumAlgorithm);

            if (!checksum.IsDefault && checksum.Length != CryptographicHashProvider.GetHashSize(checksumAlgorithm))
            {
                throw new ArgumentException(CodeAnalysisResources.InvalidHash, nameof(checksum));
            }

            _checksumAlgorithm = checksumAlgorithm;
            _lazyChecksum = checksum;
            _lazyContainer = container;
        }

        internal static void ValidateChecksumAlgorithm(SourceHashAlgorithm checksumAlgorithm)
        {
            if (!SourceHashAlgorithms.IsSupportedAlgorithm(checksumAlgorithm))
            {
                throw new ArgumentException(CodeAnalysisResources.UnsupportedHashAlgorithm, nameof(checksumAlgorithm));
            }
        }

        /// <summary>
        /// Constructs a <see cref="SourceText"/> from text in a string.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="encoding">
        /// Encoding of the file that the <paramref name="text"/> was read from or is going to be saved to.
        /// <c>null</c> if the encoding is unspecified.
        /// If the encoding is not specified the resulting <see cref="SourceText"/> isn't debuggable.
        /// If an encoding-less <see cref="SourceText"/> is written to a file a <see cref="Encoding.UTF8"/> shall be used as a default.
        /// </param>
        /// <param name="checksumAlgorithm">
        /// Hash algorithm to use to calculate checksum of the text that's saved to PDB.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="checksumAlgorithm"/> is not supported.</exception>
        public static SourceText From(string text, Encoding? encoding = null, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1)
        {
            if (text is null) throw new ArgumentNullException(nameof(text));
            return new StringText(text, encoding, checksumAlgorithm: checksumAlgorithm);
        }

        /// <summary>
        /// Constructs a <see cref="SourceText"/> from text in a string.
        /// </summary>
        /// <param name="reader">TextReader</param>
        /// <param name="length">length of content from <paramref name="reader"/></param>
        /// <param name="encoding">
        /// Encoding of the file that the <paramref name="reader"/> was read from or is going to be saved to.
        /// <c>null</c> if the encoding is unspecified.
        /// If the encoding is not specified the resulting <see cref="SourceText"/> isn't debuggable.
        /// If an encoding-less <see cref="SourceText"/> is written to a file a <see cref="Encoding.UTF8"/> shall be used as a default.
        /// </param>
        /// <param name="checksumAlgorithm">
        /// Hash algorithm to use to calculate checksum of the text that's saved to PDB.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="checksumAlgorithm"/> is not supported.</exception>
        public static SourceText From(
            TextReader reader,
            int length,
            Encoding? encoding = null,
            SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1)
        {
            if (reader is null) throw new ArgumentNullException(nameof(reader));

            // If the resulting string would end up on the large object heap, then use LargeEncodedText.
            if (length >= LargeObjectHeapLimitInChars)
            {
                return LargeText.Decode(reader, length, encoding, checksumAlgorithm);
            }

            var text = reader.ReadToEnd();
            return From(text, encoding, checksumAlgorithm);
        }

        /// <summary>
        /// Constructs a <see cref="SourceText"/> from stream content.
        /// </summary>
        /// <param name="stream">Stream. The stream must be seekable.</param>
        /// <param name="encoding">
        /// Data encoding to use if the stream doesn't start with Byte Order Mark specifying the encoding.
        /// <see cref="Encoding.UTF8"/> if not specified.
        /// </param>
        /// <param name="checksumAlgorithm">
        /// Hash algorithm to use to calculate checksum of the text that's saved to PDB.
        /// </param>
        /// <param name="throwIfBinaryDetected">If the decoded text contains at least two consecutive NUL
        /// characters, then an <see cref="InvalidDataException"/> is thrown.</param>
        ///
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="stream"/> doesn't support reading or seeking.
        /// <paramref name="checksumAlgorithm"/> is not supported.
        /// </exception>
        /// <exception cref="DecoderFallbackException">If the given encoding is set to use a throwing decoder as a fallback</exception>
        /// <exception cref="InvalidDataException">Two consecutive NUL characters were detected in the decoded text and <paramref name="throwIfBinaryDetected"/> was true.</exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <remarks>Reads from the beginning of the stream. Leaves the stream open.</remarks>
        public static SourceText From(
            Stream stream,
            Encoding? encoding = null,
            SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1,
            bool throwIfBinaryDetected = false)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead)
            {
                throw new ArgumentException(CodeAnalysisResources.StreamMustSupportReadAndSeek, nameof(stream));
            }

            ValidateChecksumAlgorithm(checksumAlgorithm);

            encoding ??= s_utf8EncodingWithNoBOM;

            if (stream.CanSeek)
            {
                // If the resulting string would end up on the large object heap, then use LargeEncodedText.
                if (encoding.GetMaxCharCountOrThrowIfHuge(stream) >= LargeObjectHeapLimitInChars)
                {
                    return LargeText.Decode(stream, encoding, checksumAlgorithm, throwIfBinaryDetected);
                }
            }

            var text = Decode(stream, encoding, out encoding);
            if (throwIfBinaryDetected && IsBinary(text))
            {
                throw new InvalidDataException();
            }

            // We must compute the checksum and embedded text blob now while we still have the original bytes in hand.
            // We cannot re-encode to obtain checksum and blob as the encoding is not guaranteed to round-trip.
            var checksum = CalculateChecksum(stream, checksumAlgorithm);
            return new StringText(text, encoding, checksum, checksumAlgorithm);
        }

        /// <summary>
        /// Constructs a <see cref="SourceText"/> from a byte array.
        /// </summary>
        /// <param name="buffer">The encoded source buffer.</param>
        /// <param name="length">The number of bytes to read from the buffer.</param>
        /// <param name="encoding">
        /// Data encoding to use if the encoded buffer doesn't start with Byte Order Mark.
        /// <see cref="Encoding.UTF8"/> if not specified.
        /// </param>
        /// <param name="checksumAlgorithm">
        /// Hash algorithm to use to calculate checksum of the text that's saved to PDB.
        /// </param>
        /// <param name="throwIfBinaryDetected">If the decoded text contains at least two consecutive NUL
        /// characters, then an <see cref="InvalidDataException"/> is thrown.</param>
        /// <returns>The decoded text.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="length"/> is negative or longer than the <paramref name="buffer"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="checksumAlgorithm"/> is not supported.</exception>
        /// <exception cref="DecoderFallbackException">If the given encoding is set to use a throwing decoder as a fallback</exception>
        /// <exception cref="InvalidDataException">Two consecutive NUL characters were detected in the decoded text and <paramref name="throwIfBinaryDetected"/> was true.</exception>
        public static SourceText From(
            byte[] buffer,
            int length,
            Encoding? encoding = null,
            SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1,
            bool throwIfBinaryDetected = false)
        {
            if (buffer is null) throw new ArgumentNullException(nameof(buffer));
            if (length < 0 || length > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            ValidateChecksumAlgorithm(checksumAlgorithm);

            var text = Decode(buffer, length, encoding ?? s_utf8EncodingWithNoBOM, out encoding);
            if (throwIfBinaryDetected && IsBinary(text))
            {
                throw new InvalidDataException();
            }

            // We must compute the checksum and embedded text blob now while we still have the original bytes in hand.
            // We cannot re-encode to obtain checksum and blob as the encoding is not guaranteed to round-trip.
            var checksum = CalculateChecksum(buffer, 0, length, checksumAlgorithm);
            return new StringText(text, encoding, checksum, checksumAlgorithm);
        }

        /// <summary>
        /// Decode text from a stream.
        /// </summary>
        /// <param name="stream">The stream containing encoded text.</param>
        /// <param name="encoding">The encoding to use if an encoding cannot be determined from the byte order mark.</param>
        /// <param name="actualEncoding">The actual encoding used.</param>
        /// <returns>The decoded text.</returns>
        /// <exception cref="DecoderFallbackException">If the given encoding is set to use a throwing decoder as a fallback</exception>
        private static string Decode(Stream stream, Encoding encoding, out Encoding actualEncoding)
        {
            LorettaDebug.Assert(stream != null);
            LorettaDebug.Assert(encoding != null);
            const int maxBufferSize = 4096;
            var bufferSize = maxBufferSize;

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);

                var length = (int) stream.Length;
                if (length == 0)
                {
                    actualEncoding = encoding;
                    return string.Empty;
                }

                bufferSize = Math.Min(maxBufferSize, length);
            }

            // Note: We are setting the buffer size to 4KB instead of the default 1KB. That's
            // because we can reach this code path for FileStreams and, to avoid FileStream
            // buffer allocations for small files, we may intentionally be using a FileStream
            // with a very small (1 byte) buffer. Using 4KB here matches the default buffer
            // size for FileStream and means we'll still be doing file I/O in 4KB chunks.
            using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: true);
            var text = reader.ReadToEnd();
            actualEncoding = reader.CurrentEncoding;
            return text;
        }

        /// <summary>
        /// Decode text from a byte array.
        /// </summary>
        /// <param name="buffer">The byte array containing encoded text.</param>
        /// <param name="length">The count of valid bytes in <paramref name="buffer"/>.</param>
        /// <param name="encoding">The encoding to use if an encoding cannot be determined from the byte order mark.</param>
        /// <param name="actualEncoding">The actual encoding used.</param>
        /// <returns>The decoded text.</returns>
        /// <exception cref="DecoderFallbackException">If the given encoding is set to use a throwing decoder as a fallback</exception>
        private static string Decode(byte[] buffer, int length, Encoding encoding, out Encoding actualEncoding)
        {
            LorettaDebug.Assert(buffer != null);
            LorettaDebug.Assert(encoding != null);
            actualEncoding = TryReadByteOrderMark(buffer, length, out var preambleLength) ?? encoding;
            return actualEncoding.GetString(buffer, preambleLength, length - preambleLength);
        }

        /// <summary>
        /// Check for occurrence of two consecutive NUL (U+0000) characters.
        /// This is unlikely to appear in genuine text, so it's a good heuristic
        /// to detect binary files.
        /// </summary>
        /// <remarks>
        /// internal for unit testing
        /// </remarks>
        internal static bool IsBinary(string text)
        {
            // PERF: We can advance two chars at a time unless we find a NUL.
            for (var i = 1; i < text.Length;)
            {
                if (text[i] == '\0')
                {
                    if (text[i - 1] == '\0')
                    {
                        return true;
                    }

                    i += 1;
                }
                else
                {
                    i += 2;
                }
            }

            return false;
        }

        /// <summary>
        /// Hash algorithm to use to calculate checksum of the text that's saved to PDB.
        /// </summary>
        public SourceHashAlgorithm ChecksumAlgorithm => _checksumAlgorithm;

        /// <summary>
        /// Encoding of the file that the text was read from or is going to be saved to.
        /// <c>null</c> if the encoding is unspecified.
        /// </summary>
        /// <remarks>
        /// If the encoding is not specified the source isn't debuggable.
        /// If an encoding-less <see cref="SourceText"/> is written to a file a <see cref="Encoding.UTF8"/> shall be used as a default.
        /// </remarks>
        public abstract Encoding? Encoding { get; }

        /// <summary>
        /// The length of the text in characters.
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// The size of the storage representation of the text (in characters).
        /// This can differ from length when storage buffers are reused to represent fragments/subtext.
        /// </summary>
        internal virtual int StorageSize => Length;

        internal virtual ImmutableArray<SourceText> Segments => ImmutableArray<SourceText>.Empty;

        internal virtual SourceText StorageKey => this;

        /// <summary>
        /// Returns a character at given position.
        /// </summary>
        /// <param name="position">The position to get the character from.</param>
        /// <returns>The character.</returns>
        /// <exception cref="ArgumentOutOfRangeException">When position is negative or
        /// greater than <see cref="Length"/>.</exception>
        public abstract char this[int position] { get; }

        /// <summary>
        /// Copy a range of characters from this SourceText to a destination array.
        /// </summary>
        public abstract void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count);

        /// <summary>
        /// The container of this <see cref="SourceText"/>.
        /// </summary>
        public virtual SourceTextContainer Container
        {
            get
            {
                if (_lazyContainer == null)
                {
                    Interlocked.CompareExchange(ref _lazyContainer, new StaticContainer(this), null);
                }

                return _lazyContainer;
            }
        }

        internal void CheckSubSpan(TextSpan span)
        {
            LorettaDebug.Assert(0 <= span.Start && span.Start <= span.End);

            if (span.End > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(span));
            }
        }

        /// <summary>
        /// Gets a <see cref="SourceText"/> that contains the characters in the specified span of this text.
        /// </summary>
        public virtual SourceText GetSubText(TextSpan span)
        {
            CheckSubSpan(span);

            var spanLength = span.Length;
            if (spanLength == 0)
            {
                return From(string.Empty, Encoding, ChecksumAlgorithm);
            }
            else if (spanLength == Length && span.Start == 0)
            {
                return this;
            }
            else
            {
                return new SubText(this, span);
            }
        }

        /// <summary>
        /// Returns a <see cref="SourceText"/> that has the contents of this text including and after the start position.
        /// </summary>
        public SourceText GetSubText(int start)
        {
            if (start < 0 || start > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (start == 0)
            {
                return this;
            }
            else
            {
                return GetSubText(new TextSpan(start, Length - start));
            }
        }

        /// <summary>
        /// Write this <see cref="SourceText"/> to a text writer.
        /// </summary>
        public void Write(TextWriter textWriter, CancellationToken cancellationToken = default) => Write(textWriter, new TextSpan(0, Length), cancellationToken);

        /// <summary>
        /// Write a span of text to a text writer.
        /// </summary>
        public virtual void Write(TextWriter writer, TextSpan span, CancellationToken cancellationToken = default)
        {
            CheckSubSpan(span);

            var buffer = s_charArrayPool.Allocate();
            try
            {
                var offset = span.Start;
                var end = span.End;
                while (offset < end)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var count = Math.Min(buffer.Length, end - offset);
                    CopyTo(offset, buffer, 0, count);
                    writer.Write(buffer, 0, count);
                    offset += count;
                }
            }
            finally
            {
                s_charArrayPool.Free(buffer);
            }
        }

        /// <summary>
        /// Returns the checksum for this text.
        /// </summary>
        /// <returns></returns>
        public ImmutableArray<byte> GetChecksum()
        {
            if (_lazyChecksum.IsDefault)
            {
                using var stream = new SourceTextStream(this, useDefaultEncodingIfNull: true);
                ImmutableInterlocked.InterlockedInitialize(ref _lazyChecksum, CalculateChecksum(stream, _checksumAlgorithm));
            }

            return _lazyChecksum;
        }

        internal static ImmutableArray<byte> CalculateChecksum(byte[] buffer, int offset, int count, SourceHashAlgorithm algorithmId)
        {
            using var algorithm = CryptographicHashProvider.TryGetAlgorithm(algorithmId);
            LorettaDebug.Assert(algorithm != null);
            return ImmutableArray.Create(algorithm.ComputeHash(buffer, offset, count));
        }

        internal static ImmutableArray<byte> CalculateChecksum(Stream stream, SourceHashAlgorithm algorithmId)
        {
            using var algorithm = CryptographicHashProvider.TryGetAlgorithm(algorithmId);
            LorettaDebug.Assert(algorithm != null);
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            return ImmutableArray.Create(algorithm.ComputeHash(stream));
        }

        /// <summary>
        /// Provides a string representation of the SourceText.
        /// </summary>
        public override string ToString() => ToString(new TextSpan(0, Length));

        /// <summary>
        /// Gets a string containing the characters in specified span.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When given span is outside of the text range.</exception>
        public virtual string ToString(TextSpan span)
        {
            CheckSubSpan(span);

            // default implementation constructs text using CopyTo
            var builder = new StringBuilder();
            var buffer = new char[Math.Min(span.Length, 1024)];

            var position = Math.Max(Math.Min(span.Start, Length), 0);
            var length = Math.Min(span.End, Length) - position;

            while (position < Length && length > 0)
            {
                var copyLength = Math.Min(buffer.Length, length);
                CopyTo(position, buffer, 0, copyLength);
                builder.Append(buffer, 0, copyLength);
                length -= copyLength;
                position += copyLength;
            }

            return builder.ToString();
        }

        #region Changes

        /// <summary>
        /// Constructs a new SourceText from this text with the specified changes.
        /// </summary>
        public virtual SourceText WithChanges(IEnumerable<TextChange> changes)
        {
            if (changes is null) throw new ArgumentNullException(nameof(changes));
            if (!changes.Any())
            {
                return this;
            }

            var segments = ArrayBuilder<SourceText>.GetInstance();
            var changeRanges = ArrayBuilder<TextChangeRange>.GetInstance();
            try
            {
                var position = 0;

                foreach (var change in changes)
                {
                    if (change.Span.End > Length)
                        throw new ArgumentException(CodeAnalysisResources.ChangesMustBeWithinBoundsOfSourceText, nameof(changes));

                    // there can be no overlapping changes
                    if (change.Span.Start < position)
                    {
                        // Handle the case of unordered changes by sorting the input and retrying. This is inefficient, but
                        // downstream consumers have been known to hit this case in the past and we want to avoid crashes.
                        // https://github.com/dotnet/roslyn/pull/26339
                        if (change.Span.End <= changeRanges.Last().Span.Start)
                        {
                            changes = (from c in changes
                                       where !c.Span.IsEmpty || c.NewText?.Length > 0
                                       orderby c.Span
                                       select c).ToList();
                            return WithChanges(changes);
                        }

                        throw new ArgumentException(CodeAnalysisResources.ChangesMustNotOverlap, nameof(changes));
                    }

                    var newTextLength = change.NewText?.Length ?? 0;

                    // ignore changes that don't change anything
                    if (change.Span.Length == 0 && newTextLength == 0)
                        continue;

                    // if we've skipped a range, add
                    if (change.Span.Start > position)
                    {
                        var subText = GetSubText(new TextSpan(position, change.Span.Start - position));
                        CompositeText.AddSegments(segments, subText);
                    }

                    if (newTextLength > 0)
                    {
                        var segment = From(change.NewText!, Encoding, ChecksumAlgorithm);
                        CompositeText.AddSegments(segments, segment);
                    }

                    position = change.Span.End;

                    changeRanges.Add(new TextChangeRange(change.Span, newTextLength));
                }

                // no changes actually happened?
                if (position == 0 && segments.Count == 0)
                {
                    return this;
                }

                if (position < Length)
                {
                    var subText = GetSubText(new TextSpan(position, Length - position));
                    CompositeText.AddSegments(segments, subText);
                }

                var newText = CompositeText.ToSourceText(segments, this, adjustSegments: true);
                if (newText != this)
                {
                    return new ChangedText(this, newText, changeRanges.ToImmutable());
                }
                else
                {
                    return this;
                }
            }
            finally
            {
                segments.Free();
                changeRanges.Free();
            }
        }

        /// <summary>
        /// Constructs a new SourceText from this text with the specified changes.
        /// <exception cref="ArgumentException">If any changes are not in bounds of this <see cref="SourceText"/>.</exception>
        /// <exception cref="ArgumentException">If any changes overlap other changes.</exception>
        /// </summary>
        /// <remarks>
        /// Changes do not have to be in sorted order.  However, <see cref="WithChanges(IEnumerable{TextChange})"/> will
        /// perform better if they are.
        /// </remarks>
        public SourceText WithChanges(params TextChange[] changes) => WithChanges((IEnumerable<TextChange>) changes);

        /// <summary>
        /// Returns a new SourceText with the specified span of characters replaced by the new text.
        /// </summary>
        public SourceText Replace(TextSpan span, string newText) => WithChanges(new TextChange(span, newText));

        /// <summary>
        /// Returns a new SourceText with the specified range of characters replaced by the new text.
        /// </summary>
        public SourceText Replace(int start, int length, string newText) => Replace(new TextSpan(start, length), newText);

        /// <summary>
        /// Gets the set of <see cref="TextChangeRange"/> that describe how the text changed
        /// between this text an older version. This may be multiple detailed changes
        /// or a single change encompassing the entire text.
        /// </summary>
        public virtual IReadOnlyList<TextChangeRange> GetChangeRanges(SourceText oldText)
        {
            if (oldText is null) throw new ArgumentNullException(nameof(oldText));
            if (oldText == this)
            {
                return TextChangeRange.NoChanges;
            }
            else
            {
                return ImmutableArray.Create(new TextChangeRange(new TextSpan(0, oldText.Length), Length));
            }
        }

        /// <summary>
        /// Gets the set of <see cref="TextChange"/> that describe how the text changed
        /// between this text and an older version. This may be multiple detailed changes
        /// or a single change encompassing the entire text.
        /// </summary>
        public virtual IReadOnlyList<TextChange> GetTextChanges(SourceText oldText)
        {
            var newPosDelta = 0;

            var ranges = GetChangeRanges(oldText).ToList();
            var textChanges = new List<TextChange>(ranges.Count);

            foreach (var range in ranges)
            {
                var newPos = range.Span.Start + newPosDelta;

                // determine where in the newText this text exists
                string newt;
                if (range.NewLength > 0)
                {
                    var span = new TextSpan(newPos, range.NewLength);
                    newt = ToString(span);
                }
                else
                {
                    newt = string.Empty;
                }

                textChanges.Add(new TextChange(range.Span, newt));

                newPosDelta += range.NewLength - range.Span.Length;
            }

            return textChanges.ToImmutableArrayOrEmpty();
        }

        #endregion

        #region Lines

        /// <summary>
        /// The collection of individual text lines.
        /// </summary>
        public TextLineCollection Lines
        {
            get
            {
                var info = _lazyLineInfo;
                return info ?? Interlocked.CompareExchange(ref _lazyLineInfo, info = GetLinesCore(), null) ?? info;
            }
        }

        internal bool TryGetLines([NotNullWhen(returnValue: true)] out TextLineCollection? lines)
        {
            lines = _lazyLineInfo;
            return lines != null;
        }

        /// <summary>
        /// Called from <see cref="Lines"/> to initialize the <see cref="TextLineCollection"/>. Thereafter,
        /// the collection is cached.
        /// </summary>
        /// <returns>A new <see cref="TextLineCollection"/> representing the individual text lines.</returns>
        protected virtual TextLineCollection GetLinesCore() => new LineInfo(this, ParseLineStarts());

        internal sealed class LineInfo : TextLineCollection
        {
            private readonly SourceText _text;
            private readonly int[] _lineStarts;
            private int _lastLineNumber;

            public LineInfo(SourceText text, int[] lineStarts)
            {
                _text = text;
                _lineStarts = lineStarts;
            }

            public override int Count => _lineStarts.Length;

            public override TextLine this[int index]
            {
                get
                {
                    if (index < 0 || index >= _lineStarts.Length)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    var start = _lineStarts[index];
                    if (index == _lineStarts.Length - 1)
                    {
                        return TextLine.FromSpan(_text, TextSpan.FromBounds(start, _text.Length));
                    }
                    else
                    {
                        var end = _lineStarts[index + 1];
                        return TextLine.FromSpan(_text, TextSpan.FromBounds(start, end));
                    }
                }
            }

            public override int IndexOf(int position)
            {
                if (position < 0 || position > _text.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(position));
                }

                int lineNumber;

                // it is common to ask about position on the same line
                // as before or on the next couple lines
                var lastLineNumber = _lastLineNumber;
                if (position >= _lineStarts[lastLineNumber])
                {
                    var limit = Math.Min(_lineStarts.Length, lastLineNumber + 4);
                    for (var i = lastLineNumber; i < limit; i++)
                    {
                        if (position < _lineStarts[i])
                        {
                            lineNumber = i - 1;
                            _lastLineNumber = lineNumber;
                            return lineNumber;
                        }
                    }
                }

                // Binary search to find the right line
                // if no lines start exactly at position, round to the left
                // EoF position will map to the last line.
                lineNumber = _lineStarts.BinarySearch(position);
                if (lineNumber < 0)
                {
                    lineNumber = (~lineNumber) - 1;
                }

                _lastLineNumber = lineNumber;
                return lineNumber;
            }

            public override TextLine GetLineFromPosition(int position) => this[IndexOf(position)];
        }

        private void EnumerateChars(Action<int, char[], int> action)
        {
            var position = 0;
            var buffer = s_charArrayPool.Allocate();

            var length = Length;
            while (position < length)
            {
                var contentLength = Math.Min(length - position, buffer.Length);
                CopyTo(position, buffer, 0, contentLength);
                action(position, buffer, contentLength);
                position += contentLength;
            }

            // once more with zero length to signal the end
            action(position, buffer, 0);

            s_charArrayPool.Free(buffer);
        }

        private int[] ParseLineStarts()
        {
            // Corner case check
            if (0 == Length)
            {
                return new[] { 0 };
            }

            var lineStarts = ArrayBuilder<int>.GetInstance();
            lineStarts.Add(0); // there is always the first line

            var lastWasCR = false;

            // The following loop goes through every character in the text. It is highly
            // performance critical, and thus inlines knowledge about common line breaks
            // and non-line breaks.
            EnumerateChars((int position, char[] buffer, int length) =>
            {
                var index = 0;
                if (lastWasCR)
                {
                    if (length > 0 && buffer[0] == '\n')
                    {
                        index++;
                    }

                    lineStarts.Add(position + index);
                    lastWasCR = false;
                }

                while (index < length)
                {
                    var c = buffer[index];
                    index++;

                    // Common case - ASCII & not a line break
                    // if (c > '\r' && c <= 127)
                    // if (c >= ('\r'+1) && c <= 127)
                    const uint bias = '\r' + 1;
                    if (unchecked(c - bias) <= (127 - bias))
                    {
                        continue;
                    }

                    // Assumes that the only 2-char line break sequence is CR+LF
                    if (c == '\r')
                    {
                        if (index < length && buffer[index] == '\n')
                        {
                            index++;
                        }
                        else if (index >= length)
                        {
                            lastWasCR = true;
                            continue;
                        }
                    }
                    else if (!TextUtilities.IsAnyLineBreakCharacter(c))
                    {
                        continue;
                    }

                    // next line starts at index
                    lineStarts.Add(position + index);
                }
            });

            return lineStarts.ToArrayAndFree();
        }
        #endregion

        /// <summary>
        /// Compares the content with content of another <see cref="SourceText"/>.
        /// </summary>
        public bool ContentEquals(SourceText other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            // Checksum may be provided by a subclass, which is thus responsible for passing us a true hash.
            var leftChecksum = _lazyChecksum;
            var rightChecksum = other._lazyChecksum;
            if (!leftChecksum.IsDefault && !rightChecksum.IsDefault && Encoding == other.Encoding && ChecksumAlgorithm == other.ChecksumAlgorithm)
            {
                return leftChecksum.SequenceEqual(rightChecksum);
            }

            return ContentEqualsImpl(other);
        }

        /// <summary>
        /// Implements equality comparison of the content of two different instances of <see cref="SourceText"/>.
        /// </summary>
        protected virtual bool ContentEqualsImpl(SourceText other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (Length != other.Length)
            {
                return false;
            }

            var buffer1 = s_charArrayPool.Allocate();
            var buffer2 = s_charArrayPool.Allocate();
            try
            {
                var position = 0;
                while (position < Length)
                {
                    var n = Math.Min(Length - position, buffer1.Length);
                    CopyTo(position, buffer1, 0, n);
                    other.CopyTo(position, buffer2, 0, n);

                    for (var i = 0; i < n; i++)
                    {
                        if (buffer1[i] != buffer2[i])
                        {
                            return false;
                        }
                    }

                    position += n;
                }

                return true;
            }
            finally
            {
                s_charArrayPool.Free(buffer2);
                s_charArrayPool.Free(buffer1);
            }
        }

        /// <summary>
        /// Detect an encoding by looking for byte order marks.
        /// </summary>
        /// <param name="source">A buffer containing the encoded text.</param>
        /// <param name="length">The length of valid data in the buffer.</param>
        /// <param name="preambleLength">The length of any detected byte order marks.</param>
        /// <returns>The detected encoding or null if no recognized byte order mark was present.</returns>
        internal static Encoding? TryReadByteOrderMark(byte[] source, int length, out int preambleLength)
        {
            LorettaDebug.Assert(source != null);
            LorettaDebug.Assert(length <= source.Length);

            if (length >= 2)
            {
                switch (source[0])
                {
                    case 0xFE:
                        if (source[1] == 0xFF)
                        {
                            preambleLength = 2;
                            return Encoding.BigEndianUnicode;
                        }

                        break;

                    case 0xFF:
                        if (source[1] == 0xFE)
                        {
                            preambleLength = 2;
                            return Encoding.Unicode;
                        }

                        break;

                    case 0xEF:
                        if (source[1] == 0xBB && length >= 3 && source[2] == 0xBF)
                        {
                            preambleLength = 3;
                            return Encoding.UTF8;
                        }

                        break;
                }
            }

            preambleLength = 0;
            return null;
        }

        private class StaticContainer : SourceTextContainer
        {
            private readonly SourceText _text;

            public StaticContainer(SourceText text)
            {
                _text = text;
            }

            public override SourceText CurrentText => _text;

            public override event EventHandler<TextChangeEventArgs> TextChanged
            {
                add
                {
                    // do nothing
                }

                remove
                {
                    // do nothing
                }
            }
        }
    }
}
