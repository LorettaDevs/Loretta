// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Text;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Collections;
using Loretta.CodeAnalysis.PooledObjects;

namespace Loretta.Utilities
{
    using EncodingKind = ObjectWriter.EncodingKind;
    using Resources = CodeAnalysisResources;

    /// <summary>
    /// An <see cref="ObjectReader"/> that deserializes objects from a byte stream.
    /// </summary>
    internal sealed partial class ObjectReader : IDisposable
    {
        /// <summary>
        /// We start the version at something reasonably random.  That way an older file, with 
        /// some random start-bytes, has little chance of matching our version.  When incrementing
        /// this version, just change VersionByte2.
        /// </summary>
        internal const byte VersionByte1 = 0b10101010;
        internal const byte VersionByte2 = 0b00001011;

        private readonly BinaryReader _reader;
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Map of reference id's to deserialized objects.
        /// </summary>
        private readonly ReaderReferenceMap<object> _objectReferenceMap;
        private readonly ReaderReferenceMap<string> _stringReferenceMap;

        /// <summary>
        /// Copy of the global binder data that maps from Types to the appropriate reading-function
        /// for that type.  Types register functions directly with <see cref="ObjectBinder"/>, but 
        /// that means that <see cref="ObjectBinder"/> is both static and locked.  This gives us 
        /// local copy we can work with without needing to worry about anyone else mutating.
        /// </summary>
        private readonly ObjectBinderSnapshot _binderSnapshot;

        private int _recursionDepth;

        /// <summary>
        /// Creates a new instance of a <see cref="ObjectReader"/>.
        /// </summary>
        /// <param name="stream">The stream to read objects from.</param>
        /// <param name="leaveOpen">True to leave the <paramref name="stream"/> open after the <see cref="ObjectWriter"/> is disposed.</param>
        /// <param name="cancellationToken"></param>
        private ObjectReader(
            Stream stream,
            bool leaveOpen,
            CancellationToken cancellationToken)
        {
            // String serialization assumes both reader and writer to be of the same endianness.
            // It can be adjusted for BigEndian if needed.
            LorettaDebug.Assert(BitConverter.IsLittleEndian);

            _reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen);
            _objectReferenceMap = ReaderReferenceMap<object>.Create();
            _stringReferenceMap = ReaderReferenceMap<string>.Create();

            // Capture a copy of the current static binder state.  That way we don't have to 
            // access any locks while we're doing our processing.
            _binderSnapshot = ObjectBinder.GetSnapshot();

            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Attempts to create a <see cref="ObjectReader"/> from the provided <paramref name="stream"/>.
        /// If the <paramref name="stream"/> does not start with a valid header, then <see langword="null"/> will
        /// be returned.
        /// </summary>
        public static ObjectReader TryGetReader(
            Stream stream,
            bool leaveOpen = false,
            CancellationToken cancellationToken = default)
        {
            if (stream == null)
            {
                return null;
            }

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0078 // Use pattern matching (may change code meaning)
            if (stream.ReadByte() != VersionByte1 ||
                stream.ReadByte() != VersionByte2)
#pragma warning restore IDE0078 // Use pattern matching (may change code meaning)
#pragma warning restore IDE0079 // Remove unnecessary suppression
            {
                return null;
            }

            return new ObjectReader(stream, leaveOpen, cancellationToken);
        }

        /// <summary>
        /// Creates an <see cref="ObjectReader"/> from the provided <paramref name="stream"/>.
        /// Unlike <see cref="TryGetReader(Stream, bool, CancellationToken)"/>, it requires the version
        /// of the data in the stream to exactly match the current format version.
        /// Should only be used to read data written by the same version of Roslyn.
        /// </summary>
        public static ObjectReader GetReader(
            Stream stream,
            bool leaveOpen,
            CancellationToken cancellationToken)
        {
            var b = stream.ReadByte();
            if (b == -1)
            {
                throw new EndOfStreamException();
            }

            if (b != VersionByte1)
            {
                throw ExceptionUtilities.UnexpectedValue(b);
            }

            b = stream.ReadByte();
            if (b == -1)
            {
                throw new EndOfStreamException();
            }

            if (b != VersionByte2)
            {
                throw ExceptionUtilities.UnexpectedValue(b);
            }

            return new ObjectReader(stream, leaveOpen, cancellationToken);
        }

