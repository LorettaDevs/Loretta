using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Syntax;
using Loretta.CodeAnalysis.Text;
using Loretta.ThirdParty.FParsec;
using Tsu;
using Xunit;

namespace Loretta.Tests.CodeAnalysis.Syntax
{
    public class LexerTests
    {
        [Theory]
        [MemberData ( nameof ( GetUnfinishedShortStringsData ) )]
        public void Lexer_Lexes_UnterminatedShortString ( LuaOptions options, String text, String value )
        {
            ImmutableArray<SyntaxToken> tokens = SyntaxTree.ParseTokens ( options, text, out ImmutableArray<Diagnostic> diagnostics );

            SyntaxToken token = Assert.Single ( tokens );
            Assert.Equal ( SyntaxKind.ShortStringToken, token.Kind );
            Assert.Equal ( text, token.Text );
            Assert.Equal ( value, token.Value.Value );

            Diagnostic diagnostic = Assert.Single ( diagnostics );
            Assert.Equal ( "LUA0003", diagnostic.Id );
            Assert.Equal ( new TextSpan ( 0, text.Length ), diagnostic.Location.Span );
            Assert.Equal ( "Unfinished string", diagnostic.Description );
        }

        [Theory]
        [MemberData ( nameof ( GetAllPresetsData ) )]
        public void Lexer_Lexes_ShebangsOnlyOnFileStart ( LuaOptions options )
        {
            const String shebang = "#!/bin/bash";
            ImmutableArray<SyntaxToken> tokens = SyntaxTree.ParseTokens ( options, shebang, includeEndOfFile: true );

            SyntaxToken eof = Assert.Single ( tokens );
            SyntaxTrivia trivia = Assert.Single ( eof.LeadingTrivia );
            Assert.Equal ( SyntaxKind.ShebangTrivia, trivia.Kind );
            Assert.Equal ( shebang, trivia.Text );
            Assert.Equal ( new TextSpan ( 0, shebang.Length ), trivia.Span );

            tokens = SyntaxTree.ParseTokens ( options, $"\n{shebang}", includeEndOfFile: true );
            ShortToken[] expectedBrokenTokens = new[]
            {
                new ShortToken ( SyntaxKind.HashToken, "#", new TextSpan ( 1, 1 ) ),
                new ShortToken ( SyntaxKind.BangToken, "!", new TextSpan ( 2, 1 ) ),
                new ShortToken ( SyntaxKind.SlashToken, "/", new TextSpan ( 3, 1 ) ),
                new ShortToken ( SyntaxKind.IdentifierToken, "bin", new TextSpan ( 4, 3 ) ),
                new ShortToken ( SyntaxKind.SlashToken, "/", new TextSpan ( 7, 1 ) ),
                new ShortToken ( SyntaxKind.IdentifierToken, "bash", new TextSpan ( 8, 4 ) ),
                new ShortToken ( SyntaxKind.EndOfFileToken, "", new TextSpan ( 12, 0 ) ),
            };

            Assert.Equal ( expectedBrokenTokens.Length, tokens.Length );
            for ( var idx = 0; idx < expectedBrokenTokens.Length; idx++ )
            {
                SyntaxToken token = tokens[idx];
                ShortToken expected = expectedBrokenTokens[idx];

                Assert.Equal ( expected.Kind, token.Kind );
                Assert.Equal ( expected.Text, token.Text );
                Assert.Equal ( expected.Span, token.Span );
            }
        }

        [Fact]
        public void Lexer_Covers_AllTokens ( )
        {
            IEnumerable<SyntaxKind> tokenKinds = Enum.GetValues<SyntaxKind> ( )
                                                     .Where ( k => k.IsToken ( ) || k.IsTrivia ( ) );

            IEnumerable<SyntaxKind> testedTokenKinds = GetTokens ( ).Concat ( GetTrivia ( ) )
                                                                    .Select ( t => t.Kind );

            var untestedTokenKinds = new SortedSet<SyntaxKind> ( tokenKinds );
            untestedTokenKinds.Remove ( SyntaxKind.BadToken );
            untestedTokenKinds.Remove ( SyntaxKind.EndOfFileToken );
            untestedTokenKinds.Remove ( SyntaxKind.SkippedTextTrivia );
            untestedTokenKinds.ExceptWith ( testedTokenKinds );

            Assert.Empty ( untestedTokenKinds );
        }

