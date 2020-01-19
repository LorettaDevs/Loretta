using System;
using GParse;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta
{
    public static partial class LuaDiagnostics
    {
        public static class SyntaxError
        {
            public static Diagnostic ThingExpected ( SourceRange range, Object expected ) =>
                new Diagnostic ( "LUA0001", range, DiagnosticSeverity.Error, $"{expected} expected." );

            public static Diagnostic ThingExpected ( SourceLocation location, Object expected ) =>
                ThingExpected ( location.To ( location ), expected );

            public static Diagnostic ThingExpected ( LuaToken token, Object expected ) =>
                ThingExpected ( token.Range, expected );

            public static Diagnostic ThingExpectedFor ( SourceRange range, Object expected, String @for ) =>
                new Diagnostic ( "LUA0002", range, DiagnosticSeverity.Error, $"{expected} expected for {@for}." );

            public static Diagnostic ThingExpectedFor ( SourceLocation location, Object expected, String @for ) =>
                ThingExpectedFor ( location.To ( location ), expected, @for );

            public static Diagnostic ThingExpectedFor ( LuaToken token, Object expected, String @for ) =>
                ThingExpectedFor ( token.Range, expected, @for );

            public static Diagnostic ThingExpectedAfter ( SourceRange range, Object expected, String after ) =>
                new Diagnostic ( "LUA0002", range, DiagnosticSeverity.Error, $"{expected} expected after {after}." );

            public static Diagnostic ThingExpectedAfter ( SourceLocation location, Object expected, String after ) =>
                ThingExpectedAfter ( location.To ( location ), expected, after );

            public static Diagnostic ThingExpectedAfter ( LuaToken token, Object expected, String after ) =>
                ThingExpectedAfter ( token.Range, expected, after );

            public static Diagnostic InvalidEscapeInString ( SourceRange range ) =>
                new Diagnostic ( "LUA0003", range, DiagnosticSeverity.Error, "Unknown escape sequence." );

            public static Diagnostic InvalidNumber ( SourceRange range, String raw ) =>
                new Diagnostic ( "LUA0004", range, DiagnosticSeverity.Error, $"Invalid number '{raw}'." );

            public static Diagnostic InvalidNumber ( LuaToken token ) =>
                InvalidNumber ( token.Range, token.Raw );

            public static Diagnostic NumberTooLarge ( SourceRange range, String raw ) =>
                new Diagnostic ( "LUA0005", range, DiagnosticSeverity.Error, $"Number '{raw}' is too large." );

            public static Diagnostic NumberTooLarge ( LuaToken token ) =>
                NumberTooLarge ( token.Range, token.Raw );

            public static Diagnostic UnfinishedString ( SourceRange range ) =>
                new Diagnostic ( "LUA0006", range, DiagnosticSeverity.Error, "Unfinished string." );

            public static Diagnostic RedeclaredLocalVariable ( LuaToken token )
            {
                if ( token.Type != Lexing.LuaTokenType.Identifier )
                    throw new ArgumentException ( "Token is not an identifier.", nameof ( token ) );

                return RedeclaredLocalVariable ( ( String ) token.Value, token.Range );
            }

            public static Diagnostic RedeclaredLocalVariable ( String name, SourceRange range ) =>
                new Diagnostic ( "LUA0007", range, DiagnosticSeverity.Error, $"Redeclared local variable '{name}'." );
        }
    }
}