        public void Dispose()
        {
            _objectReferenceMap.Dispose();
            _stringReferenceMap.Dispose();
            _recursionDepth = 0;
        }

        public bool ReadBoolean() => _reader.ReadBoolean();
        public byte ReadByte() => _reader.ReadByte();
        // read as ushort because BinaryWriter fails on chars that are unicode surrogates
        public char ReadChar() => (char) _reader.ReadUInt16();
        public decimal ReadDecimal() => _reader.ReadDecimal();
        public double ReadDouble() => _reader.ReadDouble();
        public float ReadSingle() => _reader.ReadSingle();
        public int ReadInt32() => _reader.ReadInt32();
        public long ReadInt64() => _reader.ReadInt64();
        public sbyte ReadSByte() => _reader.ReadSByte();
        public short ReadInt16() => _reader.ReadInt16();
        public uint ReadUInt32() => _reader.ReadUInt32();
        public ulong ReadUInt64() => _reader.ReadUInt64();
        public ushort ReadUInt16() => _reader.ReadUInt16();
        public string ReadString() => ReadStringValue();

        public Guid ReadGuid()
        {
            var accessor = new ObjectWriter.GuidAccessor
            {
                Low64 = ReadInt64(),
                High64 = ReadInt64()
            };

            return accessor.Guid;
        }

        public object ReadValue()
        {
            var oldDepth = _recursionDepth;
            _recursionDepth++;

            object value;
            if (_recursionDepth % ObjectWriter.MaxRecursionDepth == 0)
            {
                // If we're recursing too deep, move the work to another thread to do so we
                // don't blow the stack.
                var task = Task.Factory.StartNew(
                    ReadValueWorker,
                    _cancellationToken,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                // We must not proceed until the additional task completes. After returning from a read, the underlying
                // stream providing access to raw memory will be closed; if this occurs before the separate thread
                // completes its read then an access violation can occur attempting to read from unmapped memory.
                //
                // CANCELLATION: If cancellation is required, DO NOT attempt to cancel the operation by cancelling this
                // wait. Cancellation must only be implemented by modifying 'task' to cancel itself in a timely manner
                // so the wait can complete.
                value = task.GetAwaiter().GetResult();
            }
            else
            {
                value = ReadValueWorker();
            }

            _recursionDepth--;
            LorettaDebug.Assert(oldDepth == _recursionDepth);

            return value;
        }

