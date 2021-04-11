using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal class SourceTextReader : IDisposable
    {
        // We hold the next 2048 characters in the cache.
        private const int CacheSize = 4096;
        // And we require at least 128 characters available at all times
        // except in the last 2048 characters of the text.
        private const int CacheThreshold = 128;
        private const int CacheSizeWithoutThreshold = CacheSize - CacheThreshold;
        private static ObjectPool<char[]> s_cachePool = new ObjectPool<char[]>(() => new char[CacheSize], 128);

        private readonly SourceText _sourceText;
        private readonly char[] _cache;
        private readonly int _maxCacheStart;
        private int _position = 0;
        private int _cacheStart = -1, _cachePosition = 0, _cacheLength = 0;

        public SourceTextReader(SourceText sourceText)
        {
            _sourceText = sourceText;
            _maxCacheStart = sourceText.Length - CacheSize;
            _cache = s_cachePool.Allocate();
            LoadCache(Position = 0);
        }

        public void Dispose()
        {
            s_cachePool.Free(_cache);
        }

        private void LoadCache(int cacheStart)
        {
            // Don't load the cache if the position hasn't changed.
            if (_cacheStart == cacheStart)
                return;
            _cacheStart = cacheStart;
            _cacheLength = Math.Min(Length - cacheStart, CacheSize);
            Array.Clear(_cache, 0, CacheSize);
            _sourceText.CopyTo(cacheStart, _cache, 0, _cacheLength);
        }

        /// <summary>
        /// The reader's position. Guarantees the position is in the range [0, Length].
        /// </summary>
        public int Position
        {
            get => _position;
            set
            {
                if (_position == value)
                    return;

                _position = Math.Min(Math.Max(value, 0), Length);

                // If we moved behind the cache start
                if (_position < _cacheStart
                    // Or after the cache's threshold and not at the last N characters of the text
                    || _cacheStart + CacheSizeWithoutThreshold <= _position)
                {
                    // Then load the cache with more characters
                    LoadCache(Math.Min(_position, _maxCacheStart));
                }

                _cachePosition = _position - _cacheStart;
            }
        }

        /// <summary>
        /// The length of the text
        /// </summary>
        public int Length => _sourceText.Length;

        private ReadOnlySpan<char> CacheSpan => _cache.AsSpan(_cachePosition, _cacheLength - _cachePosition);

        public char? Peek() => Position < Length ? CacheSpan[0] : null;

        public char? Peek(int offset)
        {
            if (offset < _cacheLength - _cachePosition)
                return CacheSpan[offset];
            var position = Position + offset;
            if (position < Length)
                return _sourceText[position];
            return null;
        }

        public char? Read()
        {
            var ch = Peek();
            Position++;
            return ch;
        }

        public bool IsNext(char ch) => Peek() == ch;

        public bool IsNext(string str)
        {
            var end = Position + str.Length;
            if (end <= _cacheStart + _cacheLength)
                return CacheSpan.StartsWith(str.AsSpan(), StringComparison.Ordinal);

            if (end <= Length)
            {
                return slowIsNext(str);
            }
            return false;

            bool slowIsNext(string str)
            {
                if (str.Length <= CacheSize)
                {
                    var buff = s_cachePool.Allocate();
                    try
                    {
                        _sourceText.CopyTo(Position, buff, 0, str.Length);
                        var span = buff.AsSpan(0, str.Length);
                        return MemoryExtensions.Equals(span, str.AsSpan(), StringComparison.Ordinal);
                    }
                    finally
                    {
                        s_cachePool.Free(buff);
                    }
                }
                else
                {
                    var buff = ArrayPool<char>.Shared.Rent(str.Length);
                    try
                    {
                        _sourceText.CopyTo(Position, buff, 0, buff.Length);
                        return MemoryExtensions.Equals(buff, str.AsSpan(), StringComparison.Ordinal);
                    }
                    finally
                    {
                        ArrayPool<char>.Shared.Return(buff);
                    };
                }
            }
        }

        public bool IsAt(int offset, char ch) => Peek(offset) == ch;

        public void SkipUntilLineBreak()
        {
            // Use the optimized and vectorized impl for the cache
            var offset = CacheSpan.IndexOfAny('\n', '\r');
            if (offset > 0)
                Position += offset;

            // Then if we don't find it in the cache, do the slow approach.
            slowSkipUntilLineBreak();

            void slowSkipUntilLineBreak()
            {
                var buff = s_cachePool.Allocate();
                try
                {
                    for (int idx = _cacheStart + CacheSize + 1, length = CacheSize; idx < Length; length = Math.Min(Length - idx, CacheSize), idx += length)
                    {
                        _sourceText.CopyTo(idx, buff, 0, length);
                        var offset = MemoryExtensions.IndexOfAny(buff.AsSpan(0, length), '\n', '\r');
                        if (offset >= 0)
                        {
                            Position = idx + offset;
                            return;
                        }
                    }
                }
                finally
                {
                    s_cachePool.Free(buff);
                }
            }
        }

        public bool TryGetInternedText(
            StringTable strings,
            int start,
            int length,
            [NotNullWhen(true)] out string? str)
        {
            var startInCache = start - _cacheStart;
            if (_cacheStart <= start && length <= _cacheLength - startInCache)
            {
                str = strings.Add(_cache, startInCache, length);
                return true;
            }

            str = default;
            return false;
        }

        public int IndexOf(string value)
        {
            // Use the optimized and vectorized impl for the cache
            var idx = CacheSpan.IndexOf(value.AsSpan(), StringComparison.Ordinal);
            if (idx > 0)
                return Position + idx;
            // Then if we don't find it in the cache, do the slow approach.
            return slowIndexOf(value);

            bool isAt(int pos, string val)
            {
                switch (val.Length)
                {
                    case 1: return _sourceText[pos] == val[0];
                    case 2: return _sourceText[pos] == val[0] && _sourceText[pos + 1] == val[1];
                    case 3: return _sourceText[pos] == val[0] && _sourceText[pos + 1] == val[1] && _sourceText[pos + 2] == val[2];
                    default:
                        for (var idx = 0; idx < val.Length; idx++)
                        {
                            if (_sourceText[pos + idx] != val[idx])
                                return false;
                        }
                        return true;
                }
            }

            int slowIndexOf(string value)
            {
                for (var idx = _cacheStart + CacheSize + 1; idx < Length - value.Length; idx++)
                {
                    if (isAt(idx, value))
                    {
                        // The index starts with the stop, so skip until before it.
                        return idx;
                    }
                }
                return -1;
            }
        }

        public int IndexOfFirstNotMatching(Func<char, bool> predicate)
        {
            for (var idx = 0; idx < _cacheLength - _cachePosition; idx++)
            {
                if (!predicate(CacheSpan[idx]))
                    return Position + idx;
            }
            for (var idx = _cacheStart + CacheSize; idx < Length; idx++)
            {
                if (!predicate(_sourceText[idx]))
                    return idx;
            }
            return -1;
        }

        public void SkipUntil(string stop)
        {
            var idx = IndexOf(stop);
            if (idx < 0)
                Position = Length;
            Position = idx;
        }

        public void SkipWhile(Func<char, bool> predicate)
        {
            var idx = IndexOfFirstNotMatching(predicate);
            if (idx < 0)
                Position = Length;
            Position = idx;
        }

        public string ReadStringUntil(string stop)
        {
            var idx = IndexOf(stop);
            if (idx < 0)
                return ReadToEnd();

            string value;
            var length = idx - Position;
            if (length < _cacheLength - _cachePosition)
                value = CacheSpan.Slice(0, length).ToString();
            else
                value = _sourceText.ToString(new TextSpan(Position, length));
            Position = idx;
            return value;
        }

        public string ReadStringWhile(Func<char, bool> predicate)
        {
            var strEnd = IndexOfFirstNotMatching(predicate);
            if (strEnd < 0)
                return ReadToEnd();

            string value;
            var length = strEnd - Position;
            if (strEnd < _cacheStart + _cacheLength)
                value = CacheSpan.Slice(0, length).ToString();
            else
                value = _sourceText.ToString(new TextSpan(Position, length));
            Position = strEnd;
            return value;
        }

        public string ReadToEnd()
        {
            var length = Length - Position;
            string value;
            if (_cacheStart == _maxCacheStart)
                value = CacheSpan.Slice(0, length).ToString();
            else
                value = _sourceText.ToString(new TextSpan(Position, length));
            Position = Length;
            return value;
        }
    }
}
