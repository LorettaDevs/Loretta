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
        public static readonly LuaSyntaxOptions Lua51 = new(
            acceptBinaryNumbers: false,
            acceptCCommentSyntax: false,
            acceptCompoundAssignment: false,
            acceptEmptyStatements: false,
            acceptCBooleanOperators: false,
            acceptGoto: false,
            acceptHexEscapesInStrings: false,
            acceptHexFloatLiterals: false,
            acceptOctalNumbers: false,
            acceptShebang: true,
            acceptUnderscoreInNumberLiterals: false,
            useLuaJitIdentifierRules: false,
            acceptBitwiseOperators: false,
            acceptWhitespaceEscape: false,
            acceptUnicodeEscape: false,
            continueType: ContinueType.None,
            acceptIfExpression: false,
            acceptHashStrings: false,
            acceptInvalidEscapes: true,
            acceptLocalVariableAttributes: false,
            binaryIntegerFormat: IntegerFormats.NotSupported,
            octalIntegerFormat: IntegerFormats.NotSupported,
            decimalIntegerFormat: IntegerFormats.NotSupported,
            hexIntegerFormat: IntegerFormats.NotSupported,
            acceptTypedLua: false,
            acceptFloorDivision: false,
            acceptLuaJITNumberSuffixes: false,
            acceptNestingOfLongStrings: false);

        /// <summary>
        /// The Lua 5.2 preset.
        /// </summary>
        public static readonly LuaSyntaxOptions Lua52 = Lua51.With(
            acceptEmptyStatements: true,
            acceptGoto: true,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: true,
            acceptWhitespaceEscape: true,
            acceptInvalidEscapes: false,
            acceptNestingOfLongStrings: true);

        /// <summary>
        /// The Lua 5.3 preset.
        /// </summary>
        public static readonly LuaSyntaxOptions Lua53 = Lua52.With(
            acceptBitwiseOperators: true,
            acceptUnicodeEscape: true,
            decimalIntegerFormat: IntegerFormats.Int64,
            hexIntegerFormat: IntegerFormats.Int64,
            acceptFloorDivision: true);

        /// <summary>
        /// The Lua 5.4 preset.
        /// </summary>
        public static readonly LuaSyntaxOptions Lua54 = Lua53.With(
            acceptLocalVariableAttributes: true);

        /// <summary>
        /// The LuaJIT 2.0 preset.
        /// </summary>
        public static readonly LuaSyntaxOptions LuaJIT20 = new(
            acceptBinaryNumbers: false,
            acceptCCommentSyntax: false,
            acceptCompoundAssignment: false,
            acceptEmptyStatements: false,
            acceptCBooleanOperators: false,
            acceptGoto: true,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: true,
            acceptOctalNumbers: false,
            acceptShebang: true,
            acceptUnderscoreInNumberLiterals: false,
            useLuaJitIdentifierRules: true,
            acceptBitwiseOperators: false,
            acceptWhitespaceEscape: true,
            acceptUnicodeEscape: false,
            continueType: ContinueType.None,
            acceptIfExpression: false,
            acceptHashStrings: false,
            acceptInvalidEscapes: false,
            acceptLocalVariableAttributes: false,
            binaryIntegerFormat: IntegerFormats.NotSupported,
            octalIntegerFormat: IntegerFormats.NotSupported,
            decimalIntegerFormat: IntegerFormats.NotSupported,
            hexIntegerFormat: IntegerFormats.NotSupported,
            acceptTypedLua: false,
            acceptFloorDivision: false,
            acceptLuaJITNumberSuffixes: true,
            acceptNestingOfLongStrings: true);

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
        public static readonly LuaSyntaxOptions Luau = new(
            acceptBinaryNumbers: true,
            acceptCCommentSyntax: false,
            acceptCompoundAssignment: true,
            acceptEmptyStatements: false,
            acceptCBooleanOperators: false,
            acceptGoto: false,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: false,
            acceptOctalNumbers: false,
            acceptShebang: true,
            acceptUnderscoreInNumberLiterals: true,
            useLuaJitIdentifierRules: false,
            acceptBitwiseOperators: false,
            acceptWhitespaceEscape: true,
            acceptUnicodeEscape: true,
            continueType: ContinueType.ContextualKeyword,
            acceptIfExpression: true,
            acceptHashStrings: false,
            acceptInvalidEscapes: true,
            acceptLocalVariableAttributes: false,
            // Luau parses binary as a long and then converts it to a double
            binaryIntegerFormat: IntegerFormats.Double,
            octalIntegerFormat: IntegerFormats.NotSupported,
            // Luau always parses decimals as doubles
            decimalIntegerFormat: IntegerFormats.NotSupported,
            // Luau parses hex as a long and then converts it to a double
            hexIntegerFormat: IntegerFormats.Double,
            acceptTypedLua: true,
            acceptFloorDivision: true,
            acceptLuaJITNumberSuffixes: false,
            acceptNestingOfLongStrings: true);

        /// <summary>
        /// The Luau preset.
        /// </summary>
        public static readonly LuaSyntaxOptions Roblox = Luau;

        /// <summary>
        /// The FiveM preset.
        /// </summary>
        public static readonly LuaSyntaxOptions FiveM = Lua53.With(
            acceptHashStrings: true);

        /// <summary>
        /// The preset that sets everything to true,
        /// continue to <see cref="ContinueType.ContextualKeyword" />
        /// and integer options to <see cref="IntegerFormats.NotSupported"/>.
        /// </summary>
        public static readonly LuaSyntaxOptions All = new(
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
            continueType: ContinueType.ContextualKeyword,
            acceptIfExpression: true,
            acceptHashStrings: true,
            acceptInvalidEscapes: false,
            acceptLocalVariableAttributes: true,
            binaryIntegerFormat: IntegerFormats.NotSupported,
            octalIntegerFormat: IntegerFormats.NotSupported,
            decimalIntegerFormat: IntegerFormats.NotSupported,
            hexIntegerFormat: IntegerFormats.NotSupported,
            acceptTypedLua: true,
            acceptFloorDivision: false,
            acceptLuaJITNumberSuffixes: true,
            acceptNestingOfLongStrings: true);

        /// <summary>
        /// Same as <see cref="All"/> but with integer settings set
        /// to <see cref="IntegerFormats.Int64"/>,
        /// <see cref="AcceptFloorDivision"/> set to <see langword="true"/>
        /// and <see cref="AcceptCCommentSyntax"/> set to <see langword="false"/>.
        /// </summary>
        public static readonly LuaSyntaxOptions AllWithIntegers = All.With(
            acceptCCommentSyntax: false,
            binaryIntegerFormat: IntegerFormats.Int64,
            octalIntegerFormat: IntegerFormats.Int64,
            decimalIntegerFormat: IntegerFormats.Int64,
            hexIntegerFormat: IntegerFormats.Int64,
            acceptFloorDivision: true);

        /// <summary>
        /// All presets that are preconfigured in <see cref="LuaSyntaxOptions"/>.
        /// </summary>
        public static ImmutableArray<LuaSyntaxOptions> AllPresets { get; } = ImmutableArray.Create(new[]
        {
            Lua51,
            Lua52,
            Lua53,
            Lua54,
            LuaJIT20,
            LuaJIT21,
            GMod,
            Luau,
            FiveM,
            All,
            AllWithIntegers,
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
        /// <param name="acceptIfExpression"><inheritdoc cref="AcceptIfExpressions" path="/summary" /></param>
        /// <param name="acceptHashStrings"><inheritdoc cref="AcceptHashStrings" path="/summary" /></param>
        /// <param name="acceptInvalidEscapes"><inheritdoc cref="AcceptInvalidEscapes" path="/summary" /></param>
        /// <param name="acceptLocalVariableAttributes"><inheritdoc cref="AcceptLocalVariableAttributes" path="/summary" /></param>
        /// <param name="binaryIntegerFormat"><inheritdoc cref="BinaryIntegerFormat" path="/summary" /></param>
        /// <param name="octalIntegerFormat"><inheritdoc cref="OctalIntegerFormat" path="/summary" /></param>
        /// <param name="decimalIntegerFormat"><inheritdoc cref="DecimalIntegerFormat" path="/summary" /></param>
        /// <param name="hexIntegerFormat"><inheritdoc cref="HexIntegerFormat" path="/summary" /></param>
        /// <param name="acceptTypedLua"><inheritdoc cref="AcceptTypedLua" path="/summary" /></param>
        /// <param name="acceptFloorDivision"><inheritdoc cref="AcceptFloorDivision" path="/summary" /></param>
        /// <param name="acceptLuaJITNumberSuffixes"><inheritdoc cref="AcceptLuaJITNumberSuffixes" path="/summary" /></param>
        /// <param name="acceptNestingOfLongStrings"><inheritdoc cref="AcceptNestingOfLongStrings" path="/summary" /></param>
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
            ContinueType continueType,
            bool acceptIfExpression,
            bool acceptHashStrings,
            bool acceptInvalidEscapes,
            bool acceptLocalVariableAttributes,
            IntegerFormats binaryIntegerFormat,
            IntegerFormats octalIntegerFormat,
            IntegerFormats decimalIntegerFormat,
            IntegerFormats hexIntegerFormat,
            bool acceptTypedLua,
            bool acceptFloorDivision,
            bool acceptLuaJITNumberSuffixes,
            bool acceptNestingOfLongStrings)
        {
            if (acceptFloorDivision && acceptCCommentSyntax)
            {
                throw new ArgumentException("AcceptFloorDivision and AcceptCCommentSyntax cannot be enabled simultaneously.");
            }

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
            AcceptIfExpressions = acceptIfExpression;
            AcceptHashStrings = acceptHashStrings;
            AcceptInvalidEscapes = acceptInvalidEscapes;
            AcceptLocalVariableAttributes = acceptLocalVariableAttributes;
            BinaryIntegerFormat = binaryIntegerFormat;
            OctalIntegerFormat = octalIntegerFormat;
            DecimalIntegerFormat = decimalIntegerFormat;
            HexIntegerFormat = hexIntegerFormat;
            AcceptTypedLua = acceptTypedLua;
            AcceptFloorDivision = acceptFloorDivision;
            AcceptLuaJITNumberSuffixes = acceptLuaJITNumberSuffixes;
            AcceptNestingOfLongStrings = acceptNestingOfLongStrings;
        }

        /// <summary>
        /// Whether to accept binary numbers (format: /0b[10]+/).
        /// </summary>
        public bool AcceptBinaryNumbers { get; }

        /// <summary>
        /// Whether to accept C comment syntax (formats: "//..." and "/* ... */").
        /// </summary>
        public bool AcceptCCommentSyntax { get; }

        /// <summary>
        /// Whether to accept compound assignment syntax
        /// (format: &lt;expr&gt; ("+=" | "-=" | "*=" | "/=" | "^=" | "%=" | "..=") &lt;expr&gt;).
        /// </summary>
        public bool AcceptCompoundAssignment { get; }

        /// <summary>
        /// Whether to accept empty statements (lone semicolons).
        /// </summary>
        public bool AcceptEmptyStatements { get; }

        /// <summary>
        /// Whether to accept C boolean operators (&amp;&amp;, ||, != and !).
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
        /// Whether to accept hexadecimal floating point literals
        /// (format: /0x[a-fA-F0-9]+(\.[a-fA-F0-9])?([+-]?p[0-9]+)/).
        /// </summary>
        public bool AcceptHexFloatLiterals { get; }

        /// <summary>
        /// Whether to accept octal numbers (format: /0o[0-7]+/).
        /// </summary>
        public bool AcceptOctalNumbers { get; }

        /// <summary>
        /// Whether to accept shebangs (format: "#!...").
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
        /// Whether to accept 5.3 bitise operators.
        /// </summary>
        public bool AcceptBitwiseOperators { get; }

        /// <summary>
        /// Whether to accept <c>\z</c> escapes.
        /// </summary>
        public bool AcceptWhitespaceEscape { get; }

        /// <summary>
        /// Whether to accept Unicode (<c>\u{XXX}</c>) escapes.
        /// </summary>
        public bool AcceptUnicodeEscape { get; }

        /// <summary>
        /// The type of continue to be recognized by the parser.
        /// </summary>
        public ContinueType ContinueType { get; }

        /// <summary>
        /// Whether to accept Luau if expressions.
        /// </summary>
        public bool AcceptIfExpressions { get; }

        /// <summary>
        /// Whether to accept FiveM hash strings.
        /// </summary>
        public bool AcceptHashStrings { get; }

        /// <summary>
        /// Whether to support the Lua 5.1 lexer bug where invalid
        /// escapes in strings are read as the character in the escape.
        /// <para>
        ///   NO ERROR WILL BE EMITTED IF AN INVALID ESCAPE IS ENCOUNTERED
        ///   IF THIS IS <see langword="true"/>.
        /// </para>
        /// </summary>
        public bool AcceptInvalidEscapes { get; }

        /// <summary>
        /// Whether to accept Lua 5.4 variable attributes.
        /// </summary>
        public bool AcceptLocalVariableAttributes { get; }

        /// <summary>
        /// Format binary numeric literals are stored as.
        /// </summary>
        public IntegerFormats BinaryIntegerFormat { get; }

        /// <summary>
        /// Format octal numeric literals are stored as.
        /// </summary>
        public IntegerFormats OctalIntegerFormat { get; }

        /// <summary>
        /// Format decimal integer literals are stored as.
        /// </summary>
        public IntegerFormats DecimalIntegerFormat { get; }

        /// <summary>
        /// Format hexadecimal integer literals are stored as.
        /// </summary>
        public IntegerFormats HexIntegerFormat { get; }

        /// <summary>
        /// Whether to accept typed lua syntax or not
        /// </summary>
        public bool AcceptTypedLua { get; }

        /// <summary>
        /// Whether to accept floor division or not
        /// </summary>
        public bool AcceptFloorDivision { get; }

        /// <summary>
        /// Whether to accept LuaJIT number suffixes or not
        /// </summary>
        public bool AcceptLuaJITNumberSuffixes { get; }

        /// <summary>
        /// Whether to accept nesting of [[...]]
        /// <para>
        ///   AN ERROR WILL BE GENERATED FOR NESTED LONG STRINGS IF THIS IS <see langword="false"/>.
        /// </para>
        /// </summary>
        public bool AcceptNestingOfLongStrings { get; }

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
        /// <param name="acceptIfExpression">
        /// <inheritdoc cref="AcceptIfExpressions" path="/summary" /> If None uses the value of
        /// <see cref="AcceptIfExpressions" />.
        /// </param>
        /// <param name="acceptHashStrings">
        /// <inheritdoc cref="AcceptHashStrings" path="/summary" /> If None uses the value of
        /// <see cref="AcceptHashStrings" />.
        /// </param>
        /// <param name="acceptInvalidEscapes">
        /// <inheritdoc cref="AcceptInvalidEscapes" path="/summary" /> If None uses the value of
        /// <see cref="AcceptInvalidEscapes" />.
        /// </param>
        /// <param name="acceptLocalVariableAttributes">
        /// <inheritdoc cref="AcceptLocalVariableAttributes" path="/summary" /> If None uses the
        /// value of <see cref="AcceptLocalVariableAttributes"/>.
        /// </param>
        /// <param name="binaryIntegerFormat">
        /// <inheritdoc cref="BinaryIntegerFormat" path="/summary" /> If None uses the value
        /// of <see cref="BinaryIntegerFormat"/>.
        /// </param>
        /// <param name="octalIntegerFormat">
        /// <inheritdoc cref="OctalIntegerFormat" path="/summary" /> If None uses the value
        /// of <see cref="OctalIntegerFormat"/>.
        /// </param>
        /// <param name="decimalIntegerFormat">
        /// <inheritdoc cref="DecimalIntegerFormat" path="/summary" /> If None uses the value
        /// of <see cref="DecimalIntegerFormat"/>.
        /// </param>
        /// <param name="hexIntegerFormat">
        /// <inheritdoc cref="HexIntegerFormat" path="/summary" /> If None uses the value
        /// of <see cref="HexIntegerFormat"/>.
        /// </param>
        /// <returns></returns>
        /// <param name="acceptTypedLua">
        /// <inheritdoc cref="AcceptTypedLua" path="/summary" /> If None uses the value
        /// of <see cref="AcceptTypedLua"/>.
        /// </param>
        /// <returns></returns>
        /// <param name="acceptFloorDivision">
        /// <inheritdoc cref="AcceptFloorDivision" path="/summary" /> If None uses the value
        /// of <see cref="AcceptFloorDivision"/>.
        /// </param>
        /// <returns></returns>
        /// <param name="acceptLuaJITNumberSuffixes">
        /// <inheritdoc cref="AcceptLuaJITNumberSuffixes" path="/summary" /> If None uses the value
        /// of <see cref="AcceptLuaJITNumberSuffixes"/>.
        /// </param>
        /// <returns></returns>        
        /// /// <param name="acceptNestingOfLongStrings">
        /// <inheritdoc cref="AcceptNestingOfLongStrings" path="/summary" /> If None uses the value
        /// of <see cref="AcceptNestingOfLongStrings"/>.
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
            Option<ContinueType> continueType = default,
            Option<bool> acceptIfExpression = default,
            Option<bool> acceptHashStrings = default,
            Option<bool> acceptInvalidEscapes = default,
            Option<bool> acceptLocalVariableAttributes = default,
            Option<IntegerFormats> binaryIntegerFormat = default,
            Option<IntegerFormats> octalIntegerFormat = default,
            Option<IntegerFormats> decimalIntegerFormat = default,
            Option<IntegerFormats> hexIntegerFormat = default,
            Option<bool> acceptTypedLua = default,
            Option<bool> acceptFloorDivision = default,
            Option<bool> acceptLuaJITNumberSuffixes = default,
            Option<bool> acceptNestingOfLongStrings = default) =>
            new(
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
                continueType.UnwrapOr(ContinueType),
                acceptIfExpression.UnwrapOr(AcceptIfExpressions),
                acceptHashStrings.UnwrapOr(AcceptHashStrings),
                acceptInvalidEscapes.UnwrapOr(AcceptInvalidEscapes),
                acceptLocalVariableAttributes.UnwrapOr(AcceptLocalVariableAttributes),
                binaryIntegerFormat.UnwrapOr(BinaryIntegerFormat),
                octalIntegerFormat.UnwrapOr(OctalIntegerFormat),
                decimalIntegerFormat.UnwrapOr(DecimalIntegerFormat),
                hexIntegerFormat.UnwrapOr(HexIntegerFormat),
                acceptTypedLua.UnwrapOr(AcceptTypedLua),
                acceptFloorDivision.UnwrapOr(AcceptFloorDivision),
                acceptLuaJITNumberSuffixes.UnwrapOr(AcceptLuaJITNumberSuffixes),
                acceptNestingOfLongStrings.UnwrapOr(AcceptNestingOfLongStrings)
                );

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
                && ContinueType == other.ContinueType
                && AcceptIfExpressions == other.AcceptIfExpressions
                && AcceptHashStrings == other.AcceptHashStrings
                && AcceptLocalVariableAttributes == other.AcceptLocalVariableAttributes
                && BinaryIntegerFormat == other.BinaryIntegerFormat
                && OctalIntegerFormat == other.OctalIntegerFormat
                && DecimalIntegerFormat == other.DecimalIntegerFormat
                && HexIntegerFormat == other.HexIntegerFormat
                && AcceptTypedLua == other.AcceptTypedLua
                && AcceptFloorDivision == other.AcceptFloorDivision
                && AcceptLuaJITNumberSuffixes == other.AcceptLuaJITNumberSuffixes
                && AcceptNestingOfLongStrings == other.AcceptNestingOfLongStrings);

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
            hash.Add(AcceptIfExpressions);
            hash.Add(AcceptHashStrings);
            hash.Add(AcceptLocalVariableAttributes);
            hash.Add(BinaryIntegerFormat);
            hash.Add(OctalIntegerFormat);
            hash.Add(DecimalIntegerFormat);
            hash.Add(HexIntegerFormat);
            hash.Add(AcceptTypedLua);
            hash.Add(AcceptFloorDivision);
            hash.Add(AcceptLuaJITNumberSuffixes);
            hash.Add(AcceptNestingOfLongStrings);
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
            else if (this == Lua54)
            {
                return "Lua 5.4";
            }
            else if (this == LuaJIT20)
            {
                return "LuaJIT 2.0";
            }
            else if (this == LuaJIT21)
            {
                return "LuaJIT 2.1";
            }
            else if (this == GMod)
            {
                return "GLua";
            }
            else if (this == Luau)
            {
                return "Luau";
            }
            else if (this == FiveM)
            {
                return "FiveM";
            }
            else if (this == All)
            {
                return "All (without integers)";
            }
            else if (this == AllWithIntegers)
            {
                return "All (with integers)";
            }
            else
            {
                return $"{{ AcceptBinaryNumbers = {AcceptBinaryNumbers}, AcceptCCommentSyntax = {AcceptCCommentSyntax}, AcceptCompoundAssignment = {AcceptCompoundAssignment}, AcceptEmptyStatements = {AcceptEmptyStatements}, AcceptCBooleanOperators = {AcceptCBooleanOperators}, AcceptGoto = {AcceptGoto}, AcceptHexEscapesInStrings = {AcceptHexEscapesInStrings}, AcceptHexFloatLiterals = {AcceptHexFloatLiterals}, AcceptOctalNumbers = {AcceptOctalNumbers}, AcceptShebang = {AcceptShebang}, AcceptUnderscoreInNumberLiterals = {AcceptUnderscoreInNumberLiterals}, UseLuaJitIdentifierRules = {UseLuaJitIdentifierRules}, AcceptBitwiseOperators = {AcceptBitwiseOperators}, AcceptWhitespaceEscape = {AcceptWhitespaceEscape}, ContinueType = {ContinueType}, AcceptIfExpressions = {AcceptIfExpressions}, AcceptHashStrings = {AcceptHashStrings}, AcceptLocalVariableAttributes = {AcceptLocalVariableAttributes}, BinaryIntegerFormat = {BinaryIntegerFormat}, OctalIntegerFormat = {OctalIntegerFormat}, DecimalIntegerFormat = {DecimalIntegerFormat}, HexIntegerFormat = {HexIntegerFormat}, AcceptTypedLua = {AcceptTypedLua}, AcceptFloorDivision = {AcceptFloorDivision}, AcceptLuaJITNumberSuffixes = {AcceptLuaJITNumberSuffixes}, AcceptNestingOfLongStrings = {AcceptNestingOfLongStrings} }}";
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
