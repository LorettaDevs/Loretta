using Microsoft.CodeAnalysis;

namespace Loretta.Generators.SyntaxFactsGenerator
{
    internal static class Diagnostics
    {
        private static readonly string[] s_notConfigurableTags = new[] { WellKnownDiagnosticTags.NotConfigurable };

        public static readonly DiagnosticDescriptor SyntaxKindNotFound = new(
            "LOSK0001",
            "SyntaxKind was not found",
            "SyntaxKind was not found so SyntaxFacts is not being generated",
            "Loretta.Generators.SyntaxKind",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: s_notConfigurableTags);

        public static readonly DiagnosticDescriptor NoSyntaxKindWithAttributesFound = new(
            "LOSK0002",
            "No SyntaxKind with attributes found",
            "No SyntaxKind with attributes were found so no SyntaxFacts methods will be generated",
            "Loretta.Generators.SyntaxKind",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: s_notConfigurableTags);

        public static readonly DiagnosticDescriptor TriviaKindIsAlsoAToken = new(
            "LOSK0003",
            "Trivia kind is also a token",
            "A trivia kind can't also be a token kind",
            "Loretta.Generators.SyntaxKind",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: s_notConfigurableTags);

        public static readonly DiagnosticDescriptor OperatorKindWithoutText = new(
            "LOSK0004",
            "Invalid token text",
            "An operator kind must have a non-empty and non-whitespace text associated with it",
            "Loretta.Generators.SyntaxKind",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: s_notConfigurableTags);

        public static readonly DiagnosticDescriptor KeywordKindWithoutText = new(
            "LOSK0005",
            "Invalid token text",
            "A keyword kind must have a non-empty and non-whitespace text associated with it",
            "Loretta.Generators.SyntaxKind",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: s_notConfigurableTags);

        public static readonly DiagnosticDescriptor CategoryNotInConstantClass = new(
            "LOSK0006",
            "Syntax categories should be in SyntaxKindCategory",
            "Syntax categories should be in the SyntaxKindCategory constants class",
            "Loretta.Generators.SyntaxKind",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PropertyNotInConstantClass = new(
            "LOSK0007",
            "Syntax properties should be in SyntaxKindProperty",
            "Syntax properties should be in the SyntaxKindProperty constants class",
            "Loretta.Generators.SyntaxKind",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
