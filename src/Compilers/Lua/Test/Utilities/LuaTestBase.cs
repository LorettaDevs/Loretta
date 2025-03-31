// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Text;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Test.Utilities;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.Test.Utilities
{
    public abstract class LuaTestBase : CommonTestBase
    {
        #region SyntaxTree Factories

        public static Task<SyntaxTree> ParseAsync(
            string          text,
            string          filename = "",
            LuaParseOptions options  = null,
            Encoding        encoding = null)
        {
            options ??= LuaParseOptions.Default;

            var stringText = SourceText.From(text, encoding ?? Encoding.UTF8);
            return CheckSerializable(SyntaxFactory.ParseSyntaxTree(stringText, options, filename));
        }

        public static Task<ExpressionSyntax> ParseExpressionAsync(
            string          text,
            LuaParseOptions options  = null,
            Encoding        encoding = null)
        {
            options ??= LuaParseOptions.Default;

            var stringText = SourceText.From(text, encoding ?? Encoding.UTF8);
            return CheckSerializableAsync(SyntaxFactory.ParseExpression(stringText, options));
        }

        public static Task<StatementSyntax> ParseStatementAsync(
            string          text,
            LuaParseOptions options  = null,
            Encoding        encoding = null)
        {
            options ??= LuaParseOptions.Default;

            var stringText = SourceText.From(text, encoding ?? Encoding.UTF8);
            return CheckSerializableAsync(SyntaxFactory.ParseStatement(stringText, options));
        }

        public static Task<TypeSyntax> ParseTypeAsync(
            string          text,
            LuaParseOptions options  = null,
            Encoding        encoding = null)
        {
            options ??= new LuaParseOptions(LuaSyntaxOptions.Luau);

            var stringText = SourceText.From(text, encoding ?? Encoding.UTF8);
            return CheckSerializableAsync(SyntaxFactory.ParseType(stringText, options));
        }

        private static async Task<SyntaxTree> CheckSerializable(SyntaxTree tree)
        {
            await CheckSerializableAsync(await tree.GetRootAsync());
            return tree;
        }

        private static async Task<T> CheckSerializableAsync<T>(T node) where T : SyntaxNode
        {
            using var stream = new MemoryStream();
            node.SerializeTo(stream);
            stream.Position = 0;
            var deserializedNode = LuaSyntaxNode.DeserializeFrom(stream);
            await Assert.That(node.ToFullString()).IsEqualTo(deserializedNode.ToFullString());
            return node;
        }

        public static Task<SyntaxTree[]> ParseAsync(IEnumerable<string> sources, LuaParseOptions options = null)
        {
            var sourcesArr = sources?.ToArray();
            if (sources == null || sourcesArr.Length == 0) return Task.FromResult<SyntaxTree[]>([]);
            return ParseAsync(options, sourcesArr);
        }

        public static Task<SyntaxTree[]> ParseAsync(LuaParseOptions options = null, params string[] sources)
        {
            if (sources == null || (sources.Length == 1 && null == sources[0]))
                return Task.FromResult<SyntaxTree[]>([]);
            return Task.WhenAll(sources.Select(src => ParseAsync(src, options: options)));
        }

        public static async Task<SyntaxTree> ParseWithRoundTripCheckAsync(string text, LuaParseOptions options = null)
        {
            var tree       = await ParseAsync(text, options: options ?? LuaParseOptions.Default);
            var parsedText = await tree.GetRootAsync();
            // we validate the text round trips
            await Assert.That(text).IsEqualTo(parsedText.ToFullString());
            return tree;
        }

        public static async Task<ExpressionSyntax> ParseExpressionWithRoundTripCheckAsync(
            string          text,
            LuaParseOptions options = null)
        {
            var node = await ParseExpressionAsync(text, options: options ?? LuaParseOptions.Default);
            // we validate the text round trips
            await Assert.That(text).IsEqualTo(node.ToFullString());
            return node;
        }

        public static async Task<StatementSyntax> ParseStatementWithRoundTripCheckAsync(
            string          text,
            LuaParseOptions options = null)
        {
            var node = await ParseStatementAsync(text, options: options ?? LuaParseOptions.Default);
            // we validate the text round trips
            await Assert.That(text).IsEqualTo(node.ToFullString());
            return node;
        }

        public static async Task<TypeSyntax> ParseTypeWithRoundTripCheckAsync(
            string          text,
            LuaParseOptions options = null)
        {
            var node = await ParseTypeAsync(text, options: options ?? new LuaParseOptions(LuaSyntaxOptions.Luau));
            // we validate the text round trips
            await Assert.That(text).IsEqualTo(node.ToFullString());
            return node;
        }

        #endregion SyntaxTree Factories

        #region Compilation Factories

        protected static List<SyntaxNode> GetSyntaxNodeList(SyntaxTree syntaxTree)
            => GetSyntaxNodeList(syntaxTree.GetRoot(), null);

        protected static List<SyntaxNode> GetSyntaxNodeList(SyntaxNode node, List<SyntaxNode> synList)
        {
            synList ??= [];

            synList.Add(node);

            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.IsNode) synList = GetSyntaxNodeList(child.AsNode(), synList);
            }

            return synList;
        }

        protected static SyntaxNode GetSyntaxNodeForBinding(IEnumerable<SyntaxNode> synList)
            => GetSyntaxNodeOfTypeForBinding<SyntaxNode>(synList);

        protected const string BindingStart = "--[[bind]]";
        protected const string BindingEnd   = "--[[/bind]]";

        protected static TNode GetSyntaxNodeOfTypeForBinding<TNode>(IEnumerable<SyntaxNode> synList)
            where TNode : SyntaxNode
        {
            foreach (var node in synList.OfType<TNode>())
            {
                var exprFullText = node.ToFullString();
                exprFullText = exprFullText.Trim();

                if (exprFullText.StartsWith(BindingStart, StringComparison.Ordinal))
                {
                    if (exprFullText.Contains(BindingEnd))
                    {
                        if (exprFullText.EndsWith(BindingEnd, StringComparison.Ordinal)) return node;
                        continue;
                    }
                    return node;
                }

                if (exprFullText.EndsWith(BindingEnd, StringComparison.Ordinal))
                {
                    if (exprFullText.Contains(BindingStart))
                    {
                        if (exprFullText.StartsWith(BindingStart, StringComparison.Ordinal)) return node;
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
        public static async Task<SyntaxTree> ParseAndValidateAsync(string text, LuaSyntaxOptions? options = null)
        {
            var parsedTree = await ParseWithRoundTripCheckAsync(
                                 text,
                                 new LuaParseOptions(options ?? LuaSyntaxOptions.All));
            parsedTree.GetDiagnostics().Verify();
            return parsedTree;
        }

        public static async Task<ExpressionSyntax> ParseAndValidateExpressionAsync(
            string            text,
            LuaSyntaxOptions? options = null)
        {
            var parsedNode = await ParseExpressionWithRoundTripCheckAsync(
                                 text,
                                 new LuaParseOptions(options ?? LuaSyntaxOptions.All));
            parsedNode.GetDiagnostics().Verify();
            return parsedNode;
        }

        public static async Task<TypeSyntax> ParseAndValidateTypeAsync(string text, LuaSyntaxOptions? options = null)
        {
            var parsedNode = await ParseTypeWithRoundTripCheckAsync(
                                 text,
                                 new LuaParseOptions(options ?? LuaSyntaxOptions.Luau));
            parsedNode.GetDiagnostics().Verify();
            return parsedNode;
        }
    }
}
