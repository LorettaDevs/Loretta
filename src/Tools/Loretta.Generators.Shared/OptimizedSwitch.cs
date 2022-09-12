using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp;

namespace Loretta.Generators
{
    internal readonly record struct OptimizedSwitchClause(string Key, Action<SourceWriter> BodyWriter);

    internal class OptimizedSwitch
    {
        private static int s_id = -1;

        public static void ResetId() => s_id = -1;

        public int Id { get; } = Interlocked.Increment(ref s_id);
        public List<OptimizedSwitchClause> Clauses { get; } = new();
        public Action<SourceWriter>? DefaultBodyWriter { get; set; }

        public OptimizedSwitch AddClause(string key, Action<SourceWriter> bodyWriter)
        {
            Clauses.Add(new(key, bodyWriter));
            return this;
        }

        public void Generate(SourceWriter writer, string inputName, bool inputIsSpan)
        {
            var clauses = Clauses.ToImmutableArray();
            var groups = clauses.GroupBy(clause => clause.Key.Length);

            // Write a normal switch if we won't be able to optimize it
            if (!inputIsSpan && groups.Any(group => GetUniqueColumnLocation(group.Select(clause => clause.Key), out _) == -1))
            {
                writer.Write("switch (");
                writer.Write(inputName);
                writer.Write(')');
                using (writer.CurlyIndenter())
                {
                    foreach (var clause in clauses)
                    {
                        writer.Write("case ");
                        writer.Write(SymbolDisplay.FormatLiteral(clause.Key, true));
                        writer.Write(':');
                        using (writer.CurlyIndenter())
                        {
                            clause.BodyWriter(writer);
                        }
                    }
                    if (DefaultBodyWriter is not null)
                    {
                        writer.Write("default:");
                        using (writer.CurlyIndenter())
                        {
                            DefaultBodyWriter(writer);
                        }
                    }
                }
                return;
            }

            var defaultDestination = -1;
            if (DefaultBodyWriter is not null)
            {
                defaultDestination = clauses.Length;
            }

            var candidateName = $"__candidate__{Id}";
            var destinationName = $"__destination__{Id}";
            var skipCheckName = $"__skipCheck__{Id}";

            writer.Write("string? "); writer.Write(candidateName); writer.WriteLine(" = null;");
            writer.Write("int "); writer.Write(destinationName); writer.WriteLine(" = -1;");
            writer.Write("var "); writer.Write(skipCheckName); writer.WriteLine(" = false;");

            writer.Write("switch (");
            writer.Write(inputName);
            writer.WriteLine(".Length)");
            using (writer.CurlyIndenter())
            {
                foreach (var group in groups.OrderBy(g => g.Key))
                {
                    writer.Write("case ");
                    writer.Write(SymbolDisplay.FormatPrimitive(group.Key, false, false));
                    writer.WriteLine(':');
                    using (writer.Indenter())
                    {
                        if (group.Count() > 1)
                        {
                            var uniqueCharIdx = GetUniqueColumnLocation(group.Select(clause => clause.Key), out var charsToRead);
                            if (uniqueCharIdx != -1)
                            {
                                var uniqueCharIdxLit = SymbolDisplay.FormatPrimitive(uniqueCharIdx, false, false);
                                writer.Write("switch (");
                                WriteGetDiscriminator(writer, inputName, inputIsSpan, uniqueCharIdx, charsToRead);
                                writer.WriteLine(')');
                                using (writer.CurlyIndenter())
                                {
                                    foreach (var clause in group)
                                    {
                                        var discriminator = GetDiscriminatorFromString(clause.Key, uniqueCharIdx, charsToRead);

                                        writer.Write("case ");
                                        writer.Write(SymbolDisplay.FormatPrimitive(discriminator, true, true));
                                        writer.WriteLine(':');
                                        using (writer.Indenter())
                                        {
                                            writeDestinationAndCandidateForClause(clause, clause.Key.Length == charsToRead);
                                            writer.WriteLine("break;");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (inputIsSpan)
                                {
                                    var first = true;
                                    foreach (var clause in group)
                                    {
                                        if (!first)
                                        {
                                            writer.Write("else ");
                                            first = false;
                                        }
                                        writer.Write("if (");
                                        WriteEqualityComparison(writer, inputName, inputIsSpan, clause.Key);
                                        writer.WriteLine(')');
                                        using (writer.CurlyIndenter())
                                        {
                                            writeDestinationAndCandidateForClause(clause, true);
                                        }
                                    }
                                }
                                else
                                {
                                    writer.Write("switch (");
                                    writer.Write(inputName);
                                    writer.WriteLine(')');
                                    using (writer.CurlyIndenter())
                                    {
                                        foreach (var clause in group)
                                        {
                                            writer.Write("case ");
                                            writer.Write(SymbolDisplay.FormatLiteral(clause.Key, true));
                                            writer.WriteLine(':');
                                            using (writer.Indenter())
                                            {
                                                writeDestinationAndCandidateForClause(clause, true);
                                                writer.WriteLine("break;");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var clause = group.Single();
                            writeDestinationAndCandidateForClause(clause, false);
                            WriteClauseIfStatement(writer, inputName, inputIsSpan, clause);
                        }
                        writer.WriteLine("break;");
                    }
                }
            }

            writer.Write("if (");
            writer.Write(destinationName);
            writer.Write(" != -1 && (");
            writer.Write(skipCheckName);
            writer.Write(" || ");
            WriteEqualityComparison(writer, inputIsSpan, inputName, false, candidateName);
            writer.WriteLine("))");
            using (writer.CurlyIndenter())
            {
                writer.Write("switch ("); writer.Write(destinationName); writer.WriteLine(')');
                using (writer.CurlyIndenter())
                {
                    for (var idx = 0; idx < clauses.Length; idx++)
                    {
                        var clause = clauses[idx];
                        writer.Write("case ");
                        writer.Write(SymbolDisplay.FormatPrimitive(idx, false, false));
                        writer.WriteLine(':');
                        using (writer.CurlyIndenter())
                        {
                            clause.BodyWriter(writer);
                        }
                    }
                    writer.WriteLine("default:");
                    writer.WriteLineIndented("throw new InvalidOperationException();");
                }
            }
            if (DefaultBodyWriter is not null)
            {
                writer.WriteLine("else");
                using (writer.CurlyIndenter())
                {
                    DefaultBodyWriter(writer);
                }
            }

            void writeDestinationAndCandidateForClause(OptimizedSwitchClause clause, bool skipCheck)
            {
                var destinationIndex = clauses.IndexOf(clause);
                writeDestinationAndCandidate(clause.Key, destinationIndex, skipCheck);
            }

            void writeDestinationAndCandidate(string? key, int destination, bool skipCheck)
            {
                if (key != null)
                {
                    writer.Write(candidateName);
                    writer.Write(" = ");
                    writer.Write(SymbolDisplay.FormatLiteral(key, true));
                    writer.WriteLine(';');
                }

                writer.Write(destinationName);
                writer.Write(" = ");
                writer.Write(SymbolDisplay.FormatPrimitive(destination, false, false));
                writer.WriteLine(';');

                if (skipCheck)
                {
                    writer.Write(skipCheckName);
                    writer.WriteLine(" = true;");
                }
            }
        }

        private static object GetDiscriminatorFromString(string str, int index, int charsToRead)
        {
            return charsToRead switch
            {
                1 => (object) str[index],
                2 => (object) MemoryMarshal.Read<int>(MemoryMarshal.Cast<char, byte>(str.AsSpan(index, 2))),
                4 => (object) MemoryMarshal.Read<long>(MemoryMarshal.Cast<char, byte>(str.AsSpan(index, 4))),
                _ => throw new ArgumentOutOfRangeException(nameof(charsToRead))
            };
        }

        private static void WriteClauseIfStatement(SourceWriter writer, string inputName, bool inputIsSpan, OptimizedSwitchClause clause)
        {
            writer.Write("if (");
            WriteEqualityComparison(writer, inputName, inputIsSpan, clause.Key);
            writer.WriteLine(')');
            using (writer.CurlyIndenter())
            {
                clause.BodyWriter(writer);
            }
        }

        private static void WriteEqualityComparison(SourceWriter writer, bool leftIsSpan, string left, bool rightIsSpan, string right)
        {
            if (leftIsSpan || rightIsSpan)
            {
                writer.Write("System.MemoryExtensions.Equals(");
                writer.Write(left);
                if (!leftIsSpan)
                    writer.Write(".AsSpan()");
                writer.Write(", ");
                writer.Write(right);
                if (!rightIsSpan)
                    writer.Write(".AsSpan()");
                writer.Write(", StringComparison.Ordinal)");
            }
            else
            {
                writer.Write("string.Equals(");
                writer.Write(left);
                writer.Write(", ");
                writer.Write(right);
                writer.Write(", StringComparison.Ordinal)");
            }
        }

        private static void WriteEqualityComparison(SourceWriter writer, string left, bool leftIsSpan, string value)
        {
            if (leftIsSpan)
            {
                writer.Write(left);
                writer.Write(".Equals(");
                writer.Write(SymbolDisplay.FormatLiteral(value, true));
                writer.Write(".AsSpan(), StringComparison.Ordinal)");
            }
            else
            {
                writer.Write("string.Equals(");
                writer.Write(left);
                writer.Write(", ");
                writer.Write(SymbolDisplay.FormatLiteral(value, true));
                writer.Write(", StringComparison.Ordinal)");
            }
        }

        private static void WriteGetDiscriminator(SourceWriter writer, string inputName, bool inputIsSpan, int index, int charsToRead)
        {
            if (charsToRead == 1)
            {
                writer.Write(inputName);
                writer.Write('[');
                writer.Write(SymbolDisplay.FormatPrimitive(index, false, false));
                writer.Write(']');
                return;
            }

            var discrimType = charsToRead == 2 ? "int" : "long";

            writer.Write("System.Runtime.InteropServices.MemoryMarshal.Read<");
            writer.Write(discrimType);
            writer.Write(">(System.Runtime.InteropServices.MemoryMarshal.Cast<char, byte>(");
            writer.Write(inputName);
            if (inputIsSpan)
                writer.Write(".Slice(");
            else
                writer.Write(".AsSpan(");
            writer.Write(SymbolDisplay.FormatPrimitive(index, false, false));
            writer.Write(", ");
            writer.Write(SymbolDisplay.FormatPrimitive(charsToRead, false, false));
            writer.Write(")))");
        }

        private static int GetUniqueColumnLocation(IEnumerable<string> input, out int charsToRead)
        {
            var inputArr = input.ToArray();
            if (inputArr.Length < 1)
            {
                throw new ArgumentException("Input must contain at least one string.", nameof(input));
            }

            if (inputArr.Select(str => str.Length).Distinct().Count() > 1)
            {
                throw new ArgumentException("All strings must have the same length.", nameof(input));
            }

            var idx = GetUniqueColumnLocationCore(inputArr, charsToRead = 1);
            if (idx == -1)
                idx = GetUniqueColumnLocationCore(inputArr, charsToRead = 2);
            if (idx == -1)
                idx = GetUniqueColumnLocationCore(inputArr, charsToRead = 4);
            return idx;

            static int GetUniqueColumnLocationCore(string[] inputArr, int charsToRead)
            {
                var strLength = inputArr[0].Length;
                var occurrences = new HashSet<long>[strLength - charsToRead + 1];
                for (var idx = 0; idx < occurrences.Length; idx++)
                {
                    occurrences[idx] = new HashSet<long>();
                }

                for (var arrIdx = 0; arrIdx < inputArr.Length; arrIdx++)
                {
                    var str = inputArr[arrIdx];
                    for (var strIdx = 0; strIdx < str.Length - charsToRead + 1; strIdx++)
                    {
                        var discrim = getDiscriminator(str, strIdx, charsToRead);
                        var idxOccs = occurrences[strIdx];
                        idxOccs.Add(discrim);
                    }
                }

                for (var idx = 0; idx < occurrences.Length; idx++)
                {
                    if (occurrences[idx].Count == inputArr.Length)
                    {
                        return idx;
                    }
                }
                return -1;
            }

            static long getDiscriminator(string str, int index, int charsToRead)
            {
                return charsToRead switch
                {
                    1 => str[index],
                    2 => MemoryMarshal.Read<int>(MemoryMarshal.Cast<char, byte>(str.AsSpan(index, 2))),
                    4 => MemoryMarshal.Read<long>(MemoryMarshal.Cast<char, byte>(str.AsSpan(index, 4))),
                    _ => throw new ArgumentOutOfRangeException(nameof(charsToRead))
                };
            }
        }
    }
}
