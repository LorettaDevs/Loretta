using System;
using System.Collections.Generic;
using System.Text;

namespace Loretta.CodeAnalysis.Syntax
{
    public static class SyntaxNodeExtensions
    {
        public static SyntaxNode? FindMostSpecificNode ( this SyntaxNode root, Int32 position, Func<SyntaxNode, Boolean> filter, Boolean inclusive = false )
        {

        }
    }
}
