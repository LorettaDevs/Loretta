// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

// We only include this file in the command line version for now which is the netcoreapp target
#if NETCOREAPP

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Loretta.Generators.SyntaxXml.Grammar;

namespace Loretta.Generators.SyntaxXml
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length is < 2 or > 3)
            {
                return WriteUsage();
            }

            string inputFile = args[0];
            if (!File.Exists(inputFile))
            {
                Console.WriteLine(inputFile + " not found.");
                return 1;
            }

            bool writeSource = true;
            bool writeTests = false;
            bool writeSignatures = false;
            bool writeGrammar = false;
            string outputFile = null;

            if (args.Length == 3)
            {
                outputFile = args[1];

                if (args[2] == "/test")
                {
                    writeTests = true;
                    writeSource = false;
                }
                else if (args[2] == "/grammar")
                {
                    writeGrammar = true;
                }
                else
                {
                    return WriteUsage();
                }
            }
            else if (args.Length == 2)
            {
                if (args[1] == "/sig")
                {
                    writeSignatures = true;
                }
                else
                {
                    outputFile = args[1];
                }
            }

            return writeGrammar
                ? WriteGrammarFile(inputFile, outputFile)
                : WriteLuaSourceFiles(inputFile, writeSource, writeTests, writeSignatures, outputFile);
        }

        private static int WriteUsage()
        {
            Console.WriteLine("Invalid usage:");
            var programName = "  " + typeof(Program).GetTypeInfo().Assembly.ManifestModule.Name;
            Console.WriteLine(programName + " input-file output-file [/test | /grammar]");
            Console.WriteLine(programName + " input-file /sig");
            return 1;
        }

        private static Tree ReadTree(string inputFile)
        {
            var reader = XmlReader.Create(inputFile, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
            var serializer = new XmlSerializer(typeof(Tree));
            return (Tree) serializer.Deserialize(reader);
        }

        private static int WriteGrammarFile(string inputFile, string outputLocation)
        {
            try
            {
                var grammarText = GrammarGenerator.Run(ReadTree(inputFile).Types);
                var outputMainFile = Path.Combine(outputLocation.Trim('"'), $"Lua.Generated.g4");

                using var outFile = new StreamWriter(File.Open(outputMainFile, FileMode.Create), Encoding.UTF8);
                outFile.Write(grammarText);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Generating grammar failed.");
                Console.WriteLine(ex);

                // purposefully fall out here and don't return an error code.  We don't want to fail
                // the build in this case.  Instead, we want to have the program fixed up if
                // necessary.
            }

            return 0;
        }

        private static int WriteLuaSourceFiles(string inputFile, bool writeSource, bool writeTests, bool writeSignatures, string outputFile)
        {
            var tree = ReadTree(inputFile);

            // The syntax.xml doc contains some nodes that are useful for other tools, but which are
            // not needed by this syntax generator.  Specifically, we have `<Choice>` and
            // `<Sequence>` nodes in the xml file to help others tools understand the relationship
            // between some fields (i.e. 'only one of these children can be non-null').  To make our
            // life easier, we just flatten all those nodes, grabbing all the nested `<Field>` nodes
            // and placing into a single linear list that we can then process.
            TreeFlattening.FlattenChildren(tree);

            if (writeSignatures)
            {
                SignatureWriter.Write(Console.Out, tree);
            }
            else
            {
                if (writeSource)
                {
                    var outputPath = outputFile.Trim('"');
                    var prefix = Path.GetFileName(inputFile);
                    var outputMainFile = Path.Combine(outputPath, $"{prefix}.Main.Generated.cs");
                    var outputInternalFile = Path.Combine(outputPath, $"{prefix}.Internal.Generated.cs");
                    var outputSyntaxFile = Path.Combine(outputPath, $"{prefix}.Syntax.Generated.cs");

                    WriteToFile(writer => SourceWriter.WriteMain(writer, tree), outputMainFile);
                    WriteToFile(writer => SourceWriter.WriteInternal(writer, tree), outputInternalFile);
                    WriteToFile(writer => SourceWriter.WriteSyntax(writer, tree), outputSyntaxFile);
                }
                if (writeTests)
                {
                    WriteToFile(writer => TestWriter.Write(writer, tree), outputFile);
                }
            }

            return 0;
        }

        private static void WriteToFile(Action<TextWriter> writeAction, string outputFile)
        {
            var stringBuilder = new StringBuilder();
            var writer = new StringWriter(stringBuilder);
            writeAction(writer);

            var text = stringBuilder.ToString();
            int length;
            do
            {
                length = text.Length;
                text = text.Replace($"{{{Environment.NewLine}{Environment.NewLine}", $"{{{Environment.NewLine}");
            } while (text.Length != length);

            try
            {
                using var outFile = new StreamWriter(File.Open(outputFile, FileMode.Create), Encoding.UTF8);
                outFile.Write(text);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Unable to access {0}.  Is it checked out?", outputFile);
            }
        }
    }
}

#endif
