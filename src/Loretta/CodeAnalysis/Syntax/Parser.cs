using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.CodeAnalysis.Text;
using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly SyntaxTree _syntaxTree;
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

                            var trivia = new SyntaxTrivia ( syntaxTree, SyntaxKind.SkippedTextTrivia, badToken.Position, badToken.Text );
                            leadingTrivia.Insert ( index++, trivia );

                            foreach ( SyntaxTrivia tt in badToken.TrailingTrivia )
                                leadingTrivia.Insert ( index++, tt );
                        }

                        badTokens.Clear ( );
                        token = new SyntaxToken ( token.SyntaxTree, token.Kind, token.Position, token.Text, token.Value, leadingTrivia.ToImmutable ( ), token.TrailingTrivia );
                    }

                    tokens.Add ( token );
                }
            } while ( token.Kind != SyntaxKind.EndOfFileToken );

            this._syntaxTree = syntaxTree;
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

            this.Diagnostics.ReportUnexpectedToken ( this.Current.Location, this.Current.Kind, kind );
            return new SyntaxToken (
                this._syntaxTree,
                kind,
                this.Current.Position,
                default,
                default,
                ImmutableArray<SyntaxTrivia>.Empty,
                ImmutableArray<SyntaxTrivia>.Empty );
        }

        private SyntaxToken MatchIdentifier ( )
        {
            if ( this._luaOptions.ContinueType == ContinueType.ContextualKeyword && this.Current.Kind == SyntaxKind.ContinueKeyword )
            {
                // Transforms the continue keyword into an identifier token on-the-fly.
                SyntaxToken continueKeyword = this.Next ( );
                return new SyntaxToken (
                    this._syntaxTree,
                    SyntaxKind.IdentifierToken,
                    continueKeyword.Position,
                    continueKeyword.Text,
                    default,
                    continueKeyword.LeadingTrivia,
                    continueKeyword.TrailingTrivia );
            }
            return this.Match ( SyntaxKind.IdentifierToken );
        }

        public CompilationUnitSyntax ParseCompilationUnit ( )
        {
            ImmutableArray<MemberSyntax> members = this.ParseMembers ( );
            SyntaxToken? endOfFileToken = this.Match ( SyntaxKind.EndOfFileToken );
            return new CompilationUnitSyntax ( this._syntaxTree, members, endOfFileToken );
        }

        public ImmutableArray<MemberSyntax> ParseMembers ( ) => throw new NotImplementedException ( );

        public ExpressionSyntax ParseExpression ( ) =>
            this.ParseBinaryExpression ( );

        private ExpressionSyntax ParseBinaryExpression ( Int32 parentPrecedence = 0, SyntaxKind parentOperator = SyntaxKind.BadToken, Boolean isParentUnary = false )
        {
            ExpressionSyntax left;

            var unaryOperatorPrecedence = this.Current.Kind.GetUnaryOperatorPrecedence ( );
            if ( unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence )
            {
                SyntaxToken operatorToken = this.Next ( );
                ExpressionSyntax operand = this.ParseBinaryExpression ( unaryOperatorPrecedence, operatorToken.Kind, true );
                left = new UnaryExpressionSyntax ( this._syntaxTree, operatorToken, operand );
            }
            else
            {
                left = this.ParsePrimaryExpression ( );
            }

            while ( true )
            {
                SyntaxKind operatorKind = this.Current.Kind;
                var precedence = operatorKind.GetBinaryOperatorPrecedence ( );
                var comparePrecedence = !isParentUnary && parentOperator == operatorKind && operatorKind.IsRightAssociative ( )
                    ? precedence + 1
                    : precedence;
                if ( precedence <= 0 || comparePrecedence <= parentPrecedence )
                    break;

                SyntaxToken operatorToken = this.Next ( );
                ExpressionSyntax right = this.ParseBinaryExpression ( precedence, operatorKind, true );
                left = new BinaryExpressionSyntax ( this._syntaxTree, left, operatorToken, right );
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression ( )
        {
            return this.Current.Kind switch
            {
                SyntaxKind.NilKeyword => this.ParseNilLiteralExpression ( ),
                SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword => this.ParseBooleanLiteralExpression ( ),
                SyntaxKind.NumberToken => this.ParseNumberLiteralExpression ( ),
                SyntaxKind.ShortStringToken or SyntaxKind.LongStringToken => this.ParseStringLiteralExpression ( ),
                SyntaxKind.DotDotDotToken => this.ParseVarArgExpression ( ),
                SyntaxKind.FunctionKeyword when this.Lookahead.Kind == SyntaxKind.OpenParenthesisToken => this.ParseAnonymousFunctionDeclarationExpression ( ),
                _ => this.ParsePrefixExpression ( ),
            };
        }

        private PrefixExpressionSyntax ParsePrefixExpression ( )
        {
            PrefixExpressionSyntax expression;
            if ( this.Current.Kind == SyntaxKind.OpenParenthesisToken )
                expression = this.ParseParenthesizedExpression ( );
            else
                expression = this.ParseVariableExpression ( );

            while ( this.Current.Kind is SyntaxKind.ColonToken or SyntaxKind.OpenParenthesisToken or SyntaxKind.ShortStringToken or SyntaxKind.LongStringToken or SyntaxKind.OpenBraceToken )
            {
                SyntaxToken? colonToken = null, identifier = null;
                var isMethodCall = this.Current.Kind is SyntaxKind.ColonToken;
                if ( isMethodCall )
                {
                    colonToken = this.Match ( SyntaxKind.ColonToken );
                    identifier = this.MatchIdentifier ( );
                }

                var isAmbiguous = this.Current.Kind is SyntaxKind.OpenParenthesisToken && expression.GetLastToken ( ).TrailingTrivia.Any ( trivia => trivia.Kind == SyntaxKind.LineBreakTrivia );
                FunctionArgumentSyntax argument = this.ParseFunctionArgument ( );

                if ( isMethodCall )
                    expression = new MethodCallExpressionSyntax ( this._syntaxTree, expression, colonToken!, identifier!, argument );
                else
                    expression = new FunctionCallExpressionSyntax ( this._syntaxTree, expression, argument );
                if ( isAmbiguous )
                    this.Diagnostics.ReportAmbiguousFunctionCallOrNewStatement ( expression.Location );
            }

            return expression;
        }

        private VariableExpressionSyntax ParseVariableExpression ( )
        {
            VariableExpressionSyntax expression = this.ParseNameExpression ( );
            while ( true )
            {
                if ( this.Current.Kind == SyntaxKind.DotToken )
                    expression = this.ParseMemberAccessExpression ( expression );
                else if ( this.Current.Kind == SyntaxKind.OpenBracketToken )
                    expression = this.ParseElementAccessExpression ( expression );
                else
                    break;
            }
            return expression;
        }

        private ExpressionSyntax ParseAnonymousFunctionDeclarationExpression ( )
        {
            SyntaxToken functionKeywordToken = this.Match ( SyntaxKind.FunctionKeyword );
            ParameterListSyntax? parameterList = this.ParseParameterList ( );

            throw new NotImplementedException ( );
        }

        private NameExpressionSyntax ParseNameExpression ( )
        {
            SyntaxToken identifier = this.MatchIdentifier ( );
            return new NameExpressionSyntax ( this._syntaxTree, identifier );
        }

        private MemberAccessExpressionSyntax ParseMemberAccessExpression ( PrefixExpressionSyntax expression )
        {
            SyntaxToken dotSeparator = this.Match ( SyntaxKind.DotToken );
            SyntaxToken memberName = this.MatchIdentifier ( );
            return new MemberAccessExpressionSyntax ( this._syntaxTree, expression, dotSeparator, memberName );
        }

        private ElementAccessExpressionSyntax ParseElementAccessExpression ( PrefixExpressionSyntax expression )
        {
            SyntaxToken openBracketToken = this.Match ( SyntaxKind.OpenBracketToken );
            ExpressionSyntax elementExpression = this.ParseExpression ( );
            SyntaxToken closeBracketToken = this.Match ( SyntaxKind.CloseBracketToken );

            return new ElementAccessExpressionSyntax (
                this._syntaxTree,
                expression,
                openBracketToken,
                elementExpression,
                closeBracketToken );
        }

        private FunctionArgumentSyntax ParseFunctionArgument ( )
        {
            if ( this.Current.Kind is SyntaxKind.ShortStringToken or SyntaxKind.LongStringToken )
            {
                StringLiteralExpressionSyntax stringLiteral = this.ParseStringLiteralExpression ( );
                return new StringFunctionArgumentSyntax ( this._syntaxTree, stringLiteral );
            }
            else if ( this.Current.Kind is SyntaxKind.OpenBraceToken )
            {
                TableConstructorExpressionSyntax tableConstructor = this.ParseTableConstructorExpression ( );
                return new TableConstructorFunctionArgumentSyntax ( this._syntaxTree, tableConstructor );
            }
            else
            {
                return this.ParseFunctionArgumentList ( );
            }
        }

        private FunctionArgumentListSyntax ParseFunctionArgumentList ( )
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

            return new FunctionArgumentListSyntax (
                this._syntaxTree,
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
                    var varArgparameter = new VarArgParameterSyntax ( this._syntaxTree, varArgToken );
                    nodesAndSeparators.Add ( varArgparameter );
                    break;
                }

                SyntaxToken identifier = this.MatchIdentifier ( );
                var parameter = new NamedParameterSyntax ( this._syntaxTree, identifier );
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
                this._syntaxTree,
                openParenthesisToken,
                new SeparatedSyntaxList<ParameterSyntax> ( nodesAndSeparators.ToImmutable ( ) ),
                closeParenthesisToken );
        }

        private VarArgExpressionSyntax ParseVarArgExpression ( )
        {
            SyntaxToken varargToken = this.Match ( SyntaxKind.DotDotDotToken );
            return new VarArgExpressionSyntax ( this._syntaxTree, varargToken );
        }

        private BooleanLiteralExpressionSyntax ParseBooleanLiteralExpression ( )
        {
            SyntaxToken keywordToken = this.Current.Kind == SyntaxKind.TrueKeyword
                                       ? this.Match ( SyntaxKind.TrueKeyword )
                                       : this.Match ( SyntaxKind.FalseKeyword );
            return new BooleanLiteralExpressionSyntax ( this._syntaxTree, keywordToken );
        }

        private NilLiteralExpressionSyntax ParseNilLiteralExpression ( )
        {
            SyntaxToken nilToken = this.Match ( SyntaxKind.NilKeyword );
            return new NilLiteralExpressionSyntax ( this._syntaxTree, nilToken );
        }

        private StringLiteralExpressionSyntax ParseStringLiteralExpression ( )
        {
            SyntaxToken stringToken = this.Current.Kind == SyntaxKind.LongStringToken
                                      ? this.Match ( SyntaxKind.LongStringToken )
                                      : this.Match ( SyntaxKind.ShortStringToken );
            return new StringLiteralExpressionSyntax ( this._syntaxTree, stringToken );
        }

        private NumberLiteralExpressionSyntax ParseNumberLiteralExpression ( )
        {
            SyntaxToken number = this.Match ( SyntaxKind.NumberToken );
            return new NumberLiteralExpressionSyntax ( this._syntaxTree, number );
        }

        private ParenthesizedExpressionSyntax ParseParenthesizedExpression ( )
        {
            SyntaxToken open = this.Match ( SyntaxKind.OpenParenthesisToken );
            ExpressionSyntax expression = this.ParseExpression ( );
            SyntaxToken closing = this.Match ( SyntaxKind.CloseParenthesisToken );
            return new ParenthesizedExpressionSyntax ( this._syntaxTree, open, expression, closing );
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
                this._syntaxTree,
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

                return new IdentifierKeyedTableFieldSyntax ( this._syntaxTree, identifier, equalsToken, value );
            }
            else if ( this.Current.Kind == SyntaxKind.OpenBracketToken )
            {
                SyntaxToken openBracketToken = this.Match ( SyntaxKind.OpenBracketToken );
                ExpressionSyntax key = this.ParseExpression ( );
                SyntaxToken closeBracketToken = this.Match ( SyntaxKind.CloseBracketToken );
                SyntaxToken equalsToken = this.Match ( SyntaxKind.EqualsToken );
                ExpressionSyntax value = this.ParseExpression ( );

                return new ExpressionKeyedTableFieldSyntax ( this._syntaxTree, openBracketToken, key, closeBracketToken, equalsToken, value );
            }
            else
            {
                ExpressionSyntax value = this.ParseExpression ( );
                return new UnkeyedTableFieldSyntax ( this._syntaxTree, value );
            }
        }
    }
}
