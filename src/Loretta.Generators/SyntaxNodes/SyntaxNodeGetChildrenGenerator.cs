using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.Generators.SyntaxNodes
{
    [Generator]
    public sealed class SyntaxNodeGetChildrenGenerator : SyntaxNodeGeneratorBase
    {
        protected override void GenerateFiles (
            GeneratorExecutionContext context,
            CSharpCompilation compilation,
            INamedTypeSymbol syntaxNodeType,
            INamedTypeSymbol syntaxTokenType,
            INamedTypeSymbol syntaxTriviaType,
            ImmutableArray<INamedTypeSymbol> syntaxNodeTypes )
        {
            INamedTypeSymbol? immutableArrayType = compilation.GetTypeByMetadataName ( "System.Collections.Immutable.ImmutableArray`1" );
            INamedTypeSymbol? separatedSyntaxListType = compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.SeparatedSyntaxList`1" );
            INamedTypeSymbol? statementSyntaxType = compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.StatementSyntax" );
            INamedTypeSymbol? optionType = compilation.GetTypeByMetadataName ( "Tsu.Option`1" );

            if ( immutableArrayType is null
                 || separatedSyntaxListType is null
                 || statementSyntaxType is null
                 || optionType is null )
            {
                return;
            }

            SourceText sourceText;
            using ( var writer = new StringWriter ( ) )
            {
                writer.WriteLine ( "using System;" );
                writer.WriteLine ( "using System.Collections.Generic;" );
                writer.WriteLine ( "using System.Collections.Immutable;" );
                writer.WriteLine ( );

                foreach ( INamedTypeSymbol? type in syntaxNodeTypes )
                {
                    CompletePartialType ( writer, type, writer =>
                    {
                        IEnumerable<IPropertySymbol> properties = Utilities.AncestorsAndSelf ( type, syntaxNodeType )
                                                                           .SelectMany ( type => type.GetMembers ( )
                                                                                                     .OfType<IPropertySymbol> ( ) );
                        foreach ( IPropertySymbol? property in properties )
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
                                        writer.WriteLine ( $"if ( {property.Name}.IsSome && {property.Name}.Value != null )" );
                                        writer.Indent++;
                                    }
                                    else if ( isOptional )
                                    {
                                        writer.WriteLine ( $"if ( {property.Name}.IsSome )" );
                                        writer.Indent++;
                                    }
                                    else if ( canBeNull )
                                    {
                                        writer.WriteLine ( $"if ( {property.Name} != null )" );
                                        writer.Indent++;
                                    }

                                    if ( isOptional )
                                        writer.WriteLine ( $"yield return this.{property.Name}.Value;" );
                                    else
                                        writer.WriteLine ( $"yield return this.{property.Name};" );

                                    if ( isOptional || canBeNull )
                                        writer.Indent--;
                                }
                                else if ( SymbolEqualityComparer.Default.Equals ( actualType.OriginalDefinition, immutableArrayType )
                                          && Utilities.IsDerivedFrom ( actualType.TypeArguments[0], syntaxNodeType ) )
                                {
                                    CurlyIndenter? indenter = null;
                                    if ( isOptional )
                                    {
                                        indenter = new CurlyIndenter (
                                            writer,
                                            $"if ( {property.Name}.IsSome )" );
                                        writer.WriteLine ( $"foreach ( var child in {property.Name}.Value )" );
                                    }
                                    else
                                    {
                                        writer.WriteLine ( $"foreach ( var child in {property.Name} )" );
                                    }

                                    using ( new Indenter ( writer ) )
                                        writer.WriteLine ( "yield return child;" );

                                    indenter?.Dispose ( );
                                }
                                else if ( SymbolEqualityComparer.Default.Equals ( actualType.OriginalDefinition, separatedSyntaxListType )
                                          && Utilities.IsDerivedFrom ( actualType.TypeArguments[0], syntaxNodeType ) )
                                {
                                    CurlyIndenter? indenter = null;
                                    if ( isOptional )
                                    {
                                        indenter = new CurlyIndenter (
                                            writer,
                                            $"if ( {property.Name}.IsSome )" );
                                        writer.WriteLine ( $"foreach ( var child in {property.Name}.Value.GetWithSeparators ( ) )" );
                                    }
                                    else
                                    {
                                        writer.WriteLine ( $"foreach ( var child in {property.Name}.GetWithSeparators ( ) )" );
                                    }

                                    using ( new Indenter ( writer ) )
                                        writer.WriteLine ( "yield return child;" );

                                    indenter?.Dispose ( );
                                }
                            }
                        }
                    } );
                }

                sourceText = SourceText.From ( writer.ToString ( ), Encoding.UTF8 );
            }

            var hintName = "SyntaxNode_GetChildren.g.cs";
            context.AddSource ( hintName, sourceText );
            Utilities.DoVsCodeHack ( syntaxNodeType, hintName, sourceText );
        }
    }
}