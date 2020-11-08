using System;
using GParse;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta
{
    public static partial class LuaDiagnostics
    {
        /// <summary>
        /// The class containing all syntax-error diagnostic factory methods.
        /// </summary>
        public static class SyntaxError
        {
            /// <summary>
            /// Creates an <see cref="DiagnosticSeverity.Error" /> diagnostic with ID LUA0001 and
            /// description in the format "{expected} expected.".
            /// </summary>
            /// <param name="range">The range the diagnostic refers to.</param>
            /// <param name="expected">What was expected.</param>
            /// <returns>The created diagnostic.</returns>
            public static Diagnostic ThingExpected ( SourceRange range, Object expected ) =>
                new Diagnostic ( "LUA0001", range, DiagnosticSeverity.Error, $"{expected} expected." );

            /// <summary>
            /// <inheritdoc cref="ThingExpected(SourceRange, Object)" />
            /// </summary>
            /// <param name="location">The location the diagnostic refers to.</param>
            /// <param name="expected"><inheritdoc cref="ThingExpected(SourceRange, Object)" /></param>
            /// <returns><inheritdoc cref="ThingExpected(SourceRange, Object)" /></returns>
            public static Diagnostic ThingExpected ( SourceLocation location, Object expected ) =>
                ThingExpected ( location.To ( location ), expected );

            /// <summary>
            /// <inheritdoc cref="ThingExpected(SourceRange, Object)" />
            /// </summary>
            /// <param name="token">The token the diagnostic refers to.</param>
            /// <param name="expected"><inheritdoc cref="ThingExpected(SourceRange, Object)" /></param>
            /// <returns><inheritdoc cref="ThingExpected(SourceRange, Object)" /></returns>
            public static Diagnostic ThingExpected ( LuaToken token, Object expected ) =>
                ThingExpected ( token.Range, expected );

            /// <summary>
            /// Creates an <see cref="DiagnosticSeverity.Error" /> diagnostic with ID LUA0002 and
            /// description in the format "{expected} expected for {for}.".
            /// </summary>
            /// <param name="range">The range the diagnostic refers to.</param>
            /// <param name="expected">What was expected.</param>
            /// <param name="for">What needed the expected object.</param>
            /// <returns>The created diagnostic.</returns>
            public static Diagnostic ThingExpectedFor ( SourceRange range, Object expected, String @for ) =>
                new Diagnostic ( "LUA0002", range, DiagnosticSeverity.Error, $"{expected} expected for {@for}." );

            /// <summary>
            /// <inheritdoc cref="ThingExpectedFor(SourceRange, Object, String)" />
            /// </summary>
            /// <param name="location">The location the diagnostic refers to.</param>
            /// <param name="expected">
            /// <inheritdoc cref="ThingExpectedFor(SourceRange, Object, String)" />
            /// </param>
            /// <param name="for">
            /// <inheritdoc cref="ThingExpectedFor(SourceRange, Object, String)" />
            /// </param>
            /// <returns><inheritdoc cref="ThingExpectedFor(SourceRange, Object, String)" /></returns>
            public static Diagnostic ThingExpectedFor ( SourceLocation location, Object expected, String @for ) =>
                ThingExpectedFor ( location.To ( location ), expected, @for );

            /// <summary>
            /// <inheritdoc cref="ThingExpectedFor(SourceRange, Object, String)" />
            /// </summary>
            /// <param name="token">The token the diagnostic refers to.</param>
            /// <param name="expected">
            /// <inheritdoc cref="ThingExpectedFor(SourceRange, Object, String)" />
            /// </param>
            /// <param name="for">
            /// <inheritdoc cref="ThingExpectedFor(SourceRange, Object, String)" />
            /// </param>
            /// <returns><inheritdoc cref="ThingExpectedFor(SourceRange, Object, String)" /></returns>
            public static Diagnostic ThingExpectedFor ( LuaToken token, Object expected, String @for ) =>
                ThingExpectedFor ( token.Range, expected, @for );

            /// <summary>
            /// Creates an <see cref="DiagnosticSeverity.Error" /> diagnostic with the ID LUA0002
            /// and description in the format "{expected} expected after {after}.".
            /// </summary>
            /// <param name="range">The range the diagnostic refers to.</param>
            /// <param name="expected">What was expected.</param>
            /// <param name="after">What was before what was expected.</param>
            /// <returns>The created diagnostic.</returns>
            public static Diagnostic ThingExpectedAfter ( SourceRange range, Object expected, String after ) =>
                new Diagnostic ( "LUA0002", range, DiagnosticSeverity.Error, $"{expected} expected after {after}." );

            /// <summary>
            /// <inheritdoc cref="ThingExpectedAfter(SourceRange, Object, String)" />
            /// </summary>
            /// <param name="location">The location the diagnostic refers to.</param>
            /// <param name="expected">
            /// <inheritdoc cref="ThingExpectedAfter(SourceRange, Object, String)" />
            /// </param>
            /// <param name="after">
            /// <inheritdoc cref="ThingExpectedAfter(SourceRange, Object, String)" />
            /// </param>
            /// <returns><inheritdoc cref="ThingExpectedAfter(SourceRange, Object, String)" /></returns>
            public static Diagnostic ThingExpectedAfter ( SourceLocation location, Object expected, String after ) =>
                ThingExpectedAfter ( location.To ( location ), expected, after );

            /// <summary>
            /// <inheritdoc cref="ThingExpectedAfter(SourceRange, Object, String)" />
            /// </summary>
            /// <param name="token">The token the diagnostic refers to.</param>
            /// <param name="expected">
            /// <inheritdoc cref="ThingExpectedAfter(SourceRange, Object, String)" />
            /// </param>
            /// <param name="after">
            /// <inheritdoc cref="ThingExpectedAfter(SourceRange, Object, String)" />
            /// </param>
            /// <returns><inheritdoc cref="ThingExpectedAfter(SourceRange, Object, String)" /></returns>
            public static Diagnostic ThingExpectedAfter ( LuaToken token, Object expected, String after ) =>
                ThingExpectedAfter ( token.Range, expected, after );

            /// <summary>
            /// Creates an <see cref="DiagnosticSeverity.Error" /> diagnostic with the ID LUA0003
            /// and description in the format "Unknown escape sequence.".
            /// </summary>
            /// <param name="range">The range the diagnostic refers to.</param>
            /// <returns>The created diagnostic.</returns>
            public static Diagnostic InvalidEscapeInString ( SourceRange range ) =>
                new Diagnostic ( "LUA0003", range, DiagnosticSeverity.Error, "Unknown escape sequence." );

            /// <summary>
            /// Creates an <see cref="DiagnosticSeverity.Error" /> diagnostic with the ID LUA0004
            /// and description in the format "Invalid number '{raw}'.".
            /// </summary>
            /// <param name="range">The range the diagnostic refers to.</param>
            /// <param name="raw">The raw form of the number the diagnostic refers to.</param>
            /// <returns>The created diagnostic.</returns>
            public static Diagnostic InvalidNumber ( SourceRange range, String raw ) =>
                new Diagnostic ( "LUA0004", range, DiagnosticSeverity.Error, $"Invalid number '{raw}'." );

            /// <summary>
            /// <inheritdoc cref="InvalidNumber(SourceRange, String)" />
            /// </summary>
            /// <param name="token">The token the diagnostic refers to.</param>
            /// <returns><inheritdoc cref="InvalidNumber(SourceRange, String)" /></returns>
            public static Diagnostic InvalidNumber ( LuaToken token ) =>
                InvalidNumber ( token.Range, token.Raw );

            /// <summary>
            /// Creates an <see cref="DiagnosticSeverity.Error" /> diagnostic with the ID LUA0005
            /// and description in the format "Number '{raw}' is too large.".
            /// </summary>
            /// <param name="range">The range the diagnostic refers to.</param>
            /// <param name="raw">The raw form of the number the diagnostic refers to.</param>
            /// <returns>The created diagnostic.</returns>
            public static Diagnostic NumberTooLarge ( SourceRange range, String raw ) =>
                new Diagnostic ( "LUA0005", range, DiagnosticSeverity.Error, $"Number '{raw}' is too large." );

            /// <summary>
            /// <inheritdoc cref="NumberTooLarge(SourceRange, String)" />
            /// </summary>
            /// <param name="token">The token the diagnostic refers to.</param>
            /// <returns><inheritdoc cref="NumberTooLarge(SourceRange, String)" /></returns>
            public static Diagnostic NumberTooLarge ( LuaToken token ) =>
                NumberTooLarge ( token.Range, token.Raw );

            /// <summary>
            /// Creates an <see cref="DiagnosticSeverity.Error" /> diagnostic with the ID LUA0006
            /// and description in the format "Unfinished string.".
            /// </summary>
            /// <param name="range">The range the diagnostic refers to.</param>
            /// <returns>The created diagnostic.</returns>
            public static Diagnostic UnfinishedString ( SourceRange range ) =>
                new Diagnostic ( "LUA0006", range, DiagnosticSeverity.Error, "Unfinished string." );

            /// <summary>
            /// Creates an <see cref="DiagnosticSeverity.Error"/> diagnostic with the ID LUA0007
            /// and description in the format "Unfinished long comment.".
            /// </summary>
            /// <param name="range">The range the diagnostic refers to.</param>
            /// <returns>The created diagnostic.</returns>
            public static Diagnostic UnfinishedLongComment ( SourceRange range ) =>
                new Diagnostic ( "LUA0007", range, DiagnosticSeverity.Error, "Unfinished long comment." );

            /// <summary>
            /// Creates an <see cref="DiagnosticSeverity.Error"/> diagnostic with the ID LUA0008
            /// and description in the format "Long comment has an end with a different amount
            /// of equal signs than the start.".
            /// </summary>
            /// <param name="range"></param>
            /// <returns></returns>
            public static Diagnostic LongCommentWithIncompatibleDelimiters ( SourceRange range ) =>
                new Diagnostic ( "LUA0008", range, DiagnosticSeverity.Error, "Long comment has an end with a different amount of equal signs than the start." );

            /// <summary>
            /// Creates an <see cref="DiagnosticSeverity.Error"/> diagnostic with the ID LUA0009
            /// and description in the format "Unfinished long string.".
            /// </summary>
            /// <param name="range">The range this diagnostic refers to.</param>
            /// <returns></returns>
            public static Diagnostic UnfinishedLongString ( SourceRange range ) =>
                new Diagnostic ( "LUA0009", range, DiagnosticSeverity.Error, "Unfinished long string." );

            /// <summary>
            /// Creates an <see cref="DiagnosticSeverity.Error"/> diagnostic with the ID LUA0010
            /// and description in the format "Long string has an end with a different amount of
            /// equal signs than the start.".
            /// </summary>
            /// <param name="range"></param>
            /// <returns></returns>
            public static Diagnostic LongStringWithIncompatibleDelimiters ( SourceRange range ) =>
                new Diagnostic ( "LUA0010", range, DiagnosticSeverity.Error, "Long string has an end with a different amount of equal signs than the start." );
        }
    }
}