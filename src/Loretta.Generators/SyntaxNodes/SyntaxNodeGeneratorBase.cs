using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Loretta.Generators.SyntaxNodes
{
    public abstract class SyntaxNodeGeneratorBase : GeneratorBase
    {
        protected sealed override void GenerateFiles ( GeneratorExecutionContext context, CSharpCompilation compilation )
        {
            INamedTypeSymbol? syntaxNodeType = compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.SyntaxNode" );
            INamedTypeSymbol? syntaxTokenType = compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.SyntaxToken" );
            INamedTypeSymbol? syntaxTriviaType = compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.SyntaxTrivia" );
            if ( syntaxNodeType is null
                 || syntaxTokenType is null
                 || syntaxTriviaType is null )
            {
                return;
            }

            var syntaxNodeTypes = Utilities.GetAllTypes ( compilation.Assembly )
                                           .Where ( t => !t.IsAbstract
                                                         && Utilities.IsPartial ( t )
                                                         && Utilities.IsDerivedFrom ( t, syntaxNodeType ) )
                                           .ToImmutableArray ( );

            this.GenerateFiles (
                context,
                compilation,
                syntaxNodeType,
                syntaxTokenType,
                syntaxTriviaType,
                syntaxNodeTypes );
        }

        protected abstract void GenerateFiles (
            GeneratorExecutionContext context,
            CSharpCompilation compilation,
            INamedTypeSymbol syntaxNodeType,
            INamedTypeSymbol syntaxTokenType,
            INamedTypeSymbol syntaxTriviaType,
            ImmutableArray<INamedTypeSymbol> syntaxNodeTypes );
    }
}