        [Theory]
        [MemberData ( nameof ( GetTokensData ) )]
        public void Lexer_Lexes_Token ( LuaOptions options, ShortToken expectedToken )
        {
            ImmutableArray<SyntaxToken> tokens = SyntaxTree.ParseTokens ( options, expectedToken.Text );

            SyntaxToken token = Assert.Single ( tokens );
            Assert.Equal ( expectedToken.Kind, token.Kind );
            Assert.Equal ( expectedToken.Text, token.Text );
            Assert.Equal ( expectedToken.Span, token.Span );
            if ( expectedToken.Value.IsSome )
            {
                Assert.True ( token.Value.IsSome, "Expected token value to be Some but got None." );
                Assert.Equal ( expectedToken.Value.Value, token.Value.Value );
            }
            else
            {
                Assert.True ( token.Value.IsNone, "Expected token value to be None but got Some." );
            }
        }


        [Theory]
        [MemberData ( nameof ( GetTriviaData ) )]
        public void Lexer_Lexes_Trivia ( LuaOptions luaOptions, ShortToken expectedTrivia )
        {
            ImmutableArray<SyntaxToken> tokens = SyntaxTree.ParseTokens ( luaOptions, expectedTrivia.Text, includeEndOfFile: true );

            SyntaxToken token = Assert.Single ( tokens );
            SyntaxTrivia actualTrivia = Assert.Single ( token.LeadingTrivia );
            Assert.Equal ( expectedTrivia.Kind, actualTrivia.Kind );
            Assert.Equal ( expectedTrivia.Text, actualTrivia.Text );
            Assert.Equal ( expectedTrivia.Span, actualTrivia.Span );
        }

        [Theory]
        [MemberData ( nameof ( GetTokenPairsData ) )]
        public void Lexer_Lexes_TokenPairs ( LuaOptions luaOptions, ShortToken tokenA, ShortToken tokenB )
        {
            var text = tokenA.Text + tokenB.Text;
            ImmutableArray<SyntaxToken> tokens = SyntaxTree.ParseTokens ( luaOptions, text );

            Assert.Equal ( 2, tokens.Length );
            Assert.Equal ( tokenA.Kind, tokens[0].Kind );
            Assert.Equal ( tokenA.Text, tokens[0].Text );
            Assert.Equal ( tokenA.Span, tokens[0].Span );
            Assert.Equal ( tokenB.Kind, tokens[1].Kind );
            Assert.Equal ( tokenB.Text, tokens[1].Text );
            Assert.Equal ( tokenB.Span, tokens[1].Span );
        }

        [Theory]
        [MemberData ( nameof ( GetTokenPairsWithSeparatorsData ) )]
        public void Lexer_Lexes_TokenPairs_WithSeparators (
            LuaOptions options,
            ShortToken tokenA,
            ShortToken expectedSeparator,
            ShortToken tokenB )
        {
            var text = tokenA.Text + expectedSeparator.Text + tokenB.Text;
            ImmutableArray<SyntaxToken> tokens = SyntaxTree.ParseTokens ( options, text );

            Assert.Equal ( 2, tokens.Length );
            Assert.Equal ( tokenA.Kind, tokens[0].Kind );
            Assert.Equal ( tokenA.Text, tokens[0].Text );
            Assert.Equal ( tokenA.Span, tokens[0].Span );

            SyntaxTrivia actualSeparator = Assert.Single ( tokens[0].TrailingTrivia );
            Assert.Equal ( expectedSeparator.Kind, actualSeparator.Kind );
            Assert.Equal ( expectedSeparator.Text, actualSeparator.Text );
            Assert.Equal ( expectedSeparator.Span, actualSeparator.Span );

            Assert.Equal ( tokenB.Kind, tokens[1].Kind );
            Assert.Equal ( tokenB.Text, tokens[1].Text );
            Assert.Equal ( tokenB.Span, tokens[1].Span );
        }

