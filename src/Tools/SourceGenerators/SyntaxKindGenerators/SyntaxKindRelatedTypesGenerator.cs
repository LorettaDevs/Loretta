using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Loretta.Generators.SyntaxKindGenerators
{
    [Generator]
    public sealed partial class SyntaxKindRelatedTypesGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = (CSharpCompilation) context.Compilation;

            var syntaxKindType =
                compilation.GetTypeByMetadataName("Loretta.CodeAnalysis.Lua.SyntaxKind");

            if (syntaxKindType is null)
                throw new Exception("syntaxKindType is null.");

            context.AddSource("SyntaxKindAttributes.g.cs", KindUtils.SyntaxKindAttributesText);

            var kinds = KindUtils.GetKindInfos(context, compilation);
            if (kinds is null)
                throw new Exception("KindList is null");
            if (kinds.Count < 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.NoSyntaxKindWithAttributesFound, syntaxKindType.Locations.Single()));
                return;
            }

            GenerateSyntaxFacts(context, syntaxKindType, kinds);
        }
    }
}
