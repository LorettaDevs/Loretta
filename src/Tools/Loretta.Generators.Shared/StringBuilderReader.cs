// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Text;

namespace Loretta.Generators
{
    internal sealed class StringBuilderReader : TextReader
    {
        private readonly StringBuilder _stringBuilder;
        private int _position;

        public StringBuilderReader(StringBuilder stringBuilder)
        {
            _stringBuilder = stringBuilder;
            _position = 0;
        }

        public override int Peek()
        {
            if (_position == _stringBuilder.Length)
            {
                return -1;
            }

            return _stringBuilder[_position];
        }

        public override int Read()
        {
            if (_position == _stringBuilder.Length)
            {
                return -1;
            }

            return _stringBuilder[_position++];
        }

        public override int Read(char[] buffer, int index, int count)
        {
            var charsToCopy = Math.Min(count, _stringBuilder.Length - _position);
            _stringBuilder.CopyTo(_position, buffer, index, charsToCopy);
            _position += charsToCopy;
            return charsToCopy;
        }
    }
}
