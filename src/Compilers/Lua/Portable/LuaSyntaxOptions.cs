using System;
using System.Collections.Immutable;
using Tsu;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The options used by Loretta to adapt to the syntax of the lua flavor being parsed.
    /// </summary>
    /// <remarks>
    /// Otherwise when noted, "Accept" in this class means not generating an error when parsing,
    /// but the syntax behind the option will still be parsed normally.
    /// </remarks>
    public class LuaSyntaxOptions : IEquatable<LuaSyntaxOptions?>
    {
        /// <summary>
        /// The Lua 5.1 preset.
        /// </summary>
        public static readonly LuaSyntaxOptions Lua51 = new LuaSyntaxOptions(
            acceptBinaryNumbers: false,
            acceptCCommentSyntax: false,
            acceptCompoundAssignment: false,
            acceptEmptyStatements: false,
            acceptCBooleanOperators: false,
            acceptGoto: false,
            acceptHexEscapesInStrings: false,
            acceptHexFloatLiterals: false,
            acceptOctalNumbers: false,
            acceptShebang: false,
            acceptUnderscoreInNumberLiterals: false,
            useLuaJitIdentifierRules: false,
            acceptBitwiseOperators: false,
            acceptWhitespaceEscape: false,
            acceptUnicodeEscape: false,
            continueType: ContinueType.None);

        /// <summary>
        /// The Lua 5.2 preset.
        /// </summary>
        public static readonly LuaSyntaxOptions Lua52 = Lua51.With(
            acceptEmptyStatements: true,
            acceptGoto: true,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: true,
            acceptWhitespaceEscape: true);

        /// <summary>
        /// The Lua 5.3 preset.
        /// </summary>
        public static readonly LuaSyntaxOptions Lua53 = Lua52.With(
            acceptBitwiseOperators: true,
            acceptUnicodeEscape: true);

        /// <summary>
        /// The LuaJIT 2.0 preset.
        /// </summary>
        public static readonly LuaSyntaxOptions LuaJIT20 = new LuaSyntaxOptions(
            acceptBinaryNumbers: false,
            acceptCCommentSyntax: false,
            acceptCompoundAssignment: false,
            acceptEmptyStatements: true,
            acceptCBooleanOperators: false,
            acceptGoto: true,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: true,
            acceptOctalNumbers: false,
            acceptShebang: false,
            acceptUnderscoreInNumberLiterals: false,
            useLuaJitIdentifierRules: true,
            acceptBitwiseOperators: false,
            acceptWhitespaceEscape: true,
            acceptUnicodeEscape: false,
            continueType: ContinueType.None);

        /// <summary>
        /// The LuaJIT 2.1-beta3 preset.
        /// </summary>
        public static readonly LuaSyntaxOptions LuaJIT21 = LuaJIT20.With(
            acceptBinaryNumbers: true,
            acceptUnicodeEscape: true);

        /// <summary>
        /// The GLua preset.
        /// </summary>
        public static readonly LuaSyntaxOptions GMod = LuaJIT20.With(
            acceptCCommentSyntax: true,
            acceptCBooleanOperators: true,
            continueType: ContinueType.Keyword);

        /// <summary>
        /// The Luau preset.
        /// </summary>
        public static readonly LuaSyntaxOptions Roblox = new LuaSyntaxOptions(
            acceptBinaryNumbers: true,
            acceptCCommentSyntax: false,
            acceptCompoundAssignment: true,
            acceptEmptyStatements: false,
            acceptCBooleanOperators: false,
            acceptGoto: false,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: true,
            acceptOctalNumbers: false,
            acceptShebang: false,
            acceptUnderscoreInNumberLiterals: true,
            useLuaJitIdentifierRules: false,
            acceptBitwiseOperators: true,
            acceptWhitespaceEscape: true,
            acceptUnicodeEscape: true,
            continueType: ContinueType.ContextualKeyword);

        /// <summary>
        /// The preset that sets everything to true and continue to <see
        /// cref="ContinueType.ContextualKeyword" />.
        /// </summary>
        public static readonly LuaSyntaxOptions All = new LuaSyntaxOptions(
            acceptBinaryNumbers: true,
            acceptCCommentSyntax: true,
            acceptCompoundAssignment: true,
            acceptEmptyStatements: true,
            acceptCBooleanOperators: true,
            acceptGoto: true,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: true,
            acceptOctalNumbers: true,
            acceptShebang: true,
            acceptUnderscoreInNumberLiterals: true,
            useLuaJitIdentifierRules: true,
            acceptBitwiseOperators: true,
            acceptWhitespaceEscape: true,
            acceptUnicodeEscape: true,
            continueType: ContinueType.ContextualKeyword);

        /// <summary>
        /// All presets that are preconfigured in <see cref="LuaSyntaxOptions"/>.
        /// </summary>
        public static ImmutableArray<LuaSyntaxOptions> AllPresets { get; } = ImmutableArray.Create(new[]
        {
            Lua51,
            Lua52,
            Lua53,
            LuaJIT20,
            LuaJIT21,
            GMod,
            Roblox,
            All
        });

        /// <summary>
        /// Initializes a new lua options set.
        /// </summary>
        /// <param name="acceptBinaryNumbers"><inheritdoc cref="AcceptBinaryNumbers" path="/summary" /></param>
        /// <param name="acceptCCommentSyntax"><inheritdoc cref="AcceptCCommentSyntax" path="/summary" /></param>
        /// <param name="acceptCompoundAssignment"><inheritdoc cref="AcceptCompoundAssignment" path="/summary" /></param>
        /// <param name="acceptEmptyStatements"><inheritdoc cref="AcceptEmptyStatements" path="/summary" /></param>
        /// <param name="acceptCBooleanOperators"><inheritdoc cref="AcceptCBooleanOperators" path="/summary" /></param>
        /// <param name="acceptGoto"><inheritdoc cref="AcceptGoto" path="/summary" /></param>
        /// <param name="acceptHexEscapesInStrings"><inheritdoc cref="AcceptHexEscapesInStrings" path="/summary" /></param>
        /// <param name="acceptHexFloatLiterals"><inheritdoc cref="AcceptHexFloatLiterals" path="/summary" /></param>
        /// <param name="acceptOctalNumbers"><inheritdoc cref="AcceptOctalNumbers" path="/summary" /></param>
        /// <param name="acceptShebang"><inheritdoc cref="AcceptShebang" path="/summary" /></param>
        /// <param name="acceptUnderscoreInNumberLiterals"><inheritdoc cref="AcceptUnderscoreInNumberLiterals" path="/summary" /></param>
        /// <param name="useLuaJitIdentifierRules"><inheritdoc cref="UseLuaJitIdentifierRules" path="/summary" /></param>
        /// <param name="acceptBitwiseOperators"><inheritdoc cref="AcceptBitwiseOperators" path="/summary"/></param>
        /// <param name="acceptWhitespaceEscape"><inheritdoc cref="AcceptWhitespaceEscape" path="/summary"/></param>
        /// <param name="acceptUnicodeEscape"><inheritdoc cref="AcceptUnicodeEscape" path="/summary"/></param>
        /// <param name="continueType"><inheritdoc cref="ContinueType" path="/summary" /></param>
        public LuaSyntaxOptions(
            bool acceptBinaryNumbers,
            bool acceptCCommentSyntax,
            bool acceptCompoundAssignment,
            bool acceptEmptyStatements,
            bool acceptCBooleanOperators,
            bool acceptGoto,
            bool acceptHexEscapesInStrings,
            bool acceptHexFloatLiterals,
            bool acceptOctalNumbers,
            bool acceptShebang,
            bool acceptUnderscoreInNumberLiterals,
            bool useLuaJitIdentifierRules,
            bool acceptBitwiseOperators,
            bool acceptWhitespaceEscape,
            bool acceptUnicodeEscape,
            ContinueType continueType)
        {
            AcceptBinaryNumbers = acceptBinaryNumbers;
            AcceptCCommentSyntax = acceptCCommentSyntax;
            AcceptCompoundAssignment = acceptCompoundAssignment;
            AcceptEmptyStatements = acceptEmptyStatements;
            AcceptCBooleanOperators = acceptCBooleanOperators;
            AcceptGoto = acceptGoto;
            AcceptHexEscapesInStrings = acceptHexEscapesInStrings;
            AcceptHexFloatLiterals = acceptHexFloatLiterals;
            AcceptOctalNumbers = acceptOctalNumbers;
            AcceptShebang = acceptShebang;
            AcceptUnderscoreInNumberLiterals = acceptUnderscoreInNumberLiterals;
            UseLuaJitIdentifierRules = useLuaJitIdentifierRules;
            AcceptBitwiseOperators = acceptBitwiseOperators;
            AcceptWhitespaceEscape = acceptWhitespaceEscape;
            AcceptUnicodeEscape = acceptUnicodeEscape;
            ContinueType = continueType;
        }

        /// <summary>
        /// Whether to accept binary numbers (format: /0b[10]+/).
        /// </summary>
        /// <remarks>
        /// "accept" means an <see cref="CodeAnalysis.DiagnosticSeverity.Error"/>
        /// <see cref="CodeAnalysis.Diagnostic"/> will be raised when encountering
        /// a binary number, however the parsing process will still continue as if
        /// the number was a normal one.
        /// </remarks>
        public bool AcceptBinaryNumbers { get; }

        /// <summary>
        /// Whether to accept C comment syntax (formats: "//..." and "/* ... */").
        /// </summary>
        public bool AcceptCCommentSyntax { get; }

        /// <summary>
        /// Whether to accept compound assignment syntax (format: &lt;expr&gt; ("+=" | "-=" | "*=" |
        /// "/=" | "^=" | "%=" | "..=") &lt;expr&gt;).
        /// </summary>
        public bool AcceptCompoundAssignment { get; }

        /// <summary>
        /// Whether to accept empty statements (lone semicolons).
        /// </summary>
        public bool AcceptEmptyStatements { get; }

        /// <summary>
        /// Whether to accept the C boolean operators (&amp;&amp;, ||, != and !).
        /// </summary>
        public bool AcceptCBooleanOperators { get; }

        /// <summary>
        /// Whether to accept goto labels and statements.
        /// </summary>
        public bool AcceptGoto { get; }

        /// <summary>
        /// Whether to accept hexadecimal escapes in strings.
        /// </summary>
        public bool AcceptHexEscapesInStrings { get; }

        /// <summary>
        /// Whether to accept hexadecimal floating point literals (format: /0x[a-fA-F0-9]+(\.[a-fA-F0-9])?([+-]?p[0-9]+)/).
        /// </summary>
        public bool AcceptHexFloatLiterals { get; }

        /// <summary>
        /// Whether to accept octal numbers (format: /0o[0-7]+/).
        /// </summary>
        public bool AcceptOctalNumbers { get; }

        /// <summary>
        /// Whether to accept shebangs (format: "#!...") (currently accepted anywhere inside the file).
        /// </summary>
        public bool AcceptShebang { get; }

        /// <summary>
        /// Whether to accept underscores in any number literals (will be ignored when parsing the number).
        /// </summary>
        public bool AcceptUnderscoreInNumberLiterals { get; }

        /// <summary>
        /// Whether to use LuaJIT's identifier character rules (accepts any character greater than
        /// or equal to 0xF7).
        /// </summary>
        public bool UseLuaJitIdentifierRules { get; }

        /// <summary>
        /// Whether to error when encountering 5.3 bitise operators.
        /// </summary>
        public bool AcceptBitwiseOperators { get; }

        /// <summary>
        /// Whether to <b>not</b> error when encountering <c>\z</c> escapes.
        /// </summary>
        public bool AcceptWhitespaceEscape { get; }

        /// <summary>
        /// Whether to <b>not</b> error when encountering Unicode (<c>\u{XXX}</c>) escapes.
        /// </summary>
        public bool AcceptUnicodeEscape { get; }

        /// <summary>
        /// The type of continue to be recognized by the parser.
        /// </summary>
        public ContinueType ContinueType { get; }

        /// <summary>
        /// Creates a new lua options changing the provided fields.
        /// </summary>
        /// <param name="acceptBinaryNumbers">
        /// <inheritdoc cref="AcceptBinaryNumbers" path="/summary" /> If None uses the value of <see
        /// cref="AcceptBinaryNumbers" />.
        /// </param>
        /// <param name="acceptCCommentSyntax">
        /// <inheritdoc cref="AcceptCCommentSyntax" path="/summary" /> If None uses the value of
        /// <see cref="AcceptCCommentSyntax" />.
        /// </param>
        /// <param name="acceptCompoundAssignment">
        /// <inheritdoc cref="AcceptCompoundAssignment" path="/summary" /> If None uses the value of
        /// <see cref="AcceptCompoundAssignment" />.
        /// </param>
        /// <param name="acceptEmptyStatements">
        /// <inheritdoc cref="AcceptEmptyStatements" path="/summary" /> If None uses the value of
        /// <see cref="AcceptEmptyStatements" />.
        /// </param>
        /// <param name="acceptCBooleanOperators">
        /// <inheritdoc cref="AcceptCBooleanOperators" path="/summary" /> If None uses the value of
        /// <see cref="AcceptCBooleanOperators" />.
        /// </param>
        /// <param name="acceptGoto">
        /// <inheritdoc cref="AcceptGoto" path="/summary" /> If None uses the value of <see
        /// cref="AcceptGoto" />.
        /// </param>
        /// <param name="acceptHexEscapesInStrings">
        /// <inheritdoc cref="AcceptHexEscapesInStrings" path="/summary" /> If None uses the value
        /// of <see cref="AcceptHexEscapesInStrings" />.
        /// </param>
        /// <param name="acceptHexFloatLiterals">
        /// <inheritdoc cref="AcceptHexFloatLiterals" path="/summary" /> If None uses the value of
        /// <see cref="AcceptHexFloatLiterals" />.
        /// </param>
        /// <param name="acceptOctalNumbers">
        /// <inheritdoc cref="AcceptOctalNumbers" path="/summary" /> If None uses the value of <see
        /// cref="AcceptOctalNumbers" />.
        /// </param>
        /// <param name="acceptShebang">
        /// <inheritdoc cref="AcceptShebang" path="/summary" /> If None uses the value of <see
        /// cref="AcceptShebang" />.
        /// </param>
        /// <param name="acceptUnderscoreInNumberLiterals">
        /// <inheritdoc cref="AcceptUnderscoreInNumberLiterals" path="/summary" /> If None uses the
        /// value of <see cref="AcceptUnderscoreInNumberLiterals" />.
        /// </param>
        /// <param name="useLuaJitIdentifierRules">
        /// <inheritdoc cref="UseLuaJitIdentifierRules" path="/summary" /> If None uses the value of
        /// <see cref="UseLuaJitIdentifierRules" />.
        /// </param>
        /// <param name="acceptBitwiseOperators">
        /// <inheritdoc cref="AcceptBitwiseOperators" path="/summary"/> If None uses the value of
        /// <see cref="AcceptBitwiseOperators"/>.
        /// </param>
        /// <param name="acceptWhitespaceEscape">
        /// <inheritdoc cref="AcceptWhitespaceEscape" path="/summary"/> If None uses the value of
        /// <see cref="AcceptWhitespaceEscape"/>.
        /// </param>
        /// <param name="acceptUnicodeEscape">
        /// <inheritdoc cref="AcceptUnicodeEscape" path="/summary"/> If None uses the value of
        /// <see cref="AcceptUnicodeEscape"/>.
        /// </param>
        /// <param name="continueType">
        /// <inheritdoc cref="ContinueType" path="/summary" /> If None uses the value of <see
        /// cref="ContinueType" />.
        /// </param>
        /// <returns></returns>
        public LuaSyntaxOptions With(
            Option<bool> acceptBinaryNumbers = default,
            Option<bool> acceptCCommentSyntax = default,
            Option<bool> acceptCompoundAssignment = default,
            Option<bool> acceptEmptyStatements = default,
            Option<bool> acceptCBooleanOperators = default,
            Option<bool> acceptGoto = default,
            Option<bool> acceptHexEscapesInStrings = default,
            Option<bool> acceptHexFloatLiterals = default,
            Option<bool> acceptOctalNumbers = default,
            Option<bool> acceptShebang = default,
            Option<bool> acceptUnderscoreInNumberLiterals = default,
            Option<bool> useLuaJitIdentifierRules = default,
            Option<bool> acceptBitwiseOperators = default,
            Option<bool> acceptWhitespaceEscape = default,
            Option<bool> acceptUnicodeEscape = default,
            Option<ContinueType> continueType = default) =>
            new LuaSyntaxOptions(
                acceptBinaryNumbers.UnwrapOr(AcceptBinaryNumbers),
                acceptCCommentSyntax.UnwrapOr(AcceptCCommentSyntax),
                acceptCompoundAssignment.UnwrapOr(AcceptCompoundAssignment),
                acceptEmptyStatements.UnwrapOr(AcceptEmptyStatements),
                acceptCBooleanOperators.UnwrapOr(AcceptCBooleanOperators),
                acceptGoto.UnwrapOr(AcceptGoto),
                acceptHexEscapesInStrings.UnwrapOr(AcceptHexEscapesInStrings),
                acceptHexFloatLiterals.UnwrapOr(AcceptHexFloatLiterals),
                acceptOctalNumbers.UnwrapOr(AcceptOctalNumbers),
                acceptShebang.UnwrapOr(AcceptShebang),
                acceptUnderscoreInNumberLiterals.UnwrapOr(AcceptUnderscoreInNumberLiterals),
                useLuaJitIdentifierRules.UnwrapOr(UseLuaJitIdentifierRules),
                acceptBitwiseOperators.UnwrapOr(AcceptBitwiseOperators),
                acceptWhitespaceEscape.UnwrapOr(AcceptWhitespaceEscape),
                acceptUnicodeEscape.UnwrapOr(AcceptUnicodeEscape),
                continueType.UnwrapOr(ContinueType));

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            Equals(obj as LuaSyntaxOptions);

        /// <inheritdoc/>
        public bool Equals(LuaSyntaxOptions? other)
            => (object) this == other
            || (other != null
                && AcceptBinaryNumbers == other.AcceptBinaryNumbers
                && AcceptCCommentSyntax == other.AcceptCCommentSyntax
                && AcceptCompoundAssignment == other.AcceptCompoundAssignment
                && AcceptEmptyStatements == other.AcceptEmptyStatements
                && AcceptCBooleanOperators == other.AcceptCBooleanOperators
                && AcceptGoto == other.AcceptGoto
                && AcceptHexEscapesInStrings == other.AcceptHexEscapesInStrings
                && AcceptHexFloatLiterals == other.AcceptHexFloatLiterals
                && AcceptOctalNumbers == other.AcceptOctalNumbers
                && AcceptShebang == other.AcceptShebang
                && AcceptUnderscoreInNumberLiterals == other.AcceptUnderscoreInNumberLiterals
                && UseLuaJitIdentifierRules == other.UseLuaJitIdentifierRules
                && AcceptBitwiseOperators == other.AcceptBitwiseOperators
                && AcceptWhitespaceEscape == other.AcceptWhitespaceEscape
                && ContinueType == other.ContinueType);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(AcceptBinaryNumbers);
            hash.Add(AcceptCCommentSyntax);
            hash.Add(AcceptCompoundAssignment);
            hash.Add(AcceptEmptyStatements);
            hash.Add(AcceptCBooleanOperators);
            hash.Add(AcceptGoto);
            hash.Add(AcceptHexEscapesInStrings);
            hash.Add(AcceptHexFloatLiterals);
            hash.Add(AcceptOctalNumbers);
            hash.Add(AcceptShebang);
            hash.Add(AcceptUnderscoreInNumberLiterals);
            hash.Add(UseLuaJitIdentifierRules);
            hash.Add(AcceptBitwiseOperators);
            hash.Add(AcceptWhitespaceEscape);
            hash.Add(ContinueType);
            return hash.ToHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (this == Lua51)
            {
                return "Lua 5.1";
            }
            else if (this == Lua52)
            {
                return "Lua 5.2";
            }
            else if (this == Lua53)
            {
                return "Lua 5.3";
            }
            else if (this == LuaJIT20)
            {
                return "LuaJIT";
            }
            else if (this == GMod)
            {
                return "GLua";
            }
            else if (this == Roblox)
            {
                return "Roblox";
            }
            else if (this == All)
            {
                return "All";
            }
            else
            {
                return $"{{ AcceptBinaryNumbers = {AcceptBinaryNumbers}, AcceptCCommentSyntax = {AcceptCCommentSyntax}, AcceptCompoundAssignment = {AcceptCompoundAssignment}, AcceptEmptyStatements = {AcceptEmptyStatements}, AcceptCBooleanOperators = {AcceptCBooleanOperators}, AcceptGoto = {AcceptGoto}, AcceptHexEscapesInStrings = {AcceptHexEscapesInStrings}, AcceptHexFloatLiterals = {AcceptHexFloatLiterals}, AcceptOctalNumbers = {AcceptOctalNumbers}, AcceptShebang = {AcceptShebang}, AcceptUnderscoreInNumberLiterals = {AcceptUnderscoreInNumberLiterals}, UseLuaJitIdentifierRules = {UseLuaJitIdentifierRules}, AcceptBitwiseOperators = {AcceptBitwiseOperators}, AcceptWhitespaceEscape = {AcceptWhitespaceEscape}, ContinueType = {ContinueType} }}";
            }
        }

        /// <summary>
        /// Checks whether two lua option sets are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(LuaSyntaxOptions? left, LuaSyntaxOptions? right)
        {
            if (right is null) return left is null;
            return left == (object) right || right.Equals(left);
        }

        /// <summary>
        /// Checks whether two lua option sets are not equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(LuaSyntaxOptions? left, LuaSyntaxOptions? right) =>
            !(left == right);
    }
}