using System;
using System.Collections.Generic;
using System.Text;

namespace Loretta.CodeAnalysis.Syntax
{
    public abstract class MemberSyntax : SyntaxNode
    {
        private protected MemberSyntax ( SyntaxTree syntaxTree )
            : base ( syntaxTree )
        {
        }
    }
}
