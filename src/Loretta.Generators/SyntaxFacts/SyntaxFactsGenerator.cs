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

namespace Loretta.Generators.SyntaxFacts
{
    [Generator]
    public class SyntaxFactsGenerator : ISourceGenerator
    {
        private static readonly DiagnosticDescriptor TriviaAndToken = new (
            id: "LO0001",
            title: "A trivia kind can't also be a token",
            messageFormat: "A trivia kind can't also be a token",
            category: "Loretta.Generators",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: new[] { WellKnownDiagnosticTags.NotConfigurable } );

        private static readonly DiagnosticDescriptor NoKinds = new (
            id: "LO0002",
            title: "No SyntaxKind with attributes found",
            messageFormat: "No SyntaxKind with attributes found",
            category: "Loretta.Generators",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: new[] { WellKnownDiagnosticTags.NotConfigurable } );

        private static readonly DiagnosticDescriptor OperatorWithoutText = new (
            id: "LO0003",
            title: "An operator kind must have a non-empty and non-whitespace text associated with it",
            messageFormat: "An operator kind must have a non-empty and non-whitespace text associated with it",
            category: "Loretta.Generators",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: new[] { WellKnownDiagnosticTags.NotConfigurable } );

        private static readonly DiagnosticDescriptor KeywordWithoutText = new (
            id: "LO0003",
            title: "A keyword kind must have a non-empty and non-whitespace text associated with it",
            messageFormat: "A keyword kind must have a non-empty and non-whitespace text associated with it",
            category: "Loretta.Generators",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: new[] { WellKnownDiagnosticTags.NotConfigurable } );

        public void Initialize ( GeneratorInitializationContext context )
        {
        }

