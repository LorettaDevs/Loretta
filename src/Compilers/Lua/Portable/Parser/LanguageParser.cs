using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;
using Tsu;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal sealed class LanguageParser
    {
        private readonly LuaSyntaxOptions _luaOptions;
        private readonly SourceText _text;
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private int _position;

        public LanguageParser(Lexer lexer)
        {
            var tokens = ImmutableArray.CreateBuilder<SyntaxToken>();
            var badTokens = new List<SyntaxToken>();

            SyntaxToken token;
            do
            {
                token = lexer.Lex();

                if (token.Kind == SyntaxKind.BadToken)
                {
                    badTokens.Add(token);
                }
                else
                {
                    if (badTokens.Count > 0)
                    {
                        var leadingTrivia = token.LeadingTrivia.ToBuilder();
                        var index = 0;

                        foreach (var badToken in badTokens)
                        {
                            foreach (var lt in badToken.LeadingTrivia)
                                leadingTrivia.Insert(index++, lt);

                            var trivia = new SyntaxTrivia(SyntaxKind.SkippedTokensTrivia, badToken.Position, badToken.Text.Value);
                            leadingTrivia.Insert(index++, trivia);

                            foreach (var tt in badToken.TrailingTrivia)
                                leadingTrivia.Insert(index++, tt);
                        }

                        badTokens.Clear();
                        token = new SyntaxToken(token.Kind, token.Position, token.Text, token.Value, leadingTrivia.ToImmutable(), token.TrailingTrivia, false);
                    }

                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            _luaOptions = syntaxTree.Options;
            _text = syntaxTree.Text;
            _tokens = tokens.ToImmutable();
            Diagnostics.AddRange(lexer.Diagnostics);
        }

        public DiagnosticBag Diagnostics { get; } = new DiagnosticBag();

        private SyntaxToken Peek(int offset) =>
            _tokens[Math.Min(_position + offset, _tokens.Length - 1)];

        private SyntaxToken Next()
        {
            var ret = Current;
            if (_position < _tokens.Length - 1)
                _position++;
            return ret;
        }

        private SyntaxToken Current => Peek(0);
        private SyntaxToken Lookahead => Peek(1);

        public SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return Next();

            Diagnostics.ReportUnexpectedToken(
                new TextLocation(_text, Current.Span),
                Current.Kind,
                kind);

            return new SyntaxToken(
                kind,
                Current.Position,
                SyntaxFacts.GetText(kind) is { } txt ? txt.AsMemory() : default,
                default,
                SyntaxTrivia.Empty,
                SyntaxTrivia.Empty,
                true);
        }

        private SyntaxToken MatchIdentifier()
        {
            if (_luaOptions.ContinueType == ContinueType.ContextualKeyword && Current.Kind == SyntaxKind.ContinueKeyword)
            {
                // Transforms the continue keyword into an identifier token on-the-fly.
                var continueKeyword = Next();
                return continueKeyword.WithKind(SyntaxKind.IdentifierToken);
            }
            return this.Match(SyntaxKind.IdentifierToken);
        }

        private Option<SyntaxToken> TryMatchSemicolon()
        {
            if (Current.Kind == SyntaxKind.SemicolonToken)
                return Next();
            return default;
        }

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var statements = ParseStatementList();
            SyntaxToken? endOfFileToken = this.Match(SyntaxKind.EndOfFileToken);
            return new CompilationUnitSyntax(statements, endOfFileToken);
        }

        private ImmutableArray<StatementSyntax> ParseStatementList(params SyntaxKind[] terminalKinds)
        {
            var builder = ImmutableArray.CreateBuilder<StatementSyntax>();
            while (true)
            {
                SyntaxKind kind = Current.Kind;
                if (kind == SyntaxKind.EndOfFileToken || terminalKinds.Contains(kind))
                    break;

                var startToken = Current;

                var statement = ParseStatement();
                builder.Add(statement);

                // If ParseStatement did not consume any tokens, we have to advance ourselves
                // otherwise we get stuck in an infinite loop.
                if (Current == startToken)
                    _ = Next();
            }
            return builder.ToImmutable();
        }

        private StatementSyntax ParseStatement()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.LocalKeyword:
                    if (Lookahead.Kind is SyntaxKind.FunctionKeyword)
                        return ParseLocalFunctionDeclarationStatement();
                    else
                        return ParseLocalVariableDeclarationStatement();

                case SyntaxKind.ForKeyword:
                    if (Peek(2).Kind == SyntaxKind.EqualsToken)
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
                    var expression = ParsePrefixOrVariableExpression();
                    if (expression.Kind is SyntaxKind.BadExpression)
                    {
                        return new BadStatementSyntax((BadExpressionSyntax) expression);
                    }
                    if (Current.Kind is SyntaxKind.CommaToken or SyntaxKind.EqualsToken)
                    {
                        return ParseAssignmentStatement(expression);
                    }
                    else if (SyntaxFacts.IsCompoundAssignmentOperatorToken(Current.Kind))
                    {
                        return ParseCompoundAssignment(expression);
                    }
                    else
                    {
                        if (expression.Kind is not (SyntaxKind.FunctionCallExpression or SyntaxKind.MethodCallExpression))
                            Diagnostics.ReportNonFunctionCallBeingUsedAsStatement(new TextLocation(_text, expression.Span));
                        var semicolonToken = TryMatchSemicolon();
                        return new ExpressionStatementSyntax(
                            expression,
                            semicolonToken);
                    }
                }
            }
        }

        private LocalVariableDeclarationStatementSyntax ParseLocalVariableDeclarationStatement()
        {
            var localKeyword = this.Match(SyntaxKind.LocalKeyword);
            var namesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();

            var name = ParseNameExpression();
            namesAndSeparators.Add(name);

            while (Current.Kind is SyntaxKind.CommaToken)
            {
                var separator = this.Match(SyntaxKind.CommaToken);
                namesAndSeparators.Add(separator);

                name = ParseNameExpression();
                namesAndSeparators.Add(name);
            }

            var names = new SeparatedSyntaxList<NameExpressionSyntax>(namesAndSeparators.ToImmutable());
            if (Current.Kind == SyntaxKind.EqualsToken)
            {
                var equalsToken = this.Match(SyntaxKind.EqualsToken);

                var expressionsAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();

                var value = ParseExpression();
                expressionsAndSeparators.Add(value);

                while (Current.Kind is SyntaxKind.CommaToken)
                {
                    var separator = this.Match(SyntaxKind.CommaToken);
                    expressionsAndSeparators.Add(separator);

                    value = ParseExpression();
                    expressionsAndSeparators.Add(value);
                }

                var semicolonToken = TryMatchSemicolon();

                var values = new SeparatedSyntaxList<ExpressionSyntax>(expressionsAndSeparators.ToImmutable());

                return new LocalVariableDeclarationStatementSyntax(
                    localKeyword,
                    names,
                    equalsToken,
                    values,
                    semicolonToken);
            }
            else
            {
                var semicolonToken = TryMatchSemicolon();

                return new LocalVariableDeclarationStatementSyntax(
                    localKeyword,
                    names,
                    default,
                    default,
                    semicolonToken);
            }
        }

        private LocalFunctionDeclarationStatementSyntax ParseLocalFunctionDeclarationStatement()
        {
            var localKeyword = this.Match(SyntaxKind.LocalKeyword);
            var functionKeyword = this.Match(SyntaxKind.FunctionKeyword);
            var identifier = MatchIdentifier();
            var parameters = ParseParameterList();
            var body = this.ParseStatementList(SyntaxKind.EndKeyword);
            var endKeyword = this.Match(SyntaxKind.EndKeyword);
            var semicolonToken = TryMatchSemicolon();

            return new LocalFunctionDeclarationStatementSyntax(
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
            var forKeyword = this.Match(SyntaxKind.ForKeyword);
            var identifier = MatchIdentifier();
            var equalsToken = this.Match(SyntaxKind.EqualsToken);
            var initialValue = ParseExpression();
            var finalValueCommaToken = this.Match(SyntaxKind.CommaToken);
            var finalValue = ParseExpression();
            Option<SyntaxToken> stepValueCommaToken = default;
            Option<ExpressionSyntax> stepValue = default;
            if (Current.Kind == SyntaxKind.CommaToken)
            {
                stepValueCommaToken = this.Match(SyntaxKind.CommaToken);
                stepValue = ParseExpression();
            }
            var doKeyword = this.Match(SyntaxKind.DoKeyword);
            var body = this.ParseStatementList(SyntaxKind.EndKeyword);
            var endKeyword = this.Match(SyntaxKind.EndKeyword);
            var semicolonToken = TryMatchSemicolon();
            return new NumericForStatementSyntax(
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
            var forKeyword = this.Match(SyntaxKind.ForKeyword);

            var identifiersAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>(3);

            var identifier = MatchIdentifier();
            identifiersAndSeparators.Add(identifier);
            while (Current.Kind is SyntaxKind.CommaToken)
            {
                var separator = this.Match(SyntaxKind.CommaToken);
                identifiersAndSeparators.Add(separator);

                identifier = MatchIdentifier();
                identifiersAndSeparators.Add(identifier);
            }

            var inKeyword = this.Match(SyntaxKind.InKeyword);

            var expressionsAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>(1);

            var expression = ParseExpression();
            expressionsAndSeparators.Add(expression);
            while (Current.Kind is SyntaxKind.CommaToken)
            {
                var separator = this.Match(SyntaxKind.CommaToken);
                expressionsAndSeparators.Add(separator);

                expression = ParseExpression();
                expressionsAndSeparators.Add(expression);
            }

            var doKeyword = this.Match(SyntaxKind.DoKeyword);
            var body = this.ParseStatementList(SyntaxKind.EndKeyword);
            var endKeyword = this.Match(SyntaxKind.EndKeyword);
            var semicolonToken = TryMatchSemicolon();

            var identifiers = new SeparatedSyntaxList<SyntaxToken>(identifiersAndSeparators.ToImmutable());
            var expressions = new SeparatedSyntaxList<ExpressionSyntax>(expressionsAndSeparators.ToImmutable());
            return new GenericForStatementSyntax(
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
            var ifKeyword = this.Match(SyntaxKind.IfKeyword);
            var condition = ParseExpression();
            var thenKeyword = this.Match(SyntaxKind.ThenKeyword);
            var body = this.ParseStatementList(SyntaxKind.ElseIfKeyword, SyntaxKind.ElseKeyword, SyntaxKind.EndKeyword);

            var elseIfClausesBuilder = ImmutableArray.CreateBuilder<ElseIfClauseSyntax>();
            while (Current.Kind is SyntaxKind.ElseIfKeyword)
            {
                var elseIfKeyword = this.Match(SyntaxKind.ElseIfKeyword);
                var elseIfCondition = ParseExpression();
                var elseIfThenKeyword = this.Match(SyntaxKind.ThenKeyword);
                var elseIfBody = this.ParseStatementList(SyntaxKind.ElseIfKeyword, SyntaxKind.ElseKeyword, SyntaxKind.EndKeyword);

                elseIfClausesBuilder.Add(new ElseIfClauseSyntax(
                    elseIfKeyword,
                    elseIfCondition,
                    elseIfThenKeyword,
                    elseIfBody));
            }

            Option<ElseClauseSyntax> elseClause = default;
            if (Current.Kind is SyntaxKind.ElseKeyword)
            {
                var elseKeyword = this.Match(SyntaxKind.ElseKeyword);
                var elseBody = this.ParseStatementList(SyntaxKind.EndKeyword);
                elseClause = new ElseClauseSyntax(elseKeyword, elseBody);
            }

            var endKeyword = this.Match(SyntaxKind.EndKeyword);
            var semicolonToken = TryMatchSemicolon();

            var elseIfClauses = elseIfClausesBuilder.ToImmutable();
            return new IfStatementSyntax(
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
            var repeatKeyword = this.Match(SyntaxKind.RepeatKeyword);
            var body = this.ParseStatementList(SyntaxKind.UntilKeyword);
            var untilKeyword = this.Match(SyntaxKind.UntilKeyword);
            var condition = ParseExpression();
            var semicolonToken = TryMatchSemicolon();
            return new RepeatUntilStatementSyntax(
                repeatKeyword,
                body,
                untilKeyword,
                condition,
                semicolonToken);
        }

        private WhileStatementSyntax ParseWhileStatement()
        {
            var whileKeyword = this.Match(SyntaxKind.WhileKeyword);
            var condition = ParseExpression();
            var doKeyword = this.Match(SyntaxKind.DoKeyword);
            var body = this.ParseStatementList(SyntaxKind.EndKeyword);
            var endKeyword = this.Match(SyntaxKind.EndKeyword);
            var semicolonToken = TryMatchSemicolon();
            return new WhileStatementSyntax(
                whileKeyword,
                condition,
                doKeyword,
                body,
                endKeyword,
                semicolonToken);
        }

        private DoStatementSyntax ParseDoStatement()
        {
            var doKeyword = this.Match(SyntaxKind.DoKeyword);
            var body = this.ParseStatementList(SyntaxKind.EndKeyword);
            var endKeyword = this.Match(SyntaxKind.EndKeyword);
            var semicolonToken = TryMatchSemicolon();
            return new DoStatementSyntax(
                doKeyword,
                body,
                endKeyword,
                semicolonToken);
        }

        private GotoStatementSyntax ParseGotoStatement()
        {
            var gotoKeyword = this.Match(SyntaxKind.GotoKeyword);
            var labelName = MatchIdentifier();
            var semicolonToken = TryMatchSemicolon();
            return new GotoStatementSyntax(
                gotoKeyword,
                labelName,
                semicolonToken);
        }

        private BreakStatementSyntax ParseBreakStatement()
        {
            var breakKeyword = this.Match(SyntaxKind.BreakKeyword);
            var semicolonToken = TryMatchSemicolon();
            return new BreakStatementSyntax(breakKeyword, semicolonToken);
        }

        private ContinueStatementSyntax ParseContinueStatement()
        {
            SyntaxToken? continueKeyword = this.Match(SyntaxKind.ContinueKeyword);
            var semicolonToken = TryMatchSemicolon();
            return new ContinueStatementSyntax(continueKeyword, semicolonToken);
        }

        private GotoLabelStatementSyntax ParseGotoLabelStatement()
        {
            var leftDelimiterToken = this.Match(SyntaxKind.ColonColonToken);
            var identifier = MatchIdentifier();
            var rightDelimiterToken = this.Match(SyntaxKind.ColonColonToken);
            var semicolonToken = TryMatchSemicolon();
            return new GotoLabelStatementSyntax(
                leftDelimiterToken,
                identifier,
                rightDelimiterToken,
                semicolonToken);
        }

        private FunctionDeclarationStatementSyntax ParseFunctionDeclarationStatement()
        {
            var functionKeyword = this.Match(SyntaxKind.FunctionKeyword);
            var name = ParseFunctionName();
            var parameters = ParseParameterList();
            var body = this.ParseStatementList(SyntaxKind.EndKeyword);
            var endKeyword = this.Match(SyntaxKind.EndKeyword);
            var semicolonToken = TryMatchSemicolon();
            return new FunctionDeclarationStatementSyntax(
                functionKeyword,
                name,
                parameters,
                body,
                endKeyword,
                semicolonToken);
        }

        private FunctionNameSyntax ParseFunctionName()
        {
            var identifier = MatchIdentifier();
            FunctionNameSyntax name = new SimpleFunctionNameSyntax(identifier);

            while (Current.Kind == SyntaxKind.DotToken)
            {
                var dotToken = this.Match(SyntaxKind.DotToken);
                identifier = MatchIdentifier();
                name = new MemberFunctionNameSyntax(name, dotToken, identifier);
            }

            if (Current.Kind == SyntaxKind.ColonToken)
            {
                var colonToken = this.Match(SyntaxKind.ColonToken);
                identifier = MatchIdentifier();
                name = new MethodFunctionNameSyntax(name, colonToken, identifier);
            }

            return name;
        }

        private ReturnStatementSyntax ParseReturnStatement()
        {
            var returnKeyword = this.Match(SyntaxKind.ReturnKeyword);
            SeparatedSyntaxList<ExpressionSyntax>? expressions = null;
            if (Current.Kind is not (SyntaxKind.ElseKeyword or SyntaxKind.ElseIfKeyword or SyntaxKind.EndKeyword or SyntaxKind.UntilKeyword or SyntaxKind.SemicolonToken))
            {
                var expressionsAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>(1);
                var expression = ParseExpression();
                expressionsAndSeparators.Add(expression);

                while (Current.Kind == SyntaxKind.CommaToken)
                {
                    var separator = this.Match(SyntaxKind.CommaToken);
                    expressionsAndSeparators.Add(separator);

                    expression = ParseExpression();
                    expressionsAndSeparators.Add(expression);
                }

                expressions = new SeparatedSyntaxList<ExpressionSyntax>(expressionsAndSeparators.ToImmutable());
            }
            expressions ??= new SeparatedSyntaxList<ExpressionSyntax>(ImmutableArray<SyntaxNode>.Empty);
            var semicolonToken = TryMatchSemicolon();
            return new ReturnStatementSyntax(
                returnKeyword,
                expressions,
                semicolonToken);
        }

        private AssignmentStatementSyntax ParseAssignmentStatement(PrefixExpressionSyntax variable)
        {
            var variablesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>(1);
            if (!SyntaxFacts.IsVariableExpression(variable.Kind))
                Diagnostics.ReportCannotBeAssignedTo(new TextLocation(_text, variable.Span));
            variablesAndSeparators.Add(variable);
            while (Current.Kind == SyntaxKind.CommaToken)
            {
                var separator = this.Match(SyntaxKind.CommaToken);
                variablesAndSeparators.Add(separator);

                variable = ParsePrefixOrVariableExpression();
                if (!SyntaxFacts.IsVariableExpression(variable.Kind))
                    Diagnostics.ReportCannotBeAssignedTo(new TextLocation(_text, variable.Span));
                variablesAndSeparators.Add(variable);
            }

            var equalsToken = this.Match(SyntaxKind.EqualsToken);

            var valuesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>(1);
            var value = ParseExpression();
            valuesAndSeparators.Add(value);
            while (Current.Kind == SyntaxKind.CommaToken)
            {
                var separator = this.Match(SyntaxKind.CommaToken);
                valuesAndSeparators.Add(separator);

                value = ParseExpression();
                valuesAndSeparators.Add(value);
            }

            var semicolonToken = TryMatchSemicolon();

            var variables = new SeparatedSyntaxList<PrefixExpressionSyntax>(variablesAndSeparators.ToImmutable());
            var values = new SeparatedSyntaxList<ExpressionSyntax>(valuesAndSeparators.ToImmutable());
            return new AssignmentStatementSyntax(
                variables,
                equalsToken,
                values,
                semicolonToken);
        }

        private CompoundAssignmentStatementSyntax ParseCompoundAssignment(PrefixExpressionSyntax variable)
        {
            RoslynDebug.Assert(SyntaxFacts.IsCompoundAssignmentOperatorToken(Current.Kind));
            if (!SyntaxFacts.IsVariableExpression(variable.Kind))
                Diagnostics.ReportCannotBeAssignedTo(new TextLocation(_text, variable.Span));
            var assignmentOperatorToken = Next();
            SyntaxKind kind = SyntaxFacts.GetCompoundAssignmentStatement(assignmentOperatorToken.Kind).Value;
            var expression = ParseExpression();
            var semicolonToken = TryMatchSemicolon();
            return new CompoundAssignmentStatementSyntax(
                kind,
                variable,
                assignmentOperatorToken,
                expression,
                semicolonToken);
        }

        public ExpressionSyntax ParseExpression() =>
            ParseBinaryExpression();

        private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0, SyntaxKind parentOperator = SyntaxKind.BadToken, bool isParentUnary = false)
        {
            ExpressionSyntax left;

            var unaryOperatorPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(Current.Kind);
            if (unaryOperatorPrecedence != 0)
            {
                var operatorToken = Next();
                SyntaxKind kind = SyntaxFacts.GetUnaryExpression(operatorToken.Kind).Value;
                var operand = this.ParseBinaryExpression(unaryOperatorPrecedence, operatorToken.Kind, true);
                left = new UnaryExpressionSyntax(kind, operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }

            while (true)
            {
                SyntaxKind operatorKind = Current.Kind;
                var precedence = SyntaxFacts.GetBinaryOperatorPrecedence(operatorKind);
                var comparePrecedence = !isParentUnary && parentOperator == operatorKind && SyntaxFacts.IsRightAssociative(operatorKind)
                    ? precedence + 1
                    : precedence;
                if (precedence <= 0 || comparePrecedence <= parentPrecedence)
                    break;

                var operatorToken = Next();
                SyntaxKind kind = SyntaxFacts.GetBinaryExpression(operatorToken.Kind).Value;
                var right = this.ParseBinaryExpression(precedence, operatorKind, false);
                left = new BinaryExpressionSyntax(kind, left, operatorToken, right);
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            return Current.Kind switch
            {
                SyntaxKind.NilKeyword
                or SyntaxKind.TrueKeyword
                or SyntaxKind.FalseKeyword
                or SyntaxKind.NumericLiteralToken
                or SyntaxKind.StringLiteralToken => ParseLiteralExpression(),
                SyntaxKind.DotDotDotToken => ParseVarArgExpression(),
                SyntaxKind.OpenBraceToken => ParseTableConstructorExpression(),
                SyntaxKind.FunctionKeyword when Lookahead.Kind == SyntaxKind.OpenParenthesisToken => ParseAnonymousFunctionExpression(),
                _ => ParsePrefixOrVariableExpression(),
            };
        }

        private PrefixExpressionSyntax ParsePrefixOrVariableExpression()
        {
            PrefixExpressionSyntax expression;
            if (Current.Kind == SyntaxKind.OpenParenthesisToken)
            {
                expression = ParseParenthesizedExpression();
            }
            else if (Current.Kind == SyntaxKind.IdentifierToken
                      || (_luaOptions.ContinueType == ContinueType.ContextualKeyword && Current.Kind == SyntaxKind.ContinueKeyword))
            {
                expression = ParseNameExpression();
            }
            else
            {
                return new BadExpressionSyntax(Next());
            }

            var @continue = true;
            while (@continue)
            {
                switch (Current.Kind)
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
                    case SyntaxKind.LongStringLiteralToken:
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

        private ExpressionSyntax ParseAnonymousFunctionExpression()
        {
            var functionKeywordToken = this.Match(SyntaxKind.FunctionKeyword);
            var parameterList = ParseParameterList();
            var body = this.ParseStatementList(SyntaxKind.EndKeyword);
            var endKeyword = this.Match(SyntaxKind.EndKeyword);

            return new AnonymousFunctionExpressionSyntax(
                functionKeywordToken,
                parameterList,
                body,
                endKeyword);
        }

        private NameExpressionSyntax ParseNameExpression()
        {
            var identifier = MatchIdentifier();
            return new NameExpressionSyntax(identifier);
        }

        private MemberAccessExpressionSyntax ParseMemberAccessExpression(PrefixExpressionSyntax expression)
        {
            var dotSeparator = this.Match(SyntaxKind.DotToken);
            var memberName = MatchIdentifier();
            return new MemberAccessExpressionSyntax(expression, dotSeparator, memberName);
        }

        private ElementAccessExpressionSyntax ParseElementAccessExpression(PrefixExpressionSyntax expression)
        {
            var openBracketToken = this.Match(SyntaxKind.OpenBracketToken);
            var elementExpression = ParseExpression();
            var closeBracketToken = this.Match(SyntaxKind.CloseBracketToken);

            return new ElementAccessExpressionSyntax(
                expression,
                openBracketToken,
                elementExpression,
                closeBracketToken);
        }

        private MethodCallExpressionSyntax ParseMethodCallExpression(PrefixExpressionSyntax expression)
        {
            var colonToken = this.Match(SyntaxKind.ColonToken);
            var identifier = MatchIdentifier();
            var arguments = ParseFunctionArgument();

            return new MethodCallExpressionSyntax(
                expression,
                colonToken,
                identifier,
                arguments);
        }

        private FunctionCallExpressionSyntax ParseFunctionCall(PrefixExpressionSyntax expression)
        {
            var arguments = ParseFunctionArgument();
            return new FunctionCallExpressionSyntax(
                expression,
                arguments);
        }

        private FunctionArgumentSyntax ParseFunctionArgument()
        {
            if (Current.Kind is SyntaxKind.StringLiteralToken or SyntaxKind.LongStringLiteralToken)
            {
                var literal = ParseLiteralExpression();
                return new StringFunctionArgumentSyntax(literal);
            }
            else if (Current.Kind is SyntaxKind.OpenBraceToken)
            {
                var tableConstructor = ParseTableConstructorExpression();
                return new TableConstructorFunctionArgumentSyntax(tableConstructor);
            }
            else
            {
                return ParseFunctionArgumentList();
            }
        }

        private ExpressionListFunctionArgumentSyntax ParseFunctionArgumentList()
        {
            var openParenthesisToken = this.Match(SyntaxKind.OpenParenthesisToken);
            var argumentsAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            while (Current.Kind is not (SyntaxKind.CloseParenthesisToken or SyntaxKind.EndOfFileToken))
            {
                var argument = ParseExpression();
                argumentsAndSeparators.Add(argument);
                if (Current.Kind is SyntaxKind.CommaToken)
                {
                    var separator = this.Match(SyntaxKind.CommaToken);
                    argumentsAndSeparators.Add(separator);
                }
                else
                {
                    break;
                }
            }
            var closeParenthesisToken = this.Match(SyntaxKind.CloseParenthesisToken);

            return new ExpressionListFunctionArgumentSyntax(
                openParenthesisToken,
                new SeparatedSyntaxList<ExpressionSyntax>(argumentsAndSeparators.ToImmutable()),
                closeParenthesisToken);
        }

        private ParameterListSyntax ParseParameterList()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();

            var openParenthesisToken = this.Match(SyntaxKind.OpenParenthesisToken);
            while (Current.Kind is not (SyntaxKind.CloseParenthesisToken or SyntaxKind.EndOfFileToken))
            {

                if (Current.Kind == SyntaxKind.DotDotDotToken)
                {
                    var varArgToken = this.Match(SyntaxKind.DotDotDotToken);
                    var varArgparameter = new VarArgParameterSyntax(varArgToken);
                    nodesAndSeparators.Add(varArgparameter);
                    break;
                }

                var identifier = MatchIdentifier();
                var parameter = new NamedParameterSyntax(identifier);
                nodesAndSeparators.Add(parameter);

                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    nodesAndSeparators.Add(this.Match(SyntaxKind.CommaToken));
                }
                else
                {
                    break;
                }
            }

            var closeParenthesisToken = this.Match(SyntaxKind.CloseParenthesisToken);

            return new ParameterListSyntax(
                openParenthesisToken,
                new SeparatedSyntaxList<ParameterSyntax>(nodesAndSeparators.ToImmutable()),
                closeParenthesisToken);
        }

        private VarArgExpressionSyntax ParseVarArgExpression()
        {
            var varargToken = this.Match(SyntaxKind.DotDotDotToken);
            return new VarArgExpressionSyntax(varargToken);
        }

        private LiteralExpressionSyntax ParseLiteralExpression()
        {
            SyntaxToken? token = Next();
            SyntaxKind kind = SyntaxFacts.GetLiteralExpression(token.Kind).Value;
            return new LiteralExpressionSyntax(kind, token);
        }

        private ParenthesizedExpressionSyntax ParseParenthesizedExpression()
        {
            var open = this.Match(SyntaxKind.OpenParenthesisToken);
            var expression = ParseExpression();
            var closing = this.Match(SyntaxKind.CloseParenthesisToken);
            return new ParenthesizedExpressionSyntax(open, expression, closing);
        }

        private TableConstructorExpressionSyntax ParseTableConstructorExpression()
        {
            var openBraceToken = this.Match(SyntaxKind.OpenBraceToken);

            var fieldsAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            while (Current.Kind is not (SyntaxKind.CloseBraceToken or SyntaxKind.EndOfFileToken))
            {
                var field = ParseTableField();
                fieldsAndSeparators.Add(field);

                if (Current.Kind is SyntaxKind.CommaToken or SyntaxKind.SemicolonToken)
                {
                    var separatorToken = Next();
                    fieldsAndSeparators.Add(separatorToken);
                }
                else
                {
                    break;
                }
            }

            var closeBraceToken = this.Match(SyntaxKind.CloseBraceToken);

            return new TableConstructorExpressionSyntax(
                openBraceToken,
                new SeparatedSyntaxList<TableFieldSyntax>(fieldsAndSeparators.ToImmutable()),
                closeBraceToken);
        }

        private TableFieldSyntax ParseTableField()
        {
            if (Current.Kind == SyntaxKind.IdentifierToken && Lookahead.Kind == SyntaxKind.EqualsToken)
            {
                var identifier = this.Match(SyntaxKind.IdentifierToken);
                var equalsToken = this.Match(SyntaxKind.EqualsToken);
                var value = ParseExpression();

                return new IdentifierKeyedTableFieldSyntax(identifier, equalsToken, value);
            }
            else if (Current.Kind == SyntaxKind.OpenBracketToken)
            {
                var openBracketToken = this.Match(SyntaxKind.OpenBracketToken);
                var key = ParseExpression();
                var closeBracketToken = this.Match(SyntaxKind.CloseBracketToken);
                var equalsToken = this.Match(SyntaxKind.EqualsToken);
                var value = ParseExpression();

                return new ExpressionKeyedTableFieldSyntax(openBracketToken, key, closeBracketToken, equalsToken, value);
            }
            else
            {
                var value = ParseExpression();
                return new UnkeyedTableFieldSyntax(value);
            }
        }
    }
}
