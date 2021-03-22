using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    using System.Threading;
    using Loretta.CodeAnalysis.Syntax.InternalSyntax;

    internal sealed class LanguageParser : SyntaxParser
    {
        private readonly SyntaxListPool _pool = new SyntaxListPool();
        private int _recursionDepth;

        public LanguageParser(
            Lexer lexer,
            Lua.LuaSyntaxNode oldTree,
            IEnumerable<TextChangeRange> changes,
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
            RoslynDebug.Assert(_recursionDepth == 0);

            try
            {
                return parseFunc();
            }
            catch (InsufficientExecutionStackException)
            {
                return CreateForGlobalFailure(_lexer.Position, createEmptyNodeFunc());
            }
        }

        private TNode CreateForGlobalFailure<TNode>(int position, TNode node) where TNode : LuaSyntaxNode
        {
            // Turn the complete input into a single skipped token. This avoids running the lexer, and therefore
            // the preprocessor directive parser, which may itself run into the same problem that caused the
            // original failure.
            var builder = new SyntaxListBuilder(1);
            builder.Add(SyntaxFactory.BadToken(null, _lexer.Text.ToString(), null));
            var fileAsTrivia = SyntaxFactory.SkippedTokensTrivia(builder.ToList<SyntaxToken>());
            node = AddLeadingSkippedSyntax(node, fileAsTrivia);
            ForceEndOfFile(); // force the scanner to report that it is at the end of the input.
            return AddError(node, position, 0, ErrorCode.ERR_InsufficientStack);
        }

        private StatementListSyntax ParseStatementList(params SyntaxKind[] terminalKinds)
        {
            var builder = _pool.Allocate<StatementSyntax>();
            var progress = -1;
            while (IsMakingProgress(ref progress))
            {
                var kind = CurrentToken.Kind;
                if (kind == SyntaxKind.EndOfFileToken || terminalKinds.Contains(kind))
                    break;

                var startToken = CurrentToken;

                var statement = ParseStatement();
                builder.Add(statement);

                // If ParseStatement did not consume any tokens, we have to advance ourselves
                // otherwise we get stuck in an infinite loop.
                if (CurrentToken == startToken)
                    _ = EatToken();
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

                    default:
                    {
                        if (CurrentToken.ContextualKind == SyntaxKind.ContinueKeyword)
                            return ParseContinueStatement();

                        var expression = ParsePrefixOrVariableExpression();
                        if (expression.Kind is SyntaxKind.BadExpression)
                        {
                            var semicolon = TryMatchSemicolon();
                            return SyntaxFactory.BadStatement((BadExpressionSyntax) expression, semicolon);
                        }

                        if (CurrentToken.Kind is SyntaxKind.CommaToken or SyntaxKind.EqualsToken)
                        {
                            return ParseAssignmentStatement(expression);
                        }
                        else if (SyntaxFacts.IsCompoundAssignmentOperatorToken(CurrentToken.Kind))
                        {
                            return ParseCompoundAssignment(expression);
                        }
                        else
                        {
                            var semicolonToken = TryMatchSemicolon();
                            var node = SyntaxFactory.ExpressionStatement(expression, semicolonToken);
                            if (expression.Kind is not (SyntaxKind.FunctionCallExpression or SyntaxKind.MethodCallExpression))
                                node = AddError(node, ErrorCode.ERR_NonFunctionCallBeingUsedAsStatement);
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

        private LocalVariableDeclarationStatementSyntax ParseLocalVariableDeclarationStatement()
        {
            var localKeyword = EatToken(SyntaxKind.LocalKeyword);
            var namesAndSeparatorsBuilder =
                _pool.AllocateSeparated<IdentifierNameSyntax>();

            var name = ParseIdentifierName();
            namesAndSeparatorsBuilder.Add(name);

            while (CurrentToken.Kind is SyntaxKind.CommaToken)
            {
                var separator = EatToken(SyntaxKind.CommaToken);
                namesAndSeparatorsBuilder.AddSeparator(separator);

                name = ParseIdentifierName();
                namesAndSeparatorsBuilder.Add(name);
            }

            var names = _pool.ToListAndFree(namesAndSeparatorsBuilder);
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

                var semicolonToken = TryMatchSemicolon();

                var values = _pool.ToListAndFree(expressionsAndSeparatorsBuilder);

                return SyntaxFactory.LocalVariableDeclarationStatement(
                    localKeyword,
                    names,
                    equalsToken,
                    values,
                    semicolonToken);
            }
            else
            {
                var semicolonToken = TryMatchSemicolon();

                return SyntaxFactory.LocalVariableDeclarationStatement(
                    localKeyword,
                    names,
                    equalsToken: null,
                    values: default,
                    semicolonToken);
            }
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
                identifier,
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
                _pool.AllocateSeparated<IdentifierNameSyntax>();

            var identifier = ParseIdentifierName();
            identifiersAndSeparatorsBuilder.Add(identifier);
            while (CurrentToken.Kind is SyntaxKind.CommaToken)
            {
                var separator = EatToken(SyntaxKind.CommaToken);
                identifiersAndSeparatorsBuilder.AddSeparator(separator);

                identifier = ParseIdentifierName();
                identifiersAndSeparatorsBuilder.Add(identifier);
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
            var continueKeyword = EatContextualToken(SyntaxKind.ContinueKeyword);
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
                equalsToken,
                values,
                semicolonToken);
        }

        private CompoundAssignmentStatementSyntax ParseCompoundAssignment(PrefixExpressionSyntax variable)
        {
            RoslynDebug.Assert(SyntaxFacts.IsCompoundAssignmentOperatorToken(CurrentToken.Kind));
            if (!SyntaxFacts.IsVariableExpression(variable.Kind))
                variable = AddError(variable, ErrorCode.ERR_CannotBeAssignedTo);
            var assignmentOperatorToken = EatToken();
            var kind = SyntaxFacts.GetCompoundAssignmentStatement(assignmentOperatorToken.Kind).Value;
            var expression = ParseExpression();
            var semicolonToken = TryMatchSemicolon();
            return SyntaxFactory.CompoundAssignmentStatement(
                kind,
                variable,
                assignmentOperatorToken,
                expression,
                semicolonToken);
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

        private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0, SyntaxKind parentOperator = SyntaxKind.BadToken, bool isParentUnary = false)
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
                SyntaxKind operatorKind = CurrentToken.Kind;
                var precedence = SyntaxFacts.GetBinaryOperatorPrecedence(operatorKind);
                var comparePrecedence = !isParentUnary && parentOperator == operatorKind && SyntaxFacts.IsRightAssociative(operatorKind)
                    ? precedence + 1
                    : precedence;
                if (precedence <= 0 || comparePrecedence <= parentPrecedence)
                    break;

                var operatorToken = EatToken();
                var kind = SyntaxFacts.GetBinaryExpression(operatorToken.Kind).Value;
                var right = ParseBinaryExpression(precedence, operatorKind, false);
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
                or SyntaxKind.StringLiteralToken => ParseLiteralExpression(),
                SyntaxKind.DotDotDotToken => ParseVarArgExpression(),
                SyntaxKind.OpenBraceToken => ParseTableConstructorExpression(),
                SyntaxKind.FunctionKeyword when PeekToken(1).Kind == SyntaxKind.OpenParenthesisToken => ParseAnonymousFunctionExpression(),
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
                return SyntaxFactory.BadExpression(EatToken());
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

        private IdentifierNameSyntax ParseIdentifierName() =>
            SyntaxFactory.IdentifierName(EatToken(SyntaxKind.IdentifierToken));

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
                    var varArgparameter = SyntaxFactory.VarArgParameter(varArgToken);
                    parametersAndSeparatorsBuilder.Add(varArgparameter);
                    break;
                }

                var identifier = EatToken(SyntaxKind.IdentifierToken);
                var parameter = SyntaxFactory.NamedParameter(identifier);
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
            SyntaxToken? token = EatToken();
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
    }
}
