using System;
using Loretta.Lexing;

namespace Loretta
{
    public enum ContinueType
    {
        None,
        Keyword = LuaTokenType.Keyword,
        ContextualKeyword = LuaTokenType.Identifier
    }

    public class LuaOptions
    {
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

        public static readonly LuaOptions Lua52 = Lua51.With (
            acceptEmptyStatements: true,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: true );

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

        public static readonly LuaOptions GMod = LuaJIT.With (
            acceptCCommentSyntax: true,
            acceptGModCOperators: true,
            continueType: ContinueType.Keyword );

        public static readonly LuaOptions Roblox = new LuaOptions (
            acceptBinaryNumbers: false,
            acceptCCommentSyntax: false,
            acceptCompoundAssignment: true,
            acceptEmptyStatements: true,
            acceptGModCOperators: false,
            acceptGoto: true,
            acceptHexEscapesInStrings: true,
            acceptHexFloatLiterals: true,
            acceptOctalNumbers: false,
            acceptShebang: false,
            acceptUnderlineInNumberLiterals: false,
            useLuaJitIdentifierRules: false,
            continueType: ContinueType.ContextualKeyword );

        public Boolean AcceptBinaryNumbers { get; }
        public Boolean AcceptCCommentSyntax { get; }
        public Boolean AcceptCompoundAssignment { get; }
        public Boolean AcceptEmptyStatements { get; }
        public Boolean AcceptGModCOperators { get; }
        public Boolean AcceptGoto { get; }
        public Boolean AcceptHexEscapesInStrings { get; }
        public Boolean AcceptHexFloatLiterals { get; }
        public Boolean AcceptOctalNumbers { get; }
        public Boolean AcceptShebang { get; }
        public Boolean AcceptUnderlineInNumberLiterals { get; }
        public Boolean UseLuaJitIdentifierRules { get; }
        public ContinueType ContinueType { get; }

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