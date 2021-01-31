using System;
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

namespace Loretta.Generators
{
    public abstract class GeneratorBase : ISourceGenerator
    {
        public void Initialize ( GeneratorInitializationContext context )
        {
        }

        public void Execute ( GeneratorExecutionContext context )
        {
            var extraFiles = new List<SourceText> ( );
            this.AddFixedFiles ( context, extraFiles );
            CSharpParseOptions options = ( context.ParseOptions as CSharpParseOptions )!;
            CSharpCompilation compilation = ( context.Compilation as CSharpCompilation )!.AddSyntaxTrees ( extraFiles.Select ( t => CSharpSyntaxTree.ParseText ( t, options ) ) );
            this.GenerateFiles ( context, compilation );

        }

        protected virtual void AddFixedFiles ( GeneratorExecutionContext context, List<SourceText> extraFiles )
        {
        }

        protected abstract void GenerateFiles ( GeneratorExecutionContext context, CSharpCompilation compilation );

        protected static void CompletePartialType (
            StringWriter writer,
            INamedTypeSymbol namedTypeSymbol,
            Action<IndentedTextWriter> bodyAction,
            Action<IndentedTextWriter>? prefixAction = null )
        {
            TypeDeclarationSyntax? declaration = null;
            foreach ( SyntaxReference declarationCandidate in namedTypeSymbol.DeclaringSyntaxReferences )
            {
                SyntaxNode syntax = declarationCandidate.GetSyntax ( );
                if ( syntax is TypeDeclarationSyntax typeDeclaration )
                {
                    foreach ( SyntaxToken modifer in typeDeclaration.Modifiers )
                    {
                        if ( modifer.ValueText == "partial" )
                            declaration = typeDeclaration;
                    }
                }
            }

            if ( declaration is null )
                throw new InvalidOperationException ( "Cannot write a partial definition of a type that is not partial." );

            using var textWriter = new IndentedTextWriter ( writer, "    " );
            prefixAction?.Invoke ( textWriter );
            var stack = new Stack<CurlyIndenter> ( );
            foreach ( SyntaxNode? ancestor in declaration.AncestorsAndSelf ( ).Reverse ( ) )
            {
                switch ( ancestor.Kind ( ) )
                {
                    case SyntaxKind.NamespaceDeclaration:
                    {
                        var ancestorNamespace = ( NamespaceDeclarationSyntax ) ancestor;
                        stack.Push ( new CurlyIndenter (
                            textWriter,
                            $"namespace {ancestorNamespace.Name}" ) );
                    }
                    break;

                    case SyntaxKind.ClassDeclaration:
                    {
                        var ancestorClass = ( ClassDeclarationSyntax ) ancestor;
                        if ( !ancestorClass.Modifiers.Any ( mod => mod.ValueText == "partial" ) )
                            throw new InvalidOperationException ( "Type is contained within a non-partial class." );
                        stack.Push ( new CurlyIndenter (
                            textWriter,
                            $"{String.Join ( " ", ancestorClass.Modifiers )} class {ancestorClass.Identifier}" ) );
                    }
                    break;

                    case SyntaxKind.StructDeclaration:
                    {
                        var ancestorStruct = ( StructDeclarationSyntax ) ancestor;
                        if ( !ancestorStruct.Modifiers.Any ( mod => mod.ValueText == "partial" ) )
                            throw new InvalidOperationException ( "Type is contained within a non-partial struct." );
                        stack.Push ( new CurlyIndenter (
                            textWriter,
                            $"{String.Join ( " ", ancestorStruct.Modifiers.Where ( m => m.Kind ( ) != SyntaxKind.PartialKeyword ) )} partial struct {ancestorStruct.Identifier}" ) );
                    }
                    break;
                }
            }

            bodyAction ( textWriter );

            while ( stack.Count > 0 )
                stack.Pop ( ).Dispose ( );

            textWriter.Flush ( );
            writer.Flush ( );
        }
    }
}
