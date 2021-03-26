// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal readonly struct BlendedNode
    {
        internal readonly Lua.LuaSyntaxNode? Node;
        internal readonly SyntaxToken? Token;
        internal readonly Blender Blender;

        internal BlendedNode(Lua.LuaSyntaxNode? node, SyntaxToken? token, Blender blender)
        {
            Node = node;
            Token = token;
            Blender = blender;
        }
    }
}
