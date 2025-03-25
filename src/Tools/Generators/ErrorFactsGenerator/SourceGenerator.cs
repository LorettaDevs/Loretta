using Microsoft.CodeAnalysis;

namespace Loretta.Generators.ErrorCode
{
    [Generator(LanguageNames.CSharp)]
    public sealed class SourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var errorCodeType = context.CompilationProvider.Select(
                static (comp, _) => comp.GetTypeByMetadataName("Loretta.CodeAnalysis.Lua.ErrorCode"));

            var errorCodeFields = errorCodeType.SelectMany(
                static (type, _) => type?.GetMembers().OfType<IFieldSymbol>() ?? []).Collect();

            context.RegisterSourceOutput(
                errorCodeFields,
                // ReSharper disable once VariableHidesOuterVariable
                static (context, codes) =>
                {
                    if (codes.IsEmpty) return;

                    using var writer = new SourceWriter();
                    using (writer.CurlyIndenter("namespace Loretta.CodeAnalysis.Lua"))
                    using (writer.CurlyIndenter("internal static partial class ErrorFacts"))
                    {
                        using (writer.CurlyIndenter("public static partial bool IsWarning(ErrorCode code)"))
                        using (writer.CurlyIndenter("switch(code)"))
                        {
                            var warnings = codes.Where(
                                                    static field => field.Name.StartsWith(
                                                        "WRN_",
                                                        StringComparison.OrdinalIgnoreCase))
                                                .ToArray();
                            if (warnings.Length != 0)
                            {
                                foreach (var code in warnings) writer.WriteLine($"case ErrorCode.{code.Name}:");
                                using (writer.Indenter()) writer.WriteLine("return true;");
                            }
                            writer.WriteLine("default:");
                            using (writer.Indenter()) writer.WriteLine("return false;");
                        }
                        writer.WriteLine();
                        using (writer.CurlyIndenter("public static partial bool IsFatal(ErrorCode code)"))
                        using (writer.CurlyIndenter("switch(code)"))
                        {
                            var fatals = codes.Where(
                                                  static field => field.Name.StartsWith(
                                                      "FTL_",
                                                      StringComparison.OrdinalIgnoreCase))
                                              .ToArray();
                            if (fatals.Length != 0)
                            {
                                foreach (var code in fatals) writer.WriteLine($"case ErrorCode.{code.Name}:");
                                using (writer.Indenter()) writer.WriteLine("return true;");
                            }
                            writer.WriteLine("default:");
                            using (writer.Indenter()) writer.WriteLine("return false;");
                        }
                        writer.WriteLine();
                        using (writer.CurlyIndenter("public static partial bool IsInfo(ErrorCode code)"))
                        using (writer.CurlyIndenter("switch(code)"))
                        {
                            var infos = codes.Where(
                                                 static field => field.Name.StartsWith(
                                                     "INF_",
                                                     StringComparison.OrdinalIgnoreCase))
                                             .ToArray();
                            if (infos.Length != 0)
                            {
                                foreach (var code in infos) writer.WriteLine($"case ErrorCode.{code.Name}:");
                                using (writer.Indenter()) writer.WriteLine("return true;");
                            }
                            writer.WriteLine("default:");
                            using (writer.Indenter()) writer.WriteLine("return false;");
                        }
                        writer.WriteLine();
                        using (writer.CurlyIndenter("public static partial bool IsHidden(ErrorCode code)"))
                        using (writer.CurlyIndenter("switch(code)"))
                        {
                            var hidden = codes.Where(
                                                  static field => field.Name.StartsWith(
                                                      "HDN_",
                                                      StringComparison.OrdinalIgnoreCase))
                                              .ToArray();
                            if (hidden.Length != 0)
                            {
                                foreach (var code in hidden) writer.WriteLine($"case ErrorCode.{code.Name}:");
                                using (writer.Indenter()) writer.WriteLine("return true;");
                            }
                            writer.WriteLine("default:");
                            using (writer.Indenter()) writer.WriteLine("return false;");
                        }
                    }

                    context.AddSource("ErrorFacts.g.cs", sourceText: writer.GetText());
                });
        }
    }
}
