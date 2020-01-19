using System;
using System.Collections.Immutable;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.AST.Tables;

namespace Loretta.Parsing.Visitor
{
    public class TreeFolderBase : ITreeVisitor<LuaASTNode>
    {
        public virtual LuaASTNode VisitNil ( NilExpression node ) => node;

        public virtual LuaASTNode VisitBoolean ( BooleanExpression node ) => node;

        public virtual LuaASTNode VisitIdentifier ( IdentifierExpression node ) => node;

        public virtual LuaASTNode VisitNumber ( NumberExpression node ) => node;

        public virtual LuaASTNode VisitString ( StringExpression node ) => node;

        public virtual LuaASTNode VisitVarArg ( VarArgExpression node ) => node;

        public virtual LuaASTNode VisitIndex ( IndexExpression node )
        {
            var indexee = this.VisitNode ( node.Indexee ) as Expression;
            var indexer = this.VisitNode ( node.Indexer ) as Expression;
            if ( indexee != node.Indexee || indexer != node.Indexer )
            {
                if ( node.Type == IndexType.Indexer )
                {
                    return new IndexExpression ( indexee, node.Tokens.First ( ), indexer, node.Tokens.Last ( ) );
                }
                else
                {
                    if ( !( indexer is IdentifierExpression identifier ) )
                    {
                        throw new InvalidOperationException ( "Cannot fold an indexer of a member/method index expression to something that is not an identifier." );
                    }

                    return new IndexExpression ( indexee, node.Tokens.Single ( ), identifier );
                }
            }

            return node;
        }

        public virtual LuaASTNode VisitFunctionCall ( FunctionCallExpression node )
        {
            var function = this.VisitNode ( node.Function ) as Expression;
            var arguments = node.Arguments.Select ( this.VisitNode ).Select ( n => n as Expression ).ToImmutableArray ( );

            if ( !node.HasParenthesis )
            {
                if ( arguments.Length != 1 )
                {
                    throw new ArgumentException ( nameof ( node ), "Function call expression has no parenthesis and more than 1 argument" );
                }

                if ( !( arguments[0] is StringExpression ) || !( arguments[0] is TableConstructorExpression ) )
                {
                    throw new InvalidOperationException ( "Can't fold a function call expression that has no parenthesis" );
                }

                return new FunctionCallExpression ( function, arguments[0] );
            }

            if ( function != node.Function || !arguments.SequenceEqual ( node.Arguments ) )
            {
                return new FunctionCallExpression ( function,
                                                    node.Tokens.First ( ),
                                                    arguments,
                                                    node.Tokens.Skip ( 1 ).SkipLast ( 1 ),
                                                    node.Tokens.Last ( ) );
            }

            return node;
        }

        public virtual LuaASTNode VisitUnaryOperation ( UnaryOperationExpression node )
        {
            Token<LuaTokenType> op = node.Operator;
            var operand = this.VisitNode ( node.Operand ) as Expression;

            if ( operand != node.Operand )
            {
                return new UnaryOperationExpression ( node.Fix, op, operand );
            }
            else
            {
                return node;
            }
        }

        public virtual LuaASTNode VisitGroupedExpression ( GroupedExpression node )
        {
            var inner = this.VisitNode ( node.InnerExpression ) as Expression;
            if ( node.InnerExpression != inner )
            {
                return new GroupedExpression ( node.Tokens.First ( ), inner, node.Tokens.Last ( ) );
            }
            else
            {
                return node;
            }
        }

        public virtual LuaASTNode VisitBinaryOperation ( BinaryOperationExpression node )
        {
            var left = this.VisitNode ( node.Left ) as Expression;
            var right = this.VisitNode ( node.Right ) as Expression;
            if ( left != node.Left || right != node.Right )
            {
                return new BinaryOperationExpression ( left, node.Operator, right );
            }
            else
            {
                return node;
            }
        }

