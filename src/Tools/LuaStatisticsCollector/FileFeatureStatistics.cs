namespace Loretta.CodeAnalysis.Lua.StatisticsCollector
{
    internal record FileFeatureStatistics(
        bool HasBinaryNumbers,
        bool HasCComments,
        bool HasCompoundAssignments,
        bool HasEmptyStatements,
        bool HasCBooleanOperators,
        bool HasGoto,
        bool HasHexEscapesInStrings,
        bool HasHexFloatLiterals,
        bool HasOctalNumbers,
        bool HasShebang,
        bool HasUnderscoreInNumericLiterals,
        bool HasLuajitIdentifiers,
        ContinueType ContinueType)
    {
        public class Builder
        {
            public bool HasBinaryNumbers { get; set; }
            public bool HasCComments { get; set; }
            public bool HasCompoundAssignments { get; set; }
            public bool HasEmptyStatements { get; set; }
            public bool HasCBooleanOperators { get; set; }
            public bool HasGoto { get; set; }
            public bool HasHexEscapesInStrings { get; set; }
            public bool HasHexFloatLiterals { get; set; }
            public bool HasOctalNumbers { get; set; }
            public bool HasShebang { get; set; }
            public bool HasUnderscoreInNumericLiterals { get; set; }
            public bool HasLuajitIdentifiers { get; set; }
            public ContinueType ContinueType { get; set; }

            public void Merge(FileFeatureStatistics other)
            {
                HasBinaryNumbers |= other.HasBinaryNumbers;
                HasCComments |= other.HasCComments;
                HasCompoundAssignments |= other.HasCompoundAssignments;
                HasEmptyStatements |= other.HasEmptyStatements;
                HasCBooleanOperators |= other.HasCBooleanOperators;
                HasGoto |= other.HasGoto;
                HasHexEscapesInStrings |= other.HasHexEscapesInStrings;
                HasHexFloatLiterals |= other.HasHexFloatLiterals;
                HasOctalNumbers |= other.HasOctalNumbers;
                HasShebang |= other.HasShebang;
                HasUnderscoreInNumericLiterals |= other.HasUnderscoreInNumericLiterals;
                HasLuajitIdentifiers |= other.HasLuajitIdentifiers;
                ContinueType |= other.ContinueType;
            }

            public void Merge(Builder other)
            {
                HasBinaryNumbers |= other.HasBinaryNumbers;
                HasCComments |= other.HasCComments;
                HasCompoundAssignments |= other.HasCompoundAssignments;
                HasEmptyStatements |= other.HasEmptyStatements;
                HasCBooleanOperators |= other.HasCBooleanOperators;
                HasGoto |= other.HasGoto;
                HasHexEscapesInStrings |= other.HasHexEscapesInStrings;
                HasHexFloatLiterals |= other.HasHexFloatLiterals;
                HasOctalNumbers |= other.HasOctalNumbers;
                HasShebang |= other.HasShebang;
                HasUnderscoreInNumericLiterals |= other.HasUnderscoreInNumericLiterals;
                HasLuajitIdentifiers |= other.HasLuajitIdentifiers;
                ContinueType |= other.ContinueType;
            }

            public FileFeatureStatistics Summarize() =>
                new(HasBinaryNumbers, HasCComments, HasCompoundAssignments, HasEmptyStatements, HasCBooleanOperators, HasGoto, HasHexEscapesInStrings, HasHexFloatLiterals, HasOctalNumbers, HasShebang, HasUnderscoreInNumericLiterals, HasLuajitIdentifiers, ContinueType);
        }

        public FileFeatureStatistics()
            : this(false, false, false, false, false, false, false, false, false, false, false, false, ContinueType.None)
        {
        }

        public FileFeatureStatistics Merge(FileFeatureStatistics other) =>
            new(
                HasBinaryNumbers || other.HasBinaryNumbers,
                HasCComments || other.HasCComments,
                HasCompoundAssignments || other.HasCompoundAssignments,
                HasEmptyStatements || other.HasEmptyStatements,
                HasCBooleanOperators || other.HasCBooleanOperators,
                HasGoto || other.HasGoto,
                HasHexEscapesInStrings || other.HasHexEscapesInStrings,
                HasHexFloatLiterals || other.HasHexFloatLiterals,
                HasOctalNumbers || other.HasOctalNumbers,
                HasShebang || other.HasShebang,
                HasUnderscoreInNumericLiterals || other.HasUnderscoreInNumericLiterals,
                HasLuajitIdentifiers || other.HasLuajitIdentifiers,
                ContinueType < other.ContinueType ? other.ContinueType : ContinueType);
    }
}
