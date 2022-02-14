namespace Loretta.CodeAnalysis.Lua.StatisticsCollector
{
    using Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax;

    internal class LuaStatisticsSyntaxWalker : LuaSyntaxWalker
    {
        private readonly GlobalStatistics.Builder _globalBuilder;
        private readonly TokenStatistics.Builder _fileTokenBuilder;
        private readonly FileFeatureStatistics.Builder _featureStatisticsBuilder;

        public LuaStatisticsSyntaxWalker(GlobalStatistics.Builder globalBuilder, TokenStatistics.Builder fileTokenStatisticsBuilder) : base(SyntaxWalkerDepth.Trivia)
        {
            _globalBuilder = globalBuilder;
            _fileTokenBuilder = fileTokenStatisticsBuilder;
            _featureStatisticsBuilder = new FileFeatureStatistics.Builder();
        }

        public FileFeatureStatistics FeatureStatistics => _featureStatisticsBuilder.Summarize();

        public override void VisitCompoundAssignmentStatement(CompoundAssignmentStatementSyntax node)
        {
            if (!_featureStatisticsBuilder.HasCompoundAssignments)
                _featureStatisticsBuilder.HasCompoundAssignments = true;
            base.VisitCompoundAssignmentStatement(node);
        }

        public override void VisitEmptyStatement(EmptyStatementSyntax node)
        {
            if (!_featureStatisticsBuilder.HasEmptyStatements)
                _featureStatisticsBuilder.HasEmptyStatements = true;
            base.VisitEmptyStatement(node);
        }

        public override void VisitUnaryExpression(UnaryExpressionSyntax node)
        {
            if (!_featureStatisticsBuilder.HasCBooleanOperators && node.OperatorToken.Kind is SyntaxKind.BangToken)
                _featureStatisticsBuilder.HasCBooleanOperators = true;
            base.VisitUnaryExpression(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (!_featureStatisticsBuilder.HasCBooleanOperators && node.OperatorToken.Kind is SyntaxKind.AmpersandAmpersandToken or SyntaxKind.PipePipeToken)
                _featureStatisticsBuilder.HasCBooleanOperators = true;
            base.VisitBinaryExpression(node);
        }

        public override void VisitGotoStatement(GotoStatementSyntax node)
        {
            if (!_featureStatisticsBuilder.HasGoto)
                _featureStatisticsBuilder.HasGoto = true;
            base.VisitGotoStatement(node);
        }

        public override void VisitToken(SyntaxToken token)
        {
            _fileTokenBuilder.AddToken(token.Width);
            _globalBuilder.AddTokenWidth(token.Width);
            switch (token.Kind)
            {
                case SyntaxKind.NumericLiteralToken when !_featureStatisticsBuilder.HasBinaryNumbers
                                                         || !_featureStatisticsBuilder.HasOctalNumbers
                                                         || !_featureStatisticsBuilder.HasHexFloatLiterals
                                                         || !_featureStatisticsBuilder.HasUnderscoreInNumericLiterals:
                {
                    var text = token.Text;
                    if (!_featureStatisticsBuilder.HasBinaryNumbers && text.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
                        _featureStatisticsBuilder.HasBinaryNumbers = true;
                    else if (!_featureStatisticsBuilder.HasOctalNumbers && text.StartsWith("0o", StringComparison.OrdinalIgnoreCase))
                        _featureStatisticsBuilder.HasOctalNumbers = true;
                    else if (!_featureStatisticsBuilder.HasHexFloatLiterals && text.StartsWith("0x") && (text.Contains('.') || text.Contains('p', StringComparison.OrdinalIgnoreCase)))
                        _featureStatisticsBuilder.HasHexFloatLiterals = true;

                    if (!_featureStatisticsBuilder.HasUnderscoreInNumericLiterals && text.Contains('_'))
                        _featureStatisticsBuilder.HasUnderscoreInNumericLiterals = true;
                }
                break;

                case SyntaxKind.StringLiteralToken when !_featureStatisticsBuilder.HasHexEscapesInStrings && token.Text.Contains("\\x", StringComparison.OrdinalIgnoreCase):
                    _featureStatisticsBuilder.HasHexEscapesInStrings = true;
                    break;

                case SyntaxKind.IdentifierToken when !_featureStatisticsBuilder.HasLuajitIdentifiers && token.Text.Any(c => c >= 0x7F):
                    _featureStatisticsBuilder.HasLuajitIdentifiers = true;
                    break;

            }
            base.VisitToken(token);
        }

        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            switch (trivia.Kind)
            {
                case SyntaxKind.SingleLineCommentTrivia when !_featureStatisticsBuilder.HasCComments && trivia.Text.StartsWith("//", StringComparison.Ordinal):
                case SyntaxKind.MultiLineCommentTrivia when !_featureStatisticsBuilder.HasCComments && trivia.Text.StartsWith("/*", StringComparison.Ordinal):
                    _featureStatisticsBuilder.HasCComments = true;
                    break;
                case SyntaxKind.ShebangTrivia when !_featureStatisticsBuilder.HasShebang:
                    _featureStatisticsBuilder.HasShebang = true;
                    break;
            }
            base.VisitTrivia(trivia);
        }
    }
}
