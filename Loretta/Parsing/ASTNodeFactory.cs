using System;
using System.Collections.Generic;
using System.Text;
using GParse;
using Loretta.Lexing;
using Loretta.Parsing.AST;

namespace Loretta.Parsing
{
    public static class ASTNodeFactory
    {
        public static BooleanExpression BooleanExpression ( Boolean value, SourceRange? range = null ) =>
            new BooleanExpression ( TokenFactory.Boolean ( value, range ), value );

        public static NumberExpression NumberExpression ( Double value, String rawValue = null, SourceRange? range = null ) =>
            new NumberExpression ( TokenFactory.Number ( value, rawValue, range ) );

        public static StringExpression ShortString ( String value, String rawValue, SourceRange? range = null ) =>
            new StringExpression ( TokenFactory.ShortString ( value, rawValue, range ) );

        public static StringExpression LongString ( String value, String rawValue, SourceRange? range = null ) =>
            new StringExpression ( TokenFactory.LongString ( value, rawValue, range ) );

        public static NilExpression Nil ( SourceRange? range = null ) =>
            new NilExpression ( TokenFactory.Token ( "nil", LuaTokenType.Nil, "nil", null, range ) );
    }
}
