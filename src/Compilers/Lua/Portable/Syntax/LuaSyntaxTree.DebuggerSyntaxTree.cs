// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua
{
    public abstract partial class LuaSyntaxTree
    {
        private class DebuggerSyntaxTree : ParsedSyntaxTree
        {
            public DebuggerSyntaxTree(LuaSyntaxNode root, SourceText text, LuaParseOptions options)
                : base(
                    text,
                    text.Encoding,
                    text.ChecksumAlgorithm,
                    path: "",
                    options: options,
                    root: root,
                    cloneRoot: true)
            {
            }

            internal override bool SupportsLocations => true;
        }
    }
}
