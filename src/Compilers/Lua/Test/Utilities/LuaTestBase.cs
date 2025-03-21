// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Text;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Test.Utilities;
using Loretta.CodeAnalysis.Text;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.Test.Utilities
{
    public abstract class LuaTestBase : CommonTestBase
    {
        #region SyntaxTree Factories

        public static SyntaxTree Parse(
            string text,
            string filename = "",
            LuaParseOptions options = null,
            Encoding encoding = null)
        {
            options ??= LuaParseOptions.Default;

            var stringText = SourceText.From(text, encoding ?? Encoding.UTF8);
            return CheckSerializable(SyntaxFactory.ParseSyntaxTree(stringText, options, filename));
        }

        public static ExpressionSyntax ParseExpression(
            string text,
            LuaParseOptions options = null,
            Encoding encoding = null)
        {
            options ??= LuaParseOptions.Default;

            var stringText = SourceText.From(text, encoding ?? Encoding.UTF8);
            return CheckSerializable(SyntaxFactory.ParseExpression(stringText, options));
        }

        public static StatementSyntax ParseStatement(
            string          text,
            LuaParseOptions options  = null,
            Encoding        encoding = null)
        {
            options ??= LuaParseOptions.Default;

            var stringText = SourceText.From(text, encoding ?? Encoding.UTF8);
            return CheckSerializable(SyntaxFactory.ParseStatement(stringText, options));
        }

        public static TypeSyntax ParseType(
            string text,
            LuaParseOptions options = null,
            Encoding encoding = null)
        {
            options ??= new LuaParseOptions(LuaSyntaxOptions.Luau);

            var stringText = SourceText.From(text, encoding ?? Encoding.UTF8);
            return CheckSerializable(SyntaxFactory.ParseType(stringText, options));
        }

        private static SyntaxTree CheckSerializable(SyntaxTree tree)
        {
            _ = CheckSerializable(tree.GetRoot());
            return tree;
        }

        private static T CheckSerializable<T>(T node)
            where T : SyntaxNode
        {
            using var stream = new MemoryStream();
            node.SerializeTo(stream);
            stream.Position = 0;
            var deserializedNode = LuaSyntaxNode.DeserializeFrom(stream);
            Assert.Equal(node.ToFullString(), deserializedNode.ToFullString());
            return node;
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

        public static SyntaxTree ParseWithRoundTripCheck(
            string text,
            LuaParseOptions options = null)
        {
            var tree = Parse(text, options: options ?? LuaParseOptions.Default);
            var parsedText = tree.GetRoot();
            // we validate the text roundtrips
            Assert.Equal(text, parsedText.ToFullString());
            return tree;
        }

        public static ExpressionSyntax ParseExpressionWithRoundTripCheck(
            string text,
            LuaParseOptions options = null)
        {
            var node = ParseExpression(text, options: options ?? LuaParseOptions.Default);
            // we validate the text roundtrips
            Assert.Equal(text, node.ToFullString());
            return node;
        }

        public static StatementSyntax ParseStatementWithRoundTripCheck(
            string text,
            LuaParseOptions options = null)
        {
            var node = ParseStatement(text, options: options ?? LuaParseOptions.Default);
            // we validate the text roundtrips
            Assert.Equal(text, node.ToFullString());
            return node;
        }

        public static TypeSyntax ParseTypeWithRoundTripCheck(
            string text,
            LuaParseOptions options = null)
        {
            var node = ParseType(text, options: options ?? new LuaParseOptions(LuaSyntaxOptions.Luau));
            // we validate the text roundtrips
            Assert.Equal(text, node.ToFullString());
            return node;
        }

        #endregion SyntaxTree Factories

        #region Compilation Factories

        protected static List<SyntaxNode> GetSyntaxNodeList(SyntaxTree syntaxTree) => GetSyntaxNodeList(syntaxTree.GetRoot(), null);

        protected static List<SyntaxNode> GetSyntaxNodeList(SyntaxNode node, List<SyntaxNode> synList)
        {
            synList ??= new List<SyntaxNode>();

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

        public static ExpressionSyntax ParseAndValidateExpression(
            string text,
            LuaSyntaxOptions? options = null)
        {
            var parsedNode = ParseExpressionWithRoundTripCheck(
                text,
                new(options ?? LuaSyntaxOptions.All));
            parsedNode.GetDiagnostics().Verify();
            return parsedNode;
        }

        public static TypeSyntax ParseAndValidateType(
            string text,
            LuaSyntaxOptions? options = null)
        {
            var parsedNode = ParseTypeWithRoundTripCheck(
                text,
                new(options ?? LuaSyntaxOptions.Luau));
            parsedNode.GetDiagnostics().Verify();
            return parsedNode;
        }
#nullable disable
    }
}
