// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Word = ulong;

namespace Loretta.CodeAnalysis
{
    [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
    internal struct BitVector : IEquatable<BitVector>
    {
        private const Word ZeroWord        = 0;
        private const int  Log2BitsPerWord = 6;

        public const int BitsPerWord = 1 << Log2BitsPerWord;

        // Cannot expose the following two field publicly because this structure is mutable
        // and might become not null/empty, unless we restrict access to it.
        private Word   _bits0;
        private Word[] _bits;
        private int    _capacity;

        private BitVector(Word bits0, Word[] bits, int capacity)
        {
            int requiredWords = WordsForCapacity(capacity);
            LorettaDebug.Assert(requiredWords == 0 || requiredWords <= bits.Length);
            _bits0    = bits0;
            _bits     = bits;
            _capacity = capacity;
            Check();
        }

        public bool Equals(BitVector other)
        {
            // Bit arrays only equal if their underlying sets are of the same size
            return _capacity == other._capacity
                   // and have the same set of bits set
                   && _bits0 == other._bits0
                   && _bits.AsSpan().SequenceEqual(other._bits.AsSpan());
        }

        public override bool Equals(object? obj) => obj is BitVector other && Equals(other);

        public static bool operator ==(BitVector left, BitVector right) => left.Equals(right);

        public static bool operator !=(BitVector left, BitVector right) => !left.Equals(right);

        public override readonly int GetHashCode()
        {
            var bitsHash = _bits0.GetHashCode();

            if (_bits == null) return Hash.Combine(_capacity, bitsHash);
            foreach (var word in _bits)
            {
                bitsHash = Hash.Combine(word.GetHashCode(), bitsHash);
            }

            return Hash.Combine(_capacity, bitsHash);
        }

        private static int WordsForCapacity(int capacity)
        {
            if (capacity <= 0) return 0;
            int lastIndex = (capacity - 1) >> Log2BitsPerWord;
            return lastIndex;
        }

        public readonly int Capacity => _capacity;

        [Conditional("DEBUG_BITARRAY")]
        private readonly void Check()
            => LorettaDebug.Assert(_capacity == 0 || WordsForCapacity(_capacity) <= _bits.Length);

        public void EnsureCapacity(int newCapacity)
        {
            if (newCapacity > _capacity)
            {
                int requiredWords = WordsForCapacity(newCapacity);
                if (requiredWords > _bits.Length) Array.Resize(ref _bits, requiredWords);
                _capacity = newCapacity;
                Check();
            }
            Check();
        }

        public readonly IEnumerable<Word> Words()
        {
            if (_capacity > 0) yield return _bits0;
            for (int i = 0, n = _bits?.Length ?? 0; i < n; i++) yield return _bits![i];
        }

        public readonly IEnumerable<int> TrueBits()
        {
            Check();
            if (_bits0 != 0)
            {
                for (var bit = 0; bit < BitsPerWord; bit++)
                {
                    var mask = (Word) 1 << bit;
                    if ((_bits0 & mask) == 0) continue;
                    if (bit >= _capacity) yield break;
                    yield return bit;
                }
            }
            for (var i = 0; i < _bits.Length; i++)
            {
                var w = _bits[i];
                if (w == 0) continue;
                for (var b = 0; b < BitsPerWord; b++)
                {
                    var mask = ((Word) 1) << b;
                    if ((w & mask) == 0) continue;
                    var bit = ((i + 1) << Log2BitsPerWord) | b;
                    if (bit >= _capacity) yield break;
                    yield return bit;
                }
            }
        }

        /// <summary>
        /// Create BitArray with at least the specified number of bits.
        /// </summary>
        public static BitVector Create(int capacity)
        {
            var requiredWords = WordsForCapacity(capacity);
            var bits          = requiredWords == 0 ? [] : new Word[requiredWords];
            return new BitVector(0, bits, capacity);
        }

        /// <summary>
        /// return a bit array with all bits set from index 0 through bitCount-1
        /// </summary>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static BitVector AllSet(int capacity)
        {
            if (capacity == 0) return Empty;

            var requiredWords                          = WordsForCapacity(capacity);
            var bits                                   = requiredWords == 0 ? [] : new Word[requiredWords];
            var lastWord                               = requiredWords - 1;
            var bits0                                  = ~ZeroWord;
            for (var j = 0; j < lastWord; j++) bits[j] = ~ZeroWord;
            var numTrailingBits                        = capacity & (BitsPerWord - 1);
            if (numTrailingBits > 0)
            {
                LorettaDebug.Assert(numTrailingBits <= BitsPerWord);
                var lastBits = ~(~ZeroWord << numTrailingBits);
                if (lastWord < 0)
                    bits0 = lastBits;
                else
                    bits[lastWord] = lastBits;
            }
            else if (requiredWords > 0)
            {
                bits[lastWord] = ~ZeroWord;
            }

            return new BitVector(bits0, bits, capacity);
        }

        /// <summary>
        /// Make a copy of a bit array.
        /// </summary>
        /// <returns></returns>
        public readonly BitVector Clone()
        {
            Word[] newBits;
            if (_bits is null || _bits.Length == 0)
                newBits = [];
            else
                newBits = (Word[]) _bits.Clone();
            return new BitVector(_bits0, newBits, _capacity);
        }

        /// <summary>
        /// Invert all the bits in the vector.
        /// </summary>
        public void Invert()
        {
            _bits0 = ~_bits0;
            if (_bits is null) return;
            for (var i = 0; i < _bits.Length; i++) _bits[i] = ~_bits[i];
        }

        /// <summary>
        /// Is the given bit array null?
        /// </summary>
        public readonly bool IsNull => _bits == null;

        public static BitVector Null => default;

        public static BitVector Empty { get; } = new(0, [], 0);

        /// <summary>
        /// Modify this bit vector by bitwise AND-ing each element with the other bit vector.
        /// For the purposes of the intersection, any bits beyond the current length will be treated as zeroes.
        /// Return true if any changes were made to the bits of this bit vector.
        /// </summary>
        public bool IntersectWith(in BitVector other)
        {
            var anyChanged  = false;
            var otherLength = other._bits.Length;
            var thisBits    = _bits;
            var thisLength  = thisBits.Length;

            if (otherLength > thisLength) otherLength = thisLength;

            // intersect the inline portion
            {
                var oldV = _bits0;
                var newV = oldV & other._bits0;
                if (newV != oldV)
                {
                    _bits0     = newV;
                    anyChanged = true;
                }
            }
            // intersect up to their common length.
            for (var i = 0; i < otherLength; i++)
            {
                var oldV = thisBits[i];
                var newV = oldV & other._bits[i];
                if (newV == oldV) continue;
                thisBits[i] = newV;
                anyChanged  = true;
            }

            // treat the other bit array as being extended with zeroes
            for (var i = otherLength; i < thisLength; i++)
            {
                if (thisBits[i] == 0) continue;
                thisBits[i] = 0;
                anyChanged  = true;
            }

            Check();
            return anyChanged;
        }

        /// <summary>
        /// Modify this bit vector by <c>|</c>ing each element with the other bit vector.
        /// </summary>
        /// <returns>
        /// True if any bits were set as a result of the union.
        /// </returns>
        public bool UnionWith(in BitVector other)
        {
            var anyChanged = false;

            if (other._capacity > _capacity) EnsureCapacity(other._capacity);

            var oldBits = _bits0;
            _bits0 |= other._bits0;

            if (oldBits != _bits0) anyChanged = true;

            for (var i = 0; i < other._bits.Length; i++)
            {
                oldBits  =  _bits[i];
                _bits[i] |= other._bits[i];

                if (_bits[i] != oldBits) anyChanged = true;
            }

            Check();

            return anyChanged;
        }

        public bool this[int index]
        {
            readonly get
            {
                if (index < 0) throw new IndexOutOfRangeException();
                if (index >= _capacity) return false;

                var i    = (index >> Log2BitsPerWord) - 1;
                var word = i < 0 ? _bits0 : _bits[i];
                return IsTrue(word, index);
            }

            set
            {
                if (index < 0) throw new IndexOutOfRangeException();
                if (index >= _capacity) EnsureCapacity(index + 1);

                var i    = (index >> Log2BitsPerWord) - 1;
                var b    = index & (BitsPerWord - 1);
                var mask = (Word) 1 << b;
                if (i < 0)
                {
                    if (value)
                        _bits0 |= mask;
                    else
                        _bits0 &= ~mask;
                }
                else
                {
                    if (value)
                        _bits[i] |= mask;
                    else
                        _bits[i] &= ~mask;
                }
            }
        }

        public void Clear()
        {
            _bits0 = 0;
            if (_bits != null) Array.Clear(_bits, 0, _bits.Length);
        }

        public static bool IsTrue(Word word, int index)
        {
            int  b    = index & (BitsPerWord - 1);
            Word mask = ((Word) 1) << b;
            return (word & mask) != 0;
        }

        public static int WordsRequired(int capacity)
        {
            if (capacity <= 0) return 0;
            return WordsForCapacity(capacity) + 1;
        }

        internal string GetDebuggerDisplay()
        {
            var value = new char[_capacity];
            for (int i = 0; i < _capacity; i++)
            {
                value[_capacity - i - 1] = this[i] ? '1' : '0';
            }
            return new string(value);
        }
    }
}
