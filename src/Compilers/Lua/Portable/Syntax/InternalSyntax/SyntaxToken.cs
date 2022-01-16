using System;
using System.Collections.Generic;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    using Loretta.CodeAnalysis.Syntax.InternalSyntax;

    internal partial class SyntaxToken : LuaSyntaxNode
    {
        internal SyntaxToken(SyntaxKind kind)
            : base(kind)
        {
            FullWidth = Text.Length;
            flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
        }

        internal SyntaxToken(SyntaxKind kind, DiagnosticInfo[]? diagnostics)
            : base(kind, diagnostics)
        {
            FullWidth = Text.Length;
            flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
        }

        internal SyntaxToken(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
            : base(kind, diagnostics, annotations)
        {
            FullWidth = Text.Length;
            flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
        }

        internal SyntaxToken(SyntaxKind kind, int fullWidth)
            : base(kind, fullWidth)
        {
            flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
        }

        internal SyntaxToken(SyntaxKind kind, int fullWidth, DiagnosticInfo[]? diagnostics)
            : base(kind, diagnostics, fullWidth)
        {
            flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
        }

        internal SyntaxToken(SyntaxKind kind, int fullWidth, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
            : base(kind, diagnostics, annotations, fullWidth)
        {
            flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
        }

        internal SyntaxToken(ObjectReader reader)
            : base(reader)
        {
            var text = Text;
            if (text != null)
            {
                FullWidth = text.Length;
            }

            flags |= NodeFlags.IsNotMissing;  //note: cleared by subclasses representing missing tokens
        }

        internal override bool ShouldReuseInSerialization
            => base.ShouldReuseInSerialization && FullWidth < Lexer.MaxCachedTokenSize;

        public override bool IsToken => true;

        internal override GreenNode? GetSlot(int index) => throw ExceptionUtilities.Unreachable;

        internal static SyntaxToken Create(SyntaxKind kind)
        {
            SyntaxToken token;
            if (kind > LastTokenWithWellKnownText)
            {
                if (!SyntaxFacts.IsToken(kind))
                    throw new ArgumentException(string.Format(LuaResources.ThisMethodCanOnlyBeUsedToCreateTokens, kind), nameof(kind));
                token = CreateMissing(kind, null, null);
            }
            else
            {
                token = s_tokensWithNoTrivia[(int) kind].Value;
            }

            LorettaDebug.Assert(token is not null);
            return token;
        }

        internal static SyntaxToken Create(SyntaxKind kind, GreenNode? leading, GreenNode? trailing)
        {
            SyntaxToken token;

            if (kind > LastTokenWithWellKnownText)
            {
                if (!SyntaxFacts.IsToken(kind))
                {
                    throw new ArgumentException(string.Format(LuaResources.ThisMethodCanOnlyBeUsedToCreateTokens, kind), nameof(kind));
                }

                token = CreateMissing(kind, leading, trailing);
                goto ret;
            }

            if (leading == null)
            {
                if (trailing == null)
                {
                    token = s_tokensWithNoTrivia[(int) kind].Value;
                    goto ret;
                }
                else if (trailing == SyntaxFactory.Space)
                {
                    token = s_tokensWithSingleTrailingSpace[(int) kind].Value;
                    goto ret;
                }
                else if (trailing == SyntaxFactory.CarriageReturnLineFeed)
                {
                    token = s_tokensWithSingleTrailingCRLF[(int) kind].Value;
                    goto ret;
                }
            }

            if (leading == SyntaxFactory.ElasticZeroSpace && trailing == SyntaxFactory.ElasticZeroSpace)
            {
                token = s_tokensWithElasticTrivia[(int) kind].Value;
                goto ret;
            }

            token = new SyntaxTokenWithTrivia(kind, leading, trailing);

        ret:
            LorettaDebug.Assert(token is not null);
            return token;
        }

        internal static SyntaxToken CreateMissing(SyntaxKind kind, GreenNode? leading, GreenNode? trailing)
            => new MissingTokenWithTrivia(kind, leading, trailing);

        internal const SyntaxKind FirstTokenWithWellKnownText = SyntaxKind.EndOfFileToken;
        internal const SyntaxKind LastTokenWithWellKnownText = SyntaxKind.FalseKeyword;

        // For now we don't have enough tokens to warrant excluding the leading elements
        private static readonly ArrayElement<SyntaxToken>[] s_tokensWithNoTrivia = new ArrayElement<SyntaxToken>[(int) LastTokenWithWellKnownText + 1];
        private static readonly ArrayElement<SyntaxToken>[] s_tokensWithElasticTrivia = new ArrayElement<SyntaxToken>[(int) LastTokenWithWellKnownText + 1];
        private static readonly ArrayElement<SyntaxToken>[] s_tokensWithSingleTrailingSpace = new ArrayElement<SyntaxToken>[(int) LastTokenWithWellKnownText + 1];
        private static readonly ArrayElement<SyntaxToken>[] s_tokensWithSingleTrailingCRLF = new ArrayElement<SyntaxToken>[(int) LastTokenWithWellKnownText + 1];

        static SyntaxToken()
        {
            ObjectBinder.RegisterTypeReader(typeof(SyntaxToken), r => new SyntaxToken(r));

            for (var kind = FirstTokenWithWellKnownText; kind <= LastTokenWithWellKnownText; kind++)
            {
                s_tokensWithNoTrivia[(int) kind].Value = new SyntaxToken(kind);
                s_tokensWithElasticTrivia[(int) kind].Value = new SyntaxTokenWithTrivia(kind, SyntaxFactory.ElasticZeroSpace, SyntaxFactory.ElasticZeroSpace);
                s_tokensWithSingleTrailingSpace[(int) kind].Value = new SyntaxTokenWithTrivia(kind, null, SyntaxFactory.Space);
                s_tokensWithSingleTrailingCRLF[(int) kind].Value = new SyntaxTokenWithTrivia(kind, null, SyntaxFactory.CarriageReturnLineFeed);
            }
        }

        internal static IEnumerable<SyntaxToken> GetWellKnownTokens()
        {
            for (var kind = FirstTokenWithWellKnownText; kind <= LastTokenWithWellKnownText; kind++)
                yield return s_tokensWithNoTrivia[(int) kind];
            for (var kind = FirstTokenWithWellKnownText; kind <= LastTokenWithWellKnownText; kind++)
                yield return s_tokensWithElasticTrivia[(int) kind];
            for (var kind = FirstTokenWithWellKnownText; kind <= LastTokenWithWellKnownText; kind++)
                yield return s_tokensWithSingleTrailingSpace[(int) kind];
            for (var kind = FirstTokenWithWellKnownText; kind <= LastTokenWithWellKnownText; kind++)
                yield return s_tokensWithSingleTrailingCRLF[(int) kind];
        }

        internal static SyntaxToken Identifier(string text)
            => new SyntaxIdentifier(text);

        internal static SyntaxToken Identifier(GreenNode? leading, string text, GreenNode? trailing)
        {
            if (leading == null)
            {
                if (trailing == null)
                {
                    return Identifier(text);
                }
                else
                {
                    return new SyntaxIdentifierWithTrailingTrivia(text, trailing);
                }
            }

            return new SyntaxIdentifierWithTrivia(SyntaxKind.IdentifierToken, text, leading, trailing);
        }

        internal static SyntaxToken Identifier(SyntaxKind contextualKind, GreenNode? leading, string text, GreenNode? trailing)
        {
            if (contextualKind == SyntaxKind.IdentifierToken)
            {
                return Identifier(leading, text, trailing);
            }

            return new SyntaxIdentifierWithTrivia(contextualKind, text, leading, trailing);
        }

        internal static SyntaxToken WithValue<T>(SyntaxKind kind, string text, T value)
            => new SyntaxTokenWithValue<T>(kind, text, value);

        internal static SyntaxToken WithValue<T>(SyntaxKind kind, GreenNode? leading, string text, T value, GreenNode? trailing)
            => new SyntaxTokenWithValueAndTrivia<T>(kind, text, value, leading, trailing);

        internal static SyntaxToken StringLiteral(string text)
            => new SyntaxTokenWithValue<string>(SyntaxKind.StringLiteralToken, text, text);

        internal static SyntaxToken StringLiteral(GreenNode? leading, string text, GreenNode? trailing)
            => new SyntaxTokenWithValueAndTrivia<string>(SyntaxKind.StringLiteralToken, text, text, leading, trailing);

        public virtual SyntaxKind ContextualKind => Kind;

        public override int RawContextualKind => (int) ContextualKind;

        public virtual string Text => SyntaxFacts.GetText(Kind)!;

        /// <summary>
        /// Returns the string representation of this token, not including its leading and trailing trivia.
        /// </summary>
        /// <returns>The string representation of this token, not including its leading and trailing trivia.</returns>
        /// <remarks>The length of the returned string is always the same as Span.Length</remarks>
        public override string ToString() => Text;

        public virtual object? Value => SyntaxFacts.GetConstantValue(Kind).UnwrapOr(Text);

        public override object? GetValue() => Value;

        public virtual string? ValueText => Text;

        public override string GetValueText() => ValueText ?? "";

        public override int Width => Text.Length;

        public override int GetLeadingTriviaWidth()
        {
            var leading = GetLeadingTrivia();
            return leading != null ? leading.FullWidth : 0;
        }

        public override int GetTrailingTriviaWidth()
        {
            var trailing = GetTrailingTrivia();
            return trailing != null ? trailing.FullWidth : 0;
        }

        internal SyntaxList<LuaSyntaxNode> LeadingTrivia => new(GetLeadingTrivia());

        internal SyntaxList<LuaSyntaxNode> TrailingTrivia => new(GetTrailingTrivia());

        public sealed override GreenNode WithLeadingTrivia(GreenNode? trivia) => TokenWithLeadingTrivia(trivia);

        public virtual SyntaxToken TokenWithLeadingTrivia(GreenNode? trivia)
            => new SyntaxTokenWithTrivia(Kind, trivia, null, GetDiagnostics(), GetAnnotations());

        public sealed override GreenNode WithTrailingTrivia(GreenNode? trivia) => TokenWithTrailingTrivia(trivia);

        public virtual SyntaxToken TokenWithTrailingTrivia(GreenNode? trivia)
            => new SyntaxTokenWithTrivia(Kind, null, trivia, GetDiagnostics(), GetAnnotations());

        internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
        {
            LorettaDebug.Assert(GetType() == typeof(SyntaxToken));
            return new SyntaxToken(Kind, FullWidth, diagnostics, GetAnnotations());
        }

        internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
        {
            LorettaDebug.Assert(GetType() == typeof(SyntaxToken));
            return new SyntaxToken(Kind, FullWidth, GetDiagnostics(), annotations);
        }

        public override TResult? Accept<TResult>(LuaSyntaxVisitor<TResult> visitor) where TResult : default => visitor.VisitToken(this);

        public override void Accept(LuaSyntaxVisitor visitor) => visitor.VisitToken(this);

        protected override void WriteTokenTo(System.IO.TextWriter writer, bool leading, bool trailing)
        {
            if (leading)
            {
                var trivia = GetLeadingTrivia();
                if (trivia != null)
                {
                    trivia.WriteTo(writer, true, true);
                }
            }

            writer.Write(Text);

            if (trailing)
            {
                var trivia = GetTrailingTrivia();
                if (trivia != null)
                {
                    trivia.WriteTo(writer, true, true);
                }
            }
        }

        public override bool IsEquivalentTo(GreenNode? other)
        {
            if (!base.IsEquivalentTo(other))
            {
                return false;
            }

            var otherToken = (SyntaxToken) other;

            if (Text != otherToken.Text)
            {
                return false;
            }

            var thisLeading = GetLeadingTrivia();
            var otherLeading = otherToken.GetLeadingTrivia();
            if (thisLeading != otherLeading)
            {
                if (thisLeading == null || otherLeading == null)
                {
                    return false;
                }

                if (!thisLeading.IsEquivalentTo(otherLeading))
                {
                    return false;
                }
            }

            var thisTrailing = GetTrailingTrivia();
            var otherTrailing = otherToken.GetTrailingTrivia();
            if (thisTrailing != otherTrailing)
            {
                if (thisTrailing == null || otherTrailing == null)
                {
                    return false;
                }

                if (!thisTrailing.IsEquivalentTo(otherTrailing))
                {
                    return false;
                }
            }

            return true;
        }

        internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => throw ExceptionUtilities.Unreachable;
    }
}
