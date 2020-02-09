using System;
using System.Collections.Generic;
using GParse;
using GParse.Errors;
using GParse.Lexing;
using GParse.Parsing;
using GParse.Parsing.Parselets;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Modules;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing
{
    public class LuaParser : PrattParser<LuaTokenType, Expression>
    {
        private readonly Stack<Scope> _scopeStack = new Stack<Scope> ( );

        #region Scope and Label Management

        protected internal virtual Scope EnterScope ( Boolean isFuntion )
        {
            Scope scope = this._scopeStack.Count > 0 ? new Scope ( this._scopeStack.Peek ( ), isFuntion ) : new Scope ( isFuntion );
            this._scopeStack.Push ( scope );
            return scope;
        }

        protected internal virtual Variable GetOrCreateVariable ( in LuaToken token, Scope.FindMode findMode ) => this._scopeStack.Peek ( ).GetVariable ( token, findMode );

        protected internal virtual GotoLabel GetOrCreateLabel ( in LuaToken token, Scope.FindMode findMode ) => this._scopeStack.Peek ( ).GetLabel ( token, findMode );

        protected internal virtual Scope? LeaveScope ( )
        {
            this._scopeStack.Pop ( );
            return this._scopeStack.Count > 0 ? this._scopeStack.Peek ( ) : null;
        }

        #endregion Scope and Label Management

        protected internal LuaParser ( ITokenReader<LuaTokenType> tokenReader, PrattParserModuleTree<LuaTokenType, IPrefixParselet<LuaTokenType, Expression>> prefixModules, PrattParserModuleTree<LuaTokenType, IInfixParselet<LuaTokenType, Expression>> infixModules, IProgress<Diagnostic> diagnosticEmitter ) : base ( tokenReader, prefixModules, infixModules, diagnosticEmitter )
        {
        }

        private static readonly String[] Terminal = new[] { "end", "else", "elseif", "until" };

        private Boolean HasTerminalAhead ( ) =>
            this.TokenReader.IsAhead ( LuaTokenType.EOF ) || this.TokenReader.IsAhead ( Terminal );

        protected Expression ParseExpression ( )
        {
            if ( !this.TryParseExpression ( out Expression expression ) )
            {
                throw new FatalParsingException ( this.TokenReader.Location, "Unable to parse this expression" );
            }

            return expression;
        }

        protected IdentifierExpression ParseIdentifierExpression ( Boolean checkParents )
        {
            LuaToken ident = this.TokenReader.FatalExpect ( LuaTokenType.Identifier );
            Variable variable = this.GetOrCreateVariable ( ident, checkParents ? Scope.FindMode.CheckParents : Scope.FindMode.CheckSelf );

            return new IdentifierExpression ( ident, variable );
        }

        protected Expression ParseFunctionName ( )
        {
            LuaToken ident;
            Expression name = this.ParseIdentifierExpression ( true );

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

        private DoStatement ParseDoStatement ( )
        {
            return new DoStatement (
                this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "do" ),
                this.ParseScopedStatementList ( false ),
                this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "end" )
            );
        }

        private FunctionDefinitionStatement ParseFunctionDefinitionStatement ( )
        {
            var isLocal = this.TokenReader.Accept ( LuaTokenType.Keyword, "local", out LuaToken localKw );
            LuaToken funcKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "function" );
            Expression name = isLocal ? this.ParseIdentifierExpression ( false ) : this.ParseFunctionName ( );
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

                args.Add ( this.ParseIdentifierExpression ( false ) );
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

        private LocalVariableDeclarationStatement ParseLocalVariableDeclarationStatement ( )
        {
            LuaToken localKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "local" );
            var idents = new List<IdentifierExpression>
            {
                this.ParseIdentifierExpression ( false )
            };
            var identCommas = new List<LuaToken> ( );
            while ( this.TokenReader.Accept ( LuaTokenType.Comma, out LuaToken comma ) )
            {
                identCommas.Add ( comma );
                idents.Add ( this.ParseIdentifierExpression ( false ) );
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

                return new LocalVariableDeclarationStatement ( localKw, idents, identCommas, equals, vals, valCommas );
            }

            return new LocalVariableDeclarationStatement ( localKw, idents, identCommas );
        }

        private GotoLabelStatement ParseGotoLabelStatement ( )
        {
            LuaToken ldelim = this.TokenReader.FatalExpect ( LuaTokenType.GotoLabelDelimiter );
            LuaToken ident = this.TokenReader.FatalExpect ( LuaTokenType.Identifier );
            LuaToken rdelim = this.TokenReader.FatalExpect ( LuaTokenType.GotoLabelDelimiter );
            return new GotoLabelStatement ( ldelim, this.GetOrCreateLabel ( ident, Scope.FindMode.CheckFunctionScope ), ident, rdelim );
        }

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

        private GenericForLoopStatement ParseGenericForLoopStatement ( )
        {
            Scope scope = this.EnterScope ( false );
            LuaToken forKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "for" );

            // usually these are k, v loops, so start out with 2 elements
            var vars = new List<IdentifierExpression> ( 2 ) { this.ParseIdentifierExpression ( false ) };
            var commas = new List<LuaToken> ( 1 );
            while ( this.TokenReader.Accept ( LuaTokenType.Comma, out LuaToken comma ) )
            {
                commas.Add ( comma );
                vars.Add ( this.ParseIdentifierExpression ( false ) );
            }

            LuaToken inKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "in" );
            Expression iteratable = this.ParseExpression ( );
            LuaToken doKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "do" );
            StatementList body = this.ParseStatementList ( scope );
            LuaToken endKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "end" );
            this.LeaveScope ( );

            return new GenericForLoopStatement ( scope, forKw, vars, commas, inKw, iteratable, doKw, body, endKw );
        }

        private NumericForLoopStatement ParseNumericForLoopStatement ( )
        {
            Scope scope = this.EnterScope ( false );

            // The for keyword
            LuaToken forKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "for" );

            // Loop variable name
            IdentifierExpression var = this.ParseIdentifierExpression ( false );

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

            return step != null
                ? new NumericForLoopStatement ( scope, forKw, var, equals, initial, comma1, final, comma2, step, doKw, body, endKw )
                : new NumericForLoopStatement ( scope, forKw, var, equals, initial, comma1, final, doKw, body, endKw );
        }

        private ReturnStatement ParseReturnStatement ( )
        {
            LuaToken returnKw = this.TokenReader.FatalExpect ( LuaTokenType.Keyword, "return" );
            var retvals = new List<Expression> ( );
            var commas = new List<LuaToken> ( );
            if ( !this.HasTerminalAhead ( ) )
            {
                retvals.Add ( this.ParseExpression ( ) );
                while ( this.TokenReader.Accept ( LuaTokenType.Comma, out LuaToken comma ) )
                {
                    commas.Add ( comma );
                    retvals.Add ( this.ParseExpression ( ) );
                }
            }

            return new ReturnStatement ( returnKw, retvals, commas );
        }

        private Statement ParseStatement ( )
        {
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
            else if ( this.TokenReader.Accept ( LuaTokenType.Keyword, "goto", out LuaToken gotoKw ) )
            {
                LuaToken ident = this.TokenReader.FatalExpect ( LuaTokenType.Identifier );
                stmt = new GotoStatement ( gotoKw, ident, this.GetOrCreateLabel ( ident, Scope.FindMode.CheckFunctionScope ) );
            }
            else if ( this.TokenReader.Accept ( LuaTokenType.Keyword, "break", out LuaToken breakKw ) )
            {
                stmt = new BreakStatement ( breakKw );
            }
            else if ( this.TokenReader.Accept ( LuaTokenType.Keyword, "continue", out LuaToken continueKw ) )
            {
                stmt = new ContinueStatement ( continueKw );
            }
            else if ( this.TokenReader.IsAhead ( LuaTokenType.GotoLabelDelimiter ) )
            {
                stmt = this.ParseGotoLabelStatement ( );
            }
            else if ( !this.TokenReader.IsAhead ( LuaTokenType.Semicolon ) )
            {
                Expression expr = this.ParseExpression ( );
                stmt = ( expr is IndexExpression || expr is IdentifierExpression ) && ( this.TokenReader.IsAhead ( LuaTokenType.Operator, "=" ) || this.TokenReader.IsAhead ( LuaTokenType.Comma ) )
                    ? this.ParseAssignmentStatement ( expr )
                    : ( Statement ) new ExpressionStatement ( expr );
            }
            else
            {
                throw new NotImplementedException ( );
            }

            if ( this.TokenReader.Accept ( LuaTokenType.Semicolon, out LuaToken semicolon ) )
                stmt.Semicolon = semicolon;

            return stmt;
        }

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

        public StatementList Parse ( )
        {
            StatementList res = this.ParseScopedStatementList ( true );
            this.TokenReader.FatalExpect ( LuaTokenType.EOF );
            return res;
        }
    }
}