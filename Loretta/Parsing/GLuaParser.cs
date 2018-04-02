using System;
using System.Collections.Generic;
using GParse.Lexing;
using GParse.Parsing;
using GParse.Parsing.Errors;
using Loretta.Lexing;
using Loretta.Parsing.Nodes;
using Loretta.Parsing.Nodes.Constants;
using Loretta.Parsing.Nodes.ControlStatements;
using Loretta.Parsing.Nodes.Functions;
using Loretta.Parsing.Nodes.IfStatements;
using Loretta.Parsing.Nodes.Indexers;
using Loretta.Parsing.Nodes.Loops;
using Loretta.Parsing.Nodes.Operators;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Parsing
{
    public class GLuaParser : ParserBase
    {
        public LuaEnvironment Environment { get; }

        protected Stack<Scope> ScopeStack { get; }

        public Scope RootScope { get; }

        public Scope CurrentScope => this.ScopeStack.Peek ( );
#if DEBUG
        // Just for the sake of debugging

        private LToken CurrentToken => ( LToken ) this.Peek ( );

        private LToken NextToken => ( LToken ) this.Peek ( 1 );
#endif
        public GLuaParser ( GLuaLexer lexer, LuaEnvironment environment ) : base ( lexer )
        {
            this.Environment = environment;
            this.RootScope = this.CreateScope ( null );
            this.ScopeStack = new Stack<Scope> ( );
        }

        #region Scope Management

        public Scope CreateScope ( Scope parent )
        {
            return new Scope ( this, parent );
        }

        public void PushScope ( Scope scope )
        {
            this.ScopeStack.Push ( scope );
        }

        public Scope PopScope ( )
        {
            return this.ScopeStack.Pop ( );
        }

        #endregion Scope Management

        #region Actual Parsing

        protected NumberExpression ParseNumberExpression ( ASTNode parent, Scope scope )
        {
            var tokens = new List<LToken> ( 1 );
            // Get the number token into the list
            this.Expect ( TokenType.Number, tokens );

            return new NumberExpression ( parent, scope, tokens );
        }

        protected StringExpression ParseStringExpression ( ASTNode parent, Scope scope )
        {
            var tokens = new List<LToken> ( 1 );
            // Get the string into the list
            this.Expect ( TokenType.String, tokens );

            return new StringExpression ( parent, scope, tokens );
        }

        protected NilExpression ParseNilExpression ( ASTNode parent, Scope scope )
        {
            var tokens = new List<LToken> ( 1 );
            // Get the nil into the list
            this.Expect ( TokenType.Keyword, "nil", tokens );

            return new NilExpression ( parent, scope, tokens );
        }

        protected BooleanExpression ParseBooleanExpression ( ASTNode parent, Scope scope )
        {
            var tokens = new List<LToken> ( 1 );
            // Get the bool token into the list
            this.Expect ( new[] { "true", "false" }, tokens );

            return new BooleanExpression ( parent, scope, tokens );
        }

        protected VarArgExpression ParseVarArgExpression ( ASTNode parent, Scope scope )
        {
            var tokens = new List<LToken> ( 1 );
            // Get the vararg into the list
            this.Expect ( "...", tokens );

            return new VarArgExpression ( parent, scope, tokens );
        }

        protected TableConstructorExpression ParseTableConstructorExpression ( ASTNode parent, Scope scope )
        {
            var fullTableTokens = new List<LToken> ( );
            var tableExpr = new TableConstructorExpression ( parent, scope, fullTableTokens );

            this.Expect ( "{", fullTableTokens );

            while ( true )
            {
                TableKeyValue fieldExp;
                if ( this.NextIs ( "[" ) )
                {
                    ;// tbl = {
                    ;//     [expr] = expr(,|;)
                    ;// }
                    var fieldTokens = new List<LToken> ( );
                    this.Get ( fieldTokens );

                    ASTNode keyExpr = this.ParseExpression ( tableExpr, scope );

                    this.Expect ( "]", fieldTokens );
                    this.Expect ( "=", fieldTokens );

                    ASTNode valExpr = this.ParseExpression ( tableExpr, scope );

                    fieldExp = tableExpr.AddExplicitKeyField ( keyExpr, valExpr, "", fieldTokens );
                    fullTableTokens.AddRange ( fieldTokens );
                }
                else if ( this.NextIs ( "ident" ) )
                {
                    if ( this.Peek ( 1 ).ID == "=" )
                    {
                        ;// tbl = {
                        ;//     keyString = expr(,|;)
                        ;// }
                        var fieldTokens = new List<LToken> ( );

                        LToken strTok = this.Get<LToken> ( );
                        ASTNode strExp = new StringExpression ( tableExpr, scope, new List<LToken> ( new[] { strTok } ) );

                        this.Expect ( "=", fieldTokens );

                        ASTNode valExp = this.ParseExpression ( tableExpr, scope );

                        fieldExp = tableExpr.AddExplicitKeyField ( strExp, valExp, "", fieldTokens );
                        fullTableTokens.AddRange ( fieldTokens );
                    }
                    else
                    {
                        ;// tbl = {
                        ;//     identExpr,
                        ;// }

                        ASTNode identExpr = this.ParseExpression ( tableExpr, scope );

                        fieldExp = tableExpr.AddSequentialField ( identExpr, "", new List<LToken> ( ) );
                    }
                }
                else if ( this.Consume ( "}", out LToken _, fullTableTokens ) )
                    // table end
                    break;
                else
                {
                    ASTNode valExpr = this.ParseExpression ( tableExpr, scope );

                    fieldExp = tableExpr.AddSequentialField ( valExpr, "", new List<LToken> ( ) );
                }

                if ( this.NextIs ( "," ) || this.NextIs ( ";" ) )
                {
                    LToken sepToken = this.Get<LToken> ( );
                    fieldExp.Separator = sepToken.Raw;
                    fieldExp.Tokens.Add ( sepToken );

                    if ( this.Consume ( "}", out LToken _, fullTableTokens ) )
                    {
                        break;
                    }
                }
                else
                {
                    this.Expect ( "}", fullTableTokens );
                    break;
                }
            }

            return tableExpr;
        }

        protected VariableExpression ParseVariableExpression ( ASTNode parent, Scope scope )
        {
            var tokens = new List<LToken> ( );
            LToken identTok = this.Expect ( "ident", tokens );
            Variable variable = scope.GetVariable ( ( String ) identTok.Value );

            return new VariableExpression ( parent, scope, tokens ) { Variable = variable };
        }

        protected VariableExpression ParseLocalVariableExpression ( ASTNode parent, Scope scope )
        {
            var tokens = new List<LToken> ( );
            LToken identTok = this.Expect ( "ident", tokens );
            Variable local = scope.CreateLocalVariable ( ( String ) identTok.Value );

            return new VariableExpression ( parent, scope, tokens ) { Variable = local };
        }

        protected VariableExpression ParseFunctionParameter ( ASTNode parent, Scope scope )
            => this.ParseLocalVariableExpression ( parent, scope );

        // Helper function to parse function args
        protected ASTNode[] ParseFunctionArgs ( ASTNode parent, Scope scope, IList<LToken> tokens )
        {
            this.Expect ( "(", tokens );
            var args = new List<ASTNode> ( );

            if ( this.NextIs ( "ident" ) )
            {
                while ( true )
                {
                    if ( this.NextIs ( "..." ) )
                    {
                        VarArgExpression varArgExpr = this.ParseVarArgExpression ( parent, scope );
                        args.Add ( varArgExpr );
                        break;
                    }
                    else
                    {
                        VariableExpression arg = this.ParseFunctionParameter ( parent, scope );
                        args.Add ( arg );

                        if ( !this.Consume ( ",", out LToken _, tokens ) )
                            break;
                    }
                }
            }

            if ( args.Count == 0 )
            {
                if ( this.NextIs ( "..." ) )
                {
                    VarArgExpression varArgExpr = this.ParseVarArgExpression ( parent, scope );
                    args.Add ( varArgExpr );
                }
            }

            this.Expect ( ")", tokens );

            return args.ToArray ( );
        }

        protected AnonymousFunctionExpression ParseAnonymousFunctionExpression ( ASTNode parent, Scope daddyScope )
        {
            Scope scope = this.CreateScope ( daddyScope );
            scope.Start ( );

            var tokens = new List<LToken> ( );
            var funcExpr = new AnonymousFunctionExpression ( parent, scope, tokens );

            this.Expect ( "function", tokens );

            // Parse arguments
            funcExpr.SetArguments ( this.ParseFunctionArgs ( funcExpr, scope, tokens ) );
            // Parse function body
            funcExpr.SetBody ( this.ParseStatementList ( funcExpr, scope ) );

            this.Expect ( "end", tokens );

            scope.Finish ( );

            scope.InternalData.SetValue ( "isFunction", true );

            return funcExpr;
        }

        protected ASTNode ParsePrimaryExpression ( ASTNode parent, Scope scope )
        {
            if ( this.NextIs ( "(" ) )
            {
                var tokens = new List<LToken> ( );
                var parenExpr = new ParenthesisExpression ( parent, scope, tokens );

                this.Expect ( "(", tokens );

                ASTNode expr = this.ParseExpression ( parenExpr, scope );
                parenExpr.SetExpression ( expr );

                this.Expect ( ")", tokens );

                return parenExpr;
            }
            else if ( this.NextIs ( "ident" ) )
            {
                return this.ParseVariableExpression ( parent, scope );
            }
            else
            {
                throw new ParseException ( this.GetLocation ( ), $"Primary expression expected but found {this.Peek ( )}" );
            }
        }

        protected MemberExpression ParseMemberExpression ( ASTNode parent, Scope scope, ASTNode baseNode )
        {
            var tokens = new List<LToken> ( );
            var member = new MemberExpression ( parent, scope, tokens );
            member.SetBase ( baseNode );

            if ( this.NextIs ( ":" ) )
            {
                this.Get ( tokens );
                member.SelfRef = true;
            }
            else
            {
                this.Expect ( ".", tokens );
                member.SelfRef = false;
            }

            LToken ident = this.Expect ( "ident", tokens );
            member.SetIndexer ( ( String ) ident.Value );

            return member;
        }

        protected IndexExpression ParseIndexExpression ( ASTNode parent, Scope scope, ASTNode baseNode )
        {
            var tokens = new List<LToken> ( );
            var index = new IndexExpression ( parent, scope, tokens );
            index.SetBase ( baseNode );

            this.Expect ( "[", tokens );

            ASTNode expr = this.ParseExpression ( index, scope );
            index.SetIndexer ( expr );

            this.Expect ( "]", tokens );

            return index;
        }

        protected FunctionCallExpression ParseFunctionCall ( ASTNode parent, Scope scope, ASTNode baseNode )
        {
            var tokens = new List<LToken> ( );
            var call = new FunctionCallExpression ( parent, scope, tokens );
            call.SetBase ( baseNode );

            this.Expect ( "(", tokens );
            while ( !this.Consume ( ")", out LToken _, tokens ) )
            {
                ASTNode argExpr = this.ParseExpression ( call, scope );
                call.AddArgument ( argExpr );

                if ( !this.Consume ( ",", out LToken __, tokens ) )
                {
                    this.Expect ( ")", tokens );
                    break;
                }
            }

            return call;
        }

        protected StringFunctionCallExpression ParseStringFunctionCall ( ASTNode parent, Scope scope, ASTNode baseNode )
        {
            var tokens = new List<LToken> ( );
            var call = new StringFunctionCallExpression ( parent, scope, tokens );
            call.SetBase ( baseNode );

            StringExpression str = this.ParseStringExpression ( call, scope );
            call.SetArgument ( str );

            return call;
        }

        protected TableFunctionCallExpression ParseTableFunctionCall ( ASTNode parent, Scope scope, ASTNode baseNode )
        {
            var tokens = new List<LToken> ( );
            var call = new TableFunctionCallExpression ( parent, scope, tokens );
            call.SetBase ( baseNode );

            TableConstructorExpression tbl = this.ParseTableConstructorExpression ( call, scope );
            call.SetArgument ( tbl );

            return call;
        }

        protected ASTNode ParseChainedMemberExpression ( ASTNode parent, Scope scope )
        {
            ASTNode primaryExpr = this.ParseVariableExpression ( parent, scope );

            while ( true )
            {
                if ( this.NextIs ( "." ) || this.NextIs ( ":" ) )
                    primaryExpr = this.ParseMemberExpression ( parent, scope, primaryExpr );
                else
                    break;
            }

            // primaryExpr can be either a VariableExpression or
            // MemberExpression at this point so we return ASTNode instead
            return primaryExpr;
        }

        protected ASTNode ParseSuffixedExpression ( ASTNode parent, Scope scope )
        {
            ASTNode primary = this.ParsePrimaryExpression ( parent, scope );

            while ( true )
            {
                if ( this.NextIs ( "." ) )
                    primary = this.ParseMemberExpression ( parent, scope, primary );
                else if ( this.NextIs ( ":" ) )
                {
                    MemberExpression member = this.ParseMemberExpression ( parent, scope, primary );
                    ASTNode call;

                    if ( this.NextIs ( TokenType.String ) )
                        call = this.ParseStringFunctionCall ( parent, scope, member );
                    else if ( this.NextIs ( "{" ) )
                        call = this.ParseTableFunctionCall ( parent, scope, member );
                    else
                        call = this.ParseFunctionCall ( parent, scope, member );

                    primary = call;
                }
                else if ( this.NextIs ( "[" ) )
                    primary = this.ParseIndexExpression ( parent, scope, primary );
                else if ( this.NextIs ( "(" ) )
                    primary = this.ParseFunctionCall ( parent, scope, primary );
                else if ( this.NextIs ( TokenType.String ) )
                    primary = this.ParseStringFunctionCall ( parent, scope, primary );
                else if ( this.NextIs ( "{" ) )
                    primary = this.ParseTableFunctionCall ( parent, scope, primary );
                else
                    break;
            }

            return primary;
        }

        protected ASTNode ParseLiteralExpression ( ASTNode parent, Scope scope )
        {
            ;// parses:
            ;//     - all types of function calls
            ;//     - all types of constants (strings, tables, funcs, numbers, etc.)
            ;//     - var expression/literals

            if ( this.NextIs ( TokenType.Number ) )
                return this.ParseNumberExpression ( parent, scope );
            else if ( this.NextIs ( TokenType.String ) )
                return this.ParseStringExpression ( parent, scope );
            else if ( this.NextIs ( "nil" ) )
                return this.ParseNilExpression ( parent, scope );
            else if ( this.NextIs ( "true" ) || this.NextIs ( "false" ) )
                return this.ParseBooleanExpression ( parent, scope );
            else if ( this.NextIs ( "..." ) )
                return this.ParseVarArgExpression ( parent, scope );
            else if ( this.NextIs ( "{" ) )
                return this.ParseTableConstructorExpression ( parent, scope );
            else if ( this.NextIs ( "function" ) )
                return this.ParseAnonymousFunctionExpression ( parent, scope );
            else
                return this.ParseSuffixedExpression ( parent, scope );
        }

        protected ASTNode ParseOperatorExpression ( ASTNode parent, Scope scope, Double lastOperatorPriority )
        {
            ;// parses:
            ;//     - all types of function calls
            ;//     - all types of constants (string, tables, funcs, numbers, etc.)
            ;//     - var expressions/literals
            ;//     - all type of unary expressions with priority
            ;//     - all type of binary expressions with priority

            ASTNode expr;

            if ( this.NextIs ( TokenType.Operator ) && ParserData.UnaryOps.Contains ( this.Peek ( ).Raw ) )
            {
                // parse an unary op first
                var tokens = new List<LToken> ( );
                LToken opTok = this.Get ( tokens );

                var unOpExpr = new UnaryOperatorExpression ( parent, scope, tokens ) { Operator = opTok.Raw };
                unOpExpr.SetOperand ( this.ParseOperatorExpression ( parent, scope, ParserData.UnaryOpPriority ) );

                expr = unOpExpr;
            }
            else
            {
                // otherwise we're dealing with a literal expr
                // this may by the LHS to a binop expr

                expr = this.ParseLiteralExpression ( parent, scope );
            }

            while ( true )
            {
                if ( ParserData.OpPriorities.ContainsKey ( this.Peek ( ).ID ) )
                {
                    var priority = ParserData.OpPriorities[this.Peek ( ).ID];

                    if ( priority[0] > lastOperatorPriority )
                    {
                        var tokens = new List<LToken> ( );
                        LToken opTok = this.Get ( tokens );
                        ASTNode rhs = this.ParseOperatorExpression ( parent, scope, priority[1] );
                        var binOp = new BinaryOperatorExpression ( parent, scope, tokens ) { Operator = opTok.ID };
                        binOp.SetLeftOperand ( expr );
                        binOp.SetRightOperand ( rhs );

                        expr = binOp;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        protected ASTNode ParseExpression ( ASTNode parent, Scope scope )
            => this.ParseOperatorExpression ( parent, scope, 0 );

        protected IfStatement ParseIfStatement ( ASTNode parent, Scope daddyScope )
        {
            Scope scope = this.CreateScope ( daddyScope );
            var tokens = new List<LToken> ( );
            var ifStat = new IfStatement ( parent, scope, tokens );
            var first = true; // haha, first

            scope.Start ( );
            {
                while ( true )
                {
                    if ( first ) // tell him to fuck off, because it doesnt matters
                        this.Expect ( "if", tokens );
                    else if ( !this.Consume ( "elseif", out LToken _, tokens ) )
                        break;

                    var clauseTokens = new List<LToken> ( );
                    var clause = new IfClause ( ifStat, scope, clauseTokens );
                    clause.SetCondition ( this.ParseExpression ( clause, scope ) );
                    this.Expect ( "then", clauseTokens );
                    // each statement list creates a new scope for
                    // itself, so we're all good
                    clause.SetBody ( this.ParseStatementList ( clause, scope ) );

                    if ( first ) // didn't we already tell him to fuck off?
                    {
                        first = false;
                        ifStat.SetMainClause ( clause );
                    }
                    else
                        ifStat.AddElseIfClause ( clause );
                }

                if ( this.Consume ( "else", out LToken _, tokens ) )
                {
                    // each statement list creates a new scope for
                    // itself, so we're all good
                    ifStat.SetElseBlock ( this.ParseStatementList ( ifStat, scope ) );
                }

                this.Expect ( "end", tokens );
            }
            scope.Finish ( );

            return ifStat;
        }

        protected WhileStatement ParseWhileStatement ( ASTNode parent, Scope daddyScope )
        {
            Scope scope = this.CreateScope ( daddyScope );
            var tokens = new List<LToken> ( );
            var whileStat = new WhileStatement ( parent, scope, tokens );
            scope.Start ( );
            {
                this.Expect ( "while", tokens );
                whileStat.SetCondition ( this.ParseExpression ( whileStat, scope ) );
                this.Expect ( "do", tokens );
                whileStat.SetBody ( this.ParseStatementList ( whileStat, scope ) );
                this.Expect ( "end", tokens );
            }
            scope.Finish ( );
            return whileStat;
        }

        protected DoStatement ParseDoStatement ( ASTNode parent, Scope daddyScope )
        {
            Scope scope = this.CreateScope ( daddyScope );
            var tokens = new List<LToken> ( );
            var doStat = new DoStatement ( parent, scope, tokens );
            scope.Start ( );
            {
                this.Expect ( "do", tokens );
                doStat.SetBody ( this.ParseStatementList ( doStat, scope ) );
                this.Expect ( "end", tokens );
            }
            scope.Finish ( );
            return doStat;
        }

        protected ForNumericStatement ParseForNumericStatement ( ASTNode parent, Scope daddyScope )
        {
            Scope scope = this.CreateScope ( daddyScope );
            var tokens = new List<LToken> ( );
            var forStat = new ForNumericStatement ( parent, scope, tokens );

            scope.Start ( );
            {
                this.Expect ( "for", tokens );
                forStat.SetVariable ( this.ParseVariableExpression ( forStat, scope ) );
                this.Expect ( "=", tokens );
                forStat.SetInitialExpression ( this.ParseExpression ( forStat, scope ) );
                this.Expect ( ",", tokens );
                forStat.SetFinalExpression ( this.ParseExpression ( forStat, scope ) );
                // increment
                if ( this.Consume ( ",", out LToken _, tokens ) )
                    forStat.SetIncrementExpression ( this.ParseExpression ( forStat, scope ) );
                this.Expect ( "do", tokens );
                forStat.SetBody ( this.ParseStatementList ( forStat, scope ) );
                this.Expect ( "end", tokens );
            }
            scope.Finish ( );

            return forStat;
        }

        protected ForGenericStatement ParseForGenericStatement ( ASTNode parent, Scope daddyScope )
        {
            Scope scope = this.CreateScope ( daddyScope );
            var tokens = new List<LToken> ( );
            var forStat = new ForGenericStatement ( parent, scope, tokens );
            scope.Start ( );
            {
                this.Expect ( "for", tokens );
                do
                {
                    forStat.AddVariable ( this.ParseLocalVariableExpression ( forStat, scope ) );
                }
                while ( this.Consume ( ",", out LToken _, tokens ) );

                this.Expect ( "in", tokens );

                do
                {
                    forStat.AddGenerator ( this.ParseExpression ( forStat, scope ) );
                }
                while ( this.Consume ( ",", out LToken _, tokens ) );

                this.Expect ( "do", tokens );
                forStat.SetBody ( this.ParseStatementList ( forStat, scope ) );
                this.Expect ( "end", tokens );
            }
            scope.Finish ( );
            return forStat;
        }

        protected ASTNode ParseForStatement ( ASTNode parent, Scope scope )
        {
            if ( this.Peek ( 1 ).ID == "ident" && this.Peek ( 2 ).ID == "=" )
                return this.ParseForNumericStatement ( parent, scope );
            else
                return this.ParseForGenericStatement ( parent, scope );
        }

        protected RepeatStatement ParseRepeatStatement ( ASTNode parent, Scope daddyScope )
        {
            Scope scope = this.CreateScope ( daddyScope );
            var tokens = new List<LToken> ( );
            var repStat = new RepeatStatement ( parent, scope, tokens );

            scope.Start ( );
            {
                this.Expect ( "repeat", tokens );
                repStat.SetBody ( this.ParseStatementList ( repStat, scope ) );
                this.Expect ( "until", tokens );
                repStat.SetCondition ( this.ParseExpression ( repStat, scope ) );
                this.Expect ( "end", tokens );
            }
            scope.Finish ( );
            return repStat;
        }

        protected NamedFunctionStatement ParseNamedFunctionStatement ( ASTNode parent, Scope daddyScope )
        {
            Scope scope = this.CreateScope ( daddyScope );
            var tokens = new List<LToken> ( );
            var funExpr = new NamedFunctionStatement ( parent, scope, tokens );

            scope.Start ( );
            {
                this.Expect ( "function", tokens );
                funExpr.SetIdentifier ( this.ParseChainedMemberExpression ( funExpr, scope ) );
                funExpr.SetArguments ( this.ParseFunctionArgs ( funExpr, scope, tokens ) );
                funExpr.SetBody ( this.ParseStatementList ( funExpr, scope ) );
                this.Expect ( "end", tokens );
            }
            scope.Finish ( );

            scope.InternalData.SetValue ( "isFunction", true );
            return funExpr;
        }

        protected LocalFunctionStatement ParseLocalFunctionStatement ( ASTNode parent, Scope daddyScope )
        {
            Scope scope = this.CreateScope ( daddyScope );
            var tokens = new List<LToken> ( );
            var funcExpr = new LocalFunctionStatement ( parent, scope, tokens );

            this.Expect ( "local", tokens );
            this.Expect ( "function", tokens );
            funcExpr.SetIdentifier ( this.ParseLocalVariableExpression ( funcExpr, daddyScope ) );

            scope.Start ( );
            {
                funcExpr.SetArguments ( this.ParseFunctionArgs ( funcExpr, scope, tokens ) );
                funcExpr.SetBody ( this.ParseStatementList ( funcExpr, scope ) );
                this.Expect ( "end", tokens );
            }
            scope.Finish ( );

            scope.InternalData.SetValue ( "isFunction", true );
            return funcExpr;
        }

        protected LocalVariableStatement ParseLocalVariableStatement ( ASTNode parent, Scope scope )
        {
            var tokens = new List<LToken> ( );
            var varStat = new LocalVariableStatement ( parent, scope, tokens );

            this.Expect ( "local", tokens );
            // Get names
            do varStat.AddVariable ( this.ParseLocalVariableExpression ( varStat, scope ) );
            while ( this.Consume ( ",", out LToken _, tokens ) );

            // Check if anything is being assigned
            if ( this.Consume ( "=", out LToken _, tokens ) )
            {
                // Get valued being assigned
                do varStat.AddAssignment ( this.ParseExpression ( varStat, scope ) );
                while ( this.Consume ( ",", out LToken _, tokens ) );
            }

            return varStat;
        }

        protected ReturnStatement ParseReturnStatement ( ASTNode parent, Scope scope )
        {
            var tokens = new List<LToken> ( );
            var retStat = new ReturnStatement ( parent, scope, tokens );

            this.Expect ( "return", tokens );
            if ( !ParserData.StatListCloseKeywords.Contains ( this.Peek ( ).ID ) && !this.NextIs ( TokenType.EOF ) )
            {
                do retStat.AddReturn ( this.ParseExpression ( retStat, scope ) );
                while ( this.Consume ( ",", out LToken _, tokens ) );
            }

            return retStat;
        }

        protected BreakStatement ParseBreakStatement ( ASTNode parent, Scope scope )
        {
            var tokens = new List<LToken> ( 1 );
            var breakStat = new BreakStatement ( parent, scope, tokens );
            this.Expect ( "break", tokens );
            return breakStat;
        }

        protected ContinueStatement ParseContinueStatement ( ASTNode parent, Scope scope )
        {
            var tokens = new List<LToken> ( 1 );
            var contStat = new ContinueStatement ( parent, scope, tokens );
            this.Expect ( "continue", tokens );
            return contStat;
        }

        protected GotoLabelStatement ParseGotoLabelStatement ( ASTNode parent, Scope scope )
        {
            var tokens = new List<LToken> ( 3 );
            var labelStat = new GotoLabelStatement ( parent, scope, tokens );

            this.Expect ( "::", tokens );
            LToken ident = this.Expect ( "ident", tokens );
            this.Expect ( "::", tokens );

            labelStat.Label = scope.GetLabel ( ( String ) ident.Value );
            labelStat.Label.Node = labelStat;
            return labelStat;
        }

        protected GotoStatement ParseGotoStatement ( ASTNode parent, Scope scope )
        {
            var tokens = new List<LToken> ( 2 );
            var gotoStat = new GotoStatement ( parent, scope, tokens );

            this.Expect ( "goto", tokens );
            LToken ident = this.Expect ( "ident", tokens );

            gotoStat.Label = scope.GetLabel ( ( String ) ident.Value );
            return gotoStat;
        }

        protected AssignmentStatement ParseAssignmentStatement ( ASTNode parent, Scope scope, ASTNode baseNode )
        {
            var tokens = new List<LToken> ( );
            var assignStat = new AssignmentStatement ( parent, scope, tokens );

            if ( baseNode != null )
            {
                assignStat.AddVariable ( baseNode );

                if ( this.Consume ( ",", out LToken _, tokens ) )
                {
                    do
                    {
                        ASTNode suffixedExp = this.ParseSuffixedExpression ( assignStat, scope );
                        if ( !( suffixedExp is VariableExpression || suffixedExp is MemberExpression || suffixedExp is IndexExpression ) )
                            throw new ParseException ( this.GetLocation ( ), $"unexpected suffixed expression in assignment statement: {suffixedExp.GetType ( ).Name}" );

                        assignStat.AddVariable ( suffixedExp );
                    }
                    while ( this.Consume ( ",", out LToken _, tokens ) );
                }
            }
            else
            {
                do
                {
                    ASTNode suffixedExp = this.ParseSuffixedExpression ( assignStat, scope );
                    if ( !( suffixedExp is VariableExpression || suffixedExp is MemberExpression || suffixedExp is IndexExpression ) )
                        throw new ParseException ( this.GetLocation ( ), $"unexpected suffixed expression in assignment statement: {suffixedExp.GetType ( ).Name}" );

                    assignStat.AddVariable ( suffixedExp );
                }
                while ( this.Consume ( ",", out LToken _, tokens ) );
            }

            this.Expect ( "=", tokens );

            do assignStat.AddAssignment ( this.ParseExpression ( assignStat, scope ) );
            while ( this.Consume ( ",", out LToken _, tokens ) );

            return assignStat;
        }

        protected ASTNode ParseStatementInternal ( ASTNode parent, Scope scope )
        {
            if ( this.NextIs ( "if" ) )
                return this.ParseIfStatement ( parent, scope );
            else if ( this.NextIs ( "while" ) )
                return this.ParseWhileStatement ( parent, scope );
            else if ( this.NextIs ( "do" ) )
                return this.ParseDoStatement ( parent, scope );
            else if ( this.NextIs ( "for" ) )
                return this.ParseForStatement ( parent, scope );
            else if ( this.NextIs ( "repeat" ) )
                return this.ParseRepeatStatement ( parent, scope );
            else if ( this.NextIs ( "function" ) )
                return this.ParseNamedFunctionStatement ( parent, scope );
            else if ( this.NextIs ( "local" ) )
            {
                if ( this.Peek ( 1 ).ID == "function" )
                    return this.ParseLocalFunctionStatement ( parent, scope );
                else
                    return this.ParseLocalVariableStatement ( parent, scope );
            }
            else if ( this.NextIs ( "return" ) )
                return this.ParseReturnStatement ( parent, scope );
            else if ( this.NextIs ( "break" ) )
                return this.ParseBreakStatement ( parent, scope );
            else if ( this.NextIs ( "continue" ) )
                return this.ParseContinueStatement ( parent, scope );
            else if ( this.NextIs ( "::" ) )
                return this.ParseGotoLabelStatement ( parent, scope );
            else if ( this.NextIs ( "goto" ) )
                return this.ParseGotoStatement ( parent, scope );
            else
            {
                ASTNode ident = this.ParseSuffixedExpression ( parent, scope );

                if ( ( this.NextIs ( "=" ) || this.NextIs ( "," ) )
                    && ( ident is VariableExpression || ident is MemberExpression || ident is IndexExpression ) )
                    return this.ParseAssignmentStatement ( parent, scope, ident );
                else
                    return ident;
            }
        }

        protected ASTNode ParseStatement ( ASTNode parent, Scope scope )
        {
            ASTNode node = this.ParseStatementInternal ( parent, scope );
            if ( node is ASTStatement stat )
                stat.HasSemicolon = this.Consume ( ";", out LToken _ );
            return node;
        }

        protected StatementList ParseStatementList ( ASTNode parent, Scope daddyScope )
        {
            Scope scope = this.CreateScope ( daddyScope );
            var tokens = new List<LToken> ( );
            var statList = new StatementList ( parent, scope, tokens );

            scope.Start ( );
            {
                while ( !ParserData.StatListCloseKeywords.Contains ( this.Peek ( ).ID ) && !this.NextIs ( TokenType.EOF ) )
                    statList.AddStatement ( this.ParseStatement ( statList, scope ) );

                if ( this.NextIs ( TokenType.EOF ) )
                {
                    var eofTokens = new List<LToken> ( 1 );
                    var eof = new Eof ( statList, scope, eofTokens );
                    this.Get ( eofTokens );
                    statList.AddStatement ( eof );
                }
            }
            scope.Finish ( );

            return statList;
        }

        #endregion Actual Parsing

        public StatementList Parse ( )
        {
            try
            {
                this.RootScope.Start ( );
                this.RootScope.InternalData.SetValue ( "isFunction", true );
                this.RootScope.InternalData.SetValue ( "isRoot", true );
                StatementList list = this.ParseStatementList ( null, this.RootScope );
                this.RootScope.Finish ( );
                return list;
            }
            catch ( ParseException e )
            {
                this.Environment
                    .GetFile ( this )
                    .Errors
                    .Add ( new Error ( ErrorType.Fatal, e.Location, e.Message ) );
                throw;
            }
        }
    }
}
