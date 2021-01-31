using System;
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
    public sealed class SyntaxNodeUpdateGenerator : SyntaxNodeGeneratorBase
    {
        protected override void GenerateFiles (
            GeneratorExecutionContext context,
            CSharpCompilation compilation,
            INamedTypeSymbol syntaxNodeType,
            INamedTypeSymbol syntaxTokenType,
            INamedTypeSymbol syntaxTriviaType,
            ImmutableArray<INamedTypeSymbol> syntaxNodeTypes )
        {
            SourceText updateText;
            using ( var writer = new StringWriter ( ) )
            {
                writer.WriteLine ( "using System;" );
                writer.WriteLine ( "using System.Collections.Generic;" );
                writer.WriteLine ( "using System.Collections.Immutable;" );
                writer.WriteLine ( "using Tsu;" );
                writer.WriteLine ( );
                writer.WriteLine ( "#nullable enable" );
                writer.WriteLine ( );

                foreach ( INamedTypeSymbol type in syntaxNodeTypes )
                {
                    CompletePartialType ( writer, type, writer =>
                    {
                        var unsyntaxedName = type.Name.Replace ( "Syntax", "" );
                        foreach ( IMethodSymbol constructor in type.Constructors )
                        {
                            var funcArguments = constructor.Parameters.Select ( p => $"{Utilities.TypeToShortString ( ( INamedTypeSymbol ) p.Type )} {p.Name}" ).ToImmutableArray ( );
                            var arguments = constructor.Parameters.Select ( p => p.Name ).ToImmutableArray ( );
                            using ( new CurlyIndenter ( writer, $"public {type.Name} Update ( {String.Join ( ", ", funcArguments )} )" ) )
                            {
                                using ( new CurlyIndenter ( writer, $"if ( {String.Join ( " || ", arguments.Select ( n => $"{n} != this.{title ( n )}" ) )} )" ) )
                                {
                                    writer.WriteLine ( $"return SyntaxFactory.{unsyntaxedName} ( {String.Join ( ", ", arguments )} );" );
                                }
                                writer.WriteLine ( );
                                writer.WriteLine ( "return this;" );
                            }
                        }
                    } );
                }

                updateText = SourceText.From ( writer.ToString ( ), Encoding.UTF8 );
            }

            var hintName = "SyntaxNode_Update.g.cs";
            context.AddSource ( hintName, updateText );
            Utilities.DoVsCodeHack ( syntaxNodeType, hintName, updateText );

            static String title ( String str ) => Char.ToUpper ( str[0] ) + str.Substring ( 1 );
        }
    }
}
