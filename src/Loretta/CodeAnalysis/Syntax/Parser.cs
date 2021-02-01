using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Loretta.CodeAnalysis.Text;
using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly LuaOptions _luaOptions;
        private readonly SourceText _text;
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private Int32 _position;

        public Parser ( SyntaxTree syntaxTree )
        {
            ImmutableArray<SyntaxToken>.Builder? tokens = ImmutableArray.CreateBuilder<SyntaxToken> ( );
            var badTokens = new List<SyntaxToken> ( );

            var lexer = new Lexer ( syntaxTree );
            SyntaxToken token;
            do
            {
                token = lexer.Lex ( );

                if ( token.Kind == SyntaxKind.BadToken )
                {
                    badTokens.Add ( token );
                }
                else
                {
                    if ( badTokens.Count > 0 )
                    {
                        var leadingTrivia = token.LeadingTrivia.ToBuilder ( );
                        var index = 0;

                        foreach ( SyntaxToken badToken in badTokens )
                        {
                            foreach ( SyntaxTrivia lt in badToken.LeadingTrivia )
                                leadingTrivia.Insert ( index++, lt );

                            var trivia = new SyntaxTrivia ( SyntaxKind.SkippedTextTrivia, badToken.Position, badToken.Text.Value );
                            leadingTrivia.Insert ( index++, trivia );

                            foreach ( SyntaxTrivia tt in badToken.TrailingTrivia )
                                leadingTrivia.Insert ( index++, tt );
                        }

                        badTokens.Clear ( );
                        token = new SyntaxToken ( token.Kind, token.Position, token.Text, token.Value, leadingTrivia.ToImmutable ( ), token.TrailingTrivia, false );
                    }

                    tokens.Add ( token );
                }
            } while ( token.Kind != SyntaxKind.EndOfFileToken );

            this._luaOptions = syntaxTree.Options;
            this._text = syntaxTree.Text;
            this._tokens = tokens.ToImmutable ( );
            this.Diagnostics.AddRange ( lexer.Diagnostics );
        }

        public DiagnosticBag Diagnostics { get; } = new DiagnosticBag ( );

        private SyntaxToken Peek ( Int32 offset ) =>
            this._tokens[Math.Min ( this._position + offset, this._tokens.Length - 1 )];

        private SyntaxToken Next ( )
        {
            SyntaxToken ret = this.Current;
            if ( this._position < this._tokens.Length - 1 )
                this._position++;
            return ret;
        }

        private SyntaxToken Current => this.Peek ( 0 );
        private SyntaxToken Lookahead => this.Peek ( 1 );

        public SyntaxToken Match ( SyntaxKind kind )
        {
            if ( this.Current.Kind == kind )
                return this.Next ( );

            this.Diagnostics.ReportUnexpectedToken (
                new TextLocation ( this._text, this.Current.Span ),
                this.Current.Kind,
                kind );

            return new SyntaxToken (
                kind,
                this.Current.Position,
                SyntaxFacts.GetText ( kind ) is { } txt ? txt.AsMemory ( ) : default,
                default,
                SyntaxTrivia.Empty,
                SyntaxTrivia.Empty,
                true );
        }

        private SyntaxToken MatchIdentifier ( )
        {
            if ( this._luaOptions.ContinueType == ContinueType.ContextualKeyword && this.Current.Kind == SyntaxKind.ContinueKeyword )
            {
                // Transforms the continue keyword into an identifier token on-the-fly.
                SyntaxToken continueKeyword = this.Next ( );
                return continueKeyword.WithKind ( SyntaxKind.IdentifierToken );
            }
            return this.Match ( SyntaxKind.IdentifierToken );
        }

        private Option<SyntaxToken> TryMatchSemicolon ( )
        {
            if ( this.Current.Kind == SyntaxKind.SemicolonToken )
                return this.Next ( );
            return default;
        }

        public CompilationUnitSyntax ParseCompilationUnit ( )
        {
            ImmutableArray<StatementSyntax> statements = this.ParseStatementList ( );
            SyntaxToken? endOfFileToken = this.Match ( SyntaxKind.EndOfFileToken );
            return new CompilationUnitSyntax ( statements, endOfFileToken );
        }

        private ImmutableArray<StatementSyntax> ParseStatementList ( params SyntaxKind[] terminalKinds )
        {
            ImmutableArray<StatementSyntax>.Builder builder = ImmutableArray.CreateBuilder<StatementSyntax> ( );
            while ( true )
            {
                SyntaxKind kind = this.Current.Kind;
                if ( kind == SyntaxKind.EndOfFileToken || terminalKinds.Contains ( kind ) )
                    break;

                SyntaxToken startToken = this.Current;

                StatementSyntax statement = this.ParseStatement ( );
                builder.Add ( statement );

                // If ParseStatement did not consume any tokens, we have to advance ourselves
                // otherwise we get stuck in an infinite loop.
                if ( this.Current == startToken )
                    _ = this.Next ( );
            }
            return builder.ToImmutable ( );
        }

        private StatementSyntax ParseStatement ( )
        {
            switch ( this.Current.Kind )
            {
                case SyntaxKind.LocalKeyword:
                    if ( this.Lookahead.Kind is SyntaxKind.FunctionKeyword )
                        return this.ParseLocalFunctionDeclarationStatement ( );
                    else
                        return this.ParseLocalVariableDeclarationStatement ( );

                case SyntaxKind.ForKeyword:
                    if ( this.Peek ( 2 ).Kind == SyntaxKind.EqualsToken )
                        return this.ParseNumericForStatement ( );
                    else
                        return this.ParseGenericForStatement ( );

                case SyntaxKind.IfKeyword:
                    return this.ParseIfStatement ( );

                case SyntaxKind.RepeatKeyword:
                    return this.ParseRepeatUntilStatement ( );

                case SyntaxKind.WhileKeyword:
                    return this.ParseWhileStatement ( );

                case SyntaxKind.DoKeyword:
                    return this.ParseDoStatement ( );

                case SyntaxKind.GotoKeyword:
                    return this.ParseGotoStatement ( );

                case SyntaxKind.BreakKeyword:
                    return this.ParseBreakStatement ( );

                case SyntaxKind.ContinueKeyword:
                    return this.ParseContinueStatement ( );

                case SyntaxKind.ColonColonToken:
                    return this.ParseGotoLabelStatement ( );

                case SyntaxKind.FunctionKeyword:
                    return this.ParseFunctionDeclarationStatement ( );

                case SyntaxKind.ReturnKeyword:
                    return this.ParseReturnStatement ( );

                default:
                {
                    PrefixExpressionSyntax expression = this.ParsePrefixOrVariableExpression ( );
                    if ( expression.Kind is SyntaxKind.BadExpression )
                    {
                        return new BadStatementSyntax ( ( BadExpressionSyntax ) expression );
                    }
                    if ( this.Current.Kind is SyntaxKind.CommaToken or SyntaxKind.EqualsToken )
                    {
                        return this.ParseAssignmentStatement ( expression );
                    }
                    else if ( SyntaxFacts.IsCompoundAssignmentOperatorToken ( this.Current.Kind ) )
                    {
                        return this.ParseCompoundAssignment ( expression );
                    }
                    else
                    {
                        if ( expression.Kind is not ( SyntaxKind.FunctionCallExpression or SyntaxKind.MethodCallExpression ) )
                            this.Diagnostics.ReportNonFunctionCallBeingUsedAsStatement ( new TextLocation ( this._text, expression.Span ) );
                        Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );
                        return new ExpressionStatementSyntax (
                            expression,
                            semicolonToken );
                    }
                }
            }
        }

        private LocalVariableDeclarationStatementSyntax ParseLocalVariableDeclarationStatement ( )
        {
            SyntaxToken localKeyword = this.Match ( SyntaxKind.LocalKeyword );
            ImmutableArray<SyntaxNode>.Builder namesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode> ( );

            NameExpressionSyntax name = this.ParseNameExpression ( );
            namesAndSeparators.Add ( name );

            while ( this.Current.Kind is SyntaxKind.CommaToken )
            {
                SyntaxToken separator = this.Match ( SyntaxKind.CommaToken );
                namesAndSeparators.Add ( separator );

                name = this.ParseNameExpression ( );
                namesAndSeparators.Add ( name );
            }

            var names = new SeparatedSyntaxList<NameExpressionSyntax> ( namesAndSeparators.ToImmutable ( ) );
            if ( this.Current.Kind == SyntaxKind.EqualsToken )
            {
                SyntaxToken equalsToken = this.Match ( SyntaxKind.EqualsToken );

                ImmutableArray<SyntaxNode>.Builder expressionsAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode> ( );

                ExpressionSyntax value = this.ParseExpression ( );
                expressionsAndSeparators.Add ( value );

                while ( this.Current.Kind is SyntaxKind.CommaToken )
                {
                    SyntaxToken separator = this.Match ( SyntaxKind.CommaToken );
                    expressionsAndSeparators.Add ( separator );

                    value = this.ParseExpression ( );
                    expressionsAndSeparators.Add ( value );
                }

                Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );

                var values = new SeparatedSyntaxList<ExpressionSyntax> ( expressionsAndSeparators.ToImmutable ( ) );

                return new LocalVariableDeclarationStatementSyntax (
                    localKeyword,
                    names,
                    equalsToken,
                    values,
                    semicolonToken );
            }
            else
            {
                Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );

                return new LocalVariableDeclarationStatementSyntax (
                    localKeyword,
                    names,
                    default,
                    default,
                    semicolonToken );
            }
        }

        private LocalFunctionDeclarationStatementSyntax ParseLocalFunctionDeclarationStatement ( )
        {
            SyntaxToken localKeyword = this.Match ( SyntaxKind.LocalKeyword );
            SyntaxToken functionKeyword = this.Match ( SyntaxKind.FunctionKeyword );
            SyntaxToken identifier = this.MatchIdentifier ( );
            ParameterListSyntax parameters = this.ParseParameterList ( );
            ImmutableArray<StatementSyntax> body = this.ParseStatementList ( SyntaxKind.EndKeyword );
            SyntaxToken endKeyword = this.Match ( SyntaxKind.EndKeyword );
            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );

            return new LocalFunctionDeclarationStatementSyntax (
                localKeyword,
                functionKeyword,
                identifier,
                parameters,
                body,
                endKeyword,
                semicolonToken );
        }

        private NumericForStatementSyntax ParseNumericForStatement ( )
        {
            SyntaxToken forKeyword = this.Match ( SyntaxKind.ForKeyword );
            SyntaxToken identifier = this.MatchIdentifier ( );
            SyntaxToken equalsToken = this.Match ( SyntaxKind.EqualsToken );
            ExpressionSyntax initialValue = this.ParseExpression ( );
            SyntaxToken finalValueCommaToken = this.Match ( SyntaxKind.CommaToken );
            ExpressionSyntax finalValue = this.ParseExpression ( );
            Option<SyntaxToken> stepValueCommaToken = default;
            Option<ExpressionSyntax> stepValue = default;
            if ( this.Current.Kind == SyntaxKind.CommaToken )
            {
                stepValueCommaToken = this.Match ( SyntaxKind.CommaToken );
                stepValue = this.ParseExpression ( );
            }
            SyntaxToken doKeyword = this.Match ( SyntaxKind.DoKeyword );
            ImmutableArray<StatementSyntax> body = this.ParseStatementList ( SyntaxKind.EndKeyword );
            SyntaxToken endKeyword = this.Match ( SyntaxKind.EndKeyword );
            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );
            return new NumericForStatementSyntax (
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
                semicolonToken );
        }

        private GenericForStatementSyntax ParseGenericForStatement ( )
        {
            SyntaxToken forKeyword = this.Match ( SyntaxKind.ForKeyword );

            ImmutableArray<SyntaxNode>.Builder? identifiersAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode> ( 3 );

            SyntaxToken identifier = this.MatchIdentifier ( );
            identifiersAndSeparators.Add ( identifier );
            while ( this.Current.Kind is SyntaxKind.CommaToken )
            {
                SyntaxToken separator = this.Match ( SyntaxKind.CommaToken );
                identifiersAndSeparators.Add ( separator );

                identifier = this.MatchIdentifier ( );
                identifiersAndSeparators.Add ( identifier );
            }

            SyntaxToken inKeyword = this.Match ( SyntaxKind.InKeyword );

            ImmutableArray<SyntaxNode>.Builder? expressionsAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode> ( 1 );

            ExpressionSyntax expression = this.ParseExpression ( );
            expressionsAndSeparators.Add ( expression );
            while ( this.Current.Kind is SyntaxKind.CommaToken )
            {
                SyntaxToken separator = this.Match ( SyntaxKind.CommaToken );
                expressionsAndSeparators.Add ( separator );

                expression = this.ParseExpression ( );
                expressionsAndSeparators.Add ( expression );
            }

            SyntaxToken doKeyword = this.Match ( SyntaxKind.DoKeyword );
            ImmutableArray<StatementSyntax> body = this.ParseStatementList ( SyntaxKind.EndKeyword );
            SyntaxToken endKeyword = this.Match ( SyntaxKind.EndKeyword );
            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );

            var identifiers = new SeparatedSyntaxList<SyntaxToken> ( identifiersAndSeparators.ToImmutable ( ) );
            var expressions = new SeparatedSyntaxList<ExpressionSyntax> ( expressionsAndSeparators.ToImmutable ( ) );
            return new GenericForStatementSyntax (
                forKeyword,
                identifiers,
                inKeyword,
                expressions,
                doKeyword,
                body,
                endKeyword,
                semicolonToken );
        }

        private IfStatementSyntax ParseIfStatement ( )
        {
            SyntaxToken ifKeyword = this.Match ( SyntaxKind.IfKeyword );
            ExpressionSyntax condition = this.ParseExpression ( );
            SyntaxToken thenKeyword = this.Match ( SyntaxKind.ThenKeyword );
            ImmutableArray<StatementSyntax> body = this.ParseStatementList ( SyntaxKind.ElseIfKeyword, SyntaxKind.ElseKeyword, SyntaxKind.EndKeyword );

            ImmutableArray<ElseIfClauseSyntax>.Builder elseIfClausesBuilder = ImmutableArray.CreateBuilder<ElseIfClauseSyntax> ( );
            while ( this.Current.Kind is SyntaxKind.ElseIfKeyword )
            {
                SyntaxToken elseIfKeyword = this.Match ( SyntaxKind.ElseIfKeyword );
                ExpressionSyntax elseIfCondition = this.ParseExpression ( );
                SyntaxToken elseIfThenKeyword = this.Match ( SyntaxKind.ThenKeyword );
                ImmutableArray<StatementSyntax> elseIfBody = this.ParseStatementList ( SyntaxKind.ElseIfKeyword, SyntaxKind.ElseKeyword, SyntaxKind.EndKeyword );

                elseIfClausesBuilder.Add ( new ElseIfClauseSyntax (
                    elseIfKeyword,
                    elseIfCondition,
                    elseIfThenKeyword,
                    elseIfBody ) );
            }

            Option<ElseClauseSyntax> elseClause = default;
            if ( this.Current.Kind is SyntaxKind.ElseKeyword )
            {
                SyntaxToken elseKeyword = this.Match ( SyntaxKind.ElseKeyword );
                ImmutableArray<StatementSyntax> elseBody = this.ParseStatementList ( SyntaxKind.EndKeyword );
                elseClause = new ElseClauseSyntax ( elseKeyword, elseBody );
            }

            SyntaxToken endKeyword = this.Match ( SyntaxKind.EndKeyword );
            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );

            ImmutableArray<ElseIfClauseSyntax> elseIfClauses = elseIfClausesBuilder.ToImmutable ( );
            return new IfStatementSyntax (
                ifKeyword,
                condition,
                thenKeyword,
                body,
                elseIfClauses,
                elseClause,
                endKeyword,
                semicolonToken );
        }

        private RepeatUntilStatementSyntax ParseRepeatUntilStatement ( )
        {
            SyntaxToken repeatKeyword = this.Match ( SyntaxKind.RepeatKeyword );
            ImmutableArray<StatementSyntax> body = this.ParseStatementList ( SyntaxKind.UntilKeyword );
            SyntaxToken untilKeyword = this.Match ( SyntaxKind.UntilKeyword );
            ExpressionSyntax condition = this.ParseExpression ( );
            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );
            return new RepeatUntilStatementSyntax (
                repeatKeyword,
                body,
                untilKeyword,
                condition,
                semicolonToken );
        }

        private WhileStatementSyntax ParseWhileStatement ( )
        {
            SyntaxToken whileKeyword = this.Match ( SyntaxKind.WhileKeyword );
            ExpressionSyntax condition = this.ParseExpression ( );
            SyntaxToken doKeyword = this.Match ( SyntaxKind.DoKeyword );
            ImmutableArray<StatementSyntax> body = this.ParseStatementList ( SyntaxKind.EndKeyword );
            SyntaxToken endKeyword = this.Match ( SyntaxKind.EndKeyword );
            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );
            return new WhileStatementSyntax (
                whileKeyword,
                condition,
                doKeyword,
                body,
                endKeyword,
                semicolonToken );
        }

        private DoStatementSyntax ParseDoStatement ( )
        {
            SyntaxToken doKeyword = this.Match ( SyntaxKind.DoKeyword );
            ImmutableArray<StatementSyntax> body = this.ParseStatementList ( SyntaxKind.EndKeyword );
            SyntaxToken endKeyword = this.Match ( SyntaxKind.EndKeyword );
            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );
            return new DoStatementSyntax (
                doKeyword,
                body,
                endKeyword,
                semicolonToken );
        }

        private GotoStatementSyntax ParseGotoStatement ( )
        {
            SyntaxToken gotoKeyword = this.Match ( SyntaxKind.GotoKeyword );
            SyntaxToken labelName = this.MatchIdentifier ( );
            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );
            return new GotoStatementSyntax (
                gotoKeyword,
                labelName,
                semicolonToken );
        }

        private BreakStatementSyntax ParseBreakStatement ( )
        {
            SyntaxToken breakKeyword = this.Match ( SyntaxKind.BreakKeyword );
            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );
            return new BreakStatementSyntax ( breakKeyword, semicolonToken );
        }

        private ContinueStatementSyntax ParseContinueStatement ( )
        {
            SyntaxToken? continueKeyword = this.Match ( SyntaxKind.ContinueKeyword );
            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );
            return new ContinueStatementSyntax ( continueKeyword, semicolonToken );
        }

        private GotoLabelStatementSyntax ParseGotoLabelStatement ( )
        {
            SyntaxToken leftDelimiterToken = this.Match ( SyntaxKind.ColonColonToken );
            SyntaxToken identifier = this.MatchIdentifier ( );
            SyntaxToken rightDelimiterToken = this.Match ( SyntaxKind.ColonColonToken );
            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );
            return new GotoLabelStatementSyntax (
                leftDelimiterToken,
                identifier,
                rightDelimiterToken,
                semicolonToken );
        }

        private FunctionDeclarationStatementSyntax ParseFunctionDeclarationStatement ( )
        {
            SyntaxToken functionKeyword = this.Match ( SyntaxKind.FunctionKeyword );
            FunctionNameSyntax name = this.ParseFunctionName ( );
            ParameterListSyntax parameters = this.ParseParameterList ( );
            ImmutableArray<StatementSyntax> body = this.ParseStatementList ( SyntaxKind.EndKeyword );
            SyntaxToken endKeyword = this.Match ( SyntaxKind.EndKeyword );
            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );
            return new FunctionDeclarationStatementSyntax (
                functionKeyword,
                name,
                parameters,
                body,
                endKeyword,
                semicolonToken );
        }

        private FunctionNameSyntax ParseFunctionName ( )
        {
            SyntaxToken identifier = this.MatchIdentifier ( );
            FunctionNameSyntax name = new SimpleFunctionNameSyntax ( identifier );

            while ( this.Current.Kind == SyntaxKind.DotToken )
            {
                SyntaxToken dotToken = this.Match ( SyntaxKind.DotToken );
                identifier = this.MatchIdentifier ( );
                name = new MemberFunctionNameSyntax ( name, dotToken, identifier );
            }

            if ( this.Current.Kind == SyntaxKind.ColonToken )
            {
                SyntaxToken colonToken = this.Match ( SyntaxKind.ColonToken );
                identifier = this.MatchIdentifier ( );
                name = new MethodFunctionNameSyntax ( name, colonToken, identifier );
            }

            return name;
        }

        private ReturnStatementSyntax ParseReturnStatement ( )
        {
            SyntaxToken returnKeyword = this.Match ( SyntaxKind.ReturnKeyword );
            SeparatedSyntaxList<ExpressionSyntax>? expressions = null;
            if ( this.Current.Kind is not ( SyntaxKind.ElseKeyword or SyntaxKind.ElseIfKeyword or SyntaxKind.EndKeyword or SyntaxKind.UntilKeyword or SyntaxKind.SemicolonToken ) )
            {
                ImmutableArray<SyntaxNode>.Builder expressionsAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode> ( 1 );
                ExpressionSyntax expression = this.ParseExpression ( );
                expressionsAndSeparators.Add ( expression );

                while ( this.Current.Kind == SyntaxKind.CommaToken )
                {
                    SyntaxToken separator = this.Match ( SyntaxKind.CommaToken );
                    expressionsAndSeparators.Add ( separator );

                    expression = this.ParseExpression ( );
                    expressionsAndSeparators.Add ( expression );
                }

                expressions = new SeparatedSyntaxList<ExpressionSyntax> ( expressionsAndSeparators.ToImmutable ( ) );
            }
            expressions ??= new SeparatedSyntaxList<ExpressionSyntax> ( ImmutableArray<SyntaxNode>.Empty );
            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );
            return new ReturnStatementSyntax (
                returnKeyword,
                expressions,
                semicolonToken );
        }

        private AssignmentStatementSyntax ParseAssignmentStatement ( PrefixExpressionSyntax variable )
        {
            ImmutableArray<SyntaxNode>.Builder variablesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode> ( 1 );
            if ( !SyntaxFacts.IsVariableExpression ( variable.Kind ) )
                this.Diagnostics.ReportCannotBeAssignedTo ( new TextLocation ( this._text, variable.Span ) );
            variablesAndSeparators.Add ( variable );
            while ( this.Current.Kind == SyntaxKind.CommaToken )
            {
                SyntaxToken separator = this.Match ( SyntaxKind.CommaToken );
                variablesAndSeparators.Add ( separator );

                variable = this.ParsePrefixOrVariableExpression ( );
                if ( !SyntaxFacts.IsVariableExpression ( variable.Kind ) )
                    this.Diagnostics.ReportCannotBeAssignedTo ( new TextLocation ( this._text, variable.Span ) );
                variablesAndSeparators.Add ( variable );
            }

            SyntaxToken equalsToken = this.Match ( SyntaxKind.EqualsToken );

            ImmutableArray<SyntaxNode>.Builder valuesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode> ( 1 );
            ExpressionSyntax value = this.ParseExpression ( );
            valuesAndSeparators.Add ( value );
            while ( this.Current.Kind == SyntaxKind.CommaToken )
            {
                SyntaxToken separator = this.Match ( SyntaxKind.CommaToken );
                valuesAndSeparators.Add ( separator );

                value = this.ParseExpression ( );
                valuesAndSeparators.Add ( value );
            }

            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );

            var variables = new SeparatedSyntaxList<PrefixExpressionSyntax> ( variablesAndSeparators.ToImmutable ( ) );
            var values = new SeparatedSyntaxList<ExpressionSyntax> ( valuesAndSeparators.ToImmutable ( ) );
            return new AssignmentStatementSyntax (
                variables,
                equalsToken,
                values,
                semicolonToken );
        }

        private CompoundAssignmentStatementSyntax ParseCompoundAssignment ( PrefixExpressionSyntax variable )
        {
            Debug.Assert ( SyntaxFacts.IsCompoundAssignmentOperatorToken ( this.Current.Kind ) );
            if ( !SyntaxFacts.IsVariableExpression ( variable.Kind ) )
                this.Diagnostics.ReportCannotBeAssignedTo ( new TextLocation ( this._text, variable.Span ) );
            SyntaxToken assignmentOperatorToken = this.Next ( );
            SyntaxKind kind = SyntaxFacts.GetCompoundAssignmentStatement ( assignmentOperatorToken.Kind ).Value;
            ExpressionSyntax expression = this.ParseExpression ( );
            Option<SyntaxToken> semicolonToken = this.TryMatchSemicolon ( );
            return new CompoundAssignmentStatementSyntax (
                kind,
                variable,
                assignmentOperatorToken,
                expression,
                semicolonToken );
        }

        public ExpressionSyntax ParseExpression ( ) =>
            this.ParseBinaryExpression ( );

        private ExpressionSyntax ParseBinaryExpression ( Int32 parentPrecedence = 0, SyntaxKind parentOperator = SyntaxKind.BadToken, Boolean isParentUnary = false )
        {
            ExpressionSyntax left;

            var unaryOperatorPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence ( this.Current.Kind );
            if ( unaryOperatorPrecedence != 0 )
            {
                SyntaxToken operatorToken = this.Next ( );
                SyntaxKind kind = SyntaxFacts.GetUnaryExpression ( operatorToken.Kind ).Value;
                ExpressionSyntax operand = this.ParseBinaryExpression ( unaryOperatorPrecedence, operatorToken.Kind, true );
                left = new UnaryExpressionSyntax ( kind, operatorToken, operand );
            }
            else
            {
                left = this.ParsePrimaryExpression ( );
            }

            while ( true )
            {
                SyntaxKind operatorKind = this.Current.Kind;
                var precedence = SyntaxFacts.GetBinaryOperatorPrecedence ( operatorKind );
                var comparePrecedence = !isParentUnary && parentOperator == operatorKind && SyntaxFacts.IsRightAssociative ( operatorKind )
                    ? precedence + 1
                    : precedence;
                if ( precedence <= 0 || comparePrecedence <= parentPrecedence )
                    break;

                SyntaxToken operatorToken = this.Next ( );
                SyntaxKind kind = SyntaxFacts.GetBinaryExpression ( operatorToken.Kind ).Value;
                ExpressionSyntax right = this.ParseBinaryExpression ( precedence, operatorKind, false );
                left = new BinaryExpressionSyntax ( kind, left, operatorToken, right );
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression ( )
        {
            return this.Current.Kind switch
            {
                SyntaxKind.NilKeyword
                or SyntaxKind.TrueKeyword
                or SyntaxKind.FalseKeyword
                or SyntaxKind.NumberToken
                or SyntaxKind.ShortStringToken
                or SyntaxKind.LongStringToken => this.ParseLiteralExpression ( ),
                SyntaxKind.DotDotDotToken => this.ParseVarArgExpression ( ),
                SyntaxKind.OpenBraceToken => this.ParseTableConstructorExpression ( ),
                SyntaxKind.FunctionKeyword when this.Lookahead.Kind == SyntaxKind.OpenParenthesisToken => this.ParseAnonymousFunctionExpression ( ),
                _ => this.ParsePrefixOrVariableExpression ( ),
            };
        }

        private PrefixExpressionSyntax ParsePrefixOrVariableExpression ( )
        {
            PrefixExpressionSyntax expression;
            if ( this.Current.Kind == SyntaxKind.OpenParenthesisToken )
            {
                expression = this.ParseParenthesizedExpression ( );
            }
            else if ( this.Current.Kind == SyntaxKind.IdentifierToken
                      || ( this._luaOptions.ContinueType == ContinueType.ContextualKeyword && this.Current.Kind == SyntaxKind.ContinueKeyword ) )
            {
                expression = this.ParseNameExpression ( );
            }
            else
            {
                return new BadExpressionSyntax ( this.Next ( ) );
            }

            var @continue = true;
            while ( @continue )
            {
                switch ( this.Current.Kind )
                {
                    case SyntaxKind.DotToken:
                        expression = this.ParseMemberAccessExpression ( expression );
                        break;

                    case SyntaxKind.OpenBracketToken:
                        expression = this.ParseElementAccessExpression ( expression );
                        break;

                    case SyntaxKind.ColonToken:
                        expression = this.ParseMethodCallExpression ( expression );
                        break;

                    case SyntaxKind.ShortStringToken:
                    case SyntaxKind.LongStringToken:
                    case SyntaxKind.OpenBraceToken:
                    case SyntaxKind.OpenParenthesisToken:
                        expression = this.ParseFunctionCall ( expression );
                        break;

                    default:
                        goto endloop;
                }
            }

        endloop:
            return expression;
        }

        private ExpressionSyntax ParseAnonymousFunctionExpression ( )
        {
            SyntaxToken functionKeywordToken = this.Match ( SyntaxKind.FunctionKeyword );
            ParameterListSyntax parameterList = this.ParseParameterList ( );
            ImmutableArray<StatementSyntax> body = this.ParseStatementList ( SyntaxKind.EndKeyword );
            SyntaxToken endKeyword = this.Match ( SyntaxKind.EndKeyword );

            return new AnonymousFunctionExpressionSyntax (
                functionKeywordToken,
                parameterList,
                body,
                endKeyword );
        }

        private NameExpressionSyntax ParseNameExpression ( )
        {
            SyntaxToken identifier = this.MatchIdentifier ( );
            return new NameExpressionSyntax ( identifier );
        }

        private MemberAccessExpressionSyntax ParseMemberAccessExpression ( PrefixExpressionSyntax expression )
        {
            SyntaxToken dotSeparator = this.Match ( SyntaxKind.DotToken );
            SyntaxToken memberName = this.MatchIdentifier ( );
            return new MemberAccessExpressionSyntax ( expression, dotSeparator, memberName );
        }

        private ElementAccessExpressionSyntax ParseElementAccessExpression ( PrefixExpressionSyntax expression )
        {
            SyntaxToken openBracketToken = this.Match ( SyntaxKind.OpenBracketToken );
            ExpressionSyntax elementExpression = this.ParseExpression ( );
            SyntaxToken closeBracketToken = this.Match ( SyntaxKind.CloseBracketToken );

            return new ElementAccessExpressionSyntax (
                expression,
                openBracketToken,
                elementExpression,
                closeBracketToken );
        }

        private MethodCallExpressionSyntax ParseMethodCallExpression ( PrefixExpressionSyntax expression )
        {
            SyntaxToken colonToken = this.Match ( SyntaxKind.ColonToken );
            SyntaxToken identifier = this.MatchIdentifier ( );
            FunctionArgumentSyntax arguments = this.ParseFunctionArgument ( );

            return new MethodCallExpressionSyntax (
                expression,
                colonToken,
                identifier,
                arguments );
        }

        private FunctionCallExpressionSyntax ParseFunctionCall ( PrefixExpressionSyntax expression )
        {
            FunctionArgumentSyntax arguments = this.ParseFunctionArgument ( );
            return new FunctionCallExpressionSyntax (
                expression,
                arguments );
        }

        private FunctionArgumentSyntax ParseFunctionArgument ( )
        {
            if ( this.Current.Kind is SyntaxKind.ShortStringToken or SyntaxKind.LongStringToken )
            {
                LiteralExpressionSyntax literal = this.ParseLiteralExpression ( );
                return new StringFunctionArgumentSyntax ( literal );
            }
            else if ( this.Current.Kind is SyntaxKind.OpenBraceToken )
            {
                TableConstructorExpressionSyntax tableConstructor = this.ParseTableConstructorExpression ( );
                return new TableConstructorFunctionArgumentSyntax ( tableConstructor );
            }
            else
            {
                return this.ParseFunctionArgumentList ( );
            }
        }

        private ExpressionListFunctionArgumentSyntax ParseFunctionArgumentList ( )
        {
            SyntaxToken openParenthesisToken = this.Match ( SyntaxKind.OpenParenthesisToken );
            ImmutableArray<SyntaxNode>.Builder argumentsAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode> ( );
            while ( this.Current.Kind is not ( SyntaxKind.CloseParenthesisToken or SyntaxKind.EndOfFileToken ) )
            {
                ExpressionSyntax argument = this.ParseExpression ( );
                argumentsAndSeparators.Add ( argument );
                if ( this.Current.Kind is SyntaxKind.CommaToken )
                {
                    SyntaxToken separator = this.Match ( SyntaxKind.CommaToken );
                    argumentsAndSeparators.Add ( separator );
                }
                else
                {
                    break;
                }
            }
            SyntaxToken closeParenthesisToken = this.Match ( SyntaxKind.CloseParenthesisToken );

            return new ExpressionListFunctionArgumentSyntax (
                openParenthesisToken,
                new SeparatedSyntaxList<ExpressionSyntax> ( argumentsAndSeparators.ToImmutable ( ) ),
                closeParenthesisToken );
        }

        private ParameterListSyntax ParseParameterList ( )
        {
            ImmutableArray<SyntaxNode>.Builder nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode> ( );

            SyntaxToken openParenthesisToken = this.Match ( SyntaxKind.OpenParenthesisToken );
            while ( this.Current.Kind is not ( SyntaxKind.CloseParenthesisToken or SyntaxKind.EndOfFileToken ) )
            {

                if ( this.Current.Kind == SyntaxKind.DotDotDotToken )
                {
                    SyntaxToken varArgToken = this.Match ( SyntaxKind.DotDotDotToken );
                    var varArgparameter = new VarArgParameterSyntax ( varArgToken );
                    nodesAndSeparators.Add ( varArgparameter );
                    break;
                }

                SyntaxToken identifier = this.MatchIdentifier ( );
                var parameter = new NamedParameterSyntax ( identifier );
                nodesAndSeparators.Add ( parameter );

                if ( this.Current.Kind == SyntaxKind.CommaToken )
                {
                    nodesAndSeparators.Add ( this.Match ( SyntaxKind.CommaToken ) );
                }
                else
                {
                    break;
                }
            }

            SyntaxToken closeParenthesisToken = this.Match ( SyntaxKind.CloseParenthesisToken );

            return new ParameterListSyntax (
                openParenthesisToken,
                new SeparatedSyntaxList<ParameterSyntax> ( nodesAndSeparators.ToImmutable ( ) ),
                closeParenthesisToken );
        }

        private VarArgExpressionSyntax ParseVarArgExpression ( )
        {
            SyntaxToken varargToken = this.Match ( SyntaxKind.DotDotDotToken );
            return new VarArgExpressionSyntax ( varargToken );
        }

        private LiteralExpressionSyntax ParseLiteralExpression ( )
        {
            SyntaxToken? token = this.Next ( );
            SyntaxKind kind = SyntaxFacts.GetLiteralExpression ( token.Kind ).Value;
            return new LiteralExpressionSyntax ( kind, token );
        }

        private ParenthesizedExpressionSyntax ParseParenthesizedExpression ( )
        {
            SyntaxToken open = this.Match ( SyntaxKind.OpenParenthesisToken );
            ExpressionSyntax expression = this.ParseExpression ( );
            SyntaxToken closing = this.Match ( SyntaxKind.CloseParenthesisToken );
            return new ParenthesizedExpressionSyntax ( open, expression, closing );
        }

        private TableConstructorExpressionSyntax ParseTableConstructorExpression ( )
        {
            SyntaxToken openBraceToken = this.Match ( SyntaxKind.OpenBraceToken );

            ImmutableArray<SyntaxNode>.Builder fieldsAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode> ( );
            while ( this.Current.Kind is not ( SyntaxKind.CloseBraceToken or SyntaxKind.EndOfFileToken ) )
            {
                TableFieldSyntax field = this.ParseTableField ( );
                fieldsAndSeparators.Add ( field );

                if ( this.Current.Kind is SyntaxKind.CommaToken or SyntaxKind.SemicolonToken )
                {
                    SyntaxToken separatorToken = this.Next ( );
                    fieldsAndSeparators.Add ( separatorToken );
                }
                else
                {
                    break;
                }
            }

            SyntaxToken closeBraceToken = this.Match ( SyntaxKind.CloseBraceToken );

            return new TableConstructorExpressionSyntax (
                openBraceToken,
                new SeparatedSyntaxList<TableFieldSyntax> ( fieldsAndSeparators.ToImmutable ( ) ),
                closeBraceToken );
        }

        private TableFieldSyntax ParseTableField ( )
        {
            if ( this.Current.Kind == SyntaxKind.IdentifierToken && this.Lookahead.Kind == SyntaxKind.EqualsToken )
            {
                SyntaxToken identifier = this.Match ( SyntaxKind.IdentifierToken );
                SyntaxToken equalsToken = this.Match ( SyntaxKind.EqualsToken );
                ExpressionSyntax value = this.ParseExpression ( );

                return new IdentifierKeyedTableFieldSyntax ( identifier, equalsToken, value );
            }
            else if ( this.Current.Kind == SyntaxKind.OpenBracketToken )
            {
                SyntaxToken openBracketToken = this.Match ( SyntaxKind.OpenBracketToken );
                ExpressionSyntax key = this.ParseExpression ( );
                SyntaxToken closeBracketToken = this.Match ( SyntaxKind.CloseBracketToken );
                SyntaxToken equalsToken = this.Match ( SyntaxKind.EqualsToken );
                ExpressionSyntax value = this.ParseExpression ( );

                return new ExpressionKeyedTableFieldSyntax ( openBracketToken, key, closeBracketToken, equalsToken, value );
            }
            else
            {
                ExpressionSyntax value = this.ParseExpression ( );
                return new UnkeyedTableFieldSyntax ( value );
            }
        }
    }
}