        public static IEnumerable<Object[]> GetUnfinishedShortStringsData ( )
        {
            var textAndValues = new String[][]
            {
                new[] { "\"text", "text" },
                new[] { "'text", "text" },
                new[] { "\"text'", "text'" },
                new[] { "'text\"", "text\"" },
            };

            return from options in LuaOptions.AllPresets
                   from textAndValue in textAndValues
                   select new Object[] { options, textAndValue[0], textAndValue[1] };
        }

        public static IEnumerable<Object[]> GetTokensData ( ) =>
            from token in GetTokens ( )
            from options in LuaOptions.AllPresets
                //let options = LuaOptions.All
            select new Object[] { options, token };

        public static IEnumerable<Object[]> GetTriviaData ( ) =>
            from trivia in GetTrivia ( )
            from options in LuaOptions.AllPresets
                //let options = LuaOptions.All
            select new Object[] { options, trivia };

        public static IEnumerable<Object[]> GetTokenPairsData ( ) =>
            from pair in GetTokenPairs ( )
            from options in LuaOptions.AllPresets
                //let options = LuaOptions.LuaJIT
            select new Object[] { options, pair.tokenA, pair.tokenB };

        public static IEnumerable<Object[]> GetTokenPairsWithSeparatorsData ( ) =>
            from tuple in GetTokenPairsWithSeparators ( )
            from options in LuaOptions.AllPresets
                //let options = LuaOptions.LuaJIT
            select new Object[] { options, tuple.tokenA, tuple.separator, tuple.tokenB };

        public static IEnumerable<Object[]> GetAllPresetsData ( ) =>
            LuaOptions.AllPresets.Select ( option => new Object[] { option } );