        public virtual LuaASTNode VisitTableField ( TableField node )
        {
            Expression key = node.KeyType switch
            {
                TableFieldKeyType.Expression => this.VisitNode ( node.Key ) as Expression,
                _ => node.Key
            };
            var value = this.VisitNode ( node.Value ) as Expression;

            if ( key != node.Key || value != node.Value )
            {
                Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                return node.KeyType switch
                {
                    TableFieldKeyType.Expression => new TableField ( tokens[0], key, tokens[1], tokens[2], value, tokens[3] ),
                    TableFieldKeyType.Identifier => new TableField ( tokens[0], tokens[1], value, tokens[2] ),
                    TableFieldKeyType.None => new TableField ( value, tokens[0] ),
                    _ => throw new ArgumentException ( nameof ( node ), "Invalid key type." )
                };
            }

            return node;
        }

        public virtual LuaASTNode VisitTableConstructor ( TableConstructorExpression node )
        {
            TableField[] fields = node.Fields.Select ( f => this.VisitTableField ( f ) as TableField ).ToArray ( );
            if ( !node.Fields.SequenceEqual ( fields ) )
            {
                Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                return new TableConstructorExpression ( tokens[0], fields, tokens[1] );
            }

            return node;
        }

        public virtual LuaASTNode VisitAnonymousFunction ( AnonymousFunctionExpression node )
        {
            var body = this.VisitStatementList ( node.Body ) as StatementList;
            if ( node.Body != body )
            {
                return new AnonymousFunctionExpression ( node.Tokens, node.Arguments, body );
            }

            return node;
        }

        public virtual LuaASTNode VisitAssignment ( AssignmentStatement node )
        {
            var variables = node.Variables.Select ( var => this.VisitNode ( var ) as Expression ).ToImmutableArray ( );
            var values = node.Values.Select ( val => this.VisitNode ( val ) as Expression ).ToImmutableArray ( );
            if ( !variables.SequenceEqual ( node.Variables ) || !values.SequenceEqual ( node.Values ) )
            {
                Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                var variablesTokens = new ArraySegment<Token<LuaTokenType>> ( tokens, 0, variables.Length - 1 );
                Token<LuaTokenType> equals = tokens[variablesTokens.Count];
                var valueTokens = new ArraySegment<Token<LuaTokenType>> ( tokens, variablesTokens.Count + 1, values.Length - 1 );

                return new AssignmentStatement ( variables, variablesTokens, equals, values, valueTokens ) { Semicolon = node.Semicolon };
            }

            return node;
        }

        public virtual LuaASTNode VisitBreak ( BreakStatement node ) => node;

        public virtual LuaASTNode VisitContinue ( ContinueStatement node ) => node;

        public virtual LuaASTNode VisitDo ( DoStatement node )
        {
            var body = this.VisitStatementList ( node.Body ) as StatementList;
            if ( body != node.Body )
            {
                Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                return new DoStatement ( tokens[0], body, tokens[1] ) { Semicolon = node.Semicolon };
            }

            return node;
        }

        public virtual LuaASTNode VisitExpressionStatement ( ExpressionStatement node )
        {
            var inner = this.VisitNode ( node.Expression ) as Expression;
            if ( node.Expression != inner )
            {
                return new ExpressionStatement ( inner ) { Semicolon = node.Semicolon };
            }

            return node;
        }

        public virtual LuaASTNode VisitFunctionDefinition ( FunctionDefinitionStatement node )
        {
            Expression name = node.IsLocal ? node.Name : this.VisitNode ( node.Name ) as Expression;
            var body = this.VisitStatementList ( node.Body ) as StatementList;

            if ( name != node.Name || body != node.Body )
            {
                Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                if ( node.IsLocal )
                {
                    Token<LuaTokenType> localKw = tokens[0];
                    Token<LuaTokenType> functionKw = tokens[1];
                    Token<LuaTokenType> lparen = tokens[2];
                    var commas = new ArraySegment<Token<LuaTokenType>> ( tokens, 3, Math.Max ( 0, node.Arguments.Length - 1 ) );
                    Token<LuaTokenType> rparen = tokens[3 + commas.Count];
                    Token<LuaTokenType> endKw = tokens.Last ( );

                    return new FunctionDefinitionStatement ( localKw, functionKw, name, lparen, node.Arguments, commas, rparen, body, endKw ) { Semicolon = node.Semicolon };
                }
                else
                {
                    Token<LuaTokenType> functionKw = tokens[0];
                    Token<LuaTokenType> lparen = tokens[1];
                    var commas = new ArraySegment<Token<LuaTokenType>> ( tokens, 2, Math.Max ( 0, node.Arguments.Length - 1 ) );
                    Token<LuaTokenType> rparen = tokens[2 + commas.Count];
                    Token<LuaTokenType> endKw = tokens.Last ( );

                    return new FunctionDefinitionStatement ( functionKw, name, lparen, node.Arguments, commas, rparen, body, endKw ) { Semicolon = node.Semicolon };
                }
            }

            return node;
        }

