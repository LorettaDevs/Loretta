using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp;

namespace Loretta.Generators
{
    internal sealed class OptimizedSwitch
    {
        private readonly List<OptimizedSwitchClause> _clauses = [];

        public Action<SourceWriter>? DefaultBodyWriter { get; init; }

        public OptimizedSwitch AddClause(string key, Action<SourceWriter> bodyWriter)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (bodyWriter is null) throw new ArgumentNullException(nameof(bodyWriter));

            _clauses.Add(new OptimizedSwitchClause(key, bodyWriter));
            return this;
        }

        public void Generate(SourceWriter writer, string inputName, bool isInputSpan)
        {
            var clauses = _clauses.ToArray();
            var groups = clauses.GroupBy(static clause => clause.Key.Length).Select(
                static group =>
                {
                    var clauses = group.ToArray();
                    var index = GetDiscriminatorIndexAndLength(
                        input: Array.ConvertAll(clauses, static clause => clause.Key),
                        length: out var length);
                    return new OptimizedSwitchGroup(group.Key, index, length, clauses);
                }).ToArray();

            // Write a normal switch if we won't be able to optimize any branches
            if (groups.All(static group => group.DiscriminatorIndex == -1))
            {
                WritePlainSwitch(
                    writer,
                    isInputSpan,
                    inputName,
                    clauses,
                    static (writer, clause) => clause.BodyWriter(writer),
                    DefaultBodyWriter);
                return;
            }

            var hash = Hash.CombineValues(
                values: _clauses.Select(static c => c.Key).Append(DefaultBodyWriter is null ? "default" : "no default"),
                StringComparer.OrdinalIgnoreCase);
            var branchVariableName = $"__branch__{hash:X8}";

            writer.Write("int ");
            writer.Write(branchVariableName);
            writer.WriteLine(" = -1;");

            writer.Write("switch (");
            writer.Write(inputName);
            writer.WriteLine(".Length)");
            using (writer.CurlyIndenter())
            {
                foreach (var (length, discriminatorIndex, discriminatorLength, groupClauses) in groups.OrderBy(
                             static g => g.InputLength))
                {
                    writer.Write("case ");
                    writer.Write(
                        SymbolDisplay.FormatPrimitive(length, quoteStrings: false, useHexadecimalNumbers: false));
                    if (groupClauses.Length == 1)
                    {
                        writer.Write(" when ");
                        WriteEqualityComparison(writer, inputName, isInputSpan, groupClauses[0].Key);
                    }
                    writer.WriteLine(value: ':');
                    using (writer.Indenter())
                    {
                        if (groupClauses.Length > 1)
                        {
                            if (discriminatorIndex != -1)
                            {
                                WriteDiscriminatorSwitch(discriminatorIndex, discriminatorLength, groupClauses);
                            }
                            else
                            {
                                WritePlainSwitch(
                                    writer,
                                    isInputSpan,
                                    inputName,
                                    groupClauses,
                                    (_, clause) => WriteDestinationForClause(clause));
                            }
                        }
                        else
                        {
                            WriteDestinationForClause(groupClauses[0]);
                        }
                        writer.WriteLine("break;");
                    }
                }
            }

            writer.WriteLineNoTabs("");
            writer.Write("switch (");
            writer.Write(branchVariableName);
            writer.WriteLine(value: ')');
            using (writer.CurlyIndenter())
            {
                for (var idx = 0; idx < clauses.Length; idx++)
                {
                    var clause = clauses[idx];
                    writer.Write("case ");
                    writer.Write(SymbolDisplay.FormatPrimitive(idx, quoteStrings: false, useHexadecimalNumbers: false));
                    writer.WriteLine(value: ':');
                    using (writer.CurlyIndenter()) clause.BodyWriter(writer);
                    writer.WriteLineNoTabs("#pragma warning disable CS0162 // Unreachable code detected");
                    writer.WriteLine("break;");
                    writer.WriteLineNoTabs("#pragma warning restore CS0162 // Unreachable code detected");
                }

                writer.WriteLine("default:");
                // ReSharper disable once RemoveRedundantBraces (inf loop)
                if (DefaultBodyWriter is not null)
                {
                    using (writer.CurlyIndenter()) DefaultBodyWriter(writer);
                }
                writer.WriteLineNoTabs("#pragma warning disable CS0162 // Unreachable code detected");
                writer.WriteLine("break;");
                writer.WriteLineNoTabs("#pragma warning restore CS0162 // Unreachable code detected");
            }
            return;

            void WriteDiscriminatorSwitch(
                int                                discriminatorIndex,
                int                                discriminatorLength,
                IEnumerable<OptimizedSwitchClause> groupClauses)
            {
                writer.Write("switch (");
                WriteDiscriminatorRead(writer, inputName, isInputSpan, discriminatorIndex, discriminatorLength);
                writer.WriteLine(value: ')');
                using (writer.CurlyIndenter())
                {
                    foreach (var clause in groupClauses)
                    {
                        var discriminator = (object) ReadDiscriminator(
                            clause.Key,
                            discriminatorIndex,
                            discriminatorLength);
                        switch (discriminatorLength)
                        {
                            case 1: discriminator = (char) (long) discriminator; break;
                            case 2: discriminator = (int) (long) discriminator; break;
                        }

                        if (discriminatorLength != clause.Key.Length)
                        {
                            writer.Write("// ");
                            writer.WriteLine(clause.Key);
                        }
                        writer.Write("case /* \"");
                        writer.Write(clause.Key.Substring(discriminatorIndex, discriminatorLength));
                        writer.Write("\" = */ ");
                        writer.Write(
                            SymbolDisplay.FormatPrimitive(
                                discriminator,
                                quoteStrings: true,
                                useHexadecimalNumbers: true));
                        if (discriminatorLength != clause.Key.Length)
                        {
                            writer.Write(" when ");
                            WriteEqualityComparison(writer, inputName, isInputSpan, clause.Key);
                        }
                        writer.WriteLine(':');
                        using (writer.Indenter())
                        {
                            WriteDestinationForClause(clause);
                            writer.WriteLine("break;");
                        }
                    }
                }
            }

            void WriteDestinationForClause(OptimizedSwitchClause clause)
            {
                writer.Write(branchVariableName);
                writer.Write(" = ");
                writer.Write(
                    SymbolDisplay.FormatPrimitive(
                        obj: Array.IndexOf(clauses, clause),
                        quoteStrings: false,
                        useHexadecimalNumbers: false));
                writer.WriteLine(value: ';');
            }
        }

