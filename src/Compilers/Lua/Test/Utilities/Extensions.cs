// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests
{
    internal static partial class Extensions
    {
        public static SyntaxNodeOrToken FindNodeOrTokenByKind(this SyntaxTree syntaxTree, SyntaxKind kind, int occurrence = 1)
        {
            if (!(occurrence > 0))
            {
                throw new ArgumentException("Specified value must be greater than zero.", nameof(occurrence));
            }
            
            SyntaxNodeOrToken foundNode = default(SyntaxNodeOrToken);
            if (TryFindNodeOrToken(syntaxTree.GetCompilationUnitRoot(), kind, ref occurrence, ref foundNode))
            {
                return foundNode;
            }

            return default;
        }

        private static bool TryFindNodeOrToken(SyntaxNodeOrToken node, SyntaxKind kind, ref int occurrence, ref SyntaxNodeOrToken foundNode)
        {
            if (node.IsKind(kind))
            {
                occurrence--;
                if (occurrence == 0)
                {
                    foundNode = node;
                    return true;
                }
            }

            // we should probably did into trivia if this is a Token, but we won't

            foreach (var child in node.ChildNodesAndTokens())
            {
                if (TryFindNodeOrToken(child, kind, ref occurrence, ref foundNode))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
