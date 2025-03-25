// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

namespace Loretta.Generators.SyntaxXml
{
    internal class SignatureWriter
    {
        private readonly TextWriter                 _writer;
        private readonly Tree                       _tree;
        private readonly Dictionary<string, string> _typeMap;

        private SignatureWriter(TextWriter writer, Tree tree)
        {
            _writer  = writer;
            _tree    = tree;
            _typeMap = tree.Types.ToDictionary(static n => n.Name, static n => n.Base);
            _typeMap.Add(tree.Root, value: null);
        }

        public static void Write(TextWriter writer, Tree tree) => new SignatureWriter(writer, tree).WriteFile();

        private void WriteFile()
        {
            _writer.WriteLine("using System;");
            _writer.WriteLine("using System.Collections;");
            _writer.WriteLine("using System.Collections.Generic;");
            _writer.WriteLine("using System.Linq;");
            _writer.WriteLine("using System.Threading;");
            _writer.WriteLine();
            _writer.WriteLine("namespace Loretta.CodeAnalysis.Lua");
            _writer.WriteLine("{");

            WriteTypes();

            _writer.WriteLine("}");
        }

        private void WriteTypes()
        {
            var nodes = _tree.Types.Where(static n => n is not PredefinedNode).ToList();
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                var node = nodes[i];
                _writer.WriteLine();
                WriteType(node);
            }
        }

        private void WriteType(TreeType node)
        {
            switch (node)
            {
                case AbstractNode and:
                    _writer.WriteLine("  public abstract partial class {0} : {1}", node.Name, node.Base);
                    _writer.WriteLine("  {");
                    foreach (var field in and.Fields.Where(field => IsNodeOrNodeList(field.Type)))
                        _writer.WriteLine("    public abstract {0}{1} {2} {{ get; }}", "", field.Type, field.Name);
                    _writer.WriteLine("  }");
                    break;

                case Node nd:
                    _writer.WriteLine("  public partial class {0} : {1}", node.Name, node.Base);
                    _writer.WriteLine("  {");

                    WriteKinds(nd.Kinds);

                    foreach (var field in nd.Fields.Where(n => IsNodeOrNodeList(n.Type)).ToList())
                        _writer.WriteLine("    public {0}{1}{2} {3} {{ get; }}", "", "", field.Type, field.Name);

                    foreach (var field in nd.Fields.Where(n => !IsNodeOrNodeList(n.Type)).ToList())
                        _writer.WriteLine("    public {0}{1}{2} {3} {{ get; }}", "", "", field.Type, field.Name);

                    _writer.WriteLine("  }");
                    break;
            }
        }

        private void WriteKinds(List<Kind> kinds)
        {
            if (kinds.Count <= 1) return;
            foreach (var kind in kinds) _writer.WriteLine("    // {0}", kind.Name);
        }

        private static bool IsSeparatedNodeList(string typeName)
            => typeName.StartsWith("SeparatedSyntaxList<", StringComparison.Ordinal);

        private static bool IsNodeList(string typeName) => typeName.StartsWith("SyntaxList<", StringComparison.Ordinal);

        private bool IsNodeOrNodeList(string typeName)
            => IsNode(typeName) || IsNodeList(typeName) || IsSeparatedNodeList(typeName);

        private bool IsNode(string typeName) => _typeMap.ContainsKey(typeName);
    }
}
