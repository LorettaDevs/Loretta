using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.CodeAnalysis.Syntax;
using Xunit;

namespace Loretta.Tests.CodeAnalysis.Syntax
{
    public class SyntaxFactsTests
    {
        [Theory]
        [MemberData ( nameof ( GetSyntaxKindData ) )]
        public void SyntaxFacts_GetText_RoundTrips ( LuaOptions options, SyntaxKind kind )
        {
            var text = SyntaxFacts.GetText ( kind );
            if ( text is null ) return;

            ImmutableArray<SyntaxToken> tokens = SyntaxTree.ParseTokens ( options, text );
            SyntaxToken token = Assert.Single ( tokens );
            Assert.Equal ( kind, token.Kind );
            Assert.Equal ( text, token.Text );
        }

        public static IEnumerable<Object[]> GetSyntaxKindData ( ) =>
            from kind in Enum.GetValues<SyntaxKind> ( )
            from options in LuaOptions.AllPresets
            select new Object[] { options, kind };
    }
}
