// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Loretta.Generators
{
    internal sealed class StringBuilderReader(StringBuilder stringBuilder) : TextReader
    {
        private int _position;

        public override int Peek()
        {
            if (_position == stringBuilder.Length) return -1;

            return stringBuilder[_position];
        }

        public override int Read()
        {
            if (_position == stringBuilder.Length) return -1;

            return stringBuilder[_position++];
        }

        public override int Read(char[] buffer, int index, int count)
        {
            var charsToCopy = Math.Min(count, val2: stringBuilder.Length - _position);
            stringBuilder.CopyTo(_position, buffer, index, charsToCopy);
            _position += charsToCopy;
            return charsToCopy;
        }
    }
}