        public virtual LuaASTNode VisitGotoLabel ( GotoLabelStatement node ) => node;

        public virtual LuaASTNode VisitGoto ( GotoStatement node ) => node;

        public virtual LuaASTNode VisitIfStatement ( IfStatement node )
        {
            IfClause[] clauses = node.Clauses.Select ( clause =>
            {
                var condition = this.VisitNode ( clause.Condition ) as Expression;
                var body = this.VisitStatementList ( clause.Body ) as StatementList;

                if ( condition != clause.Condition || body != clause.Body )
                {
                    Token<LuaTokenType>[] tokens = clause.Tokens.ToArray ( );
                    return new IfClause ( tokens[0], condition, tokens[1], body );
                }

                return clause;
            } ).ToArray ( );

            if ( node.ElseBlock is StatementList elseBlock )
            {
                elseBlock = this.VisitStatementList ( elseBlock ) as StatementList;

                if ( elseBlock != node.ElseBlock || !clauses.SequenceEqual ( node.Clauses ) )
                {
                    return new IfStatement ( clauses, node.Tokens.First ( ), elseBlock, node.Tokens.Last ( ) ) { Semicolon = node.Semicolon };
                }
            }
            else if ( !clauses.SequenceEqual ( node.Clauses ) )
            {
                return new IfStatement ( clauses, node.Tokens.First ( ) ) { Semicolon = node.Semicolon };
            }

            return node;
        }

        public virtual LuaASTNode VisitGenericFor ( GenericForLoopStatement node )
        {
            var iteratable = this.VisitNode ( node.Iteratable ) as Expression;
            var body = this.VisitStatementList ( node.Body ) as StatementList;

            if ( iteratable != node.Iteratable || body != node.Body )
            {
                Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                Token<LuaTokenType> forKw = tokens[0];
                var commas = new ArraySegment<Token<LuaTokenType>> ( tokens, 1, node.Variables.Length - 1 );
                Token<LuaTokenType> inKw = tokens[1 + commas.Count];
                Token<LuaTokenType> doKw = tokens[2 + commas.Count];
                Token<LuaTokenType> endKw = tokens[3 + commas.Count];
                return new GenericForLoopStatement ( node.Scope, forKw, node.Variables, commas, inKw, iteratable, doKw, body, endKw ) { Semicolon = node.Semicolon };
            }

            return node;
        }

        public virtual LuaASTNode VisitLocalVariableDeclaration ( LocalVariableDeclarationStatement node )
        {
            if ( node.Values.Length > 0 )
            {
                Expression[] values = node.Values.Select ( val => this.VisitNode ( val ) as Expression ).ToArray ( );

                if ( !values.SequenceEqual ( node.Values ) )
                {
                    Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                    Token<LuaTokenType> localKw = tokens[0];
                    var identTokens = new ArraySegment<Token<LuaTokenType>> ( tokens, 1, node.Identifiers.Length - 1 );
                    Token<LuaTokenType> equals = tokens[1 + identTokens.Count];
                    var valTokens = new ArraySegment<Token<LuaTokenType>> ( tokens, 2 + identTokens.Count, values.Length - 1 );

                    return new LocalVariableDeclarationStatement ( localKw, node.Identifiers, identTokens, equals, values, valTokens ) { Semicolon = node.Semicolon };
                }
            }

            return node;
        }

