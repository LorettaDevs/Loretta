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

namespace Loretta.Generators.SyntaxKindGenerators
{
    public sealed partial class SyntaxKindRelatedTypesGenerator
    {
        private static void GenerateSyntaxFacts ( GeneratorExecutionContext context, INamedTypeSymbol syntaxKindType, ImmutableArray<KindInfo> kinds )
        {
            SourceText sourceText;
            using ( var writer = new SourceWriter ( ) )
            {
                writer.WriteLine ( "using System;" );
                writer.WriteLine ( "using System.Collections.Generic;" );
                writer.WriteLine ( "using System.Collections.Immutable;" );
                writer.WriteLine ( "using System.Diagnostics.CodeAnalysis;" );
                writer.WriteLine ( "using Tsu;" );
                writer.WriteLine ( );
                writer.WriteLine ( "#nullable enable" );
                writer.WriteLine ( );

                using ( writer.CurlyIndenter ( "namespace Loretta.CodeAnalysis.Syntax" ) )
                using ( writer.CurlyIndenter ( "public static partial class SyntaxFacts" ) )
                {
                    GenerateGetUnaryOperatorPrecedence ( kinds, writer );

                    writer.WriteLineNoTabs ( "" );

                    GenerateGetUnaryExpression ( kinds, writer );

                    writer.WriteLineNoTabs ( "" );

                    GenerateGetBinaryOperatorPrecedence ( kinds, writer );

                    writer.WriteLineNoTabs ( "" );

                    GenerateGetBinaryExpression ( kinds, writer );

                    writer.WriteLineNoTabs ( "" );

                    GenerateGetKeywordKind ( kinds, writer );

                    writer.WriteLineNoTabs ( "" );

                    GenerateGetUnaryOperatorKinds ( kinds, writer );

                    writer.WriteLineNoTabs ( "" );

                    GenerateGetBinaryOperatorKinds ( kinds, writer );

                    writer.WriteLineNoTabs ( "" );

                    GenerateGetText ( kinds, writer );

                    IEnumerable<IGrouping<String, (KindInfo kind, TypedConstant value)>> properties =
                        kinds.SelectMany ( kind => kind.Properties.Select ( kv => (kind, key: kv.Key, value: kv.Value) ) )
                             .GroupBy ( t => t.key, t => (t.kind, t.value) );
                    foreach ( IGrouping<String, (KindInfo kind, TypedConstant value)> propertyGroup in properties )
                    {
                        ITypeSymbol type = propertyGroup.Select ( t => t.value.Type ).Where ( t => t is not null ).Distinct ( ).Single ( )!;

                        writer.WriteLineNoTabs ( "" );
                        using ( new CurlyIndenter ( writer, $"public static Option<{type.Name}> Get{propertyGroup.Key} ( SyntaxKind kind )" ) )
                        {
                            IEnumerable<IGrouping<TypedConstant, KindInfo>> values = propertyGroup.GroupBy ( t => t.value, t => t.kind );
                            writer.WriteLine ( "return kind switch" );
                            writer.WriteLine ( "{" );
                            using ( new Indenter ( writer ) )
                            {
                                foreach ( IGrouping<TypedConstant, KindInfo> value in values )
                                {
                                    writer.Write ( String.Join ( " or ", value.Select ( k => $"SyntaxKind.{k.Field.Name}" ) ) );
                                    writer.Write ( " => " );
                                    writer.Write ( value.Key.ToCSharpString ( ) );
                                    writer.WriteLine ( "," );
                                }
                                writer.WriteLine ( "_ => default," );
                            }
                            writer.WriteLine ( "};" );
                        }
                    }

                    writer.WriteLineNoTabs ( "" );

                    // Generate IsTrivia
                    GenerateIsX ( kinds, writer, "Trivia", kind => kind.IsTrivia );

                    writer.WriteLineNoTabs ( "" );

                    // Generate IsKeyword
                    GenerateIsX ( kinds, writer, "Keyword", kind => kind.TokenInfo?.IsKeyword is true );

                    writer.WriteLineNoTabs ( "" );

                    // Generate IsToken
                    GenerateIsX ( kinds, writer, "Token", kind => kind.TokenInfo is not null );

                    writer.WriteLineNoTabs ( "" );

                    // Extra Categories
                    IEnumerable<IGrouping<String, KindInfo>> extraCategories = kinds.SelectMany ( kind => kind.ExtraCategories.Select ( cat => (cat, kind) ) ).GroupBy ( t => t.cat, t => t.kind );
                    foreach ( IGrouping<String, KindInfo> group in extraCategories )
                    {
                        writer.WriteLineNoTabs ( "" );

                        var groupKinds = group.ToImmutableArray ( );
                        GenerateIsX ( groupKinds, writer, group.Key, k => true );

                        writer.WriteLineNoTabs ( "" );
                        writer.WriteLine ( "/// <summary>" );
                        writer.WriteLine ( $"/// Returns all <see cref=\"SyntaxKind\"/>s that are in the {group.Key} category." );
                        writer.WriteLine ( "/// </summary>" );
                        writer.WriteLine ( "/// <returns></returns>" );
                        using ( writer.CurlyIndenter ( $"public static IEnumerable<SyntaxKind> Get{group.Key}Kinds ( ) => ImmutableArray.Create ( new[]", " );" ) )
                        {
                            foreach ( KindInfo kind in group )
                            {
                                writer.WriteLine ( $"SyntaxKind.{kind.Field.Name}," );
                            }
                        }
                    }
                }

                sourceText = writer.GetText ( );
            }

            context.AddSource ( "SyntaxFacts.g.cs", sourceText );
            Utilities.DoVsCodeHack ( syntaxKindType, "SyntaxFacts.g.cs", sourceText );
        }

