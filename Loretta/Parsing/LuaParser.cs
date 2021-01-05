using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GParse;
using GParse.Errors;
using GParse.Lexing;
using GParse.Parsing;
using GParse.Parsing.Parselets;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing
{
    /// <summary>
    /// The lua parser class.
    /// </summary>
    public class LuaParser : PrattParser<LuaTokenType, Expression>
    {
        private readonly Stack<Scope> _scopeStack = new Stack<Scope> ( );

        /// <summary>
        /// The lua options used by this parser.
        /// </summary>
        public LuaOptions LuaOptions { get; }

        #region Scope and Label Management

        /// <summary>
        /// Creates and enters a new scope.
        /// </summary>
        /// <param name="isFuntion">Whether the scope is a function's.</param>
        /// <returns></returns>
        protected internal virtual Scope EnterScope ( Boolean isFuntion )
        {
            Scope scope = this._scopeStack.Count > 0 ? new Scope ( this._scopeStack.Peek ( ), isFuntion ) : new Scope ( isFuntion );
            this._scopeStack.Push ( scope );
            return scope;
        }

        /// <summary>
        /// Retrieves or creates a variable.
        /// </summary>
        /// <param name="token">The token that generated the variable.</param>
        /// <param name="findMode">The scope search mode.</param>
        /// <returns></returns>
        protected internal virtual Variable GetOrCreateVariable ( in LuaToken token, Scope.FindMode findMode ) => this._scopeStack.Peek ( ).GetVariable ( token, findMode );

        /// <summary>
        /// Retrieves or creates a goto label.
        /// </summary>
        /// <param name="token">The token that generated the goto label.</param>
        /// <param name="findMode">The scope search mode.</param>
        /// <returns></returns>
        protected internal virtual GotoLabel GetOrCreateLabel ( in LuaToken token, Scope.FindMode findMode ) => this._scopeStack.Peek ( ).GetLabel ( token, findMode );

        /// <summary>
        /// Returns and leaves the current scope.
        /// </summary>
        /// <returns></returns>
        protected internal virtual Scope? LeaveScope ( )
        {
            this._scopeStack.Pop ( );
            return this._scopeStack.Count > 0 ? this._scopeStack.Peek ( ) : null;
        }

        #endregion Scope and Label Management

        /// <summary>
        /// Initializes a new lua parser.
        /// </summary>
        /// <param name="luaOptions">The lua options to use.</param>
        /// <param name="tokenReader">The token reader to use</param>
        /// <param name="prefixModules">The prefix module tree.</param>
        /// <param name="infixModules">The infix module tree.</param>
        /// <param name="diagnosticEmitter">The diagnostic emitter.</param>
        protected internal LuaParser (
            LuaOptions luaOptions,
            ITokenReader<LuaTokenType> tokenReader,
            PrattParserModuleTree<LuaTokenType, IPrefixParselet<LuaTokenType, Expression>> prefixModules,
            PrattParserModuleTree<LuaTokenType, IInfixParselet<LuaTokenType, Expression>> infixModules,
            IProgress<Diagnostic> diagnosticEmitter )
            : base ( tokenReader, prefixModules, infixModules, diagnosticEmitter )
        {
            this.LuaOptions = luaOptions;
        }

        /// <summary>
        /// The list of terminal token ids.
        /// </summary>
        private static readonly String[] terminals = new[] { "end", "else", "elseif", "until" };

        /// <summary>
        /// The compound assignment operator token ids.
        /// </summary>
        private static readonly String[] compoundAssignmentOperatorIds = new[] { "+=", "-=", "*=", "/=", "^=", "%=", "..=" };

        /// <summary>
        /// Whether the next token is a terminal.
        /// </summary>
        /// <returns></returns>
        private Boolean HasTerminalAhead ( ) =>
            this.TokenReader.IsAhead ( LuaTokenType.EOF ) || this.TokenReader.IsAhead ( terminals );

        /// <summary>
        /// Attempts to parse an expression and throws a <see cref="FatalParsingException" /> if
        /// unable to.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FatalParsingException">Thrown when the expression parsing fails.</exception>
        protected Expression ParseExpression ( )
        {
            if ( !this.TryParseExpression ( out Expression expression ) )
            {
                throw new FatalParsingException ( this.TokenReader.Location, "Unable to parse this expression" );
            }

            return expression;
        }

        /// <summary>
        /// Parses an identifier expression.
        /// </summary>
        /// <param name="findMode">The finding mode to use when getting a variable for the identifier.</param>
        /// <returns></returns>
        protected IdentifierExpression ParseIdentifierExpression ( Scope.FindMode findMode )
        {
            LuaToken ident = this.TokenReader.FatalExpect ( LuaTokenType.Identifier );
            Variable variable = this.GetOrCreateVariable ( ident, findMode );

            return new IdentifierExpression ( ident, variable );
        }

        /// <summary>
        /// Parses a function name.
        /// </summary>
        /// <returns></returns>
        protected Expression ParseFunctionName ( )
        {
            LuaToken ident;
            Expression name = this.ParseIdentifierExpression ( Scope.FindMode.CheckParents );

            while ( this.TokenReader.Accept ( LuaTokenType.Dot, out LuaToken dot ) )
            {
                ident = this.TokenReader.FatalExpect ( LuaTokenType.Identifier );
                name = new IndexExpression ( name, dot, new IdentifierExpression ( ident, null ) );
            }

            if ( this.TokenReader.Accept ( LuaTokenType.Colon, out LuaToken colon ) )
            {
                ident = this.TokenReader.FatalExpect ( LuaTokenType.Identifier );
                name = new IndexExpression ( name, colon, new IdentifierExpression ( ident, null ) );
            }

            return name;
        }

        /// <summary>
        /// Parses a <see cref="DoStatement" />.
        /// </summary>
        /// <returns></returns>
        private DoStatement ParseDoStatement ( )
        {
            return new DoStatement (
                this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "do" ),
                this.ParseScopedStatementList ( false ),
                this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "end" )
            );
        }

        /// <summary>
        /// Parses a (local or not) function definition statement.
        /// </summary>
        /// <returns></returns>
        private FunctionDefinitionStatement ParseFunctionDefinitionStatement ( )
        {
            var isLocal = this.TokenReader.Accept ( LuaTokenType.Keyword, "local", out LuaToken localKw );
            LuaToken funcKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "function" );
            Expression name = isLocal ? this.ParseIdentifierExpression ( Scope.FindMode.DontCheck ) : this.ParseFunctionName ( );
            LuaToken lparen = this.TokenReader.FatalExpect ( LuaTokenType.LParen );

            Scope scope = this.EnterScope ( true );
            var args = new List<Expression> ( );
            var commas = new List<LuaToken> ( );
            LuaToken rparen;
            while ( !this.TokenReader.Accept ( LuaTokenType.RParen, out rparen ) )
            {
                if ( this.TokenReader.Accept ( LuaTokenType.VarArg, out LuaToken vararg ) )
                {
                    args.Add ( new VarArgExpression ( vararg ) );
                    rparen = this.TokenReader.FatalExpect ( LuaTokenType.RParen );
                    break;
                }

                args.Add ( this.ParseIdentifierExpression ( Scope.FindMode.DontCheck ) );
                if ( this.TokenReader.Accept ( LuaTokenType.RParen, out rparen ) )
                    break;
                commas.Add ( this.TokenReader.FatalExpect ( LuaTokenType.Comma ) );
            }

            StatementList body = this.ParseStatementList ( scope );
            LuaToken endKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "end" );
            this.LeaveScope ( );

            return isLocal
                ? new FunctionDefinitionStatement ( localKw, funcKw, name, lparen, args, commas, rparen, body, endKw )
                : new FunctionDefinitionStatement ( funcKw, name, lparen, args, commas, rparen, body, endKw );
        }

        /// <summary>
        /// Parses an <see cref="IfStatement" />.
        /// </summary>
        /// <returns></returns>
        private IfStatement ParseIfStatement ( )
        {
            var clauses = new List<IfClause>
            {
                new IfClause (
                    this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "if" ),
                    this.ParseExpression ( ),
                    this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "then" ),
                    this.ParseScopedStatementList ( false )
                )
            };

            while ( this.TokenReader.Accept ( LuaTokenType.Keyword, "elseif", out LuaToken elseifKw ) )
            {
                clauses.Add ( new IfClause (
                    elseifKw,
                    this.ParseExpression ( ),
                    this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "then" ),
                    this.ParseScopedStatementList ( false )
                ) );
            }

            return this.TokenReader.Accept ( LuaTokenType.Keyword, "else", out LuaToken elseKw )
                ? new IfStatement (
                    clauses,
                    elseKw,
                    this.ParseScopedStatementList ( false ),
                    this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "end" ) )
                : new IfStatement ( clauses, this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "end" ) );
        }

        /// <summary>
        /// Parses a <see cref="LocalVariableDeclarationStatement" />.
        /// </summary>
        /// <returns></returns>
        private LocalVariableDeclarationStatement ParseLocalVariableDeclarationStatement ( )
        {
            LuaToken localKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "local" );
            var identTokens = new List<LuaToken>
            {
                this.TokenReader.FatalExpect ( LuaTokenType.Identifier )
            };
            var identCommas = new List<LuaToken> ( );
            while ( this.TokenReader.Accept ( LuaTokenType.Comma, out LuaToken comma ) )
            {
                identCommas.Add ( comma );
                identTokens.Add ( this.TokenReader.FatalExpect ( LuaTokenType.Identifier ) );
            }

            if ( this.TokenReader.Accept ( LuaTokenType.Operator, "=", out LuaToken equals ) )
            {
                var vals = new List<Expression> { this.ParseExpression ( ) };
                var valCommas = new List<LuaToken> ( );
                while ( this.TokenReader.Accept ( LuaTokenType.Comma, out LuaToken comma ) )
                {
                    valCommas.Add ( comma );
                    vals.Add ( this.ParseExpression ( ) );
                }

                IdentifierExpression[] idents = identTokens.Select ( ident => new IdentifierExpression ( ident, this.GetOrCreateVariable ( ident, Scope.FindMode.DontCheck ) ) )
                                                           .ToArray ( );
                return new LocalVariableDeclarationStatement ( localKw, idents, identCommas, equals, vals, valCommas );
            }
            else
            {
                IdentifierExpression[] idents = identTokens.Select ( ident => new IdentifierExpression ( ident, this.GetOrCreateVariable ( ident, Scope.FindMode.DontCheck ) ) )
                                                           .ToArray ( );
                return new LocalVariableDeclarationStatement ( localKw, idents, identCommas );
            }
        }

        /// <summary>
        /// Parses a <see cref="GotoLabelStatement" />.
        /// </summary>
        /// <returns></returns>
        private GotoLabelStatement ParseGotoLabelStatement ( )
        {
            LuaToken ldelim = this.TokenReader.FatalExpect ( LuaTokenType.GotoLabelDelimiter );
            LuaToken ident = this.TokenReader.FatalExpect ( LuaTokenType.Identifier );
            LuaToken rdelim = this.TokenReader.FatalExpect ( LuaTokenType.GotoLabelDelimiter );
            return new GotoLabelStatement ( ldelim, this.GetOrCreateLabel ( ident, Scope.FindMode.CheckFunctionScope ), ident, rdelim );
        }

        /// <summary>
        /// Parses an <see cref="AssignmentStatement" />.
        /// </summary>
        /// <param name="expr">The first expression on the left side of assignment.</param>
        /// <returns></returns>
        private AssignmentStatement ParseAssignmentStatement ( Expression expr )
        {
            var vars = new List<Expression> { expr };
            var varsCommas = new List<LuaToken> ( );
            while ( this.TokenReader.Accept ( LuaTokenType.Comma, out LuaToken comma ) )
            {
                varsCommas.Add ( comma );
                vars.Add ( this.ParseExpression ( ) );
            }

            LuaToken equals = this.TokenReader.FatalExpect ( LuaTokenType.Operator, "=" );
            var vals = new List<Expression> { this.ParseExpression ( ) };
            var valsCommas = new List<LuaToken> ( );
            while ( this.TokenReader.Accept ( LuaTokenType.Comma, out LuaToken comma ) )
            {
                valsCommas.Add ( comma );
                vals.Add ( this.ParseExpression ( ) );
            }

            return new AssignmentStatement ( vars, varsCommas, equals, vals, valsCommas );
        }

        /// <summary>
        /// Parses a <see cref="CompoundAssignmentStatement" />.
        /// </summary>
        /// <param name="assignee">The expression on the left side of the assignment.</param>
        /// <returns></returns>
        private CompoundAssignmentStatement ParseCompoundAssignmentStatement ( Expression assignee )
        {
            LuaToken compoundAssignmentOperator = this.TokenReader.FatalExpect ( new[] { LuaTokenType.Operator }, compoundAssignmentOperatorIds );

            Expression? value = this.ParseExpression ( );

            return new CompoundAssignmentStatement ( assignee, compoundAssignmentOperator, value );
        }

        /// <summary>
        /// Parses a <see cref="WhileLoopStatement" />.
        /// </summary>
        /// <returns></returns>
        private WhileLoopStatement ParseWhileLoopStatement ( )
        {
            return new WhileLoopStatement (
                this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "while" ),
                this.ParseExpression ( ),
                this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "do" ),
                this.ParseScopedStatementList ( false ),
                this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "end" )
            );
        }

        /// <summary>
        /// Parses a <see cref="RepeatUntilStatement" />.
        /// </summary>
        /// <returns></returns>
        private RepeatUntilStatement ParseRepeatUntilLoopStatement ( )
        {
            try
            {
                Scope scope = this.EnterScope ( false );
                return new RepeatUntilStatement (
                    scope,
                    this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "repeat" ),
                    this.ParseStatementList ( scope ),
                    this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "until" ),
                    this.ParseExpression ( )
                );
            }
            finally
            {
                this.LeaveScope ( );
            }
        }

        /// <summary>
        /// Parses a <see cref="GenericForLoopStatement" />.
        /// </summary>
        /// <returns></returns>
        private GenericForLoopStatement ParseGenericForLoopStatement ( )
        {
            Scope scope = this.EnterScope ( false );
            LuaToken forKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "for" );

            // usually these are k, v loops, so start out with 2 elements
            var vars = new List<IdentifierExpression> ( 2 ) { this.ParseIdentifierExpression ( Scope.FindMode.DontCheck ) };
            var commas = new List<LuaToken> ( 1 );
            while ( this.TokenReader.Accept ( LuaTokenType.Comma, out LuaToken comma ) )
            {
                commas.Add ( comma );
                vars.Add ( this.ParseIdentifierExpression ( Scope.FindMode.DontCheck ) );
            }

            LuaToken inKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "in" );
            // usually these only have a single expression so start out with 1 element
            var expressions = new List<Expression> ( 1 ) { this.ParseExpression ( ) };
            while ( this.TokenReader.Accept ( LuaTokenType.Comma, out LuaToken comma ) )
            {
                commas.Add ( comma );
                expressions.Add ( this.ParseExpression ( ) );
            }
            LuaToken doKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "do" );
            StatementList body = this.ParseStatementList ( scope );
            LuaToken endKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "end" );
            this.LeaveScope ( );

            return new GenericForLoopStatement ( scope, forKw, vars, commas, inKw, expressions, doKw, body, endKw );
        }

        /// <summary>
        /// Parses a <see cref="NumericForLoopStatement" />.
        /// </summary>
        /// <returns></returns>
        private NumericForLoopStatement ParseNumericForLoopStatement ( )
        {
            Scope scope = this.EnterScope ( false );

            // The for keyword
            LuaToken forKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "for" );

            // Loop variable name
            IdentifierExpression var = this.ParseIdentifierExpression ( Scope.FindMode.DontCheck );

            // Assignment
            LuaToken equals = this.TokenReader.FatalExpect ( LuaTokenType.Operator, "=" );

            // Initial value
            Expression initial = this.ParseExpression ( );

            // First comma
            LuaToken comma1 = this.TokenReader.FatalExpect ( LuaTokenType.Comma );

            // Final value
            Expression final = this.ParseExpression ( );

            // Attempts to get the step (if any)
            Expression? step = null;
            if ( this.TokenReader.Accept ( LuaTokenType.Comma, out LuaToken comma2 ) )
                step = this.ParseExpression ( );

            // Loop body
            LuaToken doKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "do" );
            StatementList body = this.ParseStatementList ( scope );
            LuaToken endKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "end" );

            this.LeaveScope ( );

            return step != null
                ? new NumericForLoopStatement ( scope, forKw, var, equals, initial, comma1, final, comma2, step, doKw, body, endKw )
                : new NumericForLoopStatement ( scope, forKw, var, equals, initial, comma1, final, doKw, body, endKw );
        }

        /// <summary>
        /// Parses a <see cref="ReturnStatement" />.
        /// </summary>
        /// <returns></returns>
        private ReturnStatement ParseReturnStatement ( )
        {
            LuaToken returnKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "return" );
            var retvals = new List<Expression> ( );
            var commas = new List<LuaToken> ( );
            if ( this.TryParseExpression ( out Expression firstReturn ) )
            {
                retvals.Add ( firstReturn );
                while ( this.TokenReader.Accept ( LuaTokenType.Comma, out LuaToken comma ) )
                {
                    commas.Add ( comma );
                    retvals.Add ( this.ParseExpression ( ) );
                }
            }

            return new ReturnStatement ( returnKw, retvals, commas );
        }

        /// <summary>
        /// Parses a <see cref="Statement" />.
        /// </summary>
        /// <returns></returns>
        private Statement ParseStatement ( )
        {
            SourceLocation startLocation = this.TokenReader.Location;
            Statement stmt;
            if ( this.TokenReader.IsAhead ( LuaTokenType.Keyword, "local" ) )
            {
                stmt = this.TokenReader.IsAhead ( LuaTokenType.Keyword, "function", 1 )
                    ? this.ParseFunctionDefinitionStatement ( )
                    : ( Statement ) this.ParseLocalVariableDeclarationStatement ( );
            }
            else if ( this.TokenReader.IsAhead ( LuaTokenType.Keyword, "function" ) )
            {
                stmt = this.ParseFunctionDefinitionStatement ( );
            }
            else if ( this.TokenReader.IsAhead ( LuaTokenType.Keyword, "for" ) )
            {
                stmt = this.TokenReader.IsAhead ( LuaTokenType.Operator, "=", 2 )
                    ? this.ParseNumericForLoopStatement ( )
                    : ( Statement ) this.ParseGenericForLoopStatement ( );
            }
            else if ( this.TokenReader.IsAhead ( LuaTokenType.Keyword, "if" ) )
            {
                stmt = this.ParseIfStatement ( );
            }
            else if ( this.TokenReader.IsAhead ( LuaTokenType.Keyword, "repeat" ) )
            {
                stmt = this.ParseRepeatUntilLoopStatement ( );
            }
            else if ( this.TokenReader.IsAhead ( LuaTokenType.Keyword, "while" ) )
            {
                stmt = this.ParseWhileLoopStatement ( );
            }
            else if ( this.TokenReader.IsAhead ( LuaTokenType.Keyword, "do" ) )
            {
                stmt = this.ParseDoStatement ( );
            }
            else if ( this.LuaOptions.AcceptGoto
                      && this.TokenReader.Accept ( LuaTokenType.Keyword, "goto", out LuaToken gotoKw ) )
            {
                LuaToken ident = this.TokenReader.FatalExpect ( LuaTokenType.Identifier );
                stmt = new GotoStatement ( gotoKw, ident, this.GetOrCreateLabel ( ident, Scope.FindMode.CheckFunctionScope ) );
            }
            else if ( this.TokenReader.Accept ( LuaTokenType.Keyword, "break", out LuaToken breakKw ) )
            {
                stmt = new BreakStatement ( breakKw );
            }
            else if ( this.LuaOptions.ContinueType != ContinueType.None
                      && this.TokenReader.Accept ( ( LuaTokenType ) this.LuaOptions.ContinueType,
                                                   "continue",
                                                   out LuaToken continueKw ) )
            {
                stmt = new ContinueStatement ( TokenFactory.ChangeTokenType ( continueKw, LuaTokenType.Keyword ) );
            }
            else if ( this.TokenReader.IsAhead ( LuaTokenType.GotoLabelDelimiter ) )
            {
                stmt = this.ParseGotoLabelStatement ( );
            }
            else if ( !this.TokenReader.IsAhead ( LuaTokenType.Semicolon ) )
            {
                Expression expr = this.ParseExpression ( );
                if ( expr is IndexExpression || expr is IdentifierExpression )
                {
                    if ( this.TokenReader.IsAhead ( LuaTokenType.Operator, "=" ) || this.TokenReader.IsAhead ( LuaTokenType.Comma ) )
                    {
                        stmt = this.ParseAssignmentStatement ( expr );
                    }
                    else if ( this.LuaOptions.AcceptCompoundAssignment && this.TokenReader.IsAhead ( new[] { LuaTokenType.Operator }, compoundAssignmentOperatorIds ) )
                    {
                        stmt = this.ParseCompoundAssignmentStatement ( expr );
                    }
                    else
                    {
                        goto unexpectedToken;
                    }
                }
                else if ( expr is FunctionCallExpression )
                {
                    stmt = new ExpressionStatement ( expr );
                }
                else
                {
                    goto unexpectedToken;
                }
            }
            else if ( this.LuaOptions.AcceptEmptyStatements && this.TokenReader.Accept ( LuaTokenType.Semicolon, out LuaToken emptyStmtSemicolon ) )
            {
                return new EmptyStatement { Semicolon = emptyStmtSemicolon };
            }
            else
            {
                goto unexpectedToken;
            }

            if ( this.TokenReader.Accept ( LuaTokenType.Semicolon, out LuaToken semicolon ) )
                stmt.Semicolon = semicolon;

            return stmt;

        unexpectedToken:
            this.TokenReader.Rewind ( startLocation );
            LuaToken followingToken = this.TokenReader.Lookahead ( );
            var message = $"Unexpected {followingToken.Type}";
            if ( followingToken.Type.CanUseRawInError ( ) )
                message += $" '{followingToken.Raw}'";
            message += $" at {followingToken.Range.Start}.";
            throw new FatalParsingException ( this.TokenReader.Location, message );
        }

        /// <summary>
        /// Parses a <see cref="StatementList" />.
        /// </summary>
        /// <param name="scope">The statement list's scope.</param>
        /// <returns></returns>
        public StatementList ParseStatementList ( Scope scope )
        {
            var statements = new List<Statement> ( );
            while ( !this.HasTerminalAhead ( ) )
            {
                if ( this.TokenReader.IsAhead ( LuaTokenType.Keyword, "return" ) )
                {
                    ReturnStatement returnStmt = this.ParseReturnStatement ( );
                    if ( this.TokenReader.Accept ( LuaTokenType.Semicolon, out LuaToken semicolon ) )
                        returnStmt.Semicolon = semicolon;
                    statements.Add ( returnStmt );
                    break;
                }
                statements.Add ( this.ParseStatement ( ) );
            }
            return new StatementList ( scope, statements );
        }

        /// <summary>
        /// Parses a <see cref="StatementList" /> with a dedicated scope.
        /// </summary>
        /// <param name="isFunction">Whether the scope is a function's.</param>
        /// <returns></returns>
        public StatementList ParseScopedStatementList ( Boolean isFunction )
        {
            try
            {
                Scope scope = this.EnterScope ( isFunction );
                return this.ParseStatementList ( scope );
            }
            finally
            {
                this.LeaveScope ( );
            }
        }

        /// <summary>
        /// Parses the entire provided code.
        /// </summary>
        /// <returns></returns>
        public StatementList Parse ( )
        {
            StatementList res = this.ParseScopedStatementList ( true );
            this.TokenReader.FatalExpect ( LuaTokenType.EOF );
            Debug.Assert ( this._scopeStack.Count == 0, "Leaked scope." );
            return res;
        }
    }
}