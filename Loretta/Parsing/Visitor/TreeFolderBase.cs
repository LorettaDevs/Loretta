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

        public virtual LuaASTNode VisitEmptyStatement ( EmptyStatement node ) => node;

        public virtual LuaASTNode VisitIndex ( IndexExpression node )
        {
            var indexee = ( Expression ) this.VisitNode ( node.Indexee );
            var indexer = ( Expression ) this.VisitNode ( node.Indexer );
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
            var function = ( Expression ) this.VisitNode ( node.Function );
            var arguments = node.Arguments.Select ( this.VisitNode ).Select ( n => ( Expression ) n ).ToImmutableArray ( );

            if ( !node.HasParenthesis )
            {
                if ( arguments.Length != 1 )
                {
                    throw new ArgumentException ( nameof ( node ), "Function call expression has no parenthesis and more than 1 argument" );
                }

                if ( !( arguments[0] is StringExpression ) && !( arguments[0] is TableConstructorExpression ) )
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
            var operand = ( Expression ) this.VisitNode ( node.Operand );

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
            var inner = ( Expression ) this.VisitNode ( node.InnerExpression );
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
            var left = ( Expression ) this.VisitNode ( node.Left );
            var right = ( Expression ) this.VisitNode ( node.Right );
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
            Expression? key = node.KeyType switch
            {
                TableFieldKeyType.Expression => ( Expression ) this.VisitNode ( node.Key! ),
                _ => node.Key
            };
            var value = ( Expression ) this.VisitNode ( node.Value );

            if ( key != node.Key || value != node.Value )
            {
                Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                return node.KeyType switch
                {
                    TableFieldKeyType.Expression => new TableField ( tokens[0], key!, tokens[1], tokens[2], value, tokens.Length > 3 ? tokens[3] : default ),
                    TableFieldKeyType.Identifier => new TableField ( tokens[0], tokens[1], value, tokens.Length > 2 ? tokens[2] : default ),
                    TableFieldKeyType.None => new TableField ( value, tokens.Length > 0 ? tokens[0] : default ),
                    _ => throw new ArgumentException ( nameof ( node ), "Invalid key type." )
                };
            }

            return node;
        }

        public virtual LuaASTNode VisitTableConstructor ( TableConstructorExpression node )
        {
            TableField[] fields = node.Fields.Select ( f => ( TableField ) this.VisitNode ( f ) ).ToArray ( );
            if ( !node.Fields.SequenceEqual ( fields ) )
            {
                Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                return new TableConstructorExpression ( tokens[0], fields, tokens[1] );
            }

            return node;
        }

        public virtual LuaASTNode VisitAnonymousFunction ( AnonymousFunctionExpression node )
        {
            var body = ( StatementList ) this.VisitNode ( node.Body );
            if ( node.Body != body )
            {
                return new AnonymousFunctionExpression ( node.Tokens, node.Arguments, body );
            }

            return node;
        }

        public virtual LuaASTNode VisitAssignment ( AssignmentStatement node )
        {
            var variables = node.Variables.Select ( var => ( Expression ) this.VisitNode ( var ) ).ToImmutableArray ( );
            var values = node.Values.Select ( val => ( Expression ) this.VisitNode ( val ) ).ToImmutableArray ( );
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

        public virtual LuaASTNode VisitCompoundAssignmentStatement ( CompoundAssignmentStatement node )
        {
            var assignee = ( Expression ) this.VisitNode ( node.Assignee );
            var value = ( Expression ) this.VisitNode ( node.ValueExpression );
            if ( node.Assignee != assignee || node.ValueExpression != value )
            {
                return new CompoundAssignmentStatement ( assignee, node.OperatorToken, value );
            }

            return node;
        }

        public virtual LuaASTNode VisitBreak ( BreakStatement node ) => node;

        public virtual LuaASTNode VisitContinue ( ContinueStatement node ) => node;

        public virtual LuaASTNode VisitDo ( DoStatement node )
        {
            var body = ( StatementList ) this.VisitNode ( node.Body );
            if ( body != node.Body )
            {
                Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                return new DoStatement ( tokens[0], body, tokens[1] ) { Semicolon = node.Semicolon };
            }

            return node;
        }

        public virtual LuaASTNode VisitExpressionStatement ( ExpressionStatement node )
        {
            var inner = ( Expression ) this.VisitNode ( node.Expression );
            if ( node.Expression != inner )
            {
                return new ExpressionStatement ( inner ) { Semicolon = node.Semicolon };
            }

            return node;
        }

        public virtual LuaASTNode VisitFunctionDefinition ( FunctionDefinitionStatement node )
        {
            Expression name = node.IsLocal ? node.Name : ( Expression ) this.VisitNode ( node.Name );
            var body = ( StatementList ) this.VisitNode ( node.Body );

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
                var condition = ( Expression ) this.VisitNode ( clause.Condition );
                var body = ( StatementList ) this.VisitNode ( clause.Body );

                if ( condition != clause.Condition || body != clause.Body )
                {
                    Token<LuaTokenType>[] tokens = clause.Tokens.ToArray ( );
                    return new IfClause ( tokens[0], condition, tokens[1], body );
                }

                return clause;
            } ).ToArray ( );

            if ( node.ElseBlock is StatementList elseBlock )
            {
                elseBlock = ( StatementList ) this.VisitNode ( elseBlock );

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
            Expression[] iteratables = node.Expressions.Select ( iteratable => ( Expression ) this.VisitNode ( iteratable ) ).ToArray ( );
            var body = ( StatementList ) this.VisitNode ( node.Body );

            if ( !node.Expressions.SequenceEqual ( iteratables ) || body != node.Body )
            {
                Token<LuaTokenType>[] tokens = node.Tokens.ToArray ( );
                Token<LuaTokenType> forKw = tokens[0];
                var varCommas = new ArraySegment<Token<LuaTokenType>> ( tokens, 1, node.Variables.Length - 1 );
                Token<LuaTokenType> inKw = tokens[1 + varCommas.Count];
                var iteratableCommas = new ArraySegment<Token<LuaTokenType>> ( tokens, 2 + varCommas.Count, node.Expressions.Length - 1 );
                Token<LuaTokenType> doKw = tokens[2 + varCommas.Count + iteratableCommas.Count];
                Token<LuaTokenType> endKw = tokens[3 + varCommas.Count + iteratableCommas.Count];
                return new GenericForLoopStatement ( node.Scope, forKw, node.Variables, varCommas.Concat ( iteratableCommas ), inKw, iteratables, doKw, body, endKw ) { Semicolon = node.Semicolon };
            }

            return node;
        }

        public virtual LuaASTNode VisitLocalVariableDeclaration ( LocalVariableDeclarationStatement node )
        {
            if ( node.Values.Length > 0 )
            {
                Expression[] values = node.Values.Select ( val => ( Expression ) this.VisitNode ( val ) ).ToArray ( );

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

        public virtual LuaASTNode VisitNode ( LuaASTNode node ) =>
            node.Accept ( this );

        public virtual LuaASTNode VisitNumericFor ( NumericForLoopStatement node )
        {
            var body = ( StatementList ) this.VisitNode ( node.Body );
            var initial = ( Expression ) this.VisitNode ( node.Initial );
            var final = ( Expression ) this.VisitNode ( node.Final );

            if ( node.Step != null )
            {
                var step = ( Expression ) this.VisitNode ( node.Step );
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
            var body = ( StatementList ) this.VisitNode ( node.Body );
            var cond = ( Expression ) this.VisitNode ( node.Condition );

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
            Expression[] values = node.Values.Select ( val => ( Expression ) this.VisitNode ( val ) ).ToArray ( );

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
            Statement[] body = node.Body.Select ( stmt => ( Statement ) this.VisitNode ( stmt ) ).ToArray ( );

            if ( !body.SequenceEqual ( node.Body ) )
            {
                return new StatementList ( node.Scope, body ) { Semicolon = node.Semicolon };
            }

            return node;
        }

        public virtual LuaASTNode VisitWhileLoop ( WhileLoopStatement node )
        {
            var cond = ( Expression ) this.VisitNode ( node.Condition );
            var body = ( StatementList ) this.VisitNode ( node.Body );

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