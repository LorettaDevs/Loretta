using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.Generators
{
    internal static class Utilities
    {
        public static ImmutableArray<INamedTypeSymbol> GetAllTypes ( IAssemblySymbol symbol )
        {
            var result = new List<INamedTypeSymbol> ( );
            GetAllTypes ( result, symbol.GlobalNamespace );
            result.Sort ( ( x, y ) => x.MetadataName.CompareTo ( y.MetadataName ) );
            return result.ToImmutableArray ( );
        }

        public static Boolean IsDerivedFrom ( ITypeSymbol? type, INamedTypeSymbol baseType )
        {
            ITypeSymbol? current = type;

            while ( current != null )
            {
                if ( SymbolEqualityComparer.Default.Equals ( current, baseType ) )
                    return true;

                current = current.BaseType;
            }

            return false;
        }

        public static ImmutableArray<ITypeSymbol> GetParentsUntil ( ITypeSymbol? type, INamedTypeSymbol baseType )
        {
            if ( !IsDerivedFrom ( type, baseType ) )
                throw new InvalidOperationException ( "Provided type is not derived from base type." );

            ImmutableArray<ITypeSymbol>.Builder parents = ImmutableArray.CreateBuilder<ITypeSymbol> ( );

            INamedTypeSymbol? current = type?.BaseType;
            while ( current != null )
            {
                if ( SymbolEqualityComparer.Default.Equals ( current, baseType ) )
                    break;

                parents.Add ( current );
                current = current.BaseType;
            }

            return parents.ToImmutable ( );
        }

        public static Boolean IsPartial ( INamedTypeSymbol type )
        {
            foreach ( SyntaxReference declaration in type.DeclaringSyntaxReferences )
            {
                SyntaxNode syntax = declaration.GetSyntax ( );
                if ( syntax is TypeDeclarationSyntax typeDeclaration )
                {
                    foreach ( SyntaxToken modifer in typeDeclaration.Modifiers )
                    {
                        if ( modifer.ValueText == "partial" )
                            return true;
                    }
                }
            }

            return false;
        }

        public static AttributeData? GetAttribute ( ISymbol symbol, INamedTypeSymbol attributeType ) =>
            symbol.GetAttributes ( )
                  .SingleOrDefault ( data => SymbolEqualityComparer.Default.Equals ( attributeType, data.AttributeClass ) );

        public static ImmutableArray<AttributeData> GetAttributes ( ISymbol symbol, INamedTypeSymbol attributeType ) =>
            symbol.GetAttributes ( )
                  .Where ( data => SymbolEqualityComparer.Default.Equals ( data.AttributeClass, attributeType ) )
                  .ToImmutableArray ( );

        public static void DoVsCodeHack ( INamedTypeSymbol relatedSymbol, String fileName, SourceText sourceText )
        {
            // HACK
            //
            // Make generator work in VS Code. See src\Directory.Build.props for
            // details.

            var relatedFilePath = relatedSymbol.DeclaringSyntaxReferences.First ( ).SyntaxTree.FilePath;
            var relatedDirectory = Path.GetDirectoryName ( relatedFilePath );

            DoVsCodeHack ( relatedDirectory, fileName, sourceText );
        }

        public static void DoVsCodeHack ( String relatedDirectory, String fileName, SourceText sourceText )
        {
            var filePath = Path.Combine ( relatedDirectory, fileName );
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

        public static IEnumerable<ITypeSymbol> AncestorsAndSelf ( ITypeSymbol topType, ITypeSymbol? limit = null )
        {
            for ( ITypeSymbol? type = topType; type != null && !SymbolEqualityComparer.Default.Equals ( type, limit ); type = type.BaseType )
            {
                yield return type;
            }
        }

        public static String TypeToShortString ( INamedTypeSymbol typeSymbol ) =>
            typeSymbol.TypeArguments.IsEmpty
            ? typeSymbol.Name
            : $"{typeSymbol.Name}<{String.Join ( ", ", typeSymbol.TypeArguments.Select ( t => TypeToShortString ( ( INamedTypeSymbol ) t ) ) )}>";

        private static void GetAllTypes ( List<INamedTypeSymbol> result, INamespaceOrTypeSymbol symbol )
        {
            if ( symbol is INamedTypeSymbol type )
                result.Add ( type );

            foreach ( ISymbol child in symbol.GetMembers ( ) )
            {
                if ( child is INamespaceOrTypeSymbol nsChild )
                    GetAllTypes ( result, nsChild );
            }
        }
    }
}
