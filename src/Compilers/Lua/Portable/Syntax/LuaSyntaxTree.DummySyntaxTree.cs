// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using System.Threading;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua
{
    public abstract partial class LuaSyntaxTree
    {
        internal sealed class DummySyntaxTree : LuaSyntaxTree
        {
            private readonly CompilationUnitSyntax _node;

            public DummySyntaxTree()
            {
                _node = CloneNodeAsRoot(SyntaxFactory.ParseCompilationUnit(string.Empty));
            }

            public override string ToString() => string.Empty;

            public override SourceText GetText(CancellationToken cancellationToken) => SourceText.From(string.Empty, Encoding.UTF8);

            public override bool TryGetText(out SourceText text)
            {
                text = SourceText.From(string.Empty, Encoding.UTF8);
                return true;
            }

            public override Encoding Encoding => Encoding.UTF8;

            public override int Length => 0;

            public override LuaParseOptions Options => LuaParseOptions.Default;

            public override string FilePath => string.Empty;

            public override SyntaxReference GetReference(SyntaxNode node) => new SimpleSyntaxReference(node);

            public override LuaSyntaxNode GetRoot(CancellationToken cancellationToken) => _node;

            public override bool TryGetRoot(out LuaSyntaxNode root)
            {
                root = _node;
                return true;
            }

            public override bool HasCompilationUnitRoot => true;

            public override FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default) =>
                default(FileLinePositionSpan);

            public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options) =>
                SyntaxFactory.SyntaxTree(root, options: options, path: FilePath, encoding: null);

            public override SyntaxTree WithFilePath(string path) =>
                SyntaxFactory.SyntaxTree(_node, options: Options, path: path, encoding: null);
        }
    }
}
