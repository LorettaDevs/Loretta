using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal partial class SyntaxToken : LuaSyntaxNode
    {
        internal SyntaxToken(SyntaxKind kind)
            : base(kind)
        {
            FullWidth = this.Text.Length;
            this.flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
        }

        internal SyntaxToken(SyntaxKind kind, DiagnosticInfo[] diagnostics)
            : base(kind, diagnostics)
        {
            FullWidth = this.Text.Length;
            this.flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
        }

        internal SyntaxToken(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
            : base(kind, diagnostics, annotations)
        {
            FullWidth = this.Text.Length;
            this.flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
        }

        internal SyntaxToken(SyntaxKind kind, int fullWidth)
            : base(kind, fullWidth)
        {
            this.flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
        }

        internal SyntaxToken(SyntaxKind kind, int fullWidth, DiagnosticInfo[] diagnostics)
            : base(kind, diagnostics, fullWidth)
        {
            this.flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
        }

        internal SyntaxToken(SyntaxKind kind, int fullWidth, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
            : base(kind, diagnostics, annotations, fullWidth)
        {
            this.flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
        }

        internal SyntaxToken(ObjectReader reader)
            : base(reader)
        {
            var text = this.Text;
            if (text != null)
            {
                FullWidth = text.Length;
            }

            this.flags |= NodeFlags.IsNotMissing;  //note: cleared by subclasses representing missing tokens
        }

        internal override bool ShouldReuseInSerialization
            => base.ShouldReuseInSerialization && FullWidth < Lexer.MaxCachedTokenSize;

        public override bool IsToken => true;

        internal override GreenNode? GetSlot(int index) => throw ExceptionUtilities.Unreachable;

        internal static SyntaxToken Create(SymbolKind kind)
        {
        }
    }
}
