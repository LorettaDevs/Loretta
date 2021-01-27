using System;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.Generators.SyntaxKind
{
    public sealed partial class SyntaxKindRelatedTypesGenerator
    {
        private static void GenerateSyntaxVisitor ( GeneratorExecutionContext context, INamedTypeSymbol syntaxKindType, ImmutableArray<KindInfo> kinds )
        {

            var compilation = ( CSharpCompilation ) context.Compilation;

            INamedTypeSymbol? syntaxNodeType =
                compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.SyntaxNode" );

            if ( syntaxNodeType is null )
                return;

            SourceText sourceText;
            using ( var stringWriter = new StringWriter ( ) )
            using ( var indentedTextWriter = new IndentedTextWriter ( stringWriter, "    " ) )
            {
                indentedTextWriter.WriteLine ( "using System;" );
                indentedTextWriter.WriteLine ( );
                indentedTextWriter.WriteLine ( "#nullable enable" );
                indentedTextWriter.WriteLine ( );

                using ( new CurlyIndenter ( indentedTextWriter, "namespace Loretta.CodeAnalysis.Syntax" ) )
                {
                    using ( new CurlyIndenter ( indentedTextWriter, "public abstract class SyntaxVisitor<TArgument, TReturn>" ) )
                    {
                        foreach ( KindInfo kind in kinds.Where ( kind => kind.NodeInfo is not null ) )
                        {
                            var pascalTypeName = kind.NodeInfo!.Value.NodeType.Name;
                            var camelTypeName = Char.ToLower ( pascalTypeName[0] ) + pascalTypeName.Substring ( 1 );

                            indentedTextWriter.WriteLine ( "/// <summary>" );
                            indentedTextWriter.WriteLine ( "/// </summary>" );
                            indentedTextWriter.WriteLine ( $"/// <param name=\"{camelTypeName}\">The <see cref=\"{pascalTypeName}\" /> node being visited.</param>" );
                            indentedTextWriter.WriteLine ( "/// <param name=\"argument\">The argument passed to the visitor.</param>" );
                            indentedTextWriter.WriteLine ( "/// <returns></returns>" );
                            indentedTextWriter.WriteLine ( $"protected abstract TReturn Visit{kind.Field.Name} ( {pascalTypeName} {camelTypeName}, TArgument argument );" );
                            indentedTextWriter.WriteLineNoTabs ( "" );
                        }

                        indentedTextWriter.WriteLine ( "/// <summary>" );
                        indentedTextWriter.WriteLine ( "/// </summary>" );
                        indentedTextWriter.WriteLine ( $"/// <param name=\"syntaxNode\">The <see cref=\"SyntaxNode\" /> to visit.</param>" );
                        indentedTextWriter.WriteLine ( "/// <param name=\"argument\">The argument to pass to the visitor.</param>" );
                        indentedTextWriter.WriteLine ( "/// <returns></returns>" );
                        using ( new CurlyIndenter ( indentedTextWriter, "public virtual TReturn Visit ( SyntaxNode syntaxNode, TArgument argument )" ) )
                        {
                            indentedTextWriter.WriteLine ( "return syntaxNode.Kind switch" );
                            indentedTextWriter.WriteLine ( "{" );
                            using ( new Indenter ( indentedTextWriter ) )
                            {
                                foreach ( KindInfo kind in kinds.Where ( kind => kind.NodeInfo is not null ) )
                                    indentedTextWriter.WriteLine ( $"SyntaxKind.{kind.Field.Name} => this.Visit{kind.Field.Name} ( ( {kind.NodeInfo!.Value.NodeType.Name} ) syntaxNode, argument )," );
                                indentedTextWriter.WriteLine ( "_ => throw new InvalidOperationException ( \"The provided node is not a syntax tree node.\" )" );
                            }
                            indentedTextWriter.WriteLine ( "};" );
                        }
                    }

                    indentedTextWriter.WriteLineNoTabs ( "" );

                    using ( new CurlyIndenter ( indentedTextWriter, "public abstract class SyntaxVisitor<TReturn>" ) )
                    {
                        foreach ( KindInfo kind in kinds.Where ( kind => kind.NodeInfo is not null ) )
                        {
                            var pascalTypeName = kind.NodeInfo!.Value.NodeType.Name;
                            var camelTypeName = Char.ToLower ( pascalTypeName[0] ) + pascalTypeName.Substring ( 1 );

                            indentedTextWriter.WriteLine ( "/// <summary>" );
                            indentedTextWriter.WriteLine ( "/// </summary>" );
                            indentedTextWriter.WriteLine ( $"/// <param name=\"{camelTypeName}\">The <see cref=\"{pascalTypeName}\" /> node being visited.</param>" );
                            indentedTextWriter.WriteLine ( "/// <returns></returns>" );
                            indentedTextWriter.WriteLine ( $"protected abstract TReturn Visit{kind.Field.Name} ( {pascalTypeName} {camelTypeName} );" );
                            indentedTextWriter.WriteLineNoTabs ( "" );
                        }

                        indentedTextWriter.WriteLine ( "/// <summary>" );
                        indentedTextWriter.WriteLine ( "/// </summary>" );
                        indentedTextWriter.WriteLine ( $"/// <param name=\"syntaxNode\">The <see cref=\"SyntaxNode\" /> to visit.</param>" );
                        indentedTextWriter.WriteLine ( "/// <returns></returns>" );
                        using ( new CurlyIndenter ( indentedTextWriter, "public virtual TReturn Visit ( SyntaxNode syntaxNode )" ) )
                        {
                            indentedTextWriter.WriteLine ( "return syntaxNode.Kind switch" );
                            indentedTextWriter.WriteLine ( "{" );
                            using ( new Indenter ( indentedTextWriter ) )
                            {
                                foreach ( KindInfo kind in kinds.Where ( kind => kind.NodeInfo is not null ) )
                                    indentedTextWriter.WriteLine ( $"SyntaxKind.{kind.Field.Name} => this.Visit{kind.Field.Name} ( ( {kind.NodeInfo!.Value.NodeType.Name} ) syntaxNode )," );
                                indentedTextWriter.WriteLine ( "_ => throw new InvalidOperationException ( \"The provided node is not a syntax tree node.\" )" );
                            }
                            indentedTextWriter.WriteLine ( "};" );
                        }
                    }

                    indentedTextWriter.WriteLineNoTabs ( "" );

                    using ( new CurlyIndenter ( indentedTextWriter, "public abstract class SyntaxVisitor" ) )
                    {
                        foreach ( KindInfo kind in kinds.Where ( kind => kind.NodeInfo is not null ) )
                        {
                            var pascalTypeName = kind.NodeInfo!.Value.NodeType.Name;
                            var camelTypeName = Char.ToLower ( pascalTypeName[0] ) + pascalTypeName.Substring ( 1 );

                            indentedTextWriter.WriteLine ( "/// <summary>" );
                            indentedTextWriter.WriteLine ( "/// </summary>" );
                            indentedTextWriter.WriteLine ( $"/// <param name=\"{camelTypeName}\">The <see cref=\"{pascalTypeName}\" /> node being visited.</param>" );
                            indentedTextWriter.WriteLine ( "/// <returns></returns>" );
                            indentedTextWriter.WriteLine ( $"protected abstract void Visit{kind.Field.Name} ( {pascalTypeName} {camelTypeName} );" );
                            indentedTextWriter.WriteLineNoTabs ( "" );
                        }

                        indentedTextWriter.WriteLine ( "/// <summary>" );
                        indentedTextWriter.WriteLine ( "/// </summary>" );
                        indentedTextWriter.WriteLine ( $"/// <param name=\"syntaxNode\">The <see cref=\"SyntaxNode\" /> to visit.</param>" );
                        indentedTextWriter.WriteLine ( "/// <returns></returns>" );
                        using ( new CurlyIndenter ( indentedTextWriter, "public virtual void Visit ( SyntaxNode syntaxNode )" ) )
                        {
                            using ( new CurlyIndenter ( indentedTextWriter, "switch ( syntaxNode.Kind )" ) )
                            {
                                foreach ( KindInfo kind in kinds.Where ( kind => kind.NodeInfo is not null ) )
                                {
                                    indentedTextWriter.WriteLine ( $"case SyntaxKind.{kind.Field.Name}:" );
                                    using ( new Indenter ( indentedTextWriter ) )
                                    {
                                        indentedTextWriter.WriteLine ( $"this.Visit{kind.Field.Name} ( ( {kind.NodeInfo!.Value.NodeType.Name} ) syntaxNode );" );
                                        indentedTextWriter.WriteLine ( "break;" );
                                    }
                                }
                            }
                        }
                    }
                }

                indentedTextWriter.Flush ( );
                stringWriter.Flush ( );

                sourceText = SourceText.From ( stringWriter.ToString ( ), Encoding.UTF8 );
            }

            context.AddSource ( "SyntaxVisitor.g.cs", sourceText );
            DoVsCodeHack ( syntaxKindType, "SyntaxVisitor.g.cs", sourceText );
        }
    }
}