        private static void WritePlainSwitch(
            SourceWriter                                writer,
            bool                                        isInputSpan,
            string                                      inputName,
            IEnumerable<OptimizedSwitchClause>          clauses,
            Action<SourceWriter, OptimizedSwitchClause> writeClause,
            Action<SourceWriter>?                       writeDefault = null)
        {
            if (isInputSpan)
            {
                var first = true;
                foreach (var clause in clauses)
                {
                    if (!first) writer.Write("else ");
                    first = false;

                    writer.Write("if (");
                    WriteEqualityComparison(writer, inputName, valueIsSpan: true, clause.Key);
                    writer.WriteLine(value: ')');
                    using (writer.CurlyIndenter()) writeClause(writer, clause);
                }

                if (writeDefault is null) return;
                writer.WriteLine("else");
                using (writer.CurlyIndenter()) writeDefault(writer);
            }
            else
            {
                writer.Write("switch (");
                writer.Write(inputName);
                writer.Write(value: ')');
                using (writer.CurlyIndenter())
                {
                    foreach (var clause in clauses)
                    {
                        writer.Write("case ");
                        writer.Write(SymbolDisplay.FormatLiteral(clause.Key, quote: true));
                        writer.Write(value: ':');
                        using (writer.CurlyIndenter()) writeClause(writer, clause);
                    }

                    if (writeDefault is null) return;
                    writer.Write("default:");
                    using (writer.CurlyIndenter()) writeDefault(writer);
                }
            }
        }

