using System;
using System.Collections.Generic;
using System.Text;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// The base class for algorithms that walk over nodes.
    /// </summary>
    public abstract class SyntaxWalker : SyntaxVisitor
    {
        public override void VisitToken ( SyntaxToken token )
        {
        }

        public virtual void VisitTrivia ( SyntaxTrivia trivia )
        {
        }
    }
}
