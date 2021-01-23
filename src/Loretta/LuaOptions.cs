using System;
using System.Collections.Immutable;
using Tsu;

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
        Keyword,

        /// <summary>
        /// Continue is a contextual keyword (is only a keyword when used as a statement).
        /// </summary>
        ContextualKeyword
    }

    /// <summary>
    /// The options used by Loretta to adapt to the flavor of lua being parsed.
    /// </summary>
    public class LuaOptions : IEquatable<LuaOptions?>
    {
        /// <summary>
        /// The Lua 5.1 preset.
        /// </summary>
        public static readonly LuaOptions Lua51 = new LuaOptions (
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
            acceptCBooleanOperators: false,
            acceptGoto: true,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: true,
            acceptOctalNumbers: false,
            acceptShebang: false,
            acceptUnderscoreInNumberLiterals: false,
            useLuaJitIdentifierRules: true,
            continueType: ContinueType.None );

        /// <summary>
        /// The GLua preset.
        /// </summary>
        public static readonly LuaOptions GMod = LuaJIT.With (
            acceptCCommentSyntax: true,
            acceptCBooleanOperators: true,
            continueType: ContinueType.Keyword );

        /// <summary>
        /// The Luau preset.
        /// </summary>
        public static readonly LuaOptions Roblox = new LuaOptions (
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
            acceptCBooleanOperators: true,
            acceptGoto: true,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: true,
            acceptOctalNumbers: true,
            acceptShebang: true,
            acceptUnderscoreInNumberLiterals: true,
            useLuaJitIdentifierRules: true,
            continueType: ContinueType.ContextualKeyword );

        /// <summary>
        /// All presets that are preconfigured in <see cref="LuaOptions"/>.
        /// </summary>
        public static ImmutableArray<LuaOptions> AllPresets { get; } = ImmutableArray.Create ( new[]
        {
            Lua51,
            Lua52,
            LuaJIT,
            GMod,
            Roblox,
            All
        } );

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
        /// <param name="continueType"><inheritdoc cref="ContinueType" path="/summary" /></param>
        public LuaOptions (
            Boolean acceptBinaryNumbers,
            Boolean acceptCCommentSyntax,
            Boolean acceptCompoundAssignment,
            Boolean acceptEmptyStatements,
            Boolean acceptCBooleanOperators,
            Boolean acceptGoto,
            Boolean acceptHexEscapesInStrings,
            Boolean acceptHexFloatLiterals,
            Boolean acceptOctalNumbers,
            Boolean acceptShebang,
            Boolean acceptUnderscoreInNumberLiterals,
            Boolean useLuaJitIdentifierRules,
            ContinueType continueType )
        {
            this.AcceptBinaryNumbers = acceptBinaryNumbers;
            this.AcceptCCommentSyntax = acceptCCommentSyntax;
            this.AcceptCompoundAssignment = acceptCompoundAssignment;
            this.AcceptEmptyStatements = acceptEmptyStatements;
            this.AcceptCBooleanOperators = acceptCBooleanOperators;
            this.AcceptGoto = acceptGoto;
            this.AcceptHexEscapesInStrings = acceptHexEscapesInStrings;
            this.AcceptHexFloatLiterals = acceptHexFloatLiterals;
            this.AcceptOctalNumbers = acceptOctalNumbers;
            this.AcceptShebang = acceptShebang;
            this.AcceptUnderscoreInNumberLiterals = acceptUnderscoreInNumberLiterals;
            this.UseLuaJitIdentifierRules = useLuaJitIdentifierRules;
            this.ContinueType = continueType;
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
        /// Whether to accept the C boolean operators (&amp;&amp;, ||, != and !).
        /// </summary>
        public Boolean AcceptCBooleanOperators { get; }

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
        /// Whether to accept underscores in any number literals (will be ignored when parsing the number).
        /// </summary>
        public Boolean AcceptUnderscoreInNumberLiterals { get; }

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
        /// <param name="continueType">
        /// <inheritdoc cref="ContinueType" path="/summary" /> If None uses the value of <see
        /// cref="ContinueType" />.
        /// </param>
        /// <returns></returns>
        public LuaOptions With (
            Option<Boolean> acceptBinaryNumbers = default,
            Option<Boolean> acceptCCommentSyntax = default,
            Option<Boolean> acceptCompoundAssignment = default,
            Option<Boolean> acceptEmptyStatements = default,
            Option<Boolean> acceptCBooleanOperators = default,
            Option<Boolean> acceptGoto = default,
            Option<Boolean> acceptHexEscapesInStrings = default,
            Option<Boolean> acceptHexFloatLiterals = default,
            Option<Boolean> acceptOctalNumbers = default,
            Option<Boolean> acceptShebang = default,
            Option<Boolean> acceptUnderscoreInNumberLiterals = default,
            Option<Boolean> useLuaJitIdentifierRules = default,
            Option<ContinueType> continueType = default ) =>
            new LuaOptions (
                acceptBinaryNumbers.UnwrapOr ( this.AcceptBinaryNumbers ),
                acceptCCommentSyntax.UnwrapOr ( this.AcceptCCommentSyntax ),
                acceptCompoundAssignment.UnwrapOr ( this.AcceptCompoundAssignment ),
                acceptEmptyStatements.UnwrapOr ( this.AcceptEmptyStatements ),
                acceptCBooleanOperators.UnwrapOr ( this.AcceptCBooleanOperators ),
                acceptGoto.UnwrapOr ( this.AcceptGoto ),
                acceptHexEscapesInStrings.UnwrapOr ( this.AcceptHexEscapesInStrings ),
                acceptHexFloatLiterals.UnwrapOr ( this.AcceptHexFloatLiterals ),
                acceptOctalNumbers.UnwrapOr ( this.AcceptOctalNumbers ),
                acceptShebang.UnwrapOr ( this.AcceptShebang ),
                acceptUnderscoreInNumberLiterals.UnwrapOr ( this.AcceptUnderscoreInNumberLiterals ),
                useLuaJitIdentifierRules.UnwrapOr ( this.UseLuaJitIdentifierRules ),
                continueType.UnwrapOr ( this.ContinueType ) );

        /// <inheritdoc/>
        public override Boolean Equals ( Object? obj ) =>
            this.Equals ( obj as LuaOptions );

        /// <inheritdoc/>
        public Boolean Equals ( LuaOptions? other ) =>
            other != null
            && this.AcceptBinaryNumbers == other.AcceptBinaryNumbers
            && this.AcceptCCommentSyntax == other.AcceptCCommentSyntax
            && this.AcceptCompoundAssignment == other.AcceptCompoundAssignment
            && this.AcceptEmptyStatements == other.AcceptEmptyStatements
            && this.AcceptCBooleanOperators == other.AcceptCBooleanOperators
            && this.AcceptGoto == other.AcceptGoto
            && this.AcceptHexEscapesInStrings == other.AcceptHexEscapesInStrings
            && this.AcceptHexFloatLiterals == other.AcceptHexFloatLiterals
            && this.AcceptOctalNumbers == other.AcceptOctalNumbers
            && this.AcceptShebang == other.AcceptShebang
            && this.AcceptUnderscoreInNumberLiterals == other.AcceptUnderscoreInNumberLiterals
            && this.UseLuaJitIdentifierRules == other.UseLuaJitIdentifierRules
            && this.ContinueType == other.ContinueType;

        /// <inheritdoc/>
        public override Int32 GetHashCode ( )
        {
            var hash = new HashCode ( );
            hash.Add ( this.AcceptBinaryNumbers );
            hash.Add ( this.AcceptCCommentSyntax );
            hash.Add ( this.AcceptCompoundAssignment );
            hash.Add ( this.AcceptEmptyStatements );
            hash.Add ( this.AcceptCBooleanOperators );
            hash.Add ( this.AcceptGoto );
            hash.Add ( this.AcceptHexEscapesInStrings );
            hash.Add ( this.AcceptHexFloatLiterals );
            hash.Add ( this.AcceptOctalNumbers );
            hash.Add ( this.AcceptShebang );
            hash.Add ( this.AcceptUnderscoreInNumberLiterals );
            hash.Add ( this.UseLuaJitIdentifierRules );
            hash.Add ( this.ContinueType );
            return hash.ToHashCode ( );
        }

        /// <inheritdoc/>
        public override String ToString ( )
        {
            if ( this == Lua51 )
            {
                return "Lua 5.1";
            }
            else if ( this == Lua52 )
            {
                return "Lua 5.2";
            }
            else if ( this == LuaJIT )
            {
                return "LuaJIT";
            }
            else if ( this == GMod )
            {
                return "GLua";
            }
            else if ( this == Roblox )
            {
                return "Roblox";
            }
            else
            {
                return $"{{ AcceptBinaryNumbers = {this.AcceptBinaryNumbers}, AcceptCCommentSyntax = {this.AcceptCCommentSyntax}, AcceptCompoundAssignment = {this.AcceptCompoundAssignment}, AcceptEmptyStatements = {this.AcceptEmptyStatements}, AcceptCBooleanOperators = {this.AcceptCBooleanOperators}, AcceptGoto = {this.AcceptGoto}, AcceptHexEscapesInStrings = {this.AcceptHexEscapesInStrings}, AcceptHexFloatLiterals = {this.AcceptHexFloatLiterals}, AcceptOctalNumbers = {this.AcceptOctalNumbers}, AcceptShebang = {this.AcceptShebang}, AcceptUnderscoreInNumberLiterals = {this.AcceptUnderscoreInNumberLiterals}, UseLuaJitIdentifierRules = {this.UseLuaJitIdentifierRules}, ContinueType = {this.ContinueType} }}";
            }
        }

        /// <summary>
        /// Checks whether two lua option sets are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Boolean operator == ( LuaOptions? left, LuaOptions? right )
        {
            if ( right is null ) return left is null;
            return right.Equals ( left );
        }

        /// <summary>
        /// Checks whether two lua option sets are not equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Boolean operator != ( LuaOptions? left, LuaOptions? right ) =>
            !( left == right );
    }
}