        public void Execute ( GeneratorExecutionContext context )
        {
            var compilation = ( CSharpCompilation ) context.Compilation;

            INamedTypeSymbol? immutableArrayType =
                compilation.GetTypeByMetadataName ( "System.Collections.Immutable.ImmutableArray`1" );
            INamedTypeSymbol? syntaxKindType =
                compilation.GetTypeByMetadataName ( "Loretta.CodeAnalysis.Syntax.SyntaxKind" );

            if ( immutableArrayType is null || syntaxKindType is null )
                return;

            var fields = syntaxKindType.GetMembers ( )
                                       .OfType<IFieldSymbol> ( )
                                       .ToImmutableArray ( );

            ImmutableArray<KindInfo> kinds = MapToKindInfo ( context, fields );
            if ( kinds.Length < 1 )
            {
                context.ReportDiagnostic ( Diagnostic.Create ( NoKinds, syntaxKindType.Locations.Single ( ) ) );
                return;
            }

            SourceText sourceText;
            using ( var stringWriter = new StringWriter ( ) )
            using ( var indentedTextWriter = new IndentedTextWriter ( stringWriter, "    " ) )
            {
                indentedTextWriter.WriteLine ( "using System;" );
                indentedTextWriter.WriteLine ( "using System.Collections.Generic;" );
                indentedTextWriter.WriteLine ( "using System.Collections.Immutable;" );
                indentedTextWriter.WriteLine ( "using System.Diagnostics.CodeAnalysis;" );
                indentedTextWriter.WriteLine ( );
                indentedTextWriter.WriteLine ( "#nullable enable" );
                indentedTextWriter.WriteLine ( );

                using ( new CurlyIndenter ( indentedTextWriter, "namespace Loretta.CodeAnalysis.Syntax" ) )
                {
                    using ( new CurlyIndenter ( indentedTextWriter, "public static partial class SyntaxFacts" ) )
                    {
                        GenerateGetUnaryOperatorPrecedence ( kinds, indentedTextWriter );

                        indentedTextWriter.WriteLineNoTabs ( "" );

                        GenerateGetBinaryOperatorPrecedence ( kinds, indentedTextWriter );

                        indentedTextWriter.WriteLineNoTabs ( "" );

                        GenerateGetKeywordKind ( kinds, indentedTextWriter );

                        indentedTextWriter.WriteLineNoTabs ( "" );

                        GenerateGetUnaryOperatorKinds ( kinds, indentedTextWriter );

                        indentedTextWriter.WriteLineNoTabs ( "" );

                        GenerateGetBinaryOperatorKinds ( kinds, indentedTextWriter );

                        indentedTextWriter.WriteLineNoTabs ( "" );

                        GenerateGetText ( kinds, indentedTextWriter );

                        indentedTextWriter.WriteLineNoTabs ( "" );

                        // Generate IsTrivia
                        GenerateIsX ( kinds, indentedTextWriter, "Trivia", kind => kind.IsTrivia );

                        indentedTextWriter.WriteLineNoTabs ( "" );

                        // Generate IsKeyword
                        GenerateIsX ( kinds, indentedTextWriter, "Keyword", kind => kind.TokenInfo?.IsKeyword is true );

                        indentedTextWriter.WriteLineNoTabs ( "" );

                        // Generate IsToken
                        GenerateIsX ( kinds, indentedTextWriter, "Token", kind => kind.TokenInfo is not null );

                    }
                }

                indentedTextWriter.Flush ( );
                stringWriter.Flush ( );
                sourceText = SourceText.From ( stringWriter.ToString ( ), Encoding.UTF8 );
            }

            context.AddSource ( "SyntaxFacts.g.cs", sourceText );

            // HACK
            //
            // Make generator work in VS Code. See src\Directory.Build.props for
            // details.

            var fileName = "SyntaxFacts.g.cs";
            var syntaxNodeFilePath = syntaxKindType.DeclaringSyntaxReferences.First ( ).SyntaxTree.FilePath;
            var syntaxDirectory = Path.GetDirectoryName ( syntaxNodeFilePath );
            var filePath = Path.Combine ( syntaxDirectory, fileName );

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

        private static void GenerateGetUnaryOperatorPrecedence ( ImmutableArray<KindInfo> kinds, IndentedTextWriter indentedTextWriter )
        {
            indentedTextWriter.WriteLine ( "/// <summary>" );
            indentedTextWriter.WriteLine ( "/// Returns the precedence for a given unary operator or 0 if not a unary operator." );
            indentedTextWriter.WriteLine ( "/// </summary>" );
            indentedTextWriter.WriteLine ( "/// <param name=\"kind\"></param>" );
            indentedTextWriter.WriteLine ( "/// <returns>" );
            indentedTextWriter.WriteLine ( "/// A positive number indicating the binary operator precedence or 0 if the kind is not a binary operator." );
            indentedTextWriter.WriteLine ( "/// </returns>" );
            using ( new CurlyIndenter ( indentedTextWriter, "public static Int32 GetUnaryOperatorPrecedence ( this SyntaxKind kind )" ) )
            {
                using ( new CurlyIndenter ( indentedTextWriter, "switch ( kind )" ) )
                {
                    IEnumerable<KindInfo> unaryOperators = kinds.Where ( kind => kind.UnaryOperatorInfo is not null );

                    IEnumerable<IGrouping<Int32, KindInfo>> groups = unaryOperators.GroupBy ( kind => kind.UnaryOperatorInfo!.Value.Precedence );

                    foreach ( IGrouping<Int32, KindInfo> group in groups.OrderByDescending ( g => g.Key ) )
                    {
                        foreach ( KindInfo kind in group.OrderByDescending ( info => info.Field.Name ) )
                        {
                            indentedTextWriter.WriteLine ( $"case SyntaxKind.{kind.Field.Name}:" );
                        }
                        using ( new Indenter ( indentedTextWriter ) )
                            indentedTextWriter.WriteLine ( $"return {group.Key};" );

                        indentedTextWriter.WriteLineNoTabs ( "" );
                    }

                    indentedTextWriter.WriteLine ( "default:" );
                    using ( new Indenter ( indentedTextWriter ) )
                        indentedTextWriter.WriteLine ( "return 0;" );
                }
            }
        }

        private static void GenerateGetBinaryOperatorPrecedence ( ImmutableArray<KindInfo> kinds, IndentedTextWriter indentedTextWriter )
        {
            indentedTextWriter.WriteLine ( "/// <summary>" );
            indentedTextWriter.WriteLine ( "/// Returns the precedence for a given binary operator. Returns 0 if kind is not a binary operator." );
            indentedTextWriter.WriteLine ( "/// </summary>" );
            indentedTextWriter.WriteLine ( "/// <param name=\"kind\"></param>" );
            indentedTextWriter.WriteLine ( "/// <returns>" );
            indentedTextWriter.WriteLine ( "/// A positive number indicating the binary operator precedence or 0 if the kind is not a binary operator." );
            indentedTextWriter.WriteLine ( "/// </returns>" );
            using ( new CurlyIndenter ( indentedTextWriter, "public static Int32 GetBinaryOperatorPrecedence ( this SyntaxKind kind )" ) )
            {
                using ( new CurlyIndenter ( indentedTextWriter, "switch ( kind )" ) )
                {
                    IEnumerable<KindInfo> binaryOperators = kinds.Where ( kind => kind.BinaryOperatorInfo is not null );

                    IEnumerable<IGrouping<Int32, KindInfo>> groups = binaryOperators.GroupBy ( kind => kind.BinaryOperatorInfo!.Value.Precedence );

                    foreach ( IGrouping<Int32, KindInfo> group in groups.OrderByDescending ( g => g.Key ) )
                    {
                        foreach ( KindInfo kind in group.OrderByDescending ( info => info.Field.Name ) )
                        {
                            indentedTextWriter.WriteLine ( $"case SyntaxKind.{kind.Field.Name}:" );
                        }
                        using ( new Indenter ( indentedTextWriter ) )
                            indentedTextWriter.WriteLine ( $"return {group.Key};" );

                        indentedTextWriter.WriteLineNoTabs ( "" );
                    }

                    indentedTextWriter.WriteLine ( "default:" );
                    using ( new Indenter ( indentedTextWriter ) )
                        indentedTextWriter.WriteLine ( "return 0;" );
                }
            }
        }

        private static void GenerateGetKeywordKind ( ImmutableArray<KindInfo> kinds, IndentedTextWriter indentedTextWriter )
        {
            indentedTextWriter.WriteLine ( "/// <summary>" );
            indentedTextWriter.WriteLine ( "/// Returns the <see cref=\"SyntaxKind\"/> for a given keyword." );
            indentedTextWriter.WriteLine ( "/// </summary>" );
            indentedTextWriter.WriteLine ( "/// <param name=\"text\"></param>" );
            indentedTextWriter.WriteLine ( "/// <returns></returns>" );
            using ( new CurlyIndenter ( indentedTextWriter, "public static SyntaxKind GetKeywordKind ( String text )" ) )
            {
                indentedTextWriter.WriteLine ( "return text switch" );
                indentedTextWriter.WriteLine ( "{" );
                using ( new Indenter ( indentedTextWriter ) )
                {
                    IEnumerable<KindInfo> keywords = kinds.Where ( kind => kind.TokenInfo?.IsKeyword is true );

                    foreach ( KindInfo keyword in keywords.OrderBy ( kind => kind.Field.Name ) )
                    {
                        indentedTextWriter.WriteLine ( $"\"{keyword.TokenInfo!.Value.Text}\" => SyntaxKind.{keyword.Field.Name}," );
                    }
                    indentedTextWriter.WriteLine ( $"_ => SyntaxKind.IdentifierToken," );
                }
                indentedTextWriter.WriteLine ( "};" );
            }
        }

        private static void GenerateGetUnaryOperatorKinds ( ImmutableArray<KindInfo> kinds, IndentedTextWriter indentedTextWriter )
        {
            indentedTextWriter.WriteLine ( "/// <summary>" );
            indentedTextWriter.WriteLine ( "/// Returns all <see cref=\"SyntaxKind\"/>s that can be considered unary operators." );
            indentedTextWriter.WriteLine ( "/// </summary>" );
            indentedTextWriter.WriteLine ( "/// <returns></returns>" );
            indentedTextWriter.WriteLine ( "public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds ( ) => ImmutableArray.Create ( new[]" );
            indentedTextWriter.WriteLine ( "{" );
            using ( new Indenter ( indentedTextWriter ) )
            {
                IEnumerable<KindInfo> unaryOperators = kinds.Where ( kind => kind.UnaryOperatorInfo is not null );
                foreach ( KindInfo unaryOperator in unaryOperators.OrderBy ( unaryOp => unaryOp.Field.Name ) )
                {
                    indentedTextWriter.WriteLine ( $"SyntaxKind.{unaryOperator.Field.Name}," );
                }
            }
            indentedTextWriter.WriteLine ( "} );" );
        }

        private static void GenerateGetBinaryOperatorKinds ( ImmutableArray<KindInfo> kinds, IndentedTextWriter indentedTextWriter )
        {
            indentedTextWriter.WriteLine ( "/// <summary>" );
            indentedTextWriter.WriteLine ( "/// Returns all <see cref=\"SyntaxKind\"/>s that can be considered binary operators." );
            indentedTextWriter.WriteLine ( "/// </summary>" );
            indentedTextWriter.WriteLine ( "/// <returns></returns>" );
            indentedTextWriter.WriteLine ( "public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds ( ) => ImmutableArray.Create ( new[]" );
            indentedTextWriter.WriteLine ( "{" );
            using ( new Indenter ( indentedTextWriter ) )
            {
                IEnumerable<KindInfo> binaryOperators = kinds.Where ( kind => kind.BinaryOperatorInfo is not null );
                foreach ( KindInfo binaryOperator in binaryOperators.OrderBy ( binaryOp => binaryOp.Field.Name ) )
                {
                    indentedTextWriter.WriteLine ( $"SyntaxKind.{binaryOperator.Field.Name}," );
                }
            }
            indentedTextWriter.WriteLine ( "} );" );
        }

        private static void GenerateGetText ( ImmutableArray<KindInfo> kinds, IndentedTextWriter indentedTextWriter )
        {
            indentedTextWriter.WriteLine ( "/// <summary>" );
            indentedTextWriter.WriteLine ( "/// Gets the predefined text that corresponds to the provided syntax kind." );
            indentedTextWriter.WriteLine ( "/// </summary>" );
            indentedTextWriter.WriteLine ( "/// <param name=\"kind\">The kind to obtain the text for.</param>" );
            indentedTextWriter.WriteLine ( "/// <returns>The text corresponding to the provided kind or null if none.</returns>" );
            using ( new CurlyIndenter ( indentedTextWriter, "public static String? GetText ( SyntaxKind kind )" ) )
            {
                indentedTextWriter.WriteLine ( "return kind switch" );
                indentedTextWriter.WriteLine ( "{" );
                using ( new Indenter ( indentedTextWriter ) )
                {
                    IEnumerable<KindInfo> tokens = kinds.Where ( kind => kind.TokenInfo is { IsKeyword: false, Text: not null and not "" } );
                    IEnumerable<KindInfo> keywords = kinds.Where ( kind => kind.TokenInfo is { IsKeyword: true, Text: not null and not "" } );

                    indentedTextWriter.WriteLine ( "#region Tokens" );
                    indentedTextWriter.WriteLineNoTabs ( "" );
                    foreach ( KindInfo token in tokens.OrderBy ( tok => tok.Field.Name ) )
                    {
                        indentedTextWriter.WriteLine ( $"SyntaxKind.{token.Field.Name} => \"{token.TokenInfo!.Value.Text}\"," );
                    }
                    indentedTextWriter.WriteLineNoTabs ( "" );
                    indentedTextWriter.WriteLine ( "#endregion Tokens" );

                    indentedTextWriter.WriteLine ( "#region Keywords" );
                    indentedTextWriter.WriteLineNoTabs ( "" );
                    foreach ( KindInfo keyword in keywords.OrderBy ( kw => kw.Field.Name ) )
                    {
                        indentedTextWriter.WriteLine ( $"SyntaxKind.{keyword.Field.Name} => \"{keyword.TokenInfo!.Value.Text}\"," );
                    }
                    indentedTextWriter.WriteLineNoTabs ( "" );
                    indentedTextWriter.WriteLine ( "#endregion Keywords" );

                    indentedTextWriter.WriteLineNoTabs ( "" );
                    indentedTextWriter.WriteLine ( "_ => null," );
                }
                indentedTextWriter.WriteLine ( "};" );
            }
        }

        private static void GenerateIsX ( ImmutableArray<KindInfo> kinds, IndentedTextWriter indentedTextWriter, String typeName, Func<KindInfo, Boolean> filter )
        {
            indentedTextWriter.WriteLine ( "/// <summary>" );
            indentedTextWriter.WriteLine ( $"/// Checks whether the provided <see cref=\"SyntaxKind\"/> is a {typeName.ToLower ( )}'s." );
            indentedTextWriter.WriteLine ( "/// </summary>" );
            indentedTextWriter.WriteLine ( "/// <param name=\"kind\"></param>" );
            indentedTextWriter.WriteLine ( "/// <returns></returns>" );
            using ( new CurlyIndenter ( indentedTextWriter, $"public static Boolean Is{typeName} ( this SyntaxKind kind )" ) )
            {
                using ( new CurlyIndenter ( indentedTextWriter, "switch ( kind )" ) )
                {
                    IEnumerable<KindInfo> keywords = kinds.Where ( filter );
                    foreach ( KindInfo keyword in keywords.OrderBy ( kw => kw.Field.Name ) )
                        indentedTextWriter.WriteLine ( $"case SyntaxKind.{keyword.Field.Name}:" );
                    using ( new Indenter ( indentedTextWriter ) )
                        indentedTextWriter.WriteLine ( "return true;" );
                    indentedTextWriter.WriteLineNoTabs ( "" );

                    indentedTextWriter.WriteLine ( "default:" );
                    using ( new Indenter ( indentedTextWriter ) )
                        indentedTextWriter.WriteLine ( "return false;" );
                }
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

            if ( triviaAttributeType is null || tokenAttributeType is null || keywordAttributeType is null || unaryOperatorAttributeType is null || binaryOperatorAttributeType is null )
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

                var hasErrors = false;
                if ( isTrivia && tokenInfo is not null )
                {
                    hasErrors = true;
                    context.ReportDiagnostic ( Diagnostic.Create ( TriviaAndToken, field.Locations.Single ( ) ) );
                }

                if ( tokenInfo is { IsKeyword: true, Text: null } )
                {
                    hasErrors = true;
                    context.ReportDiagnostic ( Diagnostic.Create ( KeywordWithoutText, field.Locations.Single ( ) ) );
                }

                if ( ( unaryOperatorInfo is not null || binaryOperatorInfo is not null ) && String.IsNullOrWhiteSpace ( tokenInfo?.Text ) )
                {
                    hasErrors = true;
                    context.ReportDiagnostic ( Diagnostic.Create ( OperatorWithoutText, field.Locations.Single ( ) ) );
                }

                if ( hasErrors )
                    continue;

                infos.Add ( new KindInfo (
                    field,
                    isTrivia,
                    tokenInfo,
                    unaryOperatorInfo,
                    binaryOperatorInfo ) );
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

            var precedence = ( Int32 ) attr.ConstructorArguments.Single ( ).Value!;
            return new OperatorInfo ( precedence );
        }
    }
}
