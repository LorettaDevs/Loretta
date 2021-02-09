using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.Generators.SyntaxKindGenerators
{
    [Generator]
    public sealed partial class SyntaxKindRelatedTypesGenerator : ISourceGenerator
    {
        public void Initialize ( GeneratorInitializationContext context )
        {
        }

        public void Execute ( GeneratorExecutionContext context )
        {
            var compilation = ( CSharpCompilation ) context.Compilation;

            INamedTypeSymbol? syntaxKindType =
                compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.SyntaxKind" );

            if ( syntaxKindType is null )
                return;

            try
            {
                var fields = syntaxKindType.GetMembers ( )
                                           .OfType<IFieldSymbol> ( )
                                           .ToImmutableArray ( );

                ImmutableArray<KindInfo> kinds = MapToKindInfo ( context, fields );
                if ( kinds.Length < 1 )
                {
                    context.ReportDiagnostic ( Diagnostic.Create ( Diagnostics.NoSyntaxKindWithAttributesFound, syntaxKindType.Locations.Single ( ) ) );
                    return;
                }

                GenerateSyntaxFacts ( context, syntaxKindType, kinds );
            }
            catch ( Exception ex )
            {
                var syntaxKindFilePath = syntaxKindType.DeclaringSyntaxReferences.First ( ).SyntaxTree.FilePath;
                var syntaxDirectory = Path.GetDirectoryName ( syntaxKindFilePath );
                var filePath = Path.Combine ( syntaxDirectory, "exception.log" );
                var contents = String.Join ( Environment.NewLine, new String ( '-', 30 ), ex.ToString ( ) );
                File.AppendAllText ( filePath, contents );

                throw;
            }
        }

        private static ImmutableArray<KindInfo> MapToKindInfo (
            GeneratorExecutionContext context,
            IEnumerable<IFieldSymbol> fields )
        {
            var compilation = ( CSharpCompilation ) context.Compilation;

            INamedTypeSymbol? triviaAttributeType =
                compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.TriviaAttribute" );
            INamedTypeSymbol? tokenAttributeType =
                compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.TokenAttribute" );
            INamedTypeSymbol? keywordAttributeType =
                compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.KeywordAttribute" );
            INamedTypeSymbol? unaryOperatorAttributeType =
                compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.UnaryOperatorAttribute" );
            INamedTypeSymbol? binaryOperatorAttributeType =
                compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.BinaryOperatorAttribute" );
            INamedTypeSymbol? extraCategoriesAttributeType =
                compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.ExtraCategoriesAttribute" );
            INamedTypeSymbol? propertyAttributeType =
                compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.PropertyAttribute" );

            if ( triviaAttributeType is null
                 || tokenAttributeType is null
                 || keywordAttributeType is null
                 || unaryOperatorAttributeType is null
                 || binaryOperatorAttributeType is null
                 || extraCategoriesAttributeType is null
                 || propertyAttributeType is null )
            {
                return ImmutableArray<KindInfo>.Empty;
            }

            var fieldsArray = fields.ToImmutableArray ( );
            ImmutableArray<KindInfo>.Builder infos = ImmutableArray.CreateBuilder<KindInfo> ( fieldsArray.Length );
            foreach ( IFieldSymbol field in fieldsArray )
            {
                var isTrivia = IsTrivia ( triviaAttributeType, field );
                TokenInfo? tokenInfo =
                    GetTokenInfo ( tokenAttributeType, keywordAttributeType, field );
                OperatorInfo? unaryOperatorInfo =
                    GetOperatorInfo ( unaryOperatorAttributeType, field );
                OperatorInfo? binaryOperatorInfo =
                    GetOperatorInfo ( binaryOperatorAttributeType, field );
                ImmutableArray<String> extraCategories =
                    GetExtraCategories ( extraCategoriesAttributeType, field );
                ImmutableDictionary<String, TypedConstant> properties =
                    GetProperties ( propertyAttributeType, field );

                var hasErrors = false;
                Location location = field.Locations.Single ( );
                if ( isTrivia && tokenInfo is not null )
                {
                    hasErrors = true;
                    context.ReportDiagnostic ( Diagnostic.Create ( Diagnostics.TriviaKindIsAlsoAToken, location ) );
                }

                if ( tokenInfo is { IsKeyword: true, Text: null } )
                {
                    hasErrors = true;
                    context.ReportDiagnostic ( Diagnostic.Create ( Diagnostics.KeywordKindWithoutText, location ) );
                }

                if ( ( unaryOperatorInfo is not null || binaryOperatorInfo is not null ) && String.IsNullOrWhiteSpace ( tokenInfo?.Text ) )
                {
                    hasErrors = true;
                    context.ReportDiagnostic ( Diagnostic.Create ( Diagnostics.OperatorKindWithoutText, location ) );
                }

                if ( hasErrors )
                    continue;

                infos.Add ( new KindInfo (
                    field,
                    isTrivia,
                    tokenInfo,
                    unaryOperatorInfo,
                    binaryOperatorInfo,
                    extraCategories,
                    properties ) );
            }

            return infos.ToImmutable ( );
        }

        private static Boolean IsTrivia ( INamedTypeSymbol triviaAttributeType, IFieldSymbol field ) =>
            Utilities.GetAttribute ( field, triviaAttributeType ) is not null;

        private static TokenInfo? GetTokenInfo (
            INamedTypeSymbol tokenAttributeType,
            INamedTypeSymbol keywordAttributeType,
            IFieldSymbol field )
        {
            if ( Utilities.GetAttribute ( field, keywordAttributeType ) is AttributeData keywordAttributeData )
            {
                var text = keywordAttributeData.ConstructorArguments.Single ( ).Value as String;
                if ( String.IsNullOrWhiteSpace ( text ) ) text = null;
                return new TokenInfo ( text, true );
            }
            else if ( Utilities.GetAttribute ( field, tokenAttributeType ) is AttributeData tokenAttributeData )
            {
                var text = tokenAttributeData.NamedArguments.SingleOrDefault ( kv => kv.Key == "Text" ).Value.Value as String;
                if ( String.IsNullOrWhiteSpace ( text ) ) text = null;
                return new TokenInfo ( text, false );
            }
            else
            {
                return null;
            }
        }

        private static OperatorInfo? GetOperatorInfo (
            INamedTypeSymbol operatorAttributeType,
            IFieldSymbol field )
        {
            AttributeData? attr = Utilities.GetAttribute ( field, operatorAttributeType );
            if ( attr is null )
                return null;

            var precedence = ( Int32 ) attr.ConstructorArguments[0].Value!;
            var expression = attr.ConstructorArguments[1];
            return new OperatorInfo ( precedence, expression );
        }

        private static ImmutableArray<String> GetExtraCategories ( INamedTypeSymbol extraCategoriesAttributeType, IFieldSymbol field )
        {
            AttributeData? attr = Utilities.GetAttribute ( field, extraCategoriesAttributeType );
            if ( attr is null )
                return ImmutableArray<String>.Empty;

            var categories = attr.ConstructorArguments.Single ( ).Values.Select ( arg => ( String ) arg.Value! ).ToImmutableArray ( );
            return categories;
        }

        private static ImmutableDictionary<String, TypedConstant> GetProperties ( INamedTypeSymbol propertyAttributeType, IFieldSymbol field )
        {
            ImmutableArray<AttributeData> attributes = Utilities.GetAttributes ( field, propertyAttributeType );
            if ( attributes.IsEmpty )
                return ImmutableDictionary<String, TypedConstant>.Empty;

            IEnumerable<KeyValuePair<String, TypedConstant>> properties = attributes.Select ( attr => new KeyValuePair<String, TypedConstant> ( ( String ) attr.ConstructorArguments[0].Value!, attr.ConstructorArguments[1] ) );
            return ImmutableDictionary.CreateRange ( properties );
        }
    }
}