        private static void GenerateGetUnaryOperatorPrecedence ( ImmutableArray<KindInfo> kinds, SourceWriter writer )
        {
            writer.WriteLine ( "/// <summary>" );
            writer.WriteLine ( "/// Returns the precedence for a given unary operator or 0 if not a unary operator." );
            writer.WriteLine ( "/// </summary>" );
            writer.WriteLine ( "/// <param name=\"kind\"></param>" );
            writer.WriteLine ( "/// <returns>" );
            writer.WriteLine ( "/// A positive number indicating the binary operator precedence or 0 if the kind is not a binary operator." );
            writer.WriteLine ( "/// </returns>" );
            using ( writer.CurlyIndenter ( "public static Int32 GetUnaryOperatorPrecedence ( this SyntaxKind kind )" ) )
            using ( writer.CurlyIndenter ( "switch ( kind )" ) )
            {
                IEnumerable<KindInfo> unaryOperators = kinds.Where ( kind => kind.UnaryOperatorInfo is not null );

                IEnumerable<IGrouping<Int32, KindInfo>> groups = unaryOperators.GroupBy ( kind => kind.UnaryOperatorInfo!.Value.Precedence );

                foreach ( IGrouping<Int32, KindInfo> group in groups.OrderByDescending ( g => g.Key ) )
                {
                    foreach ( KindInfo kind in group.OrderByDescending ( info => info.Field.Name ) )
                    {
                        writer.WriteLine ( $"case SyntaxKind.{kind.Field.Name}:" );
                    }
                    using ( writer.Indenter ( ) )
                        writer.WriteLine ( $"return {group.Key};" );
                }

                writer.WriteLine ( "default:" );
                using ( writer.Indenter ( ) )
                    writer.WriteLine ( "return 0;" );
            }
        }

        private static void GenerateGetUnaryExpression ( ImmutableArray<KindInfo> kinds, SourceWriter writer )
        {
            writer.WriteLine ( "/// <summary>" );
            writer.WriteLine ( "/// Returns the expression kind for a given unary operator or None if not a unary operator." );
            writer.WriteLine ( "/// </summary>" );
            writer.WriteLine ( "/// <param name=\"kind\"></param>" );
            writer.WriteLine ( "/// <returns>" );
            writer.WriteLine ( "/// A positive number indicating the binary operator precedence or 0 if the kind is not a binary operator." );
            writer.WriteLine ( "/// </returns>" );
            using ( writer.Indenter ( "public static Option<SyntaxKind> GetUnaryExpression ( SyntaxKind kind ) =>" ) )
            using ( writer.CurlyIndenter ( "kind switch", ";" ) )
            {
                IEnumerable<KindInfo> unaryOperators = kinds.Where ( kind => kind.UnaryOperatorInfo is not null );

                foreach ( KindInfo unaryOperator in unaryOperators )
                {
                    writer.WriteLine ( $"SyntaxKind.{unaryOperator.Field.Name} => {unaryOperator.UnaryOperatorInfo!.Value.Expression.ToCSharpString ( )}," );
                }
                writer.WriteLine ( "_ => default," );
            }
        }

