using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.Generators.ErrorCode
{
    [Generator]
    public sealed class ErrorFactsGenerator : GeneratorBase
    {
        protected override void GenerateFiles(GeneratorExecutionContext context, CSharpCompilation compilation)
        {
            var errorCodeType =
                compilation.GetTypeByMetadataName("Loretta.CodeAnalysis.Lua.ErrorCode");

            if (errorCodeType is null) return;

            var codes = errorCodeType.GetMembers().OfType<IFieldSymbol>().ToImmutableArray();

            SourceText sourceText;
            using (var writer = new SourceWriter())
            {
                using (writer.CurlyIndenter("namespace Loretta.CodeAnalysis.Lua"))
                using (writer.CurlyIndenter("internal static partial class ErrorFacts"))
                {
                    using (writer.CurlyIndenter("public static partial bool IsWarning(ErrorCode code)"))
                    using (writer.CurlyIndenter("switch(code)"))
                    {
                        var warnings = codes.Where(field => field.Name.StartsWith("WRN_", StringComparison.OrdinalIgnoreCase));
                        if (warnings.Any())
                        {
                            foreach (var code in warnings)
                                writer.WriteLine($"case ErrorCode.{code.Name}:");
                            using (writer.Indenter())
                                writer.WriteLine("return true;");
                        }
                        writer.WriteLine("default:");
                        using (writer.Indenter())
                            writer.WriteLine("return false;");
                    }
                    writer.WriteLine();
                    using (writer.CurlyIndenter("public static partial bool IsFatal(ErrorCode code)"))
                    using (writer.CurlyIndenter("switch(code)"))
                    {
                        var fatals = codes.Where(field => field.Name.StartsWith("FTL_", StringComparison.OrdinalIgnoreCase));
                        if (fatals.Any())
                        {
                            foreach (var code in fatals)
                                writer.WriteLine($"case ErrorCode.{code.Name}:");
                            using (writer.Indenter())
                                writer.WriteLine("return true;");
                        }
                        writer.WriteLine("default:");
                        using (writer.Indenter())
                            writer.WriteLine("return false;");
                    }
                    writer.WriteLine();
                    using (writer.CurlyIndenter("public static partial bool IsInfo(ErrorCode code)"))
                    using (writer.CurlyIndenter("switch(code)"))
                    {
                        var infos = codes.Where(field => field.Name.StartsWith("INF_", StringComparison.OrdinalIgnoreCase));
                        if (infos.Any())
                        {
                            foreach (var code in infos)
                                writer.WriteLine($"case ErrorCode.{code.Name}:");
                            using (writer.Indenter())
                                writer.WriteLine("return true;");
                        }
                        writer.WriteLine("default:");
                        using (writer.Indenter())
                            writer.WriteLine("return false;");
                    }
                    writer.WriteLine();
                    using (writer.CurlyIndenter("public static partial bool IsHidden(ErrorCode code)"))
                    using (writer.CurlyIndenter("switch(code)"))
                    {
                        var hidden = codes.Where(field => field.Name.StartsWith("HDN_", StringComparison.OrdinalIgnoreCase));
                        if (hidden.Any())
                        {
                            foreach (var code in hidden)
                                writer.WriteLine($"case ErrorCode.{code.Name}:");
                            using (writer.Indenter())
                                writer.WriteLine("return true;");
                        }
                        writer.WriteLine("default:");
                        using (writer.Indenter())
                            writer.WriteLine("return false;");
                    }
                }

                sourceText = writer.GetText();
            }

            context.AddSource("ErrorFacts.g.cs", sourceText);
            Utilities.DoVsCodeHack(errorCodeType, "ErrorFacts.g.cs", sourceText);
        }
    }
}
