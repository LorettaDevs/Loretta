using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
