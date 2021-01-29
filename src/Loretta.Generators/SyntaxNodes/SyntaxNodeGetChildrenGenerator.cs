using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.Generators.SyntaxFacts
{
    [Generator]
    public sealed class SyntaxNodeGetChildrenGenerator : ISourceGenerator
    {
        public void Initialize ( GeneratorInitializationContext context )
        {
        }

        public void Execute ( GeneratorExecutionContext context )
        {
            SourceText sourceText;

            var compilation = ( CSharpCompilation ) context.Compilation;

            INamedTypeSymbol? immutableArrayType = compilation.GetTypeByMetadataName ( "System.Collections.Immutable.ImmutableArray`1" );
            INamedTypeSymbol? separatedSyntaxListType = compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.SeparatedSyntaxList`1" );
            INamedTypeSymbol? syntaxNodeType = compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.SyntaxNode" );
            INamedTypeSymbol? statementSyntaxType = compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.StatementSyntax" );
            INamedTypeSymbol? optionType = compilation.GetTypeByMetadataName ( "Tsu.Option`1" );

            if ( immutableArrayType is null
                 || separatedSyntaxListType is null
                 || syntaxNodeType is null
                 || statementSyntaxType is null
                 || optionType is null )
            {
                return;
            }

            ImmutableArray<INamedTypeSymbol> types = Utilities.GetAllTypes ( compilation.Assembly );
            IEnumerable<INamedTypeSymbol> syntaxNodeTypes = types.Where ( t => !t.IsAbstract && Utilities.IsPartial ( t ) && Utilities.IsDerivedFrom ( t, syntaxNodeType ) );

            var indentString = "    ";
            using ( var stringWriter = new StringWriter ( ) )
            using ( var indentedTextWriter = new IndentedTextWriter ( stringWriter, indentString ) )
            {
                indentedTextWriter.WriteLine ( "using System;" );
                indentedTextWriter.WriteLine ( "using System.Collections.Generic;" );
                indentedTextWriter.WriteLine ( "using System.Collections.Immutable;" );
                indentedTextWriter.WriteLine ( );

                using ( new CurlyIndenter ( indentedTextWriter, "namespace Loretta.CodeAnalysis.Syntax" ) )
                {
                    foreach ( INamedTypeSymbol? type in syntaxNodeTypes )
                    {
                        using ( new CurlyIndenter ( indentedTextWriter, $"partial class {type.Name}" ) )
                        {
                            indentedTextWriter.WriteLine ( "/// <inheritdoc />" );
                            using ( new CurlyIndenter ( indentedTextWriter, "public override IEnumerable<SyntaxNode> GetChildren ( )" ) )
                            {
                                foreach ( IPropertySymbol? property in type.GetMembers ( ).OfType<IPropertySymbol> ( ) )
                                {
                                    if ( property.Type is INamedTypeSymbol propertyType )
                                    {
                                        var isOptional = SymbolEqualityComparer.Default.Equals (
                                            propertyType.OriginalDefinition,
                                            optionType );
                                        INamedTypeSymbol actualType = isOptional
                                                                      ? ( INamedTypeSymbol ) propertyType.TypeArguments[0]
                                                                      : propertyType;

                                        if ( Utilities.IsDerivedFrom ( actualType, syntaxNodeType ) )
                                        {
                                            var canBeNull = property.NullableAnnotation == NullableAnnotation.Annotated;
                                            if ( isOptional && canBeNull )
                                            {
                                                indentedTextWriter.WriteLine ( $"if ( {property.Name}.IsSome && {property.Name}.Value != null )" );
                                                indentedTextWriter.Indent++;
                                            }
                                            else if ( isOptional )
                                            {
                                                indentedTextWriter.WriteLine ( $"if ( {property.Name}.IsSome )" );
                                                indentedTextWriter.Indent++;
                                            }
                                            else if ( canBeNull )
                                            {
                                                indentedTextWriter.WriteLine ( $"if ( {property.Name} != null )" );
                                                indentedTextWriter.Indent++;
                                            }

                                            if ( isOptional )
                                                indentedTextWriter.WriteLine ( $"yield return this.{property.Name}.Value;" );
                                            else
                                                indentedTextWriter.WriteLine ( $"yield return this.{property.Name};" );

                                            if ( isOptional || canBeNull )
                                                indentedTextWriter.Indent--;
                                        }
                                        else if ( SymbolEqualityComparer.Default.Equals ( actualType.OriginalDefinition, immutableArrayType )
                                                  && Utilities.IsDerivedFrom ( actualType.TypeArguments[0], syntaxNodeType ) )
                                        {
                                            CurlyIndenter? indenter = null;
                                            if ( isOptional )
                                            {
                                                indenter = new CurlyIndenter (
                                                    indentedTextWriter,
                                                    $"if ( {property.Name}.IsSome )" );
                                                indentedTextWriter.WriteLine ( $"foreach ( var child in {property.Name}.Value )" );
                                            }
                                            else
                                            {
                                                indentedTextWriter.WriteLine ( $"foreach ( var child in {property.Name} )" );
                                            }

                                            using ( new Indenter ( indentedTextWriter ) )
                                                indentedTextWriter.WriteLine ( "yield return child;" );

                                            indenter?.Dispose ( );
                                        }
                                        else if ( SymbolEqualityComparer.Default.Equals ( actualType.OriginalDefinition, separatedSyntaxListType )
                                                  && Utilities.IsDerivedFrom ( actualType.TypeArguments[0], syntaxNodeType ) )
                                        {
                                            CurlyIndenter? indenter = null;
                                            if ( isOptional )
                                            {
                                                indenter = new CurlyIndenter (
                                                    indentedTextWriter,
                                                    $"if ( {property.Name}.IsSome )" );
                                                indentedTextWriter.WriteLine ( $"foreach ( var child in {property.Name}.Value.GetWithSeparators ( ) )" );
                                            }
                                            else
                                            {
                                                indentedTextWriter.WriteLine ( $"foreach ( var child in {property.Name}.GetWithSeparators ( ) )" );
                                            }

                                            using ( new Indenter ( indentedTextWriter ) )
                                                indentedTextWriter.WriteLine ( "yield return child;" );

                                            indenter?.Dispose ( );
                                        }
                                    }
                                }

                                if ( Utilities.IsDerivedFrom ( type, statementSyntaxType ) )
                                {
                                    using ( new Indenter ( indentedTextWriter, "if ( this.SemicolonToken.IsSome )" ) )
                                        indentedTextWriter.WriteLine ( "yield return this.SemicolonToken.Value;" );
                                }
                            }
                        }
                    }
                }

                indentedTextWriter.Flush ( );
                stringWriter.Flush ( );

                sourceText = SourceText.From ( stringWriter.ToString ( ), Encoding.UTF8 );
            }

            var hintName = "SyntaxNode_GetChildren.g.cs";
            context.AddSource ( hintName, sourceText );

            // HACK
            //
            // Make generator work in VS Code. See src\Directory.Build.props for
            // details.

            var fileName = "SyntaxNode_GetChildren.g.cs";
            var syntaxNodeFilePath = syntaxNodeType.DeclaringSyntaxReferences.First ( ).SyntaxTree.FilePath;
            var syntaxDirectory = Path.GetDirectoryName ( syntaxNodeFilePath );
            var filePath = Path.Combine ( syntaxDirectory, fileName );

            if ( File.Exists ( filePath ) )
            {
                var fileText = File.ReadAllText ( filePath );
                var sourceFileText = SourceText.From ( fileText, Encoding.UTF8 );
                if ( sourceText.ContentEquals ( sourceFileText ) )
                    return;
            }

            using var writer = new StreamWriter ( filePath );
            sourceText.Write ( writer );
        }
    }
}