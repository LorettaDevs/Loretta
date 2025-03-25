// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// We only build the Source Generator in the netstandard target

#if NETSTANDARD

#nullable enable

using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.Generators.SyntaxXml
{
    [Generator(LanguageNames.CSharp)]
    public sealed class SourceGenerator : IIncrementalGenerator
    {
        private static readonly DiagnosticDescriptor s_missingSyntaxXml = new(
            "LSSG1001",
            "Syntax.xml is missing",
            "The Syntax.xml file was not included in the project, so we are not generating source",
            "SyntaxGenerator",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor s_unableToReadSyntaxXml = new(
            "LSSG1002",
            "Syntax.xml could not be read",
            "The Syntax.xml file could not even be read. Does it exist?",
            "SyntaxGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor s_syntaxXmlError = new(
            "LSSG1003",
            "Syntax.xml has a syntax error",
            "{0}",
            "SyntaxGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor s_syntaxXmlException = new(
            "LSSG1004",
            "Syntax.xml generator threw an exception",
            "{0}",
            "SyntaxGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var inputProvider = context.AdditionalTextsProvider
                                       .Where(static text => Path.GetFileName(text.Path) == "Syntax.xml").Collect()
                                       .Select(static (arr, _) => arr.SingleOrDefault());

            var inputTextProvider = inputProvider.Select(static (input, token) => input?.GetText(token));

            context.RegisterSourceOutput(
                source: inputProvider.Combine(inputTextProvider),
                static (context, inputs) =>
                {
                    var (input, inputText) = inputs;
                    if (input is null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(s_missingSyntaxXml, location: null));
                        return;
                    }
                    if (inputText is null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(s_unableToReadSyntaxXml, location: null));
                        return;
                    }

                    try
                    {
                        Tree tree;
                        var reader = XmlReader.Create(
                            input: new SourceTextReader(inputText),
                            settings: new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });

                        try
                        {
                            var serializer = new XmlSerializer(typeof(Tree));
                            tree = (Tree) serializer.Deserialize(reader);
                        }
                        catch (InvalidOperationException ex) when (ex.InnerException is XmlException xmlException)
                        {
                            var line     = inputText.Lines[xmlException.LineNumber - 1]; // LineNumber is one-based.
                            var offset   = xmlException.LinePosition - 1;                // LinePosition is one-based
                            var position = line.Start + offset;
                            var span     = new TextSpan(position, length: 0);
                            var lineSpan = inputText.Lines.GetLinePositionSpan(span);

                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    s_syntaxXmlError,
                                    location: Location.Create(input.Path, span, lineSpan),
                                    xmlException.Message));
                            return;
                        }

                        TreeFlattening.FlattenChildren(tree);

                        AddResult(
                            writer => SourceWriter.WriteMain(writer, tree, context.CancellationToken),
                            "Syntax.xml.Main.g.cs");
                        AddResult(
                            writer => SourceWriter.WriteInternal(writer, tree, context.CancellationToken),
                            "Syntax.xml.Internal.g.cs");
                        AddResult(
                            writer => SourceWriter.WriteSyntax(writer, tree, context.CancellationToken),
                            "Syntax.xml.Syntax.g.cs");

                        void AddResult(Action<TextWriter> writeFunction, string hintName)
                        {
                            // Write out the contents to a StringBuilder to avoid creating a single large string
                            // in memory
                            var stringBuilder = new StringBuilder();
                            using (var textWriter = new StringWriter(stringBuilder)) writeFunction(textWriter);

                            // And create a SourceText from the StringBuilder, once again avoiding allocating a single massive string
                            var sourceText = SourceText.From(
                                reader: new StringBuilderReader(stringBuilder),
                                stringBuilder.Length,
                                Encoding.UTF8);
                            context.AddSource(hintName, sourceText);
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        try
                        {
                            var path = Path.Combine(path1: Path.GetDirectoryName(input.Path), "SyntaxXmlException.log");
                            File.AppendAllText(
                                path,
                                contents: $"""
                                           {new string(c: '-', count: 40)}
                                           {ex}

                                           """);
                        }
                        catch
                        {
                            // Doesn't matter if it fails, it won't cause any issues.
                        }
                        context.ReportDiagnostic(
                            Diagnostic.Create(s_syntaxXmlException, location: null, ex.ToString()));
                    }
                });
        }
    }
}

#endif
