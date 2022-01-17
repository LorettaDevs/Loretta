// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Loretta.CodeAnalysis.Text
{
    internal class StringTextWriter : SourceTextWriter
    {
        private readonly StringBuilder _builder;
        private readonly Encoding? _encoding;
        private readonly SourceHashAlgorithm _checksumAlgorithm;

        public StringTextWriter(Encoding? encoding, SourceHashAlgorithm checksumAlgorithm, int capacity)
        {
            _builder = new StringBuilder(capacity);
            _encoding = encoding;
            _checksumAlgorithm = checksumAlgorithm;
        }

        // https://github.com/dotnet/roslyn/issues/40830
        public override Encoding Encoding => _encoding!;

        public override SourceText ToSourceText() =>
            new StringText(_builder.ToString(), _encoding, checksumAlgorithm: _checksumAlgorithm);

        public override void Write(char value) => _builder.Append(value);

        public override void Write(string? value) => _builder.Append(value);

        public override void Write(char[] buffer, int index, int count) =>
            _builder.Append(buffer, index, count);
    }
}
