using System;
using Loretta.Lexing;

namespace Loretta
{
    /// <summary>
    /// The type of continue the lua flavor being parsed has.
    /// </summary>
    public enum ContinueType
    {
        /// <summary>
        /// No continue.
        /// </summary>
        None,

        /// <summary>
        /// Continue is a keyword.
        /// </summary>
        Keyword = LuaTokenType.Keyword,

        /// <summary>
        /// Continue is a contextual keyword (is only a keyword when used as a statement).
        /// </summary>
        ContextualKeyword = LuaTokenType.Identifier
    }

    /// <summary>
    /// The options used by Loretta to adapt to the flavor of lua being parsed.
    /// </summary>
    public class LuaOptions
    {
        /// <summary>
        /// The Lua 5.1 preset.
        /// </summary>
        public static readonly LuaOptions Lua51 = new LuaOptions (
            acceptBinaryNumbers: false,
            acceptCCommentSyntax: false,
            acceptCompoundAssignment: false,
            acceptEmptyStatements: false,
            acceptGModCOperators: false,
            acceptGoto: false,
            acceptHexEscapesInStrings: false,
            acceptHexFloatLiterals: false,
            acceptOctalNumbers: false,
            acceptShebang: false,
            acceptUnderlineInNumberLiterals: false,
            useLuaJitIdentifierRules: false,
            continueType: ContinueType.None );

        /// <summary>
        /// The Lua 5.2 preset.
        /// </summary>
        public static readonly LuaOptions Lua52 = Lua51.With (
            acceptEmptyStatements: true,
            acceptGoto: true,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: true );

        /// <summary>
        /// The LuaJIT preset.
        /// </summary>
        public static readonly LuaOptions LuaJIT = new LuaOptions (
            acceptBinaryNumbers: false,
            acceptCCommentSyntax: false,
            acceptCompoundAssignment: false,
            acceptEmptyStatements: true,
            acceptGModCOperators: false,
            acceptGoto: true,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: true,
            acceptOctalNumbers: false,
            acceptShebang: false,
            acceptUnderlineInNumberLiterals: false,
            useLuaJitIdentifierRules: true,
            continueType: ContinueType.None );

        /// <summary>
        /// The GLua preset.
        /// </summary>
        public static readonly LuaOptions GMod = LuaJIT.With (
            acceptCCommentSyntax: true,
            acceptGModCOperators: true,
            continueType: ContinueType.Keyword );

        /// <summary>
        /// The Luau preset.
        /// </summary>
        public static readonly LuaOptions Roblox = new LuaOptions (
            acceptBinaryNumbers: true,
            acceptCCommentSyntax: false,
            acceptCompoundAssignment: true,
            acceptEmptyStatements: false,
            acceptGModCOperators: false,
            acceptGoto: false,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: true,
            acceptOctalNumbers: false,
            acceptShebang: false,
            acceptUnderlineInNumberLiterals: true,
            useLuaJitIdentifierRules: false,
            continueType: ContinueType.ContextualKeyword );

        /// <summary>
        /// The preset that sets everything to true and continue to <see
        /// cref="ContinueType.ContextualKeyword" />.
        /// </summary>
        public static readonly LuaOptions All = new LuaOptions (
            acceptBinaryNumbers: true,
            acceptCCommentSyntax: true,
            acceptCompoundAssignment: true,
            acceptEmptyStatements: true,
            acceptGModCOperators: true,
            acceptGoto: true,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: true,
            acceptOctalNumbers: true,
            acceptShebang: true,
            acceptUnderlineInNumberLiterals: true,
            useLuaJitIdentifierRules: true,
            continueType: ContinueType.ContextualKeyword );

        /// <summary>
        /// Whether to accept binary numbers (format: /0b[10]+/).
        /// </summary>
        public Boolean AcceptBinaryNumbers { get; }

        /// <summary>
        /// Whether to accept C comment syntax (formats: "//..." and "/* ... */").
        /// </summary>
        public Boolean AcceptCCommentSyntax { get; }

        /// <summary>
        /// Whether to accept compound assignment syntax (format: &lt;expr&gt; ("+=" | "-=" | "*=" |
        /// "/=" | "^=" | "%=" | "..=") &lt;expr&gt;).
        /// </summary>
        public Boolean AcceptCompoundAssignment { get; }

        /// <summary>
        /// Whether to accept empty statements (lone semicolons).
        /// </summary>
        public Boolean AcceptEmptyStatements { get; }

        /// <summary>
        /// Whether to accept the C operators addde in GLua (formats: "&amp;&amp;", "||", "!=", "!").
        /// </summary>
        public Boolean AcceptGModCOperators { get; }

        /// <summary>
        /// Whether to accept goto labels and statements.
        /// </summary>
        public Boolean AcceptGoto { get; }