        private object ReadValueWorker()
        {
            var kind = (EncodingKind) _reader.ReadByte();
            return kind switch
            {
                EncodingKind.Null => null,
                EncodingKind.Boolean_True => true,
                EncodingKind.Boolean_False => false,
                EncodingKind.Int8 => _reader.ReadSByte(),
                EncodingKind.UInt8 => _reader.ReadByte(),
                EncodingKind.Int16 => _reader.ReadInt16(),
                EncodingKind.UInt16 => _reader.ReadUInt16(),
                EncodingKind.Int32 => _reader.ReadInt32(),
                EncodingKind.Int32_1Byte => (int) _reader.ReadByte(),
                EncodingKind.Int32_2Bytes => (int) _reader.ReadUInt16(),
                EncodingKind.Int32_0
                or EncodingKind.Int32_1
                or EncodingKind.Int32_2
                or EncodingKind.Int32_3
                or EncodingKind.Int32_4
                or EncodingKind.Int32_5
                or EncodingKind.Int32_6
                or EncodingKind.Int32_7
                or EncodingKind.Int32_8
                or EncodingKind.Int32_9
                or EncodingKind.Int32_10 => (int) kind - (int) EncodingKind.Int32_0,
                EncodingKind.UInt32 => _reader.ReadUInt32(),
                EncodingKind.UInt32_1Byte => (uint) _reader.ReadByte(),
                EncodingKind.UInt32_2Bytes => (uint) _reader.ReadUInt16(),
                EncodingKind.UInt32_0
                or EncodingKind.UInt32_1
                or EncodingKind.UInt32_2
                or EncodingKind.UInt32_3
                or EncodingKind.UInt32_4
                or EncodingKind.UInt32_5
                or EncodingKind.UInt32_6
                or EncodingKind.UInt32_7
                or EncodingKind.UInt32_8
                or EncodingKind.UInt32_9
                or EncodingKind.UInt32_10 => (uint) ((int) kind - (int) EncodingKind.UInt32_0),
                EncodingKind.Int64 => _reader.ReadInt64(),
                EncodingKind.UInt64 => _reader.ReadUInt64(),
                EncodingKind.Float4 => _reader.ReadSingle(),
                EncodingKind.Float8 => _reader.ReadDouble(),
                EncodingKind.Decimal => _reader.ReadDecimal(),
                // read as ushort because BinaryWriter fails on chars that are unicode surrogates
                EncodingKind.Char => (char) _reader.ReadUInt16(),
                EncodingKind.StringUtf8
                or EncodingKind.StringUtf16
                or EncodingKind.StringRef_4Bytes
                or EncodingKind.StringRef_1Byte or EncodingKind.StringRef_2Bytes => ReadStringValue(kind),
                EncodingKind.ObjectRef_4Bytes => _objectReferenceMap.GetValue(_reader.ReadInt32()),
                EncodingKind.ObjectRef_1Byte => _objectReferenceMap.GetValue(_reader.ReadByte()),
                EncodingKind.ObjectRef_2Bytes => _objectReferenceMap.GetValue(_reader.ReadUInt16()),
                EncodingKind.Object => ReadObject(),
                EncodingKind.DateTime => DateTime.FromBinary(_reader.ReadInt64()),
                EncodingKind.Array
                or EncodingKind.Array_0
                or EncodingKind.Array_1
                or EncodingKind.Array_2
                or EncodingKind.Array_3 => ReadArray(kind),
                EncodingKind.EncodingName => Encoding.GetEncoding(ReadString()),
                EncodingKind.EncodingUTF8 => s_encodingUTF8,
                EncodingKind.EncodingUTF8_BOM => Encoding.UTF8,
                EncodingKind.EncodingUTF32_BE => s_encodingUTF32_BE,
                EncodingKind.EncodingUTF32_BE_BOM => s_encodingUTF32_BE_BOM,
                EncodingKind.EncodingUTF32_LE => s_encodingUTF32_LE,
                EncodingKind.EncodingUTF32_LE_BOM => Encoding.UTF32,
                EncodingKind.EncodingUnicode_BE => s_encodingUnicode_BE,
                EncodingKind.EncodingUnicode_BE_BOM => Encoding.BigEndianUnicode,
                EncodingKind.EncodingUnicode_LE => s_encodingUnicode_LE,
                EncodingKind.EncodingUnicode_LE_BOM => Encoding.Unicode,
                _ => throw ExceptionUtilities.UnexpectedValue(kind),
            };
        }

        private static readonly Encoding s_encodingUTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        private static readonly Encoding s_encodingUTF32_BE = new UTF32Encoding(bigEndian: true, byteOrderMark: false);
        private static readonly Encoding s_encodingUTF32_BE_BOM = new UTF32Encoding(bigEndian: true, byteOrderMark: true);
        private static readonly Encoding s_encodingUTF32_LE = new UTF32Encoding(bigEndian: false, byteOrderMark: false);
        private static readonly Encoding s_encodingUnicode_BE = new UnicodeEncoding(bigEndian: true, byteOrderMark: false);
        private static readonly Encoding s_encodingUnicode_LE = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);

        /// <summary>
        /// A reference-id to object map, that can share base data efficiently.
        /// </summary>
        private struct ReaderReferenceMap<T> : IDisposable
            where T : class
        {
            private readonly SegmentedList<T> _values;

            private static readonly ObjectPool<SegmentedList<T>> s_objectListPool
                = new(() => new SegmentedList<T>(20));

            private ReaderReferenceMap(SegmentedList<T> values)
            {
                _values = values;
            }

            public static ReaderReferenceMap<T> Create()
                => new(s_objectListPool.Allocate());

            public void Dispose()
            {
                _values.Clear();
                s_objectListPool.Free(_values);
            }

            public int GetNextObjectId()
            {
                var id = _values.Count;
                _values.Add(null);
                return id;
            }

            public void AddValue(T value)
                => _values.Add(value);

            public void AddValue(int index, T value)
                => _values[index] = value;

            public T GetValue(int referenceId)
                => _values[referenceId];
        }