        public virtual LuaASTNode VisitNode ( LuaASTNode node ) => node.Accept ( this );

        public virtual LuaASTNode VisitNumericFor ( NumericForLoopStatement node )
        {
            var body = this.VisitStatementList ( node.Body ) as StatementList;
            var initial = this.VisitNode ( node.Initial ) as Expression;
            var final = this.VisitNode ( node.Final ) as Expression;

            if ( node.Step != null )
            {
                var step = this.VisitNode ( node.Step ) as Expression;
                if ( body != node.Body || initial != node.Initial || final != node.Final || step != node.Step )
                {
                    Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                    Token<LuaTokenType> forKw = tokens[0];
                    Token<LuaTokenType> equals = tokens[1];
                    Token<LuaTokenType> finalComma = tokens[2];
                    Token<LuaTokenType> stepComma = tokens[3];
                    Token<LuaTokenType> doKw = tokens[4];
                    Token<LuaTokenType> endKw = tokens[5];
                    return new NumericForLoopStatement ( node.Scope, forKw, node.Variable, equals, initial, finalComma, final, stepComma, step, doKw, body, endKw ) { Semicolon = node.Semicolon };
                }
            }
            else if ( body != node.Body || initial != node.Initial || final != node.Final )
            {
                Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                Token<LuaTokenType> forKw = tokens[0];
                Token<LuaTokenType> equals = tokens[1];
                Token<LuaTokenType> finalComma = tokens[2];
                Token<LuaTokenType> doKw = tokens[3];
                Token<LuaTokenType> endKw = tokens[4];
                return new NumericForLoopStatement ( node.Scope, forKw, node.Variable, equals, initial, finalComma, final, doKw, body, endKw ) { Semicolon = node.Semicolon };
            }

            return node;
        }

        public virtual LuaASTNode VisitRepeatUntil ( RepeatUntilStatement node )
        {
            var body = this.VisitStatementList ( node.Body ) as StatementList;
            var cond = this.VisitNode ( node.Condition ) as Expression;

            if ( body != node.Body || cond != node.Condition )
            {
                Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                Token<LuaTokenType> repeatKw = tokens[0];
                Token<LuaTokenType> untilKw = tokens[1];

                return new RepeatUntilStatement ( node.Scope, repeatKw, body, untilKw, cond ) { Semicolon = node.Semicolon };
            }

            return node;
        }

        public virtual LuaASTNode VisitReturn ( ReturnStatement node )
        {
            Expression[] values = node.Values.Select ( val => this.VisitNode ( val ) as Expression ).ToArray ( );

            if ( !values.SequenceEqual ( node.Values ) )
            {
                Token<LuaTokenType> returnKw = node.Tokens.First ( );
                Token<LuaTokenType>[] commas = node.Tokens.Skip ( 1 ).ToArray ( );

                return new ReturnStatement ( returnKw, values, commas ) { Semicolon = node.Semicolon };
            }

            return node;
        }

        public virtual LuaASTNode VisitStatementList ( StatementList node )
        {
            Statement[] body = node.Body.Select ( stmt => this.VisitNode ( stmt ) as Statement ).ToArray ( );

            if ( !body.SequenceEqual ( node.Body ) )
            {
                return new StatementList ( node.Scope, body ) { Semicolon = node.Semicolon };
            }

            return node;
        }

        public virtual LuaASTNode VisitWhileLoop ( WhileLoopStatement node )
        {
            var cond = this.VisitNode ( node.Condition ) as Expression;
            var body = this.VisitStatementList ( node.Body ) as StatementList;

            if ( cond != node.Condition || body != node.Body )
            {
                Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                Token<LuaTokenType> whileKw = tokens[0];
                Token<LuaTokenType> doKw = tokens[1];
                Token<LuaTokenType> endKw = tokens[2];

                return new WhileLoopStatement ( whileKw, cond, doKw, body, endKw );
            }

            return node;
        }
    }
}