        private static void GenerateGetBinaryOperatorPrecedence ( ImmutableArray<KindInfo> kinds, SourceWriter writer )
        {
            writer.WriteLine ( "/// <summary>" );
            writer.WriteLine ( "/// Returns the precedence for a given binary operator. Returns 0 if kind is not a binary operator." );
            writer.WriteLine ( "/// </summary>" );
            writer.WriteLine ( "/// <param name=\"kind\"></param>" );
            writer.WriteLine ( "/// <returns>" );
            writer.WriteLine ( "/// A positive number indicating the binary operator precedence or 0 if the kind is not a binary operator." );
            writer.WriteLine ( "/// </returns>" );
            using ( writer.CurlyIndenter ( "public static Int32 GetBinaryOperatorPrecedence ( this SyntaxKind kind )" ) )
            using ( writer.CurlyIndenter ( "switch ( kind )" ) )
            {
                IEnumerable<KindInfo> binaryOperators = kinds.Where ( kind => kind.BinaryOperatorInfo is not null );

                IEnumerable<IGrouping<Int32, KindInfo>> groups = binaryOperators.GroupBy ( kind => kind.BinaryOperatorInfo!.Value.Precedence );

                foreach ( IGrouping<Int32, KindInfo> group in groups.OrderByDescending ( g => g.Key ) )
                {
                    foreach ( KindInfo kind in group.OrderByDescending ( info => info.Field.Name ) )
                    {
                        writer.WriteLine ( $"case SyntaxKind.{kind.Field.Name}:" );
                    }
                    using ( new Indenter ( writer ) )
                        writer.WriteLine ( $"return {group.Key};" );

                    writer.WriteLineNoTabs ( "" );
                }

                using ( writer.Indenter ( "default:" ) )
                    writer.WriteLine ( "return 0;" );
            }
        }

        private static void GenerateGetBinaryExpression ( ImmutableArray<KindInfo> kinds, SourceWriter writer )
        {
            writer.WriteLine ( "/// <summary>" );
            writer.WriteLine ( "/// Returns the expression kind for a given unary operator or None if not a unary operator." );
            writer.WriteLine ( "/// </summary>" );
            writer.WriteLine ( "/// <param name=\"kind\"></param>" );
            writer.WriteLine ( "/// <returns>" );
            writer.WriteLine ( "/// A positive number indicating the binary operator precedence or 0 if the kind is not a binary operator." );
            writer.WriteLine ( "/// </returns>" );
            using ( writer.Indenter ( "public static Option<SyntaxKind> GetBinaryExpression ( SyntaxKind kind ) =>" ) )
            using ( writer.CurlyIndenter ( "kind switch", ";" ) )
            {
                IEnumerable<KindInfo> binaryOperators = kinds.Where ( kind => kind.BinaryOperatorInfo is not null );

                foreach ( KindInfo binaryOperator in binaryOperators )
                {
                    writer.WriteLine ( $"SyntaxKind.{binaryOperator.Field.Name} => {binaryOperator.BinaryOperatorInfo!.Value.Expression.ToCSharpString ( )}," );
                }
                writer.WriteLine ( "_ => default," );
            }
        }

        private static void GenerateGetKeywordKind ( ImmutableArray<KindInfo> kinds, SourceWriter writer )
        {
            writer.WriteLine ( "/// <summary>" );
            writer.WriteLine ( "/// Returns the <see cref=\"SyntaxKind\"/> for a given keyword." );
            writer.WriteLine ( "/// </summary>" );
            writer.WriteLine ( "/// <param name=\"text\"></param>" );
            writer.WriteLine ( "/// <returns></returns>" );
            using ( writer.Indenter ( "public static SyntaxKind GetKeywordKind ( String text ) =>" ) )
            using ( writer.CurlyIndenter ( "text switch", ";" ) )
            {
                IEnumerable<KindInfo> keywords = kinds.Where ( kind => kind.TokenInfo?.IsKeyword is true );

                foreach ( KindInfo keyword in keywords.OrderBy ( kind => kind.Field.Name ) )
                {
                    writer.WriteLine ( $"\"{keyword.TokenInfo!.Value.Text}\" => SyntaxKind.{keyword.Field.Name}," );
                }
                writer.WriteLine ( $"_ => SyntaxKind.IdentifierToken," );
            }
        }

        private static void GenerateGetUnaryOperatorKinds ( ImmutableArray<KindInfo> kinds, SourceWriter writer )
        {
            writer.WriteLine ( "/// <summary>" );
            writer.WriteLine ( "/// Returns all <see cref=\"SyntaxKind\"/>s that can be considered unary operators." );
            writer.WriteLine ( "/// </summary>" );
            writer.WriteLine ( "/// <returns></returns>" );
            using ( writer.CurlyIndenter ( "public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds ( ) => ImmutableArray.Create ( new[]", " );" ) )
            {
                IEnumerable<KindInfo> unaryOperators = kinds.Where ( kind => kind.UnaryOperatorInfo is not null );
                foreach ( KindInfo unaryOperator in unaryOperators.OrderBy ( unaryOp => unaryOp.Field.Name ) )
                {
                    writer.WriteLine ( $"SyntaxKind.{unaryOperator.Field.Name}," );
                }
            }
        }

