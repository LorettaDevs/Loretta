namespace Loretta.CodeAnalysis.Lua.StatisticsCollector
{
    record GlobalFeatureStatistics(
        int HasBinaryNumbers,
        int HasCComments,
        int HasCompoundAssignments,
        int HasEmptyStatements,
        int HasCBooleanOperators,
        int HasGoto,
        int HasHexEscapesInStrings,
        int HasHexFloatLiterals,
        int HasOctalNumbers,
        int HasShebang,
        int HasUnderscoreInNumericLiterals,
        int HasLuajitIdentifiers)
    {
        public class Builder
        {
            private int _hasBinaryNumbers;
            private int _hasCComments;
            private int _hasCompoundAssignments;
            private int _hasEmptyStatements;
            private int _hasCBooleanOperators;
            private int _hasGoto;
            private int _hasHexEscapesInStrings;
            private int _hasHexFloatLiterals;
            private int _hasOctalNumbers;
            private int _hasShebang;
            private int _hasUnderscoreInNumericLiterals;
            private int _hasLuajitIdentifiers;

            public void Merge(FileFeatureStatistics featureStatistics)
            {
                if (featureStatistics.HasBinaryNumbers) _hasBinaryNumbers++;
                if (featureStatistics.HasCComments) _hasCComments++;
                if (featureStatistics.HasCompoundAssignments) _hasCompoundAssignments++;
                if (featureStatistics.HasEmptyStatements) _hasEmptyStatements++;
                if (featureStatistics.HasCBooleanOperators) _hasCBooleanOperators++;
                if (featureStatistics.HasGoto) _hasGoto++;
                if (featureStatistics.HasHexEscapesInStrings) _hasHexEscapesInStrings++;
                if (featureStatistics.HasHexFloatLiterals) _hasHexFloatLiterals++;
                if (featureStatistics.HasOctalNumbers) _hasOctalNumbers++;
                if (featureStatistics.HasShebang) _hasShebang++;
                if (featureStatistics.HasUnderscoreInNumericLiterals) _hasUnderscoreInNumericLiterals++;
                if (featureStatistics.HasLuajitIdentifiers) _hasLuajitIdentifiers++;
            }

            public GlobalFeatureStatistics Summarize()
            {
                return new GlobalFeatureStatistics(
                    _hasBinaryNumbers,
                    _hasCComments,
                    _hasCompoundAssignments,
                    _hasEmptyStatements,
                    _hasCBooleanOperators,
                    _hasGoto,
                    _hasHexEscapesInStrings,
                    _hasHexFloatLiterals,
                    _hasOctalNumbers,
                    _hasShebang,
                    _hasUnderscoreInNumericLiterals,
                    _hasLuajitIdentifiers);
            }
        }
    }
}
