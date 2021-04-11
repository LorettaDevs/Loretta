using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal class SourceTextReader
    {
        private readonly SourceText _sourceText;
        private int _position = 0;

        public SourceTextReader(SourceText sourceText)
        {
            _sourceText = sourceText;
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
            }
        }

        /// <summary>
        /// The length of the text
        /// </summary>
        public int Length => _sourceText.Length;

        public char? Peek() => Position < Length ? _sourceText[Position] : null;

        public char? Peek(int offset)
        {
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
            if (Position + str.Length > Length)
                return false;

            var buff = ArrayPool<char>.Shared.Rent(str.Length);
            try
            {
                _sourceText.CopyTo(Position, buff, 0, str.Length);
                return MemoryExtensions.Equals(buff.AsSpan(0, str.Length), str.AsSpan(), StringComparison.Ordinal);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buff);
            };
        }

        public bool IsAt(int offset, char ch) => Peek(offset) == ch;

        public void SkipUntilLineBreak()
        {
            for (var idx = Position; idx < Length; idx++)
            {
                if (_sourceText[idx] is '\n' or '\r')
                {
                    Position = idx;
                    return;
                }
            }

            Position = Length;
            return;
        }

        public int IndexOf(string value)
        {
            for (var idx = Position; idx <= Length - value.Length; idx++)
            {
                if (isAt(idx, value))
                {
                    // The index starts with the stop, so skip until before it.
                    return idx;
                }
            }
            return -1;

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
        }

        public int IndexOfFirstNotMatching(Func<char, bool> predicate)
        {
            for (var idx = Position; idx < Length; idx++)
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
            else
                Position = idx;
        }

        public void SkipWhile(Func<char, bool> predicate)
        {
            var idx = IndexOfFirstNotMatching(predicate);
            if (idx < 0)
                Position = Length;
            else
                Position = idx;
        }

        public string ReadStringUntil(string stop)
        {
            var idx = IndexOf(stop);
            if (idx < 0)
                return ReadToEnd();

            var length = idx - Position;
            string value = _sourceText.ToString(new TextSpan(Position, length));
            Position = idx;
            return value;
        }

        public string ReadStringWhile(Func<char, bool> predicate)
        {
            var strEnd = IndexOfFirstNotMatching(predicate);
            if (strEnd < 0)
                return ReadToEnd();

            var length = strEnd - Position;
            string value = _sourceText.ToString(new TextSpan(Position, length));
            Position = strEnd;
            return value;
        }

        public string ReadToEnd()
        {
            var length = Length - Position;
            string value = _sourceText.ToString(new TextSpan(Position, length));
            Position = Length;
            return value;
        }
    }
}
