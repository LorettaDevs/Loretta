using System;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    using System.Globalization;

    internal partial class SyntaxToken
    {
        internal class SyntaxTokenWithValue<T> : SyntaxToken
        {
            static SyntaxTokenWithValue()
            {
                ObjectBinder.RegisterTypeReader(typeof(SyntaxTokenWithValue<T>), r => new SyntaxTokenWithValue<T>(r));
            }

            protected readonly string _text;
            protected readonly T _value;

            internal SyntaxTokenWithValue(SyntaxKind kind, string text, T value)
                : base(kind, text.Length)
            {
                _text = text;
                _value = value;
            }

            internal SyntaxTokenWithValue(
                SyntaxKind kind,
                string text,
                T value,
                DiagnosticInfo[]? diagnostics,
                SyntaxAnnotation[]? annotations)
                : base(kind, text.Length, diagnostics, annotations)
            {
                _text = text;
                _value = value;
            }

            internal SyntaxTokenWithValue(ObjectReader reader)
                : base(reader)
            {
                _text = reader.ReadString();
                FullWidth = _text.Length;
                _value = (T) reader.ReadValue();
            }

            internal override void WriteTo(ObjectWriter writer)
            {
                base.WriteTo(writer);
                writer.WriteString(_text);
                writer.WriteValue(_value);
            }

            public override string Text => _text;

            public override object? Value => _value;

            public override string? ValueText => Convert.ToString(_value, CultureInfo.InvariantCulture);
        }
    }
}
