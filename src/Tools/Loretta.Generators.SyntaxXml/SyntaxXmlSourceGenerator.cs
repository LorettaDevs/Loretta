// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.Generators.SyntaxXml
{
    [Generator(LanguageNames.CSharp)]
    public sealed class SyntaxXmlSourceGenerator : IIncrementalGenerator
    {
        private static readonly DiagnosticDescriptor s_missingSyntaxXml = new(
            "LSSG1001",
            title: "Syntax.xml is missing",
            messageFormat: "The Syntax.xml file was not included in the project, so we are not generating source",
            category: "SyntaxGenerator",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
        private static readonly DiagnosticDescriptor s_unableToReadSyntaxXml = new(
            "LSSG1002",
            title: "Syntax.xml could not be read",
            messageFormat: "The Syntax.xml file could not even be read. Does it exist?",
            category: "SyntaxGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor s_syntaxXmlError = new(
            "LSSG1003",
            title: "Syntax.xml has a syntax error",
            messageFormat: "{0}",
            category: "SyntaxGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor s_syntaxXmlException = new(
            "LSSG1004",
            title: "Syntax.xml generator threw an exception",
            messageFormat: "{0}",
            category: "SyntaxGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var inputProvider = context.AdditionalTextsProvider.Where(text => Path.GetFileName(text.Path) == "Syntax.xml")
                .Collect()
                .Select((arr, _) => arr.SingleOrDefault());

            var inputTextProvider = inputProvider.Select((input, token) => input?.GetText(token));

            context.RegisterSourceOutput(inputProvider.Combine(inputTextProvider), (context, inputs) =>
            {
                var (input, inputText) = inputs;
                if (input is null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(s_missingSyntaxXml, location: null));
                    return;
                }
                else if (inputText is null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(s_unableToReadSyntaxXml, location: null));
                    return;
                }

                try
                {
                    Tree tree;
                    var reader = XmlReader.Create(new SourceTextReader(inputText), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });

                    try
                    {
                        var serializer = new XmlSerializer(typeof(Tree));
                        tree = (Tree) serializer.Deserialize(reader);
                    }
                    catch (InvalidOperationException ex) when (ex.InnerException is XmlException xmlException)
                    {
                        var line = inputText.Lines[xmlException.LineNumber - 1]; // LineNumber is one-based.
                        var offset = xmlException.LinePosition - 1; // LinePosition is one-based
                        var position = line.Start + offset;
                        var span = new TextSpan(position, 0);
                        var lineSpan = inputText.Lines.GetLinePositionSpan(span);

                        context.ReportDiagnostic(Diagnostic.Create(
                                s_syntaxXmlError,
                                location: Location.Create(input.Path, span, lineSpan),
                                xmlException.Message));
                        return;
                    }

                    TreeFlattening.FlattenChildren(tree);

                    addResult(writer => SourceWriter.WriteMain(writer, tree, context.CancellationToken), "Syntax.xml.Main.g.cs");
                    addResult(writer => SourceWriter.WriteInternal(writer, tree, context.CancellationToken), "Syntax.xml.Internal.g.cs");
                    addResult(writer => SourceWriter.WriteSyntax(writer, tree, context.CancellationToken), "Syntax.xml.Syntax.g.cs");

                    void addResult(Action<TextWriter> writeFunction, string hintName)
                    {
                        // Write out the contents to a StringBuilder to avoid creating a single large string
                        // in memory
                        var stringBuilder = new StringBuilder();
                        using (var textWriter = new StringWriter(stringBuilder))
                        {
                            writeFunction(textWriter);
                        }

                        // And create a SourceText from the StringBuilder, once again avoiding allocating a single massive string
                        var sourceText = SourceText.From(new StringBuilderReader(stringBuilder), stringBuilder.Length, encoding: Encoding.UTF8);
                        context.AddSource(hintName, sourceText);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    var path = Path.Combine(Path.GetDirectoryName(input.Path), "SyntaxXmlException.log");
                    try { File.AppendAllText(path, "\r\n" + new string('-', 40) + "\r\n" + ex.ToString()); } catch { }
                    context.ReportDiagnostic(Diagnostic.Create(
                        s_syntaxXmlException,
                        null,
                        ex.ToString()));
                }
            });
        }
    }
}
