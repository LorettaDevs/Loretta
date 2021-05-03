using System;
using Microsoft.CodeAnalysis;

namespace Loretta.Generators.SyntaxKindGenerator
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

        public static readonly DiagnosticDescriptor CategoryNotInConstantClass = new DiagnosticDescriptor(
            id: "LOSK0006",
            title: "Syntax categories should be in SyntaxKindCategory",
            messageFormat: "Syntax categories should be in the SyntaxKindCategory constants class",
            category: "Loretta.Generators.SyntaxKind",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PropertyNotInConstantClass = new DiagnosticDescriptor(
            id: "LOSK0007",
            title: "Syntax properties should be in SyntaxKindProperty",
            messageFormat: "Syntax properties should be in the SyntaxKindProperty constants class",
            category: "Loretta.Generators.SyntaxKind",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
