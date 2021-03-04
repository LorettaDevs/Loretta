using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.Generators.SyntaxKindGenerators
{
    internal static class KindUtils
    {
        private readonly struct CachedKindList
        {
            public CachedKindList(KindList? list, ImmutableArray<byte> checksum)
            {
                List = list;
                Checksum = checksum;
            }

            public KindList? List { get; }
            public ImmutableArray<byte> Checksum { get; }
        }

        private static CachedKindList cachedList = new CachedKindList();

        public static KindList? GetKindInfos(
            GeneratorExecutionContext context,
            CSharpCompilation compilation)
        {
            var syntaxKindType =
                compilation.GetTypeByMetadataName("Loretta.CodeAnalysis.Lua.SyntaxKind");

            if (syntaxKindType is null)
                return null;

            var syntaxKindChecksum =
                syntaxKindType.DeclaringSyntaxReferences.Single()
                                                        .GetSyntax()
                                                        .GetText()
                                                        .GetChecksum();

            if (cachedList.Checksum.IsDefault || !syntaxKindChecksum.SequenceEqual(cachedList.Checksum))
            {
                var list = GetKindInfosCore(context, compilation);
                cachedList = new CachedKindList(list, syntaxKindChecksum);
            }

            return cachedList.List;
        }

        private static KindList? GetKindInfosCore(
            GeneratorExecutionContext context,
            CSharpCompilation compilation)
        {
            var options = (CSharpParseOptions) compilation.SyntaxTrees[0].Options;
            compilation = compilation.AddSyntaxTrees(
                CSharpSyntaxTree.ParseText(SyntaxKindAttributesText, options));

            var syntaxKindType =
                compilation.GetTypeByMetadataName("Loretta.CodeAnalysis.Lua.SyntaxKind");

            if (syntaxKindType is null)
                return null;

            var fields = syntaxKindType.GetMembers()
                                   .OfType<IFieldSymbol>()
                                   .ToImmutableArray();

            return new KindList(MapToKindInfo(context, compilation, fields));
        }

        private static ImmutableArray<KindInfo> MapToKindInfo(
            GeneratorExecutionContext context,
            CSharpCompilation compilation,
            IEnumerable<IFieldSymbol> fields)
        {
            var extraCategoriesAttributeType =
                compilation.GetTypeByMetadataName("Loretta.CodeAnalysis.Lua.ExtraCategoriesAttribute");
            var triviaAttributeType =
                compilation.GetTypeByMetadataName("Loretta.CodeAnalysis.Lua.TriviaAttribute");
            var tokenAttributeType =
                compilation.GetTypeByMetadataName("Loretta.CodeAnalysis.Lua.TokenAttribute");
            var keywordAttributeType =
                compilation.GetTypeByMetadataName("Loretta.CodeAnalysis.Lua.KeywordAttribute");
            var unaryOperatorAttributeType =
                compilation.GetTypeByMetadataName("Loretta.CodeAnalysis.Lua.UnaryOperatorAttribute");
            var binaryOperatorAttributeType =
                compilation.GetTypeByMetadataName("Loretta.CodeAnalysis.Lua.BinaryOperatorAttribute");
            var propertyAttributeType =
                compilation.GetTypeByMetadataName("Loretta.CodeAnalysis.Lua.PropertyAttribute");

            if (triviaAttributeType is null
                 || tokenAttributeType is null
                 || keywordAttributeType is null
                 || unaryOperatorAttributeType is null
                 || binaryOperatorAttributeType is null
                 || extraCategoriesAttributeType is null
                 || propertyAttributeType is null)
            {
                return ImmutableArray<KindInfo>.Empty;
            }

            var fieldsArray = fields.ToImmutableArray();
            var infos = ImmutableArray.CreateBuilder<KindInfo>(fieldsArray.Length);
            foreach (var field in fieldsArray)
            {
                var isTrivia = IsTrivia(triviaAttributeType, field);
                var tokenInfo =
                    GetTokenInfo(tokenAttributeType, keywordAttributeType, field);
                var unaryOperatorInfo =
                    GetOperatorInfo(unaryOperatorAttributeType, field);
                var binaryOperatorInfo =
                    GetOperatorInfo(binaryOperatorAttributeType, field);
                var extraCategories =
                    GetExtraCategories(extraCategoriesAttributeType, field);
                var properties =
                    GetProperties(propertyAttributeType, field);

                var hasErrors = false;
                var location = field.Locations.Single();
                if (isTrivia && tokenInfo is not null)
                {
                    hasErrors = true;
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.TriviaKindIsAlsoAToken, location));
                }

                if (tokenInfo is { IsKeyword: true, Text: null })
                {
                    hasErrors = true;
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.KeywordKindWithoutText, location));
                }

                if ((unaryOperatorInfo is not null || binaryOperatorInfo is not null) && string.IsNullOrWhiteSpace(tokenInfo?.Text))
                {
                    hasErrors = true;
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.OperatorKindWithoutText, location));
                }

                if (hasErrors)
                    continue;

                infos.Add(new KindInfo(
                    field,
                    isTrivia,
                    tokenInfo,
                    unaryOperatorInfo,
                    binaryOperatorInfo,
                    extraCategories,
                    properties));
            }

            return infos.ToImmutable();
        }

        private static bool IsTrivia(INamedTypeSymbol triviaAttributeType, IFieldSymbol field) =>
            Utilities.GetAttribute(field, triviaAttributeType) is not null;

        private static TokenInfo? GetTokenInfo(
            INamedTypeSymbol tokenAttributeType,
            INamedTypeSymbol keywordAttributeType,
            IFieldSymbol field)
        {
            if (Utilities.GetAttribute(field, keywordAttributeType) is AttributeData keywordAttributeData)
            {
                var text = keywordAttributeData.ConstructorArguments.Single().Value as string;
                if (string.IsNullOrWhiteSpace(text)) text = null;
                return new TokenInfo(text, true);
            }
            else if (Utilities.GetAttribute(field, tokenAttributeType) is AttributeData tokenAttributeData)
            {
                var text = tokenAttributeData.NamedArguments.SingleOrDefault(kv => kv.Key == "Text").Value.Value as string;
                if (string.IsNullOrWhiteSpace(text)) text = null;
                return new TokenInfo(text, false);
            }
            else
            {
                return null;
            }
        }

        private static OperatorInfo? GetOperatorInfo(
            INamedTypeSymbol operatorAttributeType,
            IFieldSymbol field)
        {
            var attr = Utilities.GetAttribute(field, operatorAttributeType);
            if (attr is null)
                return null;

            var precedence = (int) attr.ConstructorArguments[0].Value!;
            var expression = attr.ConstructorArguments[1];
            return new OperatorInfo(precedence, expression);
        }

        private static ImmutableArray<string> GetExtraCategories(INamedTypeSymbol extraCategoriesAttributeType, IFieldSymbol field)
        {
            var attr = Utilities.GetAttribute(field, extraCategoriesAttributeType);
            if (attr is null)
                return ImmutableArray<string>.Empty;

            var categories = attr.ConstructorArguments.Single().Values.Select(arg => (string) arg.Value!).ToImmutableArray();
            return categories;
        }

        private static ImmutableDictionary<string, TypedConstant> GetProperties(INamedTypeSymbol propertyAttributeType, IFieldSymbol field)
        {
            var attributes = Utilities.GetAttributes(field, propertyAttributeType);
            if (attributes.IsEmpty)
                return ImmutableDictionary<string, TypedConstant>.Empty;

            var properties = attributes.Select(attr => new KeyValuePair<string, TypedConstant>((string) attr.ConstructorArguments[0].Value!, attr.ConstructorArguments[1]));
            return ImmutableDictionary.CreateRange(properties);
        }

        public static readonly SourceText SyntaxKindAttributesText = SourceText.From(SyntaxKindAttributes, Encoding.UTF8);

        public const string SyntaxKindAttributes = @"
