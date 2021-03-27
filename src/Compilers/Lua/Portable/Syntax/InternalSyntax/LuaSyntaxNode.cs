using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Loretta.CodeAnalysis.Syntax.InternalSyntax;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal abstract class LuaSyntaxNode : GreenNode
    {
        protected LuaSyntaxNode(SyntaxKind kind)
            : base((ushort) kind)
        {
            GreenStats.NoteGreen(this);
        }

        protected LuaSyntaxNode(SyntaxKind kind, int fullWidth)
            : base((ushort) kind, fullWidth)
        {
            GreenStats.NoteGreen(this);
        }

        protected LuaSyntaxNode(SyntaxKind kind, DiagnosticInfo[]? diagnostics)
            : base((ushort) kind, diagnostics)
        {
            GreenStats.NoteGreen(this);
        }

        protected LuaSyntaxNode(SyntaxKind kind, DiagnosticInfo[]? diagnostics, int fullWidth)
            : base((ushort) kind, diagnostics, fullWidth)
        {
            GreenStats.NoteGreen(this);
        }

        protected LuaSyntaxNode(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
            : base((ushort) kind, diagnostics, annotations)
        {
            GreenStats.NoteGreen(this);
        }

        protected LuaSyntaxNode(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations, int fullWidth)
            : base((ushort) kind, diagnostics, annotations, fullWidth)
        {
            GreenStats.NoteGreen(this);
        }

        internal LuaSyntaxNode(ObjectReader reader) : base(reader)
        {
        }

        public override string Language => LanguageNames.Lua;

        public SyntaxKind Kind => (SyntaxKind) RawKind;

        public override string KindText => Kind.ToString();

        public override int RawContextualKind => RawKind;

        public override bool IsStructuredTrivia => this is StructuredTriviaSyntax;
        public override bool IsSkippedTokensTrivia => Kind == SyntaxKind.SkippedTokensTrivia;

        public SyntaxToken? GetFirstToken() => (SyntaxToken?) GetFirstTerminal();

        public SyntaxToken? GetLastToken() => (SyntaxToken?) GetLastTerminal();

        public SyntaxToken? GetLastNonmissingToken() => (SyntaxToken?) GetLastNonmissingTerminal();

        public virtual GreenNode? GetLeadingTrivia() => null;

        public override GreenNode? GetLeadingTriviaCore() => GetLeadingTrivia();

        public virtual GreenNode? GetTrailingTrivia() => null;

        public override GreenNode? GetTrailingTriviaCore() => GetTrailingTrivia();

        public abstract TResult? Accept<TResult>(LuaSyntaxVisitor<TResult> visitor);

        public abstract void Accept(LuaSyntaxVisitor visitor);

        public override CodeAnalysis.SyntaxToken CreateSeparator<TNode>(SyntaxNode element) =>
            Lua.SyntaxFactory.Token(SyntaxKind.CommaToken);

        public override bool IsTriviaWithEndOfLine() =>
            Kind is SyntaxKind.EndOfLineTrivia or SyntaxKind.SingleLineCommentTrivia;

        // Use conditional weak table so we always return same identity for structured trivia
        private static readonly ConditionalWeakTable<SyntaxNode, Dictionary<CodeAnalysis.SyntaxTrivia, SyntaxNode>> s_structuresTable = new();

        /// <summary>
        /// Gets the syntax node represented the structure of this trivia, if any. The HasStructure property can be used to 
        /// determine if this trivia has structure.
        /// </summary>
        /// <returns>
        /// A LuaSyntaxNode derived from StructuredTriviaSyntax, with the structured view of this trivia node. 
        /// If this trivia node does not have structure, returns null.
        /// </returns>
        /// <remarks>
        /// Some types of trivia have structure that can be accessed as additional syntax nodes.
        /// <c>However, currently, there are none.</c>
        /// </remarks>
        public override SyntaxNode? GetStructure(CodeAnalysis.SyntaxTrivia trivia)
        {
            throw new InvalidOperationException("There are no structured trivia currently.");
#pragma warning disable CS0162 // Unreachable code detected (will be used once structured trivia are created)
            if (trivia.HasStructure)
#pragma warning restore CS0162 // Unreachable code detected (will be used once structured trivia are created)
            {
                var parent = trivia.Token.Parent;
                if (parent != null)
                {
                    SyntaxNode structure;
                    var structsInParent = s_structuresTable.GetOrCreateValue(parent);
                    lock (structsInParent)
                    {
                        if (!structsInParent.TryGetValue(trivia, out structure))
                        {
                            structure = Lua.Syntax.StructuredTriviaSyntax.Create(trivia);
                            structsInParent.Add(trivia, structure);
                        }
                    }

                    return structure;
                }
                else
                {
                    return Lua.Syntax.StructuredTriviaSyntax.Create(trivia);
                }
            }

            return null;
        }
    }
}