        internal uint ReadCompressedUInt()
        {
            var info = _reader.ReadByte();
            byte marker = (byte) (info & ObjectWriter.ByteMarkerMask);
            byte byte0 = (byte) (info & ~ObjectWriter.ByteMarkerMask);

            if (marker == ObjectWriter.Byte1Marker)
            {
                return byte0;
            }

            if (marker == ObjectWriter.Byte2Marker)
            {
                var byte1 = _reader.ReadByte();
                return (((uint) byte0) << 8) | byte1;
            }

            if (marker == ObjectWriter.Byte4Marker)
            {
                var byte1 = _reader.ReadByte();
                var byte2 = _reader.ReadByte();
                var byte3 = _reader.ReadByte();

                return (((uint) byte0) << 24) | (((uint) byte1) << 16) | (((uint) byte2) << 8) | byte3;
            }

            throw ExceptionUtilities.UnexpectedValue(marker);
        }

        private string ReadStringValue()
        {
            var kind = (EncodingKind) _reader.ReadByte();
            return kind == EncodingKind.Null ? null : ReadStringValue(kind);
        }

        private string ReadStringValue(EncodingKind kind)
        {
            return kind switch
            {
                EncodingKind.StringRef_1Byte => _stringReferenceMap.GetValue(_reader.ReadByte()),
                EncodingKind.StringRef_2Bytes => _stringReferenceMap.GetValue(_reader.ReadUInt16()),
                EncodingKind.StringRef_4Bytes => _stringReferenceMap.GetValue(_reader.ReadInt32()),
                EncodingKind.StringUtf16 or EncodingKind.StringUtf8 => ReadStringLiteral(kind),
                _ => throw ExceptionUtilities.UnexpectedValue(kind),
            };
        }

        private unsafe string ReadStringLiteral(EncodingKind kind)
        {
            string value;
            if (kind == EncodingKind.StringUtf8)
            {
                value = _reader.ReadString();
            }
            else
            {
                // This is rare, just allocate UTF16 bytes for simplicity.
                int characterCount = (int) ReadCompressedUInt();
                byte[] bytes = _reader.ReadBytes(characterCount * sizeof(char));
                fixed (byte* bytesPtr = bytes)
                {
                    value = new string((char*) bytesPtr, 0, characterCount);
                }
            }

            _stringReferenceMap.AddValue(value);
            return value;
        }

        private Array ReadArray(EncodingKind kind)
        {
            var length = kind switch
            {
                EncodingKind.Array_0 => 0,
                EncodingKind.Array_1 => 1,
                EncodingKind.Array_2 => 2,
                EncodingKind.Array_3 => 3,
                _ => (int) ReadCompressedUInt(),
            };

            // SUBTLE: If it was a primitive array, only the EncodingKind byte of the element type was written, instead of encoding as a type.
            var elementKind = (EncodingKind) _reader.ReadByte();

            var elementType = ObjectWriter.s_reverseTypeMap[(int) elementKind];
            if (elementType != null)
            {
                return ReadPrimitiveTypeArrayElements(elementType, elementKind, length);
            }
            else
            {
                // custom type case
                elementType = ReadTypeAfterTag();

                // recursive: create instance and read elements next in stream
                Array array = Array.CreateInstance(elementType, length);

                for (int i = 0; i < length; ++i)
                {
                    var value = ReadValue();
                    array.SetValue(value, i);
                }

                return array;
            }
        }

        private Array ReadPrimitiveTypeArrayElements(Type type, EncodingKind kind, int length)
        {
            LorettaDebug.Assert(ObjectWriter.s_reverseTypeMap[(int) kind] == type);

            // optimizations for supported array type by binary reader
            if (type == typeof(byte)) { return _reader.ReadBytes(length); }
            if (type == typeof(char)) { return _reader.ReadChars(length); }

            // optimizations for string where object reader/writer has its own mechanism to
            // reduce duplicated strings
            if (type == typeof(string)) { return ReadStringArrayElements(CreateArray<string>(length)); }
            if (type == typeof(bool)) { return ReadBooleanArrayElements(CreateArray<bool>(length)); }

            // otherwise, read elements directly from underlying binary writer
            return kind switch
            {
                EncodingKind.Int8 => ReadInt8ArrayElements(CreateArray<sbyte>(length)),
                EncodingKind.Int16 => ReadInt16ArrayElements(CreateArray<short>(length)),
                EncodingKind.Int32 => ReadInt32ArrayElements(CreateArray<int>(length)),
                EncodingKind.Int64 => ReadInt64ArrayElements(CreateArray<long>(length)),
                EncodingKind.UInt16 => ReadUInt16ArrayElements(CreateArray<ushort>(length)),
                EncodingKind.UInt32 => ReadUInt32ArrayElements(CreateArray<uint>(length)),
                EncodingKind.UInt64 => ReadUInt64ArrayElements(CreateArray<ulong>(length)),
                EncodingKind.Float4 => ReadFloat4ArrayElements(CreateArray<float>(length)),
                EncodingKind.Float8 => ReadFloat8ArrayElements(CreateArray<double>(length)),
                EncodingKind.Decimal => ReadDecimalArrayElements(CreateArray<decimal>(length)),
                _ => throw ExceptionUtilities.UnexpectedValue(kind),
            };
        }

