using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    using System.Threading;
    using Loretta.CodeAnalysis.Syntax.InternalSyntax;

    internal sealed class LanguageParser : SyntaxParser
    {
        private readonly SyntaxListPool _pool = new();
        private int _recursionDepth;

        public LanguageParser(
            Lexer lexer,
            Lua.LuaSyntaxNode? oldTree,
            IEnumerable<TextChangeRange>? changes,
            CancellationToken cancellationToken = default)
            : base(lexer, oldTree, changes, preLexIfNotIncremental: true, cancellationToken: cancellationToken)
        {
        }

        private SyntaxToken? TryMatchSemicolon()
        {
            if (CurrentToken.Kind == SyntaxKind.SemicolonToken)
                return EatToken();
            return null;
        }

        internal CompilationUnitSyntax ParseCompilationUnit()
        {
            return ParseWithStackGuard(
                ParseCompilationUnitCore,
                () => SyntaxFactory.CompilationUnit(
                    SyntaxFactory.StatementList(),
                    SyntaxFactory.Token(SyntaxKind.EndOfFileToken)));
        }

        internal CompilationUnitSyntax ParseCompilationUnitCore()
        {
            var statements = ParseStatementList();
            var endOfFileToken = EatTokenAsKind(SyntaxKind.EndOfFileToken);
            return SyntaxFactory.CompilationUnit(statements, endOfFileToken);
        }

        internal TNode ParseWithStackGuard<TNode>(Func<TNode> parseFunc, Func<TNode> createEmptyNodeFunc) where TNode : LuaSyntaxNode
        {
            // If this value is non-zero then we are nesting calls to ParseWithStackGuard which should not be
            // happening.  It's not a bug but it's inefficient and should be changed.
            LorettaDebug.Assert(_recursionDepth == 0);

            try
            {
                return parseFunc();
            }
            catch (InsufficientExecutionStackException)
            {
                return CreateForGlobalFailure(_lexer.TextWindow.Position, createEmptyNodeFunc());
            }
        }

        private TNode CreateForGlobalFailure<TNode>(int position, TNode node) where TNode : LuaSyntaxNode
        {
            // Turn the complete input into a single skipped token. This avoids running the lexer, and therefore
            // the preprocessor directive parser, which may itself run into the same problem that caused the
            // original failure.
            var builder = new SyntaxListBuilder(1);
            builder.Add(SyntaxFactory.BadToken(null, _lexer.TextWindow.Text.ToString(), null));
            var fileAsTrivia = SyntaxFactory.SkippedTokensTrivia(builder.ToList<SyntaxToken>());
            node = AddLeadingSkippedSyntax(node, fileAsTrivia);
            ForceEndOfFile(); // force the scanner to report that it is at the end of the input.
            return AddError(node, position, 0, ErrorCode.ERR_InsufficientStack);
        }

        private StatementListSyntax ParseStatementList(params SyntaxKind[] terminalKinds)
        {
            if (IsIncremental && CurrentNodeKind == SyntaxKind.StatementList)
                return (StatementListSyntax) EatNode();

            var builder = _pool.Allocate<StatementSyntax>();
            var progress = -1;
            while (IsMakingProgress(ref progress))
            {
                var kind = CurrentToken.Kind;
                if (kind == SyntaxKind.EndOfFileToken || terminalKinds.Length > 0 && terminalKinds.Contains(kind))
                    break;

                var statement = ParseStatement();
                builder.Add(statement);
            }
            return SyntaxFactory.StatementList(_pool.ToListAndFree(builder));
        }

        internal StatementSyntax ParseStatement()
        {
            _recursionDepth++;
            StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
            var result = ParseStatementCore();
            _recursionDepth--;
            return result;
        }

        private StatementSyntax ParseStatementCore()
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (IsIncremental && CurrentNode is Syntax.StatementSyntax)
                return (StatementSyntax) EatNode();

            try
            {
                _recursionDepth++;
                StackGuard.EnsureSufficientExecutionStack(_recursionDepth);

                switch (CurrentToken.Kind)
                {
                    case SyntaxKind.LocalKeyword:
                        if (PeekToken(1).Kind is SyntaxKind.FunctionKeyword)
                            return ParseLocalFunctionDeclarationStatement();
                        else
                            return ParseLocalVariableDeclarationStatement();

                    case SyntaxKind.ForKeyword:
                        if (PeekToken(2).Kind == SyntaxKind.EqualsToken)
                            return ParseNumericForStatement();
                        else
                            return ParseGenericForStatement();

                    case SyntaxKind.IfKeyword:
                        return ParseIfStatement();

                    case SyntaxKind.RepeatKeyword:
                        return ParseRepeatUntilStatement();

                    case SyntaxKind.WhileKeyword:
                        return ParseWhileStatement();

                    case SyntaxKind.DoKeyword:
                        return ParseDoStatement();

                    case SyntaxKind.GotoKeyword:
                        return ParseGotoStatement();

                    case SyntaxKind.BreakKeyword:
                        return ParseBreakStatement();

                    case SyntaxKind.ContinueKeyword:
                        return ParseContinueStatement();

                    case SyntaxKind.ColonColonToken:
                        return ParseGotoLabelStatement();

                    case SyntaxKind.FunctionKeyword:
                        return ParseFunctionDeclarationStatement();

                    case SyntaxKind.ReturnKeyword:
                        return ParseReturnStatement();

                    case SyntaxKind.SemicolonToken when Options.SyntaxOptions.AcceptEmptyStatements:
                        return SyntaxFactory.EmptyStatement(EatToken(SyntaxKind.SemicolonToken));

                    case SyntaxKind.ExportKeyword or SyntaxKind.TypeKeyword:
                        SyntaxToken? exportKeyword = null;

                        if (CurrentToken.Kind == SyntaxKind.ExportKeyword)
                        {
                            exportKeyword = EatToken();
                        }

                        var typeKeyword = EatToken(SyntaxKind.TypeKeyword);
                        var typeName = ParseTypeName();
                        var equalsToken = EatToken(SyntaxKind.EqualsToken);
                        var type = ParseType();
                        var optionalSemiColonToken = TryMatchSemicolon();

                        return SyntaxFactory.TypeDeclarationStatement(exportKeyword, typeKeyword, typeName, equalsToken, type, optionalSemiColonToken);

                    default:
                    {
                        if (CurrentToken.ContextualKind == SyntaxKind.ContinueKeyword)
                            return ParseContinueStatement();

                        var restorePoint = GetResetPoint();
                        var expression = ParsePrefixOrVariableExpression();
                        if (expression.IsMissing)
                        {
                            // If the expression is missing, reset and then consume the token we cannot process
                            // generating a *minimal* missing statement so that we can continue.
                            Reset(ref restorePoint);
                            var token = EatToken();
                            return AddError(
                                SyntaxFactory.ExpressionStatement(
                                    AddLeadingSkippedSyntax(
                                        CreateMissingIdentifierName(),
                                        token),
                                    null),
                                ErrorCode.ERR_InvalidStatement);
                        }
                        else
                        {
                            if (CurrentToken.Kind is SyntaxKind.CommaToken or SyntaxKind.EqualsToken)
                            {
                                return ParseAssignmentStatement(expression);
                            }
                            else if (SyntaxFacts.IsCompoundAssignmentOperatorToken(CurrentToken.Kind))
                            {
                                return ParseCompoundAssignment(expression);
                            }

                            var semicolonToken = TryMatchSemicolon();
                            var node = SyntaxFactory.ExpressionStatement(expression, semicolonToken);
                            if (expression.Kind is not (SyntaxKind.FunctionCallExpression or SyntaxKind.MethodCallExpression))
                            {
                                node = AddError(node, ErrorCode.ERR_NonFunctionCallBeingUsedAsStatement);
                            }

                            return node;
                        }
                    }
                }
            }
            finally
            {
                _recursionDepth--;
            }
        }

        private LocalDeclarationNameSyntax ParseLocalDeclarationName()
        {
            var name = ParseIdentifierName();

            VariableAttributeSyntax? attribute = null;
            TypeBindingSyntax? typeBinding = null;

            switch (CurrentToken.Kind)
            {
                case SyntaxKind.ColonToken:
                    var colonToken = EatToken(SyntaxKind.ColonToken);
                    var type = ParseType();

                    typeBinding = SyntaxFactory.TypeBinding(colonToken, type);
                    break;
                case SyntaxKind.LessThanToken:
                    var lessThanToken = EatToken(SyntaxKind.LessThanToken);
                    var identifierName = EatToken(SyntaxKind.IdentifierToken);
                    var greaterThanToken = EatToken(SyntaxKind.GreaterThanToken);

                    attribute = SyntaxFactory.VariableAttribute(lessThanToken, identifierName, greaterThanToken);
                    break;
            }

            return SyntaxFactory.LocalDeclarationName(name, attribute, typeBinding);
        }

        private LocalVariableDeclarationStatementSyntax ParseLocalVariableDeclarationStatement()
        {
            var localKeyword = EatToken(SyntaxKind.LocalKeyword);
            var namesAndSeparatorsBuilder =
                _pool.AllocateSeparated<LocalDeclarationNameSyntax>();

            var name = ParseLocalDeclarationName();
            namesAndSeparatorsBuilder.Add(name);

            while (CurrentToken.Kind is SyntaxKind.CommaToken)
            {
                var separator = EatToken(SyntaxKind.CommaToken);
                namesAndSeparatorsBuilder.AddSeparator(separator);

                name = ParseLocalDeclarationName();
                namesAndSeparatorsBuilder.Add(name);
            }

            var names = _pool.ToListAndFree(namesAndSeparatorsBuilder);

            EqualsValuesClauseSyntax? equalsValuesClause = null;
            if (CurrentToken.Kind == SyntaxKind.EqualsToken)
            {
                var equalsToken = EatToken(SyntaxKind.EqualsToken);

                var expressionsAndSeparatorsBuilder =
                    _pool.AllocateSeparated<ExpressionSyntax>();

                var value = ParseExpression();
                expressionsAndSeparatorsBuilder.Add(value);

                while (CurrentToken.Kind is SyntaxKind.CommaToken)
                {
                    var separator = EatToken(SyntaxKind.CommaToken);
                    expressionsAndSeparatorsBuilder.AddSeparator(separator);

                    value = ParseExpression();
                    expressionsAndSeparatorsBuilder.Add(value);
                }

                var values = _pool.ToListAndFree(expressionsAndSeparatorsBuilder);
                equalsValuesClause = SyntaxFactory.EqualsValuesClause(equalsToken, values);
            }

            var semicolonToken = TryMatchSemicolon();

            return SyntaxFactory.LocalVariableDeclarationStatement(
                localKeyword,
                names,
                equalsValuesClause,
                semicolonToken);
        }

        private LocalFunctionDeclarationStatementSyntax ParseLocalFunctionDeclarationStatement()
        {
            var localKeyword = EatToken(SyntaxKind.LocalKeyword);
            var functionKeyword = EatToken(SyntaxKind.FunctionKeyword);
            var identifier = ParseIdentifierName();
            var parameters = ParseParameterList();
            var body = ParseStatementList(SyntaxKind.EndKeyword);
            var endKeyword = EatToken(SyntaxKind.EndKeyword);
            var semicolonToken = TryMatchSemicolon();

            return SyntaxFactory.LocalFunctionDeclarationStatement(
                localKeyword,
                functionKeyword,
                identifier,
                parameters,
                body,
                endKeyword,
                semicolonToken);
        }

        private NumericForStatementSyntax ParseNumericForStatement()
        {
            var forKeyword = EatToken(SyntaxKind.ForKeyword);
            var identifier = ParseIdentifierName();

            TypeBindingSyntax? typeBinding = null;

            if (CurrentToken.Kind == SyntaxKind.ColonToken)
            {
                var colonToken = EatToken();
                var type = ParseType();

                typeBinding = SyntaxFactory.TypeBinding(colonToken, type);
            }

            var forLoopVariable = SyntaxFactory.ForLoopVariable(identifier, typeBinding);

            var equalsToken = EatToken(SyntaxKind.EqualsToken);
            var initialValue = ParseExpression();
            var finalValueCommaToken = EatToken(SyntaxKind.CommaToken);
            var finalValue = ParseExpression();
            SyntaxToken? stepValueCommaToken = null;
            ExpressionSyntax? stepValue = null;
            if (CurrentToken.Kind == SyntaxKind.CommaToken)
            {
                stepValueCommaToken = EatToken(SyntaxKind.CommaToken);
                stepValue = ParseExpression();
            }
            var doKeyword = EatToken(SyntaxKind.DoKeyword);
            var body = ParseStatementList(SyntaxKind.EndKeyword);
            var endKeyword = EatToken(SyntaxKind.EndKeyword);
            var semicolonToken = TryMatchSemicolon();
            return SyntaxFactory.NumericForStatement(
                forKeyword,
                forLoopVariable,
                equalsToken,
                initialValue,
                finalValueCommaToken,
                finalValue,
                stepValueCommaToken,
                stepValue,
                doKeyword,
                body,
                endKeyword,
                semicolonToken);
        }

        private GenericForStatementSyntax ParseGenericForStatement()
        {
            var forKeyword = EatToken(SyntaxKind.ForKeyword);

            var identifiersAndSeparatorsBuilder =
                _pool.AllocateSeparated<ForLoopVariableSyntax>();

            var identifier = ParseIdentifierName();
            
            TypeBindingSyntax? typeBinding = null;
            if (CurrentToken.Kind == SyntaxKind.ColonToken)
            {
                var colonToken = EatToken();
                var type = ParseType();

                typeBinding = SyntaxFactory.TypeBinding(colonToken, type);
            }

            var variable = SyntaxFactory.ForLoopVariable(identifier, typeBinding);
            identifiersAndSeparatorsBuilder.Add(variable);

            while (CurrentToken.Kind is SyntaxKind.CommaToken)
            {
                var separator = EatToken(SyntaxKind.CommaToken);
                identifiersAndSeparatorsBuilder.AddSeparator(separator);

                identifier = ParseIdentifierName();

                if (CurrentToken.Kind == SyntaxKind.ColonToken)
                {
                    var colonToken = EatToken();
                    var type = ParseType();

                    typeBinding = SyntaxFactory.TypeBinding(colonToken, type);
                }

                variable = SyntaxFactory.ForLoopVariable(identifier, typeBinding);
                identifiersAndSeparatorsBuilder.Add(variable);
            }

            var inKeyword = EatToken(SyntaxKind.InKeyword);

            var expressionsAndSeparatorsBuilder =
                _pool.AllocateSeparated<ExpressionSyntax>();

            var expression = ParseExpression();
            expressionsAndSeparatorsBuilder.Add(expression);
            while (CurrentToken.Kind is SyntaxKind.CommaToken)
            {
                var separator = EatToken(SyntaxKind.CommaToken);
                expressionsAndSeparatorsBuilder.AddSeparator(separator);

                expression = ParseExpression();
                expressionsAndSeparatorsBuilder.Add(expression);
            }

            var doKeyword = EatToken(SyntaxKind.DoKeyword);
            var body = ParseStatementList(SyntaxKind.EndKeyword);
            var endKeyword = EatToken(SyntaxKind.EndKeyword);
            var semicolonToken = TryMatchSemicolon();

            var identifiers = _pool.ToListAndFree(identifiersAndSeparatorsBuilder);
            var expressions = _pool.ToListAndFree(expressionsAndSeparatorsBuilder);
            return SyntaxFactory.GenericForStatement(
                forKeyword,
                identifiers,
                inKeyword,
                expressions,
                doKeyword,
                body,
                endKeyword,
                semicolonToken);
        }

        private IfStatementSyntax ParseIfStatement()
        {
            var ifKeyword = EatToken(SyntaxKind.IfKeyword);
            var condition = ParseExpression();
            var thenKeyword = EatToken(SyntaxKind.ThenKeyword);
            var body = ParseStatementList(SyntaxKind.ElseIfKeyword, SyntaxKind.ElseKeyword, SyntaxKind.EndKeyword);

            var elseIfClausesBuilder = _pool.Allocate<ElseIfClauseSyntax>();
            while (CurrentToken.Kind is SyntaxKind.ElseIfKeyword)
            {
                var elseIfKeyword = EatToken(SyntaxKind.ElseIfKeyword);
                var elseIfCondition = ParseExpression();
                var elseIfThenKeyword = EatToken(SyntaxKind.ThenKeyword);
                var elseIfBody = ParseStatementList(SyntaxKind.ElseIfKeyword, SyntaxKind.ElseKeyword, SyntaxKind.EndKeyword);

                elseIfClausesBuilder.Add(SyntaxFactory.ElseIfClause(
                    elseIfKeyword,
                    elseIfCondition,
                    elseIfThenKeyword,
                    elseIfBody));
            }

            ElseClauseSyntax? elseClause = null;
            if (CurrentToken.Kind is SyntaxKind.ElseKeyword)
            {
                var elseKeyword = EatToken(SyntaxKind.ElseKeyword);
                var elseBody = ParseStatementList(SyntaxKind.EndKeyword);
                elseClause = SyntaxFactory.ElseClause(elseKeyword, elseBody);
            }

            var endKeyword = EatToken(SyntaxKind.EndKeyword);
            var semicolonToken = TryMatchSemicolon();

            var elseIfClauses = _pool.ToListAndFree(elseIfClausesBuilder);
            return SyntaxFactory.IfStatement(
                ifKeyword,
                condition,
                thenKeyword,
                body,
                elseIfClauses,
                elseClause,
                endKeyword,
                semicolonToken);
        }

        private RepeatUntilStatementSyntax ParseRepeatUntilStatement()
        {
            var repeatKeyword = EatToken(SyntaxKind.RepeatKeyword);
            var body = ParseStatementList(SyntaxKind.UntilKeyword);
            var untilKeyword = EatToken(SyntaxKind.UntilKeyword);
            var condition = ParseExpression();
            var semicolonToken = TryMatchSemicolon();
            return SyntaxFactory.RepeatUntilStatement(
                repeatKeyword,
                body,
                untilKeyword,
                condition,
                semicolonToken);
        }

        private WhileStatementSyntax ParseWhileStatement()
        {
            var whileKeyword = EatToken(SyntaxKind.WhileKeyword);
            var condition = ParseExpression();
            var doKeyword = EatToken(SyntaxKind.DoKeyword);
            var body = ParseStatementList(SyntaxKind.EndKeyword);
            var endKeyword = EatToken(SyntaxKind.EndKeyword);
            var semicolonToken = TryMatchSemicolon();
            return SyntaxFactory.WhileStatement(
                whileKeyword,
                condition,
                doKeyword,
                body,
                endKeyword,
                semicolonToken);
        }

        private DoStatementSyntax ParseDoStatement()
        {
            var doKeyword = EatToken(SyntaxKind.DoKeyword);
            var body = ParseStatementList(SyntaxKind.EndKeyword);
            var endKeyword = EatToken(SyntaxKind.EndKeyword);
            var semicolonToken = TryMatchSemicolon();
            return SyntaxFactory.DoStatement(
                doKeyword,
                body,
                endKeyword,
                semicolonToken);
        }

        private GotoStatementSyntax ParseGotoStatement()
        {
            var gotoKeyword = EatToken(SyntaxKind.GotoKeyword);
            var labelName = EatToken(SyntaxKind.IdentifierToken);
            var semicolonToken = TryMatchSemicolon();
            return SyntaxFactory.GotoStatement(
                gotoKeyword,
                labelName,
                semicolonToken);
        }

        private BreakStatementSyntax ParseBreakStatement()
        {
            var breakKeyword = EatToken(SyntaxKind.BreakKeyword);
            var semicolonToken = TryMatchSemicolon();
            return SyntaxFactory.BreakStatement(breakKeyword, semicolonToken);
        }

        private ContinueStatementSyntax ParseContinueStatement()
        {
            LorettaDebug.Assert(Options.SyntaxOptions.ContinueType is ContinueType.ContextualKeyword or ContinueType.Keyword);
            var continueKeyword = Options.SyntaxOptions.ContinueType == ContinueType.ContextualKeyword
                ? EatContextualToken(SyntaxKind.ContinueKeyword)
                : EatTokenWithPrejudice(SyntaxKind.ContinueKeyword);
            var semicolonToken = TryMatchSemicolon();
            return SyntaxFactory.ContinueStatement(continueKeyword, semicolonToken);
        }

        private GotoLabelStatementSyntax ParseGotoLabelStatement()
        {
            var leftDelimiterToken = EatToken(SyntaxKind.ColonColonToken);
            var identifier = EatToken(SyntaxKind.IdentifierToken);
            var rightDelimiterToken = EatToken(SyntaxKind.ColonColonToken);
            var semicolonToken = TryMatchSemicolon();
            return SyntaxFactory.GotoLabelStatement(
                leftDelimiterToken,
                identifier,
                rightDelimiterToken,
                semicolonToken);
        }

        private FunctionDeclarationStatementSyntax ParseFunctionDeclarationStatement()
        {
            var functionKeyword = EatToken(SyntaxKind.FunctionKeyword);
            var name = ParseFunctionName();
            var parameters = ParseParameterList();
            var body = ParseStatementList(SyntaxKind.EndKeyword);
            var endKeyword = EatToken(SyntaxKind.EndKeyword);
            var semicolonToken = TryMatchSemicolon();
            return SyntaxFactory.FunctionDeclarationStatement(
                functionKeyword,
                name,
                parameters,
                body,
                endKeyword,
                semicolonToken);
        }

        private FunctionNameSyntax ParseFunctionName()
        {
            var identifier = EatToken(SyntaxKind.IdentifierToken);
            FunctionNameSyntax name = SyntaxFactory.SimpleFunctionName(identifier);

            while (CurrentToken.Kind == SyntaxKind.DotToken)
            {
                var dotToken = EatToken(SyntaxKind.DotToken);
                identifier = EatToken(SyntaxKind.IdentifierToken);
                name = SyntaxFactory.MemberFunctionName(name, dotToken, identifier);
            }

            if (CurrentToken.Kind == SyntaxKind.ColonToken)
            {
                var colonToken = EatToken(SyntaxKind.ColonToken);
                identifier = EatToken(SyntaxKind.IdentifierToken);
                name = SyntaxFactory.MethodFunctionName(name, colonToken, identifier);
            }

            return name;
        }

        private ReturnStatementSyntax ParseReturnStatement()
        {
            var returnKeyword = EatToken(SyntaxKind.ReturnKeyword);
            SeparatedSyntaxList<ExpressionSyntax> expressions = default;
            if (CurrentToken.Kind is not (SyntaxKind.ElseKeyword or SyntaxKind.ElseIfKeyword or SyntaxKind.EndKeyword or SyntaxKind.UntilKeyword or SyntaxKind.SemicolonToken))
            {
                var expressionsAndSeparatorsBuilder =
                    _pool.AllocateSeparated<ExpressionSyntax>();

                var expression = ParseExpression();
                expressionsAndSeparatorsBuilder.Add(expression);

                while (CurrentToken.Kind == SyntaxKind.CommaToken)
                {
                    var separator = EatToken(SyntaxKind.CommaToken);
                    expressionsAndSeparatorsBuilder.AddSeparator(separator);

                    expression = ParseExpression();
                    expressionsAndSeparatorsBuilder.Add(expression);
                }

                expressions = _pool.ToListAndFree(expressionsAndSeparatorsBuilder);
            }

            var semicolonToken = TryMatchSemicolon();
            return SyntaxFactory.ReturnStatement(
                returnKeyword,
                expressions,
                semicolonToken);
        }

        private AssignmentStatementSyntax ParseAssignmentStatement(PrefixExpressionSyntax variable)
        {
            var variablesAndSeparatorsBuilder =
                _pool.AllocateSeparated<PrefixExpressionSyntax>();
            if (!SyntaxFacts.IsVariableExpression(variable.Kind))
                variable = AddError(variable, ErrorCode.ERR_CannotBeAssignedTo);
            variablesAndSeparatorsBuilder.Add(variable);
            while (CurrentToken.Kind == SyntaxKind.CommaToken)
            {
                var separator = EatToken(SyntaxKind.CommaToken);
                variablesAndSeparatorsBuilder.AddSeparator(separator);

                variable = ParsePrefixOrVariableExpression();
                if (!SyntaxFacts.IsVariableExpression(variable.Kind))
                    variable = AddError(variable, ErrorCode.ERR_CannotBeAssignedTo);
                variablesAndSeparatorsBuilder.Add(variable);
            }

            var equalsToken = EatToken(SyntaxKind.EqualsToken);

            var valuesAndSeparatorsBuilder =
                _pool.AllocateSeparated<ExpressionSyntax>();
            var value = ParseExpression();
            valuesAndSeparatorsBuilder.Add(value);
            while (CurrentToken.Kind == SyntaxKind.CommaToken)
            {
                var separator = EatToken(SyntaxKind.CommaToken);
                valuesAndSeparatorsBuilder.AddSeparator(separator);

                value = ParseExpression();
                valuesAndSeparatorsBuilder.Add(value);
            }

            var semicolonToken = TryMatchSemicolon();

            var variables = _pool.ToListAndFree(variablesAndSeparatorsBuilder);
            var values = _pool.ToListAndFree(valuesAndSeparatorsBuilder);
            return SyntaxFactory.AssignmentStatement(
                variables,
                SyntaxFactory.EqualsValuesClause(
                    equalsToken,
                    values),
                semicolonToken);
        }

        private CompoundAssignmentStatementSyntax ParseCompoundAssignment(PrefixExpressionSyntax variable)
        {
            LorettaDebug.Assert(SyntaxFacts.IsCompoundAssignmentOperatorToken(CurrentToken.Kind));
            if (!SyntaxFacts.IsVariableExpression(variable.Kind))
                variable = AddError(variable, ErrorCode.ERR_CannotBeAssignedTo);
            var assignmentOperatorToken = EatToken();
            var kind = SyntaxFacts.GetCompoundAssignmentStatement(assignmentOperatorToken.Kind).Value;
            var expression = ParseExpression();
            var semicolonToken = TryMatchSemicolon();

            var compoundAssignment = SyntaxFactory.CompoundAssignmentStatement(
                kind,
                variable,
                assignmentOperatorToken,
                expression,
                semicolonToken);
            if (!Options.SyntaxOptions.AcceptCompoundAssignment)
                compoundAssignment = AddError(compoundAssignment, ErrorCode.ERR_CompoundAssignmentNotSupportedInLuaVersion);
            return compoundAssignment;
        }

        internal ExpressionSyntax ParseExpression() =>
            ParseBinaryExpression(0, SyntaxKind.BadToken, false);

        internal ExpressionSyntax ParseBinaryExpression(int parentPrecedence, SyntaxKind parentOperator, bool isParentUnary)
        {
            try
            {
                _recursionDepth++;
                StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
                return ParseBinaryExpressionCore(parentPrecedence, parentOperator, isParentUnary);
            }
            finally
            {
                _recursionDepth--;
            }
        }

        private ExpressionSyntax ParseBinaryExpressionCore(int parentPrecedence, SyntaxKind parentOperator, bool isParentUnary)
        {
            ExpressionSyntax left;

            var unaryOperatorPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(CurrentToken.Kind);
            if (unaryOperatorPrecedence != 0)
            {
                var operatorToken = EatToken();
                var kind = SyntaxFacts.GetUnaryExpression(operatorToken.Kind).Value;
                var operand = ParseBinaryExpression(unaryOperatorPrecedence, operatorToken.Kind, true);
                left = new UnaryExpressionSyntax(kind, operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }

            var pos = -1;
            while (IsMakingProgress(ref pos))
            {
                var tk = CurrentToken.Kind;
                SyntaxKind operatorKind;

                if (tk == SyntaxKind.GreaterThanToken
                    && PeekToken(1).Kind == SyntaxKind.GreaterThanToken
                    && NoTriviaBetween(CurrentToken, PeekToken(1)))
                {
                    operatorKind = SyntaxKind.GreaterThanGreaterThanToken;
                }
                else if (SyntaxFacts.IsBinaryOperatorToken(tk))
                {
                    operatorKind = tk;
                }
                else
                {
                    break;
                }

                var newPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(operatorKind);
                // If we get an invalid precedence, then quit.
                if (newPrecedence <= 0)
                {
                    break;
                }
                // Otherwise, if we have a lower precedence then quit as well.
                // Results in a + b * c being parsed as:
                //     +
                //    / \
                //   a  *
                //     / \
                //    b   c
                //
                // But a * b + c being parsed as:
                //     +
                //    / \
                //   *   c
                //  / \
                // a   b
                if (newPrecedence < parentPrecedence)
                {
                    break;
                }
                // Also quit if precedence is the same and the operator is not right associative.
                // Results in a + b + c being parsed as:
                //     +
                //    / \
                //   +   c
                //  / \
                // a   b
                //
                // But a ^ b ^ c being parsed as:
                //     ^
                //    / \
                //   a  ^
                //     / \
                //    b   c

                if (newPrecedence == parentPrecedence && !SyntaxFacts.IsRightAssociative(operatorKind))
                {
                    break;
                }

                SyntaxToken operatorToken = EatToken(tk);
                if (operatorKind == SyntaxKind.GreaterThanGreaterThanToken)
                {
                    var trailingToken = EatToken(SyntaxKind.GreaterThanToken);
                    operatorToken = SyntaxFactory.Token(
                        operatorToken.GetLeadingTrivia(),
                        SyntaxKind.GreaterThanGreaterThanToken,
                        trailingToken.GetTrailingTrivia());
                }

                var kind = SyntaxFacts.GetBinaryExpression(operatorToken.Kind).Value;
                var right = ParseBinaryExpression(newPrecedence, operatorKind, false);
                left = new BinaryExpressionSyntax(kind, left, operatorToken, right);
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            return CurrentToken.Kind switch
            {
                SyntaxKind.NilKeyword
                or SyntaxKind.TrueKeyword
                or SyntaxKind.FalseKeyword
                or SyntaxKind.NumericLiteralToken
                or SyntaxKind.StringLiteralToken
                or SyntaxKind.HashStringLiteralToken => ParseLiteralExpression(),
                SyntaxKind.DotDotDotToken => ParseVarArgExpression(),
                SyntaxKind.OpenBraceToken => ParseTableConstructorExpression(),
                SyntaxKind.FunctionKeyword when PeekToken(1).Kind == SyntaxKind.OpenParenthesisToken => ParseAnonymousFunctionExpression(),
                SyntaxKind.IfKeyword => ParseIfExpression(),
                _ => ParsePrefixOrVariableExpression(),
            };
        }

        private PrefixExpressionSyntax ParsePrefixOrVariableExpression()
        {
            PrefixExpressionSyntax expression;
            if (CurrentToken.Kind == SyntaxKind.OpenParenthesisToken)
            {
                expression = ParseParenthesizedExpression();
            }
            else if (CurrentToken.Kind == SyntaxKind.IdentifierToken)
            {
                expression = ParseIdentifierName();
            }
            else
            {
                var node = CreateMissingIdentifierName();
                if (CurrentToken.Kind == SyntaxKind.EndOfFileToken)
                    node = AddError(node, ErrorCode.ERR_ExpressionExpected);
                else
                    node = AddError(node, ErrorCode.ERR_InvalidExpressionPart, SyntaxFacts.GetText(CurrentToken.Kind));
                return node;
            }

            var pos = -1;
            while (IsMakingProgress(ref pos))
            {
                switch (CurrentToken.Kind)
                {
                    case SyntaxKind.DotToken:
                        expression = ParseMemberAccessExpression(expression);
                        break;

                    case SyntaxKind.OpenBracketToken:
                        expression = ParseElementAccessExpression(expression);
                        break;

                    case SyntaxKind.ColonToken:
                        expression = ParseMethodCallExpression(expression);
                        break;

                    case SyntaxKind.StringLiteralToken:
                    case SyntaxKind.OpenBraceToken:
                    case SyntaxKind.OpenParenthesisToken:
                        expression = ParseFunctionCall(expression);
                        break;

                    default:
                        goto endloop;
                }
            }

        endloop:
            return expression;
        }

        private AnonymousFunctionExpressionSyntax ParseAnonymousFunctionExpression()
        {
            var functionKeywordToken = EatToken(SyntaxKind.FunctionKeyword);
            var parameterList = ParseParameterList();
            var body = ParseStatementList(SyntaxKind.EndKeyword);
            var endKeyword = EatToken(SyntaxKind.EndKeyword);

            return SyntaxFactory.AnonymousFunctionExpression(
                functionKeywordToken,
                parameterList,
                body,
                endKeyword);
        }

        private IdentifierNameSyntax ParseIdentifierName()
        {
            Syntax.IdentifierNameSyntax? currentNode;
            if (IsIncremental
                && CurrentNodeKind == SyntaxKind.IdentifierName
                && (currentNode = CurrentNode as Syntax.IdentifierNameSyntax) is not null
                // If the Kind is equal to the ContextualKind, then the token wasn't parsed as a contextual one
                && currentNode.Identifier.Kind() == currentNode.Identifier.ContextualKind())
            {
                return (IdentifierNameSyntax) EatNode();
            }

            return SyntaxFactory.IdentifierName(EatToken(SyntaxKind.IdentifierToken));
        }

        private MemberAccessExpressionSyntax ParseMemberAccessExpression(PrefixExpressionSyntax expression)
        {
            var dotSeparator = EatToken(SyntaxKind.DotToken);
            var memberName = EatToken(SyntaxKind.IdentifierToken);
            return SyntaxFactory.MemberAccessExpression(expression, dotSeparator, memberName);
        }

        private ElementAccessExpressionSyntax ParseElementAccessExpression(PrefixExpressionSyntax expression)
        {
            var openBracketToken = EatToken(SyntaxKind.OpenBracketToken);
            var elementExpression = ParseExpression();
            var closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);

            return SyntaxFactory.ElementAccessExpression(
                expression,
                openBracketToken,
                elementExpression,
                closeBracketToken);
        }

        private MethodCallExpressionSyntax ParseMethodCallExpression(PrefixExpressionSyntax expression)
        {
            var colonToken = EatToken(SyntaxKind.ColonToken);
            var identifier = EatToken(SyntaxKind.IdentifierToken);
            var arguments = ParseFunctionArgument();

            return SyntaxFactory.MethodCallExpression(
                expression,
                colonToken,
                identifier,
                arguments);
        }

        private FunctionCallExpressionSyntax ParseFunctionCall(PrefixExpressionSyntax expression)
        {
            var arguments = ParseFunctionArgument();
            return SyntaxFactory.FunctionCallExpression(
                expression,
                arguments);
        }

        private FunctionArgumentSyntax ParseFunctionArgument()
        {
            if (IsIncremental && CurrentNode is Syntax.FunctionArgumentSyntax)
                return (FunctionArgumentSyntax) EatNode();

            if (CurrentToken.Kind is SyntaxKind.StringLiteralToken)
            {
                var literal = ParseLiteralExpression();
                return SyntaxFactory.StringFunctionArgument(literal);
            }
            else if (CurrentToken.Kind is SyntaxKind.OpenBraceToken)
            {
                var tableConstructor = ParseTableConstructorExpression();
                return SyntaxFactory.TableConstructorFunctionArgument(tableConstructor);
            }
            else
            {
                return ParseFunctionArgumentList();
            }
        }

        private ExpressionListFunctionArgumentSyntax ParseFunctionArgumentList()
        {
            var openParenthesisToken = EatToken(SyntaxKind.OpenParenthesisToken);
            var argumentsAndSeparatorsBuilder = _pool.AllocateSeparated<ExpressionSyntax>();
            while (CurrentToken.Kind is not (SyntaxKind.CloseParenthesisToken or SyntaxKind.EndOfFileToken))
            {
                var argument = ParseExpression();
                argumentsAndSeparatorsBuilder.Add(argument);
                if (CurrentToken.Kind is SyntaxKind.CommaToken)
                {
                    var separator = EatToken(SyntaxKind.CommaToken);
                    argumentsAndSeparatorsBuilder.AddSeparator(separator);
                }
                else
                {
                    break;
                }
            }
            var closeParenthesisToken = EatToken(SyntaxKind.CloseParenthesisToken);

            return SyntaxFactory.ExpressionListFunctionArgument(
                openParenthesisToken,
                _pool.ToListAndFree(argumentsAndSeparatorsBuilder),
                closeParenthesisToken);
        }

        private ParameterListSyntax ParseParameterList()
        {
            var parametersAndSeparatorsBuilder = _pool.AllocateSeparated<ParameterSyntax>();

            var openParenthesisToken = EatToken(SyntaxKind.OpenParenthesisToken);
            while (CurrentToken.Kind is not (SyntaxKind.CloseParenthesisToken or SyntaxKind.EndOfFileToken))
            {

                if (CurrentToken.Kind == SyntaxKind.DotDotDotToken)
                {
                    var varArgToken = EatToken(SyntaxKind.DotDotDotToken);

                    TypeBindingSyntax? optionalTypeBinding = null;
                    if (CurrentToken.Kind == SyntaxKind.ColonToken)
                    {
                        var colonToken = EatToken();
                        var type = ParseType();

                        optionalTypeBinding = SyntaxFactory.TypeBinding(colonToken, type);
                    }

                    var varArgparameter = SyntaxFactory.VarArgParameter(varArgToken, optionalTypeBinding);

                    parametersAndSeparatorsBuilder.Add(varArgparameter);
                    break;
                }

                var identifier = EatToken(SyntaxKind.IdentifierToken);

                TypeBindingSyntax? typeBinding = null;
                if (CurrentToken.Kind == SyntaxKind.ColonToken)
                {
                    var colonToken = EatToken();
                    var type = ParseType();

                    typeBinding = SyntaxFactory.TypeBinding(colonToken, type);
                }

                var parameter = SyntaxFactory.NamedParameter(identifier, typeBinding);
                parametersAndSeparatorsBuilder.Add(parameter);

                if (CurrentToken.Kind == SyntaxKind.CommaToken)
                {
                    parametersAndSeparatorsBuilder.AddSeparator(EatToken(SyntaxKind.CommaToken));
                }
                else
                {
                    break;
                }
            }

            var closeParenthesisToken = EatToken(SyntaxKind.CloseParenthesisToken);

            return SyntaxFactory.ParameterList(
                openParenthesisToken,
                _pool.ToListAndFree(parametersAndSeparatorsBuilder),
                closeParenthesisToken);
        }

        private VarArgExpressionSyntax ParseVarArgExpression()
        {
            var varargToken = EatToken(SyntaxKind.DotDotDotToken);
            return SyntaxFactory.VarArgExpression(varargToken);
        }

        private LiteralExpressionSyntax ParseLiteralExpression()
        {
            var token = EatToken();
            var kind = SyntaxFacts.GetLiteralExpression(token.Kind).Value;
            return SyntaxFactory.LiteralExpression(kind, token);
        }

        private ParenthesizedExpressionSyntax ParseParenthesizedExpression()
        {
            var open = EatToken(SyntaxKind.OpenParenthesisToken);
            var expression = ParseExpression();
            var closing = EatToken(SyntaxKind.CloseParenthesisToken);
            return SyntaxFactory.ParenthesizedExpression(open, expression, closing);
        }

        private TableConstructorExpressionSyntax ParseTableConstructorExpression()
        {
            var openBraceToken = EatToken(SyntaxKind.OpenBraceToken);

            var fieldsAndSeparatorsBuilder = _pool.AllocateSeparated<TableFieldSyntax>();
            while (CurrentToken.Kind is not (SyntaxKind.CloseBraceToken or SyntaxKind.EndOfFileToken))
            {
                var field = ParseTableField();
                fieldsAndSeparatorsBuilder.Add(field);

                if (CurrentToken.Kind is SyntaxKind.CommaToken or SyntaxKind.SemicolonToken)
                {
                    var separatorToken = EatToken();
                    fieldsAndSeparatorsBuilder.AddSeparator(separatorToken);
                }
                else
                {
                    break;
                }
            }

            var closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);

            return SyntaxFactory.TableConstructorExpression(
                openBraceToken,
                _pool.ToListAndFree(fieldsAndSeparatorsBuilder),
                closeBraceToken);
        }

        private TableFieldSyntax ParseTableField()
        {
            if (CurrentToken.Kind == SyntaxKind.IdentifierToken && PeekToken(1).Kind == SyntaxKind.EqualsToken)
            {
                var identifier = EatToken(SyntaxKind.IdentifierToken);
                var equalsToken = EatToken(SyntaxKind.EqualsToken);
                var value = ParseExpression();

                return SyntaxFactory.IdentifierKeyedTableField(identifier, equalsToken, value);
            }
            else if (CurrentToken.Kind == SyntaxKind.OpenBracketToken)
            {
                var openBracketToken = EatToken(SyntaxKind.OpenBracketToken);
                var key = ParseExpression();
                var closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
                var equalsToken = EatToken(SyntaxKind.EqualsToken);
                var value = ParseExpression();

                return SyntaxFactory.ExpressionKeyedTableField(openBracketToken, key, closeBracketToken, equalsToken, value);
            }
            else
            {
                var value = ParseExpression();
                return SyntaxFactory.UnkeyedTableField(value);
            }
        }

        private IfExpressionSyntax ParseIfExpression()
        {
            var ifKeyword = EatToken(SyntaxKind.IfKeyword);

            ExpressionSyntax condition;
            // Check for missing condition
            if (CurrentToken.Kind == SyntaxKind.ThenKeyword)
            {
                // And generate a "missing expression" for it with an error
                condition = AddError(
                    CreateMissingIdentifierName(),
                    ErrorCode.ERR_IfExpressionConditionExpected);
            }
            else
            {
                condition = ParseExpression();
            }
            var thenKeyword = EatToken(SyntaxKind.ThenKeyword);
            var trueValue = ParseExpression();

            var elseIfClauses = _pool.Allocate<ElseIfExpressionClauseSyntax>();
            while (CurrentToken.Kind == SyntaxKind.ElseIfKeyword)
            {
                var elseIfKeyword = EatToken(SyntaxKind.ElseIfKeyword);
                ExpressionSyntax elseIfCondition;
                // Check for missing condition
                if (CurrentToken.Kind == SyntaxKind.ThenKeyword)
                {
                    // And generate a "missing expression" for it with an error
                    elseIfCondition = AddError(
                        CreateMissingIdentifierName(),
                        ErrorCode.ERR_IfExpressionConditionExpected);
                }
                else
                {
                    elseIfCondition = ParseExpression();
                }
                var elseIfThenKeyword = EatToken(SyntaxKind.ThenKeyword);
                var elseIfValue = ParseExpression();

                elseIfClauses.Add(SyntaxFactory.ElseIfExpressionClause(
                    elseIfKeyword,
                    elseIfCondition,
                    elseIfThenKeyword,
                    elseIfValue));
            }

            var elseKeyword = EatToken(SyntaxKind.ElseKeyword);
            var falseValue = ParseExpression();

            var ifExpression = SyntaxFactory.IfExpression(
                ifKeyword,
                condition,
                thenKeyword,
                trueValue,
                _pool.ToListAndFree(elseIfClauses),
                elseKeyword,
                falseValue);
            if (!Options.SyntaxOptions.AcceptIfExpressions)
                ifExpression = AddError(ifExpression, ErrorCode.ERR_IfExpressionsNotSupportedInLuaVersion);
            return ifExpression;
        }

        public TypeSyntax ParseType()
        {
            var type = ParseSimpleType();

            var isNilable = false;
            var isUnion = false;
            var isIntersection = false;

            while (CurrentToken.Kind is SyntaxKind.QuestionToken or SyntaxKind.PipeToken or SyntaxKind.AmpersandToken)
            {
                if (CurrentToken.Kind == SyntaxKind.QuestionToken)
                {
                    var questionToken = EatTokenWithPrejudice(SyntaxKind.QuestionToken);
                    type = SyntaxFactory.NilableType(type, questionToken);
                    isNilable = true;
                }
                else if (CurrentToken.Kind == SyntaxKind.PipeToken)
                {
                    var pipeToken = EatTokenWithPrejudice(SyntaxKind.PipeToken);
                    var rightType = ParseType();
                    type = SyntaxFactory.UnionType(type, pipeToken, rightType);
                    isUnion = true;
                }
                else if (CurrentToken.Kind == SyntaxKind.AmpersandToken)
                {
                    var ampersandToken = EatTokenWithPrejudice(SyntaxKind.AmpersandToken);
                    var rightType = ParseType();
                    type = SyntaxFactory.IntersectionType(type, ampersandToken, rightType);
                    isIntersection = true;
                }
            }

            if (isNilable && isIntersection)
            {
                type = AddError(type, ErrorCode.ERR_MixingNilableAndIntersectionNotAllowed);
            }
            if (isUnion && isIntersection)
            {
                type = AddError(type, ErrorCode.ERR_MixingUnionsAndIntersectionsNotAllowed);
            }

            return type;
        }

        private TypeSyntax ParseSimpleType()
        {
            return CurrentToken.Kind switch
            {
                // function or parenthesized
                SyntaxKind.OpenParenthesisToken =>
                    ParseTypeStartingWithParenthesis(acceptPacks: true),
                // table
                SyntaxKind.OpenBraceToken =>
                    ParseTableBasedType(),
                // typeof type
                SyntaxKind.IdentifierToken when CurrentToken.ContextualKind is SyntaxKind.TypeofKeyword =>
                    ParseTypeofType(),
                // literal types
                SyntaxKind.StringLiteralToken =>
                    SyntaxFactory.LiteralType(SyntaxKind.StringType, EatToken(SyntaxKind.StringLiteralToken)),
                SyntaxKind.NilKeyword =>
                    SyntaxFactory.LiteralType(SyntaxKind.NilType, EatToken(SyntaxKind.NilKeyword)),
                SyntaxKind.TrueKeyword =>
                    SyntaxFactory.LiteralType(SyntaxKind.TrueType, EatToken(SyntaxKind.TrueKeyword)),
                SyntaxKind.FalseKeyword =>
                    SyntaxFactory.LiteralType(SyntaxKind.FalseType, EatToken(SyntaxKind.FalseKeyword)),
                // generic types
                SyntaxKind.LessThanToken =>
                    ParseGeneric(),

                // If everything else fails, try to parse an identifier-based name.
                _ => ParseTypeName(),
            };
        }

        private TypeSyntax ParseTypePack()
        {
            throw new NotImplementedException();
            //var typePack = _pool.AllocateSeparated<TypeSyntax>();
            //var type = ParseSimpleType();

            //typePack.Add(type);

            //while (CurrentToken.Kind is SyntaxKind.CommaToken)
            //{
            //    var separator = EatToken(SyntaxKind.CommaToken);
            //    typePack.AddSeparator(separator);

            //    switch (CurrentToken.Kind)
            //    {
            //        case SyntaxKind.DotDotDotToken:
            //            var dot3Token = EatToken();
            //            typePack.Add(SyntaxFactory.VariadicTypePack(dot3Token, ParseType()));
            //            break;
            //        case SyntaxKind.OpenParenthesisToken:
            //            var parsed = ParseTypeStartingWithParenthesis(acceptPacks: true);
            //            typePack.Add(parsed);
            //            break;

            //        default:
            //            typePack.Add(ParseType());

            //            break;
            //    }
            //}

            //return SyntaxFactory.TypePack(SyntaxKind.OpenParenthesisToken, typePack, SyntaxKind.CloseParenthesisToken);
        }

        private TypeSyntax ParseGeneric()
        {
            throw new NotImplementedException();
            var lessThanToken = EatToken();
            var typesList = _pool.AllocateSeparated<TypeSyntax>();

            while (CurrentToken.Kind is SyntaxKind.CommaToken)
            {
            }
        }

        private TypeSyntax ParseReturnType()
        {
            if (CurrentToken.Kind is SyntaxKind.OpenParenthesisToken)
            {
                return ParseTypeStartingWithParenthesis(acceptPacks: false);
            }
            else
            {
                return ParseType();
            }
        }

        private TypeSyntax ParseTypeStartingWithParenthesis(bool acceptPacks)
        {
            var openParenthesisToken = EatToken();
            var typesListBuilder = _pool.AllocateSeparated<TypeSyntax>();

            if (CurrentToken.Kind != SyntaxKind.CloseParenthesisToken)
            {
                var type = ParseType();
                typesListBuilder.Add(type);

                while (CurrentToken.Kind is SyntaxKind.CommaToken)
                {
                    var separator = EatToken(SyntaxKind.CommaToken);
                    typesListBuilder.AddSeparator(separator);

                    type = ParseType();
                    typesListBuilder.Add(type);
                }
            }

            var typesList = _pool.ToListAndFree(typesListBuilder);
            var closeParenthesisToken = EatToken(SyntaxKind.CloseParenthesisToken);

            if (CurrentToken.Kind == SyntaxKind.SlimArrowToken
                // If there's not an arrow, then we need to error if there's more than
                // one type in the list and we aren't allowed to accept packs
                || (typesList.Count != 1 && !acceptPacks))
            {
                var slimArrow = EatToken(SyntaxKind.SlimArrowToken);
                var returnType = ParseReturnType();

                return SyntaxFactory.FunctionType(
                    openParenthesisToken,
                    typesList,
                    closeParenthesisToken,
                    slimArrow,
                    returnType);
            }
            else if (typesList.Count == 1)
            {
                var type = typesList[0]!;
                return SyntaxFactory.ParenthesizedType(
                    openParenthesisToken,
                    type,
                    closeParenthesisToken);
            }

            return SyntaxFactory.TypePack(
                openParenthesisToken,
                typesList,
                closeParenthesisToken);
        }

        private TableBasedTypeSyntax ParseTableBasedType()
        {
            var openBrace = EatToken();

            if (CurrentToken.Kind is SyntaxKind.OpenBracketToken or SyntaxKind.CloseBraceToken
                || PeekToken(1).Kind == SyntaxKind.ColonToken)
            {
                var elementsBuilder = _pool.AllocateSeparated<TableTypeElementSyntax>();
                while (CurrentToken.Kind is not (SyntaxKind.CloseBraceToken or SyntaxKind.EndOfFileToken))
                {
                    var element = ParseTableTypeElement();
                    elementsBuilder.Add(element);

                    // If there's a separator, then there might be a field next
                    if (CurrentToken.Kind is SyntaxKind.CommaToken or SyntaxKind.SemicolonToken)
                    {
                        elementsBuilder.AddSeparator(EatToken());
                    }
                    else
                    {
                        // Otherwise we're done.
                        break;
                    }
                }

                var elements = _pool.ToListAndFree(elementsBuilder);
                var closeBrace = EatToken(SyntaxKind.CloseBraceToken);
                return SyntaxFactory.TableType(
                    openBrace,
                    elements,
                    closeBrace);
            }
            else
            {
                var type = ParseType();
                var closeBrace = EatToken(SyntaxKind.CloseBraceToken);

                return SyntaxFactory.ArrayType(
                    openBrace,
                    type,
                    closeBrace);
            }
        }

        private TableTypeElementSyntax ParseTableTypeElement()
        {
            if (CurrentToken.Kind is SyntaxKind.OpenBracketToken)
            {
                var openBracket = EatTokenWithPrejudice(SyntaxKind.OpenBracketToken);
                var indexType = ParseType();
                var closeBracket = EatToken(SyntaxKind.CloseBracketToken);
                var colon = EatToken(SyntaxKind.ColonToken);
                var valueType = ParseType();

                return SyntaxFactory.TableTypeIndexer(
                    openBracket,
                    indexType,
                    closeBracket,
                    colon,
                    valueType);
            }
            else
            {
                var identifier = EatTokenWithPrejudice(SyntaxKind.IdentifierName);
                var colonToken = EatToken(SyntaxKind.ColonToken);
                var valueType = ParseType();

                return SyntaxFactory.TableTypeProperty(
                    identifier,
                    colonToken,
                    valueType);
            }
        }

        private TypeSyntax ParseTypeofType()
        {
            var typeofKeyword = EatContextualToken(SyntaxKind.TypeofKeyword);
            var openParenthesis = EatToken(SyntaxKind.OpenParenthesisToken);
            var expression = ParseExpression();
            var closeParenthesis = EatToken(SyntaxKind.CloseParenthesisToken);

            return SyntaxFactory.TypeofType(typeofKeyword, openParenthesis, expression, closeParenthesis);
        }

        private TypeNameSyntax ParseTypeName()
        {
            var identifier = EatToken(SyntaxKind.IdentifierName);
            TypeNameSyntax currentName = SyntaxFactory.SimpleTypeName(identifier);

            while (CurrentToken.Kind is SyntaxKind.DotToken)
            {
                var dotToken = EatToken(SyntaxKind.DotToken);
                var memberName = EatToken(SyntaxKind.IdentifierName);
                currentName = SyntaxFactory.CompositeTypeName(currentName, dotToken, memberName);
            }

            return currentName;
        }

        private static bool NoTriviaBetween(SyntaxToken left, SyntaxToken right) =>
            left.GetTrailingTriviaWidth() == 0 && right.GetLeadingTriviaWidth() == 0;


        /// <summary>
        /// Creates a missing <see cref="IdentifierNameSyntax"/>.
        /// Used for places where we expected an expression but got something else.
        /// </summary>
        /// <returns></returns>
        private static IdentifierNameSyntax CreateMissingIdentifierName() =>
            SyntaxFactory.IdentifierName(CreateMissingIdentifierToken());

        /// <summary>
        /// Creates a missing identifier <see cref="SyntaxToken"/>.
        /// Used for places where we expected a token but got something else.
        /// </summary>
        /// <returns></returns>
        private static SyntaxToken CreateMissingIdentifierToken() =>
            SyntaxToken.CreateMissing(SyntaxKind.IdentifierToken, null, null);

        internal TNode ConsumeUnexpectedTokens<TNode>(TNode node) where TNode : LuaSyntaxNode
        {
            if (CurrentToken.Kind == SyntaxKind.EndOfFileToken) return node;

            var builder = _pool.Allocate<SyntaxToken>();
            while (CurrentToken.Kind != SyntaxKind.EndOfFileToken)
            {
                builder.Add(EatToken());
            }
            var trailingTrash = _pool.ToListAndFree(builder);

            node = AddError(node, ErrorCode.ERR_UnexpectedToken, trailingTrash[0]!.ToString());
            node = AddTrailingSkippedSyntax(node, trailingTrash.Node);
            return node;
        }
    }
}