        private static IEnumerable<ShortToken> GetTokens ( )
        {
            const String shortStringContentText = "hi\\\n\\\r\\\r\n\\a\\b\\f\\n\\r\\t\\v\\\\\\'\\\"\\0\\10\\255\\xF\\xFF";
            const String shortStringContentValue = "hi\n\r\r\n\a\b\f\n\r\t\v\\'\"\0\xA\xFF\xF\xFF";
            IEnumerable<ShortToken> fixedTokens = from kind in Enum.GetValues<SyntaxKind> ( )
                                                  let text = SyntaxFacts.GetText ( kind )
                                                  where text is not null
                                                  let value = SyntaxFacts.GetKeywordValue ( kind )
                                                  select new ShortToken ( kind, text, value );

            var dynamicTokens = new List<ShortToken>
            {
                #region Numbers

                // Binary
                new ShortToken ( SyntaxKind.NumberToken, "0b10", Option.Some<Double> ( 0b10 ) ),
                new ShortToken ( SyntaxKind.NumberToken, "0b10_10", Option.Some<Double> ( 0b1010 ) ),

                // Octal
                new ShortToken ( SyntaxKind.NumberToken, "0o77", Option.Some<Double> ( Convert.ToInt32 ( "77", 8 ) ) ),
                new ShortToken ( SyntaxKind.NumberToken, "0o77_77", Option.Some<Double> ( Convert.ToInt32 ( "7777", 8 ) ) ),

                // Decimal
                new ShortToken ( SyntaxKind.NumberToken, "1", 1d ),
                new ShortToken ( SyntaxKind.NumberToken, "1.1", 1.1d ),
                new ShortToken ( SyntaxKind.NumberToken, "1.1e10", 1.1e10d ),
                new ShortToken ( SyntaxKind.NumberToken, ".1", .1d ),
                new ShortToken ( SyntaxKind.NumberToken, ".1e10", .1e10d ),
                new ShortToken ( SyntaxKind.NumberToken, "1_1", 11d ),
                new ShortToken ( SyntaxKind.NumberToken, "1_1.1_1", 11.11d ),
                new ShortToken ( SyntaxKind.NumberToken, "1_1.1_1e1_0", 11.11e10d ),
                new ShortToken ( SyntaxKind.NumberToken, ".1_1", .11d ),
                new ShortToken ( SyntaxKind.NumberToken, ".1_1e1_0", .11e10d ),

                // Hexadecimal
                new ShortToken ( SyntaxKind.NumberToken, "0xf", HexFloat.DoubleFromHexString ( "0xf".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumberToken, "0xf.f", HexFloat.DoubleFromHexString ( "0xf.f".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumberToken, "0xf.fp10", HexFloat.DoubleFromHexString ( "0xf.fp10".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumberToken, "0x.f", HexFloat.DoubleFromHexString ( "0x.f".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumberToken, "0x.fp10", HexFloat.DoubleFromHexString ( "0x.fp10".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumberToken, "0xf_f", HexFloat.DoubleFromHexString ( "0xf_f".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumberToken, "0xf_f.f_f", HexFloat.DoubleFromHexString ( "0xf_f.f_f".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumberToken, "0xf_f.f_fp1_0", HexFloat.DoubleFromHexString ( "0xf_f.f_fp1_0".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumberToken, "0x.f_f", HexFloat.DoubleFromHexString ( "0x.f_f".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumberToken, "0x.f_fp1_0", HexFloat.DoubleFromHexString ( "0x.f_fp1_0".Replace ( "_", "" ) ) ),

                #endregion Numbers

                // Short strings
                new ShortToken ( SyntaxKind.ShortStringToken, '\'' + shortStringContentText + '\'', shortStringContentValue ),
                new ShortToken ( SyntaxKind.ShortStringToken, '"' + shortStringContentText + '"', shortStringContentValue ),

                // Identifiers
                new ShortToken ( SyntaxKind.IdentifierToken, "a" ),
                new ShortToken ( SyntaxKind.IdentifierToken, "abc" ),
                new ShortToken ( SyntaxKind.IdentifierToken, "_" ),
                new ShortToken ( SyntaxKind.IdentifierToken, "🅱" ),
                new ShortToken ( SyntaxKind.IdentifierToken, "\ufeff" ),  /* ZERO WIDTH NO-BREAK SPACE */
                new ShortToken ( SyntaxKind.IdentifierToken, "\u206b" ),  /* ACTIVATE SYMMETRIC SWAPPING */
                new ShortToken ( SyntaxKind.IdentifierToken, "\u202a" ),  /* LEFT-TO-RIGHT EMBEDDING */
                new ShortToken ( SyntaxKind.IdentifierToken, "\u206a" ),  /* INHIBIT SYMMETRIC SWAPPING */
                new ShortToken ( SyntaxKind.IdentifierToken, "\ufeff" ),  /* ZERO WIDTH NO-BREAK SPACE */
                new ShortToken ( SyntaxKind.IdentifierToken, "\u206a" ),  /* INHIBIT SYMMETRIC SWAPPING */
                new ShortToken ( SyntaxKind.IdentifierToken, "\u200e" ),  /* LEFT-TO-RIGHT MARK */
                new ShortToken ( SyntaxKind.IdentifierToken, "\u200c" ),  /* ZERO WIDTH NON-JOINER */
                new ShortToken ( SyntaxKind.IdentifierToken, "\u200e" ),  /* LEFT-TO-RIGHT MARK */
            };

            #region Strings

            var longStringContent = @"first line \n
second line \r\n
third line \r
fourth line \xFF.";

            // Long Strings
            IEnumerable<String> separators = Enumerable.Range ( 0, 6 )
                                                       .Select ( n => new String ( '=', n ) )
                                                       .ToImmutableArray ( );

            dynamicTokens.AddRange ( separators.Select ( sep => new ShortToken ( SyntaxKind.LongStringToken, $"[{sep}[{longStringContent}]{sep}]", longStringContent ) ) );

            #endregion Strings

            return fixedTokens.Concat ( dynamicTokens );
        }

        private static IEnumerable<ShortToken> GetTrivia ( )
        {
            return GetSeparators ( ).Concat ( new[]
            {
                new ShortToken ( SyntaxKind.SingleLineCommentTrivia, "-- hi" ),
                new ShortToken ( SyntaxKind.SingleLineCommentTrivia, "// hi" ),
                new ShortToken ( SyntaxKind.ShebangTrivia, "#!/bin/bash" ),
            } );
        }

        private static IEnumerable<ShortToken> GetSeparators ( )
        {
            return new[]
            {
                new ShortToken ( SyntaxKind.WhitespaceTrivia, " " ),
                new ShortToken ( SyntaxKind.WhitespaceTrivia, "  " ),
                new ShortToken ( SyntaxKind.WhitespaceTrivia, "\t" ),
                new ShortToken ( SyntaxKind.LineBreakTrivia, "\r" ),
                new ShortToken ( SyntaxKind.LineBreakTrivia, "\n" ),
                new ShortToken ( SyntaxKind.LineBreakTrivia, "\r\n" ),
                new ShortToken ( SyntaxKind.MultiLineCommentTrivia, "/**/" ),
                new ShortToken ( SyntaxKind.MultiLineCommentTrivia, "--[[]]" ),
                new ShortToken ( SyntaxKind.MultiLineCommentTrivia, "--[=[]=]" ),
                new ShortToken ( SyntaxKind.MultiLineCommentTrivia, "--[====[]====]" ),
                // Longs comments can't be used as separators because of the minus token.
            };
        }

        private static Boolean RequiresSeparator ( SyntaxKind kindA, SyntaxKind kindB )
        {
            var kindAIsKeyword = kindA.IsKeyword ( );
            var kindBIsKeyowrd = kindB.IsKeyword ( );

            if ( kindA is SyntaxKind.IdentifierToken && kindB is SyntaxKind.IdentifierToken )
                return true;
            if ( kindAIsKeyword && kindBIsKeyowrd )
                return true;
            if ( kindAIsKeyword && kindB is SyntaxKind.IdentifierToken )
                return true;
            if ( kindA is SyntaxKind.IdentifierToken && kindBIsKeyowrd )
                return true;
            if ( kindA is SyntaxKind.IdentifierToken && kindB is SyntaxKind.NumberToken )
                return true;
            if ( kindA is SyntaxKind.NumberToken && kindB is SyntaxKind.IdentifierToken )
                return true;
            if ( kindA is SyntaxKind.NumberToken && kindBIsKeyowrd )
                return true;
            if ( kindA is SyntaxKind.NumberToken && kindB is SyntaxKind.DotToken or SyntaxKind.DotDotToken or SyntaxKind.DotDotDotToken or SyntaxKind.DotDotEqualsToken )
                return true;
            if ( kindAIsKeyword && kindB is SyntaxKind.NumberToken )
                return true;
            if ( kindA is SyntaxKind.NumberToken && kindB is SyntaxKind.NumberToken )
                return true;
            if ( kindA is SyntaxKind.OpenBracketToken && kindB is SyntaxKind.OpenBracketToken or SyntaxKind.LongStringToken )
                return true;
            if ( kindA is SyntaxKind.ColonToken && kindB is SyntaxKind.ColonToken or SyntaxKind.ColonColonToken )
                return true;
            if ( kindA is SyntaxKind.PlusToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken )
                return true;
            if ( kindA is SyntaxKind.MinusToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken or SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia )
                return true;
            if ( kindA is SyntaxKind.MinusToken && kindB is SyntaxKind.MinusToken or SyntaxKind.MinusEqualsToken )
                return true;
            if ( kindA is SyntaxKind.StarToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken )
                return true;
            if ( kindA is SyntaxKind.SlashToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.SlashEqualsToken or SyntaxKind.EqualsEqualsToken )
                return true;
            if ( kindA is SyntaxKind.SlashToken && kindB is SyntaxKind.SlashToken or SyntaxKind.StarToken or SyntaxKind.StartEqualsToken or SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia )
                return true;
            if ( kindA is SyntaxKind.HatToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken )
                return true;
            if ( kindA is SyntaxKind.PercentToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken )
                return true;
            if ( kindA is SyntaxKind.DotDotToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken )
                return true;
            if ( kindA is SyntaxKind.DotToken or SyntaxKind.DotDotToken && kindB is SyntaxKind.DotToken or SyntaxKind.DotDotToken or SyntaxKind.DotDotDotToken or SyntaxKind.DotDotEqualsToken )
                return true;
            if ( kindA is SyntaxKind.EqualsToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken )
                return true;
            if ( kindA is SyntaxKind.BangToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken )
                return true;
            if ( kindA is SyntaxKind.LessThanToken && kindB is SyntaxKind.LessThanToken or SyntaxKind.LessThanEqualsToken or SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken or SyntaxKind.LessThanLessThanToken )
                return true;
            if ( kindA is SyntaxKind.GreaterThanToken && kindB is SyntaxKind.GreaterThanToken or SyntaxKind.GreaterThanEqualsToken or SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken or SyntaxKind.GreaterThanGreaterThanToken )
                return true;
            if ( kindA is SyntaxKind.AmpersandToken && kindB is SyntaxKind.AmpersandToken or SyntaxKind.AmpersandAmpersandToken )
                return true;
            if ( kindA is SyntaxKind.PipeToken && kindB is SyntaxKind.PipeToken or SyntaxKind.PipePipeToken )
                return true;
            // Dot can be the start of a number
            if ( kindA is SyntaxKind.DotToken or SyntaxKind.DotDotToken or SyntaxKind.DotDotDotToken && kindB is SyntaxKind.NumberToken )
                return true;
            // Shebang
            if ( kindA is SyntaxKind.HashToken && kindB is SyntaxKind.BangToken or SyntaxKind.BangEqualsToken )
                return true;

            return false;
        }

        private static IEnumerable<(ShortToken tokenA, ShortToken tokenB)> GetTokenPairs ( ) =>
            from tokenA in GetTokens ( )
            from tokB in GetTokens ( )
            where !RequiresSeparator ( tokenA.Kind, tokB.Kind )
            let tokenB = tokB.WithSpan ( new TextSpan ( tokenA.Span.End, tokB.Span.Length ) )
            select (tokenA, tokenB);

        private static IEnumerable<(ShortToken tokenA, ShortToken separator, ShortToken tokenB)> GetTokenPairsWithSeparators ( ) =>
            from tokenA in GetTokens ( )
            from tokB in GetTokens ( )
            where RequiresSeparator ( tokenA.Kind, tokB.Kind )
            from sep in GetSeparators ( )
            where !RequiresSeparator ( tokenA.Kind, sep.Kind ) && !RequiresSeparator ( sep.Kind, tokB.Kind )
            let separator = sep.WithSpan ( new TextSpan ( tokenA.Span.End, sep.Span.Length ) )
            let tokenB = tokB.WithSpan ( new TextSpan ( separator.Span.End, tokB.Span.Length ) )
            select (tokenA, separator, tokenB);
    }

    public readonly struct ShortDiagnostic
    {
        public readonly String Id;
        public readonly String Description;
        public readonly TextSpan Span;

        public ShortDiagnostic ( String id, String description, TextSpan span )
        {
            this.Id = id;
            this.Description = description;
            this.Span = span;
        }

        public ShortDiagnostic ( Diagnostic diagnostic )
            : this ( diagnostic.Id, diagnostic.Description, diagnostic.Location.Span )
        {
        }

        public void Deconstruct ( out String id, out String description, out TextSpan span )
        {
            id = this.Id;
            description = this.Description;
            span = this.Span;
        }
    }

    public readonly struct ShortToken
    {
        public readonly SyntaxKind Kind;
        public readonly String Text;
        public readonly Option<Object?> Value;
        public readonly TextSpan Span;

        public ShortToken ( SyntaxKind kind, String text, Option<Object?> value = default )
        {
            this.Kind = kind;
            this.Text = text;
            this.Value = value;
            this.Span = new TextSpan ( 0, text.Length );
        }

        public ShortToken ( SyntaxKind kind, String text, TextSpan span, Option<Object?> value = default )
        {
            this.Kind = kind;
            this.Text = text;
            this.Value = value;
            this.Span = span;
        }

        public ShortToken ( SyntaxToken token )
            : this ( token.Kind, token.Text, token.Span, token.Value )
        {
        }

        public ShortToken WithSpan ( TextSpan span ) =>
            new ShortToken ( this.Kind, this.Text, span, this.Value );

        public override String ToString ( ) => $"{this.Kind}<{this.Text}>";
    }
}
