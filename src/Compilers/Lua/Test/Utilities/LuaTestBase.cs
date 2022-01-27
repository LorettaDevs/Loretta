// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Test.Utilities;
using Loretta.CodeAnalysis.Text;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.Test.Utilities
{
    public abstract class LuaTestBase : CommonTestBase
    {
        #region SyntaxTree Factories

        public static SyntaxTree Parse(string text, string filename = "", LuaParseOptions options = null, Encoding encoding = null)
        {
            if (options is null)
            {
                options = LuaParseOptions.Default;
            }

            var stringText = SourceText.From(text, encoding ?? Encoding.UTF8);
            return CheckSerializable(SyntaxFactory.ParseSyntaxTree(stringText, options, filename));
        }

        private static SyntaxTree CheckSerializable(SyntaxTree tree)
        {
            var stream = new MemoryStream();
            var root = tree.GetRoot();
            root.SerializeTo(stream);
            stream.Position = 0;
            _ = LuaSyntaxNode.DeserializeFrom(stream);
            return tree;
        }

        public static SyntaxTree[] Parse(IEnumerable<string> sources, LuaParseOptions options = null)
        {
            if (sources == null || !sources.Any())
            {
                return Array.Empty<SyntaxTree>();
            }

            return Parse(options, sources.ToArray());
        }

        public static SyntaxTree[] Parse(LuaParseOptions options = null, params string[] sources)
        {
            if (sources == null || (sources.Length == 1 && null == sources[0]))
            {
                return Array.Empty<SyntaxTree>();
            }

            return sources.Select(src => Parse(src, options: options)).ToArray();
        }

        public static SyntaxTree ParseWithRoundTripCheck(string text, LuaParseOptions options = null)
        {
            var tree = Parse(text, options: options ?? LuaParseOptions.Default);
            var parsedText = tree.GetRoot();
            // we validate the text roundtrips
            Assert.Equal(text, parsedText.ToFullString());
            return tree;
        }

        #endregion SyntaxTree Factories

        #region Compilation Factories

        protected static List<SyntaxNode> GetSyntaxNodeList(SyntaxTree syntaxTree) => GetSyntaxNodeList(syntaxTree.GetRoot(), null);

        protected static List<SyntaxNode> GetSyntaxNodeList(SyntaxNode node, List<SyntaxNode> synList)
        {
            if (synList == null)
                synList = new List<SyntaxNode>();

            synList.Add(node);

            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.IsNode)
                    synList = GetSyntaxNodeList(child.AsNode(), synList);
            }

            return synList;
        }

        protected static SyntaxNode GetSyntaxNodeForBinding(List<SyntaxNode> synList) => GetSyntaxNodeOfTypeForBinding<SyntaxNode>(synList);

        protected const string StartString = "--[[bind]]";
        protected const string EndString = "--[[/bind]]";

        protected static TNode GetSyntaxNodeOfTypeForBinding<TNode>(List<SyntaxNode> synList) where TNode : SyntaxNode
        {
            foreach (var node in synList.OfType<TNode>())
            {
                var exprFullText = node.ToFullString();
                exprFullText = exprFullText.Trim();

                if (exprFullText.StartsWith(StartString, StringComparison.Ordinal))
                {
                    if (exprFullText.Contains(EndString))
                    {
                        if (exprFullText.EndsWith(EndString, StringComparison.Ordinal))
                        {
                            return node;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        return node;
                    }
                }

                if (exprFullText.EndsWith(EndString, StringComparison.Ordinal))
                {
                    if (exprFullText.Contains(StartString))
                    {
                        if (exprFullText.StartsWith(StartString, StringComparison.Ordinal))
                        {
                            return node;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        return node;
                    }
                }
            }

            return null;
        }

        #endregion Compilation Factories

#nullable enable
        public static SyntaxTree ParseAndValidate(
            string text,
            LuaSyntaxOptions? options = null)
        {
            var parsedTree = ParseWithRoundTripCheck(
                text,
                new(options ?? LuaSyntaxOptions.All));
            parsedTree.GetDiagnostics().Verify();
            return parsedTree;
        }
#nullable disable
    }
}
