using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.Generators.SyntaxNodes
{
    [Generator]
    public class SyntaxVisitorGenerator : SyntaxNodeGeneratorBase
    {
        protected override void GenerateFiles (
            GeneratorExecutionContext context,
            CSharpCompilation compilation,
            INamedTypeSymbol syntaxNodeType,
            INamedTypeSymbol syntaxTokenType,
            INamedTypeSymbol syntaxTriviaType,
            ImmutableArray<INamedTypeSymbol> syntaxNodeTypes )
        {
            INamedTypeSymbol? voidSyntaxVisitorType = compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.SyntaxVisitor" );
            INamedTypeSymbol? genericSyntaxVisitorType = compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.SyntaxVisitor`1" );

            if ( voidSyntaxVisitorType is null
                 || !Utilities.IsPartial ( voidSyntaxVisitorType )
                 || genericSyntaxVisitorType is null
                 || !Utilities.IsPartial ( genericSyntaxVisitorType ) )
            {
                return;
            }

            SourceText visitorSourceText;
            using ( var writer = new StringWriter ( ) )
            {
                writer.WriteLine ( "#nullable enable" );
                writer.WriteLine ( );

                CompletePartialType ( writer, voidSyntaxVisitorType, writer =>
                {
                    foreach ( INamedTypeSymbol? type in syntaxNodeTypes )
                    {
                        var unsyntaxedName = type.Name.Replace ( "Syntax", "" );
                        var camelName = Char.ToLower ( type.Name[0] ) + type.Name.Substring ( 1 );

                        writer.WriteLine ( "/// <summary>" );
                        writer.WriteLine ( $"/// Visits a <see cref=\"{type.Name}\"/>." );
                        writer.WriteLine ( "/// </summary>" );
                        writer.WriteLine ( $"/// <param name=\"{camelName}\">The node being visited.</param>" );
                        writer.WriteLine ( $"public virtual void Visit{unsyntaxedName} ( {type.Name} {camelName} ) => this.DefaultVisit ( {camelName} );" );
                    }
                } );

                writer.WriteLine ( );

                CompletePartialType ( writer, genericSyntaxVisitorType, writer =>
                {
                    foreach ( INamedTypeSymbol? type in syntaxNodeTypes )
                    {
                        var unsyntaxedName = type.Name.Replace ( "Syntax", "" );
                        var camelName = Char.ToLower ( type.Name[0] ) + type.Name.Substring ( 1 );

                        writer.WriteLine ( "/// <summary>" );
                        writer.WriteLine ( $"/// Visits a <see cref=\"{type.Name}\"/>." );
                        writer.WriteLine ( "/// </summary>" );
                        writer.WriteLine ( $"/// <param name=\"{camelName}\">The node being visited.</param>" );
                        writer.WriteLine ( $"/// <returns></returns>" );
                        writer.WriteLine ( $"public virtual TReturn? Visit{unsyntaxedName} ( {type.Name} {camelName} ) => this.DefaultVisit ( {camelName} );" );
                    }
                } );

                visitorSourceText = SourceText.From ( writer.ToString ( ), Encoding.UTF8 );
            }

            var hintName = "SyntaxVisitor.g.cs";
            context.AddSource ( hintName, visitorSourceText );
            Utilities.DoVsCodeHack ( voidSyntaxVisitorType, hintName, visitorSourceText );
        }
    }
}