        private static void WriteEqualityComparison(SourceWriter writer, string value, bool valueIsSpan, string literal)
        {
            if (valueIsSpan)
            {
                writer.Write(value);
                writer.Write(".Equals(");
                writer.Write(SymbolDisplay.FormatLiteral(literal, quote: true));
                writer.Write(".AsSpan(), StringComparison.Ordinal)");
            }
            else
            {
                writer.Write("string.Equals(");
                writer.Write(value);
                writer.Write(", ");
                writer.Write(SymbolDisplay.FormatLiteral(literal, quote: true));
                writer.Write(", StringComparison.Ordinal)");
            }
        }

        private static void WriteDiscriminatorRead(
            SourceWriter writer,
            string       inputName,
            bool         inputIsSpan,
            int          index,
            int          discriminatorLength)
        {
            if (discriminatorLength == 1)
            {
                writer.Write(inputName);
                writer.Write(value: '[');
                writer.Write(SymbolDisplay.FormatPrimitive(index, quoteStrings: false, useHexadecimalNumbers: false));
                writer.Write(value: ']');
                return;
            }

            var discriminatorType = discriminatorLength == 2 ? "int" : "long";

            writer.Write("System.Runtime.InteropServices.MemoryMarshal.Read<");
            writer.Write(discriminatorType);
            writer.Write(">(System.Runtime.InteropServices.MemoryMarshal.Cast<char, byte>(");
            writer.Write(inputName);
            writer.Write(inputIsSpan ? ".Slice(" : ".AsSpan(");
            writer.Write(SymbolDisplay.FormatPrimitive(index, quoteStrings: false, useHexadecimalNumbers: false));
            writer.Write(", ");
            writer.Write(
                SymbolDisplay.FormatPrimitive(discriminatorLength, quoteStrings: false, useHexadecimalNumbers: false));
            writer.Write(")))");
        }

        private static int GetDiscriminatorIndexAndLength(IEnumerable<string> input, out int length)
        {
            var inputArr = input.ToArray();

            if (inputArr.Length < 1)
                throw new ArgumentException("Input must contain at least one string.", paramName: nameof(input));

            if (inputArr.Select(static str => str.Length).Distinct().Count() > 1)
                throw new ArgumentException("All strings must have the same length.", paramName: nameof(input));

            // Just read the full thing if we can, that way we can skip the equals check.
            var len = inputArr[0].Length;
            switch (len)
            {
                case 2: return GetDiscriminatorIndexAndLengthCore(inputArr, length: length = 2);
                case 4: return GetDiscriminatorIndexAndLengthCore(inputArr, length: length = 4);
            }

            var idx            = GetDiscriminatorIndexAndLengthCore(inputArr, length: length = 1);
            if (idx == -1) idx = GetDiscriminatorIndexAndLengthCore(inputArr, length: length = 2);
            if (idx == -1) idx = GetDiscriminatorIndexAndLengthCore(inputArr, length: length = 4);
            return idx;

            static int GetDiscriminatorIndexAndLengthCore(string[] keys, int length)
            {
                var keysLength = keys[0].Length;
                for (var offset = 0; offset < keysLength - length + 1; offset++)
                {
                    var occurrences = new HashSet<long>(keys.Select(str => ReadDiscriminator(str, offset, length)));

                    if (occurrences.Count == keys.Length) return offset;
                }
                return -1;
            }
        }

        private static long ReadDiscriminator(string str, int index, int charsToRead)
        {
            return charsToRead switch
            {
                1 => str[index],
                2 => MemoryMarshal.Read<int>(MemoryMarshal.Cast<char, byte>(str.AsSpan(index, length: 2))),
                4 => MemoryMarshal.Read<long>(MemoryMarshal.Cast<char, byte>(str.AsSpan(index, length: 4))),
                _ => throw new ArgumentOutOfRangeException(nameof(charsToRead)),
            };
        }

        private readonly record struct OptimizedSwitchClause(string Key, Action<SourceWriter> BodyWriter);

        private readonly record struct OptimizedSwitchGroup(
            int                     InputLength,
            int                     DiscriminatorIndex,
            int                     DiscriminatorLength,
            OptimizedSwitchClause[] Clauses
        );
    }
}
