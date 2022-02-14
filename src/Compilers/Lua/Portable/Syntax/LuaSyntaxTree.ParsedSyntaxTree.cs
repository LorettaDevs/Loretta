// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua
{
    public abstract partial class LuaSyntaxTree
    {
        private class ParsedSyntaxTree : LuaSyntaxTree
        {
            private readonly LuaParseOptions _options;
            private readonly string _path;
            private readonly LuaSyntaxNode _root;
            private readonly bool _hasCompilationUnitRoot;
            private readonly Encoding? _encodingOpt;
            private readonly SourceHashAlgorithm _checksumAlgorithm;
            private SourceText? _lazyText;

            internal ParsedSyntaxTree(
                SourceText? textOpt,
                Encoding? encodingOpt,
                SourceHashAlgorithm checksumAlgorithm,
                string path,
                LuaParseOptions options,
                LuaSyntaxNode root,
                bool cloneRoot)
            {
                LorettaDebug.Assert(root != null);
                LorettaDebug.Assert(options != null);
                LorettaDebug.Assert(textOpt == null || textOpt.Encoding == encodingOpt && textOpt.ChecksumAlgorithm == checksumAlgorithm);

                _lazyText = textOpt;
                _encodingOpt = encodingOpt ?? textOpt?.Encoding;
                _checksumAlgorithm = checksumAlgorithm;
                _options = options;
                _path = path ?? string.Empty;
                _root = cloneRoot ? CloneNodeAsRoot(root) : root;
                _hasCompilationUnitRoot = root.Kind() == SyntaxKind.CompilationUnit;
            }

            public override string FilePath => _path;

            public override SourceText GetText(CancellationToken cancellationToken)
            {
                if (_lazyText == null)
                {
                    Interlocked.CompareExchange(ref _lazyText, GetRoot(cancellationToken).GetText(_encodingOpt, _checksumAlgorithm), null);
                }

                return _lazyText;
            }

            public override bool TryGetText([NotNullWhen(true)] out SourceText? text)
            {
                text = _lazyText;
                return text != null;
            }

            public override Encoding? Encoding => _encodingOpt;

            public override int Length => _root.FullSpan.Length;

            public override LuaSyntaxNode GetRoot(CancellationToken cancellationToken) => _root;

            public override bool TryGetRoot(out LuaSyntaxNode root)
            {
                root = _root;
                return true;
            }

            public override bool HasCompilationUnitRoot => _hasCompilationUnitRoot;

            public override LuaParseOptions Options => _options;

            public override SyntaxReference GetReference(SyntaxNode node) => new SimpleSyntaxReference(node);

            public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
            {
                if (ReferenceEquals(_root, root) && ReferenceEquals(_options, options))
                {
                    return this;
                }

                return new ParsedSyntaxTree(
                    textOpt: null,
                    _encodingOpt,
                    _checksumAlgorithm,
                    _path,
                    (LuaParseOptions) options,
                    (LuaSyntaxNode) root,
                    cloneRoot: true);
            }

            public override SyntaxTree WithFilePath(string path)
            {
                if (_path == path)
                {
                    return this;
                }

                return new ParsedSyntaxTree(
                    _lazyText,
                    _encodingOpt,
                    _checksumAlgorithm,
                    path,
                    _options,
                    _root,
                    cloneRoot: true);
            }
        }
    }
}
