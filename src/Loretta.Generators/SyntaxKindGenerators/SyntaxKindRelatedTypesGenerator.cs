using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Loretta.Generators.SyntaxKindGenerators
{
    [Generator]
    public sealed partial class SyntaxKindRelatedTypesGenerator : ISourceGenerator
    {
        public void Initialize ( GeneratorInitializationContext context )
        {
        }

        public void Execute ( GeneratorExecutionContext context )
        {
            var compilation = ( CSharpCompilation ) context.Compilation;

            INamedTypeSymbol? syntaxKindType =
                compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.SyntaxKind" );

            if ( syntaxKindType is null )
                return;

            try
            {
                KindList? kinds = KindUtils.GetKindInfos ( context, compilation );
                if ( kinds is null )
                    return;
                if ( kinds.Count < 1 )
                {
                    context.ReportDiagnostic ( Diagnostic.Create ( Diagnostics.NoSyntaxKindWithAttributesFound, syntaxKindType.Locations.Single ( ) ) );
                    return;
                }

                GenerateSyntaxFacts ( context, syntaxKindType, kinds );
            }
            catch ( Exception ex )
            {
                var syntaxKindFilePath = syntaxKindType.DeclaringSyntaxReferences.First ( ).SyntaxTree.FilePath;
                var syntaxDirectory = Path.GetDirectoryName ( syntaxKindFilePath );
                var filePath = Path.Combine ( syntaxDirectory, "exception.log" );
                var contents = String.Join ( Environment.NewLine, new String ( '-', 30 ), ex.ToString ( ) );
                File.AppendAllText ( filePath, contents );

                throw;
            }
        }
    }
}