        private static void GenerateGetBinaryOperatorKinds ( ImmutableArray<KindInfo> kinds, SourceWriter writer )
        {
            writer.WriteLine ( "/// <summary>" );
            writer.WriteLine ( "/// Returns all <see cref=\"SyntaxKind\"/>s that can be considered binary operators." );
            writer.WriteLine ( "/// </summary>" );
            writer.WriteLine ( "/// <returns></returns>" );
            using ( writer.CurlyIndenter ( "public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds ( ) => ImmutableArray.Create ( new[]", " );" ) )
            {
                IEnumerable<KindInfo> binaryOperators = kinds.Where ( kind => kind.BinaryOperatorInfo is not null );
                foreach ( KindInfo binaryOperator in binaryOperators.OrderBy ( binaryOp => binaryOp.Field.Name ) )
                {
                    writer.WriteLine ( $"SyntaxKind.{binaryOperator.Field.Name}," );
                }
            }
        }

        private static void GenerateGetText ( ImmutableArray<KindInfo> kinds, SourceWriter writer )
        {
            writer.WriteLine ( "/// <summary>" );
            writer.WriteLine ( "/// Gets the predefined text that corresponds to the provided syntax kind." );
            writer.WriteLine ( "/// </summary>" );
            writer.WriteLine ( "/// <param name=\"kind\">The kind to obtain the text for.</param>" );
            writer.WriteLine ( "/// <returns>The text corresponding to the provided kind or null if none.</returns>" );
            using ( writer.Indenter ( "public static String? GetText ( SyntaxKind kind ) =>" ) )
            using ( writer.CurlyIndenter ( "kind switch", ";" ) )
            {
                IEnumerable<KindInfo> tokens = kinds.Where ( kind => kind.TokenInfo is { IsKeyword: false, Text: not null and not "" } );
                IEnumerable<KindInfo> keywords = kinds.Where ( kind => kind.TokenInfo is { IsKeyword: true, Text: not null and not "" } );

                writer.WriteLine ( "#region Tokens" );
                writer.WriteLineNoTabs ( "" );
                foreach ( KindInfo token in tokens.OrderBy ( tok => tok.Field.Name ) )
                {
                    writer.WriteLine ( $"SyntaxKind.{token.Field.Name} => \"{token.TokenInfo!.Value.Text}\"," );
                }
                writer.WriteLineNoTabs ( "" );
                writer.WriteLine ( "#endregion Tokens" );

                writer.WriteLine ( "#region Keywords" );
                writer.WriteLineNoTabs ( "" );
                foreach ( KindInfo keyword in keywords.OrderBy ( kw => kw.Field.Name ) )
                {
                    writer.WriteLine ( $"SyntaxKind.{keyword.Field.Name} => \"{keyword.TokenInfo!.Value.Text}\"," );
                }
                writer.WriteLineNoTabs ( "" );
                writer.WriteLine ( "#endregion Keywords" );

                writer.WriteLineNoTabs ( "" );
                writer.WriteLine ( "_ => null," );
            }
        }

        private static void GenerateIsX ( ImmutableArray<KindInfo> kinds, SourceWriter writer, String typeName, Func<KindInfo, Boolean> filter )
        {
            writer.WriteLine ( "/// <summary>" );
            writer.WriteLine ( $"/// Checks whether the provided <see cref=\"SyntaxKind\"/> is a {typeName.ToLower ( )}'s." );
            writer.WriteLine ( "/// </summary>" );
            writer.WriteLine ( "/// <param name=\"kind\"></param>" );
            writer.WriteLine ( "/// <returns></returns>" );
            using ( writer.CurlyIndenter ( $"public static Boolean Is{typeName} ( this SyntaxKind kind )" ) )
            using ( writer.CurlyIndenter ( "switch ( kind )" ) )
            {
                IEnumerable<KindInfo> keywords = kinds.Where ( filter );
                foreach ( KindInfo keyword in keywords.OrderBy ( kw => kw.Field.Name ) )
                    writer.WriteLine ( $"case SyntaxKind.{keyword.Field.Name}:" );
                using ( writer.Indenter ( ) )
                    writer.WriteLine ( "return true;" );
                writer.WriteLineNoTabs ( "" );

                using ( writer.Indenter ( "default:" ) )
                    writer.WriteLine ( "return false;" );
            }
        }
    }
}