        /// <summary>
        /// Whether to accept hexadecimal escapes in strings.
        /// </summary>
        public Boolean AcceptHexEscapesInStrings { get; }

        /// <summary>
        /// Whether to accept hexadecimal floating point literals (format: /0x[a-fA-F0-9]+(\.[a-fA-F0-9])?([+-]?p[0-9]+)/).
        /// </summary>
        public Boolean AcceptHexFloatLiterals { get; }

        /// <summary>
        /// Whether to accept octal numbers (format: /0o[0-7]+/).
        /// </summary>
        public Boolean AcceptOctalNumbers { get; }

        /// <summary>
        /// Whether to accept shebangs (format: "#!...") (currently accepted anywhere inside the file).
        /// </summary>
        public Boolean AcceptShebang { get; }

        /// <summary>
        /// Whether to accept underlines in any number literals (will be ignored when parsing the number).
        /// </summary>
        public Boolean AcceptUnderlineInNumberLiterals { get; }

        /// <summary>
        /// Whether to use LuaJIT's identifier character rules (accepts any character greater than
        /// or equal to 0xF7).
        /// </summary>
        public Boolean UseLuaJitIdentifierRules { get; }

        /// <summary>
        /// The type of continue to be recognized by the parser.
        /// </summary>
        public ContinueType ContinueType { get; }

        /// <summary>
        /// Initializes a new lua options set.
        /// </summary>
        /// <param name="acceptBinaryNumbers">
        /// <inheritdoc cref="AcceptBinaryNumbers" path="/summary" />
        /// </param>
        /// <param name="acceptCCommentSyntax">
        /// <inheritdoc cref="AcceptCCommentSyntax" path="/summary" />
        /// </param>
        /// <param name="acceptCompoundAssignment">
        /// <inheritdoc cref="AcceptCompoundAssignment" path="/summary" />
        /// </param>
        /// <param name="acceptEmptyStatements">
        /// <inheritdoc cref="AcceptEmptyStatements" path="/summary" />
        /// </param>
        /// <param name="acceptGModCOperators">
        /// <inheritdoc cref="AcceptGModCOperators" path="/summary" />
        /// </param>
        /// <param name="acceptGoto"><inheritdoc cref="AcceptGoto" path="/summary" /></param>
        /// <param name="acceptHexEscapesInStrings">
        /// <inheritdoc cref="AcceptHexEscapesInStrings" path="/summary" />
        /// </param>
        /// <param name="acceptHexFloatLiterals">
        /// <inheritdoc cref="AcceptHexFloatLiterals" path="/summary" />
        /// </param>
        /// <param name="acceptOctalNumbers">
        /// <inheritdoc cref="AcceptOctalNumbers" path="/summary" />
        /// </param>
        /// <param name="acceptShebang"><inheritdoc cref="AcceptShebang" path="/summary" /></param>
        /// <param name="acceptUnderlineInNumberLiterals">
        /// <inheritdoc cref="AcceptUnderlineInNumberLiterals" path="/summary" />
        /// </param>
        /// <param name="useLuaJitIdentifierRules">
        /// <inheritdoc cref="UseLuaJitIdentifierRules" path="/summary" />
        /// </param>
        /// <param name="continueType"><inheritdoc cref="ContinueType" path="/summary" /></param>
        public LuaOptions (
            Boolean acceptBinaryNumbers,
            Boolean acceptCCommentSyntax,
            Boolean acceptCompoundAssignment,
            Boolean acceptEmptyStatements,
            Boolean acceptGModCOperators,
            Boolean acceptGoto,
            Boolean acceptHexEscapesInStrings,
            Boolean acceptHexFloatLiterals,
            Boolean acceptOctalNumbers,
            Boolean acceptShebang,
            Boolean acceptUnderlineInNumberLiterals,
            Boolean useLuaJitIdentifierRules,
            ContinueType continueType )
        {
            this.AcceptBinaryNumbers = acceptBinaryNumbers;
            this.AcceptCCommentSyntax = acceptCCommentSyntax;
            this.AcceptCompoundAssignment = acceptCompoundAssignment;
            this.AcceptEmptyStatements = acceptEmptyStatements;
            this.AcceptGModCOperators = acceptGModCOperators;
            this.AcceptGoto = acceptGoto;
            this.AcceptHexEscapesInStrings = acceptHexEscapesInStrings;
            this.AcceptHexFloatLiterals = acceptHexFloatLiterals;
            this.AcceptOctalNumbers = acceptOctalNumbers;
            this.AcceptShebang = acceptShebang;
            this.AcceptUnderlineInNumberLiterals = acceptUnderlineInNumberLiterals;
            this.UseLuaJitIdentifierRules = useLuaJitIdentifierRules;
            this.ContinueType = continueType;
        }

