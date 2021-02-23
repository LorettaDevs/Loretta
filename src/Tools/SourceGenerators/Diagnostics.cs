using Microsoft.CodeAnalysis;

namespace Loretta.Generators
{
    internal static class Diagnostics
    {
        private static readonly string[] notConfigurableTags = new[] { WellKnownDiagnosticTags.NotConfigurable };

        public static readonly DiagnosticDescriptor SyntaxKindNotFound = new DiagnosticDescriptor(
            id: "LOSK0001",
            title: "SyntaxKind was not found",
            messageFormat: "SyntaxKind was not found so SyntaxFacts is not being generated",
            category: "Loretta.Generators.SyntaxKind",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: notConfigurableTags);

        public static readonly DiagnosticDescriptor NoSyntaxKindWithAttributesFound = new DiagnosticDescriptor(
            id: "LOSK0002",
            title: "No SyntaxKind with attributes found",
            messageFormat: "No SyntaxKind with attributes were found so no SyntaxFacts methods will be generated",
            category: "Loretta.Generators.SyntaxKind",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: notConfigurableTags);

        public static readonly DiagnosticDescriptor TriviaKindIsAlsoAToken = new DiagnosticDescriptor(
            id: "LOSK0003",
            title: "Trivia kind is also a token",
            messageFormat: "A trivia kind can't also be a token kind",
            category: "Loretta.Generators.SyntaxKind",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: notConfigurableTags);

        public static readonly DiagnosticDescriptor OperatorKindWithoutText = new DiagnosticDescriptor(
            id: "LOSK0004",
            title: "Invalid token text",
            messageFormat: "An operator kind must have a non-empty and non-whitespace text associated with it",
            category: "Loretta.Generators.SyntaxKind",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: notConfigurableTags);

        public static readonly DiagnosticDescriptor KeywordKindWithoutText = new DiagnosticDescriptor(
            id: "LOSK0005",
            title: "Invalid token text",
            messageFormat: "A keyword kind must have a non-empty and non-whitespace text associated with it",
            category: "Loretta.Generators.SyntaxKind",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: notConfigurableTags);

        public static readonly DiagnosticDescriptor SyntaxXmlNotFound = new DiagnosticDescriptor(
            id: "LOSX0001",
            title: "Syntax.xml not found",
            messageFormat: "Syntax.xml was not found so no syntax tree nodes, visitors, rewriters or factories will be generated",
            category: "Loretta.Generators.SyntaxXml",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: notConfigurableTags);

        public static readonly DiagnosticDescriptor SyntaxXmlHasNoText = new DiagnosticDescriptor(
            id: "LOSX0002",
            title: "Syntax.xml has no text",
            messageFormat: "Syntax.xml had no text when AdditionalText.GetText() was called so no syntax tree nodes, visitors, rewriters or factories will be generated",
            category: "Loretta.Generators.SyntaxXml",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: notConfigurableTags);

        public static readonly DiagnosticDescriptor SyntaxXmlError = new DiagnosticDescriptor(
            id: "LOSX0003",
            title: "Syntax.xml has a syntax error",
            messageFormat: "{0}",
            category: "Loretta.Generators.SyntaxXml",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: notConfigurableTags);
    }
}