using System;

#nullable enable

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The extra categories attribute
    /// Can be checked by the Is{CategoryName} methods in SyntaxFacts.
    /// All members of a category can also be retrieved by the Get{CategoryName} methods in SyntaxFacts.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class ExtraCategoriesAttribute : Attribute
    {
        public ExtraCategoriesAttribute(params string[] extraCategories)
        {
            Categories = extraCategories;
        }

        public string[] Categories { get; }
    }

    /// <summary>
    /// Properties associated with the enum value.
    /// Can be retrieved from the Get{Key} methods in SyntaxFacts.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    internal sealed class PropertyAttribute : Attribute
    {
        public PropertyAttribute(string key, object? value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public object Value { get; }
    }

    /// <summary>
    /// The trivia indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that this <see cref=""SyntaxKind""/> is a trivia's.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class TriviaAttribute : Attribute
    {
    }

    /// <summary>
    /// The token indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that this <see cref=""SyntaxKind""/> is a token's.
    /// May optionally indicate a fixed text for the token.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class TokenAttribute : Attribute
    {
        /// <summary>
        /// The <see cref=""SyntaxToken""/>'s fixed text.
        /// </summary>
        public string? Text { get; set; }
    }

    /// <summary>
    /// The keyword indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that this <see cref=""SyntaxKind""/> is a keywords's
    /// and the keyword fixed text.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class KeywordAttribute : Attribute
    {
        public KeywordAttribute(string text)
        {
            Text = text;
        }

        /// <summary>
        /// The keyword's text.
        /// </summary>
        public String Text { get; }
    }

    /// <summary>
    /// The unary operator indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that this
    /// <see cref=""SyntaxKind""/> is an unary operator's with the
    /// provided precedence.
    /// THIS DOES NOT IMPLY THE <see cref=""TokenAttribute""/>
    /// ATTRIBUTE.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class UnaryOperatorAttribute : Attribute
    {
        public UnaryOperatorAttribute(int precedence, SyntaxKind expressionKind)
        {
            Precedence = precedence;
        }

        /// <summary>
        /// The unary operator's precedence.
        /// </summary>
        public int Precedence { get; }
    }

    /// <summary>
    /// The binary operator indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that
    /// this <see cref=""SyntaxKind""/> is a binary operator's
    /// with the provided precedence.
    /// THIS DOES NOT IMPLY THE <see cref=""TokenAttribute""/>
    /// ATTRIBUTE.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class BinaryOperatorAttribute : Attribute
    {
        public BinaryOperatorAttribute(int precedence, SyntaxKind expressionKind)
        {
            Precedence = precedence;
        }

        /// <summary>
        /// The binary operator's precedence.
        /// </summary>
        public int Precedence { get; }
    }
}";
    }
}
