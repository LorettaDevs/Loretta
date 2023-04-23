using Microsoft.CodeAnalysis;

namespace Loretta.Generators.ErrorCode
{
    [Generator(LanguageNames.CSharp)]
    public sealed class SourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var errorCodeType = context.CompilationProvider.Select((comp, _) =>
                comp.GetTypeByMetadataName("Loretta.CodeAnalysis.Lua.ErrorCode"));

            var errorCodeFields = errorCodeType.SelectMany((type, _) =>
                type?.GetMembers().OfType<IFieldSymbol>() ?? Enumerable.Empty<IFieldSymbol>())
                                               .Collect();

            context.RegisterSourceOutput(errorCodeFields, (context, codes) =>
            {
                if (codes.IsEmpty)
                {
                    return;
                }
                else
                {
                    using var writer = new SourceWriter();
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

                    context.AddSource("ErrorFacts.g.cs", writer.GetText());
                }
            });
        }
    }
}