        /// <summary>
        /// Creates a new lua options changing the provided fields.
        /// </summary>
        /// <param name="acceptBinaryNumbers">
        /// <inheritdoc cref="AcceptBinaryNumbers" path="/summary" /> If null uses the value of <see
        /// cref="AcceptBinaryNumbers" />.
        /// </param>
        /// <param name="acceptCCommentSyntax">
        /// <inheritdoc cref="AcceptCCommentSyntax" path="/summary" /> If null uses the value of
        /// <see cref="AcceptCCommentSyntax" />.
        /// </param>
        /// <param name="acceptCompoundAssignment">
        /// <inheritdoc cref="AcceptCompoundAssignment" path="/summary" /> If null uses the value of
        /// <see cref="AcceptCompoundAssignment" />.
        /// </param>
        /// <param name="acceptEmptyStatements">
        /// <inheritdoc cref="AcceptEmptyStatements" path="/summary" /> If null uses the value of
        /// <see cref="AcceptEmptyStatements" />.
        /// </param>
        /// <param name="acceptGModCOperators">
        /// <inheritdoc cref="AcceptGModCOperators" path="/summary" /> If null uses the value of
        /// <see cref="AcceptGModCOperators" />.
        /// </param>
        /// <param name="acceptGoto">
        /// <inheritdoc cref="AcceptGoto" path="/summary" /> If null uses the value of <see
        /// cref="AcceptGoto" />.
        /// </param>
        /// <param name="acceptHexEscapesInStrings">
        /// <inheritdoc cref="AcceptHexEscapesInStrings" path="/summary" /> If null uses the value
        /// of <see cref="AcceptHexEscapesInStrings" />.
        /// </param>
        /// <param name="acceptHexFloatLiterals">
        /// <inheritdoc cref="AcceptHexFloatLiterals" path="/summary" /> If null uses the value of
        /// <see cref="AcceptHexFloatLiterals" />.
        /// </param>
        /// <param name="acceptOctalNumbers">
        /// <inheritdoc cref="AcceptOctalNumbers" path="/summary" /> If null uses the value of <see
        /// cref="AcceptOctalNumbers" />.
        /// </param>
        /// <param name="acceptShebang">
        /// <inheritdoc cref="AcceptShebang" path="/summary" /> If null uses the value of <see
        /// cref="AcceptShebang" />.
        /// </param>
        /// <param name="acceptUnderlineInNumberLiterals">
        /// <inheritdoc cref="AcceptUnderlineInNumberLiterals" path="/summary" /> If null uses the
        /// value of <see cref="AcceptUnderlineInNumberLiterals" />.
        /// </param>
        /// <param name="useLuaJitIdentifierRules">
        /// <inheritdoc cref="UseLuaJitIdentifierRules" path="/summary" /> If null uses the value of
        /// <see cref="UseLuaJitIdentifierRules" />.
        /// </param>
        /// <param name="continueType">
        /// <inheritdoc cref="ContinueType" path="/summary" /> If null uses the value of <see
        /// cref="ContinueType" />.
        /// </param>
        /// <returns></returns>
        public LuaOptions With (
            Boolean? acceptBinaryNumbers = null,
            Boolean? acceptCCommentSyntax = null,
            Boolean? acceptCompoundAssignment = null,
            Boolean? acceptEmptyStatements = null,
            Boolean? acceptGModCOperators = null,
            Boolean? acceptGoto = null,
            Boolean? acceptHexEscapesInStrings = null,
            Boolean? acceptHexFloatLiterals = null,
            Boolean? acceptOctalNumbers = null,
            Boolean? acceptShebang = null,
            Boolean? acceptUnderlineInNumberLiterals = null,
            Boolean? useLuaJitIdentifierRules = null,
            ContinueType? continueType = null ) =>
            new LuaOptions (
                acceptBinaryNumbers ?? this.AcceptBinaryNumbers,
                acceptCCommentSyntax ?? this.AcceptCCommentSyntax,
                acceptCompoundAssignment ?? this.AcceptCompoundAssignment,
                acceptEmptyStatements ?? this.AcceptEmptyStatements,
                acceptGModCOperators ?? this.AcceptGModCOperators,
                acceptGoto ?? this.AcceptGoto,
                acceptHexEscapesInStrings ?? this.AcceptHexEscapesInStrings,
                acceptHexFloatLiterals ?? this.AcceptHexFloatLiterals,
                acceptOctalNumbers ?? this.AcceptOctalNumbers,
                acceptShebang ?? this.AcceptShebang,
                acceptUnderlineInNumberLiterals ?? this.AcceptUnderlineInNumberLiterals,
                useLuaJitIdentifierRules ?? this.UseLuaJitIdentifierRules,
                continueType ?? this.ContinueType );
    }
}