        private bool[] ReadBooleanArrayElements(bool[] array)
        {
            // Confirm the type to be read below is ulong
            LorettaDebug.Assert(BitVector.BitsPerWord == 64);

            var wordLength = BitVector.WordsRequired(array.Length);

            var count = 0;
            for (var i = 0; i < wordLength; i++)
            {
                var word = _reader.ReadUInt64();

                for (var p = 0; p < BitVector.BitsPerWord; p++)
                {
                    if (count >= array.Length)
                    {
                        return array;
                    }

                    array[count++] = BitVector.IsTrue(word, p);
                }
            }

            return array;
        }

        private static T[] CreateArray<T>(int length)
        {
            if (length == 0)
            {
                // quick check
                return Array.Empty<T>();
            }
            else
            {
                return new T[length];
            }
        }

        private string[] ReadStringArrayElements(string[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = ReadStringValue();
            }

            return array;
        }

        private sbyte[] ReadInt8ArrayElements(sbyte[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = _reader.ReadSByte();
            }

            return array;
        }

        private short[] ReadInt16ArrayElements(short[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = _reader.ReadInt16();
            }

            return array;
        }

        private int[] ReadInt32ArrayElements(int[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = _reader.ReadInt32();
            }

            return array;
        }

        private long[] ReadInt64ArrayElements(long[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = _reader.ReadInt64();
            }

            return array;
        }

        private ushort[] ReadUInt16ArrayElements(ushort[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = _reader.ReadUInt16();
            }

            return array;
        }

        private uint[] ReadUInt32ArrayElements(uint[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = _reader.ReadUInt32();
            }

            return array;
        }

        private ulong[] ReadUInt64ArrayElements(ulong[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = _reader.ReadUInt64();
            }

            return array;
        }

        private decimal[] ReadDecimalArrayElements(decimal[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = _reader.ReadDecimal();
            }

            return array;
        }

        private float[] ReadFloat4ArrayElements(float[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = _reader.ReadSingle();
            }

            return array;
        }

        private double[] ReadFloat8ArrayElements(double[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = _reader.ReadDouble();
            }

            return array;
        }

        public Type ReadType()
        {
            _reader.ReadByte();
            return Type.GetType(ReadString());
        }

        private Type ReadTypeAfterTag()
            => _binderSnapshot.GetTypeFromId(ReadInt32());

        private object ReadObject()
        {
            var objectId = _objectReferenceMap.GetNextObjectId();

            // reading an object may recurse.  So we need to grab our ID up front as we'll
            // end up making our sub-objects before we make this object.

            var typeReader = _binderSnapshot.GetTypeReaderFromId(ReadInt32());

            // recursive: read and construct instance immediately from member elements encoding next in the stream
            var instance = typeReader(this);

            if (instance.ShouldReuseInSerialization)
            {
                _objectReferenceMap.AddValue(objectId, instance);
            }

            return instance;
        }

        private static Exception DeserializationReadIncorrectNumberOfValuesException(string typeName) =>
            throw new InvalidOperationException(string.Format(Resources.Deserialization_reader_for_0_read_incorrect_number_of_values, typeName));

        private static Exception NoSerializationTypeException(string typeName) =>
            new InvalidOperationException(string.Format(Resources.The_type_0_is_not_understood_by_the_serialization_binder, typeName));

        private static Exception NoSerializationReaderException(string typeName) =>
            new InvalidOperationException(string.Format(Resources.Cannot_serialize_type_0, typeName));
    }
}
