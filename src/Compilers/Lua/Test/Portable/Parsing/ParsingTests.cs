// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Lua.Test.Utilities;
using Loretta.CodeAnalysis.Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Parsing
{
    public abstract class ParsingTests : LuaTestBase, IDisposable
    {
        private LuaSyntaxNode? _node;
        private IEnumerator<SyntaxNodeOrToken>? _treeEnumerator;
        private readonly ITestOutputHelper _output;

        public ParsingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public virtual void Dispose()
        {
            VerifyEnumeratorConsumed();
            GC.SuppressFinalize(this);
        }

        private void VerifyEnumeratorConsumed()
        {
            if (_treeEnumerator != null)
            {
                var hasNext = _treeEnumerator.MoveNext();
                if (hasNext)
                {
                    DumpAndCleanup();
                    Assert.False(hasNext, "Test contains unconsumed syntax left over from UsingNode()");
                }
            }
        }

        private bool DumpAndCleanup()
        {
            _treeEnumerator = null; // Prevent redundant errors across different test helpers
            foreach (var _ in EnumerateNodes(_node!, dump: true)) { }
            return false;
        }

        public static void ParseAndValidate(string text, LuaSyntaxOptions? options = null, params DiagnosticDescription[] expectedErrors)
        {
            var parsedTree = ParseWithRoundTripCheck(text, new(options ?? LuaSyntaxOptions.All));
            var actualErrors = parsedTree.GetDiagnostics();
            actualErrors.Verify(expectedErrors);
        }

        public static void ParseAndValidate(string text, LuaParseOptions options, params DiagnosticDescription[] expectedErrors)
        {
            var parsedTree = ParseWithRoundTripCheck(text, options: options);
            var actualErrors = parsedTree.GetDiagnostics();
            actualErrors.Verify(expectedErrors);
        }

        public static void ParseAndValidateFirst(string text, DiagnosticDescription expectedFirstError)
        {
            var parsedTree = ParseWithRoundTripCheck(text);
            var actualErrors = parsedTree.GetDiagnostics();
            actualErrors.Take(1).Verify(expectedFirstError);
        }

        protected virtual SyntaxTree ParseTree(string text, LuaParseOptions? options) => SyntaxFactory.ParseSyntaxTree(text, options);

        public static CompilationUnitSyntax ParseFile(string text, LuaParseOptions? parseOptions = null) =>
            SyntaxFactory.ParseCompilationUnit(text, options: parseOptions);

        protected virtual LuaSyntaxNode ParseNode(string text, LuaParseOptions? options) =>
            ParseTree(text, options).GetCompilationUnitRoot();

        internal void UsingStatement(string text, params DiagnosticDescription[] expectedErrors) => UsingStatement(text, options: null, expectedErrors);

        internal void UsingStatement(string text, LuaParseOptions? options, params DiagnosticDescription[] expectedErrors)
        {
            var node = SyntaxFactory.ParseStatement(text, options: options);
            // we validate the text roundtrips
            Assert.Equal(text, node.ToFullString());
            var actualErrors = node.GetDiagnostics();
            actualErrors.Verify(expectedErrors);
            UsingNode(node);
        }

        internal void UsingExpression(string text, LuaParseOptions? options, params DiagnosticDescription[] expectedErrors) =>
            UsingNode(text, SyntaxFactory.ParseExpression(text, options: options), expectedErrors);

        protected void UsingNode(string text, LuaSyntaxNode node, DiagnosticDescription[] expectedErrors)
        {
            // we validate the text roundtrips
            Assert.Equal(text, node.ToFullString());
            var actualErrors = node.GetDiagnostics();
            actualErrors.Verify(expectedErrors);
            UsingNode(node);
        }

        internal void UsingExpression(string text, params DiagnosticDescription[] expectedErrors) =>
            UsingExpression(text, options: null, expectedErrors);

        /// <summary>
        /// Parses given string and initializes a depth-first preorder enumerator.
        /// </summary>
        protected SyntaxTree UsingTree(string text, LuaParseOptions? options = null)
        {
            VerifyEnumeratorConsumed();
            var tree = ParseTree(text, options);
            _node = tree.GetCompilationUnitRoot();
            var nodes = EnumerateNodes(_node, dump: false);
            _treeEnumerator = nodes.GetEnumerator();

            return tree;
        }

        /// <summary>
        /// Parses given string and initializes a depth-first preorder enumerator.
        /// </summary>
        protected LuaSyntaxNode UsingNode(string text)
        {
            var root = ParseNode(text, options: null);
            UsingNode(root);
            return root;
        }

        protected LuaSyntaxNode UsingNode(string text, LuaParseOptions options, params DiagnosticDescription[] expectedErrors)
        {
            var node = ParseNode(text, options);
            UsingNode(text, node, expectedErrors);
            return node;
        }

        /// <summary>
        /// Initializes a depth-first preorder enumerator for the given node.
        /// </summary>
        protected void UsingNode(LuaSyntaxNode root)
        {
            VerifyEnumeratorConsumed();
            _node = root;
            var nodes = EnumerateNodes(root, dump: false);
            _treeEnumerator = nodes.GetEnumerator();
        }

        /// <summary>
        /// Moves the enumerator and asserts that the current node is of the given kind.
        /// </summary>
        [DebuggerHidden]
        protected SyntaxNodeOrToken N(SyntaxKind kind, string? value = null)
        {
            try
            {
                Assert.True(_treeEnumerator!.MoveNext());
                Assert.Equal(kind, _treeEnumerator.Current.Kind());
                Assert.False(_treeEnumerator.Current.IsMissing);

                if (value != null)
                {
                    Assert.Equal(_treeEnumerator.Current.ToString(), value);
                }

                return _treeEnumerator.Current;
            }
            catch when (DumpAndCleanup())
            {
                throw;
            }
        }

        /// <summary>
        /// Moves the enumerator and asserts that the current node is of the given kind
        /// and is missing.
        /// </summary>
        [DebuggerHidden]
        protected SyntaxNodeOrToken M(SyntaxKind kind)
        {
            try
            {
                Assert.True(_treeEnumerator!.MoveNext());
                var current = _treeEnumerator.Current;
                Assert.Equal(kind, current.Kind());
                Assert.True(current.IsMissing);
                return current;
            }
            catch when (DumpAndCleanup())
            {
                throw;
            }
        }

        /// <summary>
        /// Asserts that the enumerator does not have any more nodes.
        /// </summary>
        [DebuggerHidden]
        protected void EOF()
        {
            if (_treeEnumerator!.MoveNext())
            {
                var tk = _treeEnumerator.Current.Kind();
                DumpAndCleanup();
                Assert.False(true, "Found unexpected node or token of kind: " + tk);
            }
        }

        private IEnumerable<SyntaxNodeOrToken> EnumerateNodes(LuaSyntaxNode node, bool dump)
        {
            Print(node, dump);
            yield return node;

            var stack = new Stack<ChildSyntaxList.Enumerator>(24);
            stack.Push(node.ChildNodesAndTokens().GetEnumerator());
            Open(dump);

            while (stack.Count > 0)
            {
                var en = stack.Pop();
                if (!en.MoveNext())
                {
                    // no more down this branch
                    Close(dump);
                    continue;
                }

                var current = en.Current;
                stack.Push(en); // put it back on stack (struct enumerator)

                Print(current, dump);
                yield return current;

                if (current.IsNode)
                {
                    // not token, so consider children
                    stack.Push(current.ChildNodesAndTokens().GetEnumerator());
                    Open(dump);
                    continue;
                }
            }

            Done(dump);
        }

        private void Print(SyntaxNodeOrToken node, bool dump)
        {
            if (dump)
            {
                switch (node.Kind())
                {
                    case SyntaxKind.IdentifierToken:
                    case SyntaxKind.NumericLiteralToken:
                        if (node.IsMissing)
                        {
                            goto default;
                        }
                        _output.WriteLine(@"N(SyntaxKind.{0}, ""{1}"");", node.Kind(), node.ToString());
                        break;
                    default:
                        _output.WriteLine("{0}(SyntaxKind.{1});", node.IsMissing ? "M" : "N", node.Kind());
                        break;
                }
            }
        }

        private void Open(bool dump)
        {
            if (dump)
            {
                _output.WriteLine("{");
            }
        }

        private void Close(bool dump)
        {
            if (dump)
            {
                _output.WriteLine("}");
            }
        }

        private void Done(bool dump)
        {
            if (dump)
            {
                _output.WriteLine("EOF();");
            }
        }
    }
}
