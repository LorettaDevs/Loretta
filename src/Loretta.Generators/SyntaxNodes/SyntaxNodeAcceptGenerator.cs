using System;
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
    public class SyntaxNodeAcceptGenerator : SyntaxNodeGeneratorBase
    {
        protected override void GenerateFiles (
            GeneratorExecutionContext context,
            CSharpCompilation compilation,
            INamedTypeSymbol syntaxNodeType,
            INamedTypeSymbol syntaxTokenType,
            INamedTypeSymbol syntaxTriviaType,
            ImmutableArray<INamedTypeSymbol> syntaxNodeTypes )
        {
            SourceText nodesSourceText;
            
            using ( var writer = new StringWriter ( ) )
            {
                writer.WriteLine ( "#nullable enable" );

                foreach ( INamedTypeSymbol? type in syntaxNodeTypes )
                {
                    var unsyntaxedName = type.Name.Replace ( "Syntax", "" );
                    var camelName = Char.ToLower ( type.Name[0] ) + type.Name.Substring ( 1 );
                    writer.WriteLine ( );

                    CompletePartialType ( writer, type, writer =>
                    {
                        writer.WriteLine ( "/// <inheritdoc/>" );
                        using ( new Indenter ( writer, "public override void Accept ( SyntaxVisitor syntaxVisitor ) =>" ) )
                            writer.WriteLine ( $"syntaxVisitor.Visit{unsyntaxedName} ( this );" );

                        writer.WriteLine ( "/// <inheritdoc/>" );
                        using ( new Indenter ( writer, "public override TReturn? Accept<TReturn> ( SyntaxVisitor<TReturn> syntaxVisitor ) where TReturn : default =>" ) )
                            writer.WriteLine ( $"syntaxVisitor.Visit{unsyntaxedName} ( this );" );
                    } );
                }
                nodesSourceText = SourceText.From ( writer.ToString ( ), Encoding.UTF8 );
            }

            var hintName = "SyntaxNode_Accept.g.cs";
            context.AddSource ( hintName, nodesSourceText );
            Utilities.DoVsCodeHack ( syntaxNodeType, hintName, nodesSourceText );
        }
    }
}
