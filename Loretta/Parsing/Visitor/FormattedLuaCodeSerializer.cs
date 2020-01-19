using System;
using System.Collections.Immutable;
using System.Linq;
using GParse.IO;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.AST.Tables;

namespace Loretta.Parsing.Visitor
{
    public class FormattedLuaCodeSerializer : ITreeVisitor
    {
        private readonly CodeWriter _writer;

        public FormattedLuaCodeSerializer ( String indentation = "\t" )
        {
            this._writer = new CodeWriter ( indentation );
        }

        #region Code Serialization Helpers

        private void WriteStatementLineEnd ( Statement statement )
        {
            if ( statement.Semicolon is Token<LuaTokenType> )
                this._writer.WriteLine ( ";" );
            else
                this._writer.WriteLine ( );
        }

        private void WriteSeparatedNodeList<T> ( String separator, ImmutableArray<T> nodes )
            where T : LuaASTNode
        {
            for ( var i = 0; i < nodes.Length; i++ )
            {
                this.VisitNode ( nodes[i] );
                if ( i != nodes.Length - 1 )
                {
                    this._writer.Write ( separator );
                }
            }
        }

        #endregion Code Serialization Helpers

        #region ITreeVisitor

        public virtual void VisitNode ( LuaASTNode node ) =>
            node.Accept ( this );

        #region Expressions

        public virtual void VisitAnonymousFunction ( AnonymousFunctionExpression anonymousFunction )
        {
            this._writer.Write ( "function ( " );
            this.WriteSeparatedNodeList ( ", ", anonymousFunction.Arguments );
            this._writer.WriteLine ( " )" );
            this._writer.WithIndentation ( ( ) => this.VisitStatementList ( anonymousFunction.Body ) );
            this._writer.WriteIndented ( "end" );
        }

        public virtual void VisitBinaryOperation ( BinaryOperationExpression binaryOperaion )
        {
            this.VisitNode ( binaryOperaion.Left );
            this._writer.Write ( ' ' );
            this._writer.Write ( binaryOperaion.Operator.Value );
            this._writer.Write ( ' ' );
            this.VisitNode ( binaryOperaion.Right );
        }

        public virtual void VisitBoolean ( BooleanExpression booleanExpression ) =>
            this._writer.Write ( booleanExpression.Value ? "true" : "false" );

        public virtual void VisitFunctionCall ( FunctionCallExpression node )
        {
            this.VisitNode ( node.Function );
            this._writer.Write ( " ( " );
            this.WriteSeparatedNodeList ( ", ", node.Arguments );
            this._writer.Write ( " )" );
        }

        public virtual void VisitGroupedExpression ( GroupedExpression node )
        {
            this._writer.Write ( "( " );
            this.VisitNode ( node.InnerExpression );
            this._writer.Write ( " )" );
        }

        public virtual void VisitIdentifier ( IdentifierExpression identifier ) =>
            this._writer.Write ( identifier.Identifier );

        public virtual void VisitIndex ( IndexExpression node )
        {
            this.VisitNode ( node.Indexee );
            switch ( node.Type )
            {
                case IndexType.Indexer:
                    this._writer.Write ( "[" );
                    this.VisitNode ( node.Indexer );
                    this._writer.Write ( "]" );
                    break;

                case IndexType.Member:
                    this._writer.Write ( "." );
                    this.VisitNode ( node.Indexer );
                    break;

                case IndexType.Method:
                    this._writer.Write ( ":" );
                    this.VisitNode ( node.Indexer );
                    break;

                default:
                    throw new InvalidOperationException ( "Invalid index type." );
            }
        }

        public virtual void VisitNil ( NilExpression nilExpression ) =>
            this._writer.Write ( "nil" );

        public virtual void VisitNumber ( NumberExpression numberExpression ) =>
            this._writer.Write ( numberExpression.Tokens.Single ( ).Raw );

        public virtual void VisitString ( StringExpression node ) =>
            this._writer.Write ( node.Tokens.Single ( ).Raw );

        public virtual void VisitTableConstructor ( TableConstructorExpression node )
        {
            this._writer.WriteLine ( "{" );
            this._writer.WithIndentation ( ( ) =>
            {
                for ( var i = 0; i < node.Fields.Length; i++ )
                {
                    this._writer.WriteIndentation ( );
                    this.VisitTableField ( node.Fields[i] );
                    this._writer.WriteLine ( );
                }
            } );
            this._writer.WriteIndented ( "}" );
        }

        public virtual void VisitTableField ( TableField node )
        {
            switch ( node.KeyType )
            {
                case TableFieldKeyType.Expression:
                    this._writer.Write ( "[" );
                    this.VisitNode ( node.Key );
                    this._writer.Write ( "] = " );
                    break;

                case TableFieldKeyType.Identifier:
                    this.VisitNode ( node.Key );
                    this._writer.Write ( " = " );
                    break;

                case TableFieldKeyType.None:
                    break;

                default:
                    throw new InvalidOperationException ( "Invalid table key type." );
            }
            this.VisitNode ( node.Value );
            if ( node.Delimiter != default )
                this._writer.Write ( node.Delimiter.Raw );
        }

        public virtual void VisitUnaryOperation ( UnaryOperationExpression node )
        {
            switch ( node.Fix )
            {
                case UnaryOperationFix.Prefix:
                    this._writer.Write ( node.Operator.Value );
                    this.VisitNode ( node.Operand );
                    break;

                case UnaryOperationFix.Postfix:
                    this.VisitNode ( node.Operand );
                    this._writer.Write ( node.Operator.Value );
                    break;

                default:
                    throw new InvalidOperationException ( "Invalid unary operator fix." );
            }
        }

        public virtual void VisitVarArg ( VarArgExpression varArg ) =>
            this._writer.Write ( "..." );

        #endregion

        public virtual void VisitAssignment ( AssignmentStatement assignmentStatement )
        {
            this._writer.WriteIndentation ( );
            this.WriteSeparatedNodeList ( ", ", assignmentStatement.Variables );
            this._writer.Write ( " = " );
            this.WriteSeparatedNodeList ( ", ", assignmentStatement.Values );
            this.WriteStatementLineEnd ( assignmentStatement );
        }

        public virtual void VisitBreak ( BreakStatement breakStatement )
        {
            this._writer.WriteIndented ( "break" );
            this.WriteStatementLineEnd ( breakStatement );
        }

        public virtual void VisitContinue ( ContinueStatement continueStatement )
        {
            this._writer.WriteIndented ( "continue" );
            this.WriteStatementLineEnd ( continueStatement );
        }

        public virtual void VisitDo ( DoStatement doStatement )
        {
            this._writer.WriteLineIndented ( "do" );
            this._writer.WithIndentation ( ( ) => this.VisitStatementList ( doStatement.Body ) );
            this._writer.WriteIndented ( "end" );
            this.WriteStatementLineEnd ( doStatement );
        }

        public virtual void VisitExpressionStatement ( ExpressionStatement expressionStatement )
        {
            this._writer.WriteIndentation ( );
            this.VisitNode ( expressionStatement.Expression );
            this.WriteStatementLineEnd ( expressionStatement );
        }

        public virtual void VisitFunctionDefinition ( FunctionDefinitionStatement functionDeclaration )
        {
            if ( functionDeclaration.IsLocal )
            {
                this._writer.WriteIndented ( "local function " );
            }
            else
            {
                this._writer.WriteIndented ( "function " );
            }

            this.VisitNode ( functionDeclaration.Name );
            this._writer.Write ( " ( " );
            this.WriteSeparatedNodeList ( ", ", functionDeclaration.Arguments );
            this._writer.WriteLine ( " ) " );
            this._writer.WithIndentation ( ( ) => this.VisitStatementList ( functionDeclaration.Body ) );
            this._writer.WriteIndented ( "end" );
            this.WriteStatementLineEnd ( functionDeclaration );
        }

        public virtual void VisitGotoLabel ( GotoLabelStatement gotoLabelStatement )
        {
            this._writer.WriteIndented ( "::" );
            this._writer.Write ( gotoLabelStatement.Label.Identifier );
            this._writer.Write ( "::" );
            this.WriteStatementLineEnd ( gotoLabelStatement );
        }

        public virtual void VisitGoto ( GotoStatement gotoStatement )
        {
            this._writer.WriteIndented ( "goto " );
            this._writer.Write ( gotoStatement.Target.Identifier );
            this.WriteStatementLineEnd ( gotoStatement );
        }

        public virtual void VisitIfStatement ( IfStatement ifStatement )
        {
            for ( var i = 0; i < ifStatement.Clauses.Length; i++ )
            {
                IfClause clause = ifStatement.Clauses[i];
                if ( i == 0 )
                {
                    this._writer.WriteIndented ( "if " );
                }
                else
                {
                    this._writer.WriteIndented ( "elseif " );
                }

                this.VisitNode ( clause.Condition );

                this._writer.WriteLine ( " then" );
                this._writer.WithIndentation ( ( ) => this.VisitStatementList ( clause.Body ) );
            }

            if ( ifStatement.ElseBlock is StatementList )
            {
                this._writer.WriteLineIndented ( "else" );
                this._writer.WithIndentation ( ( ) => this.VisitStatementList ( ifStatement.ElseBlock ) );
            }

            this._writer.WriteIndented ( "end" );
            this.WriteStatementLineEnd ( ifStatement );
        }

        public virtual void VisitGenericFor ( GenericForLoopStatement genericForLoop )
        {
            this._writer.WriteIndented ( "for " );
            this.WriteSeparatedNodeList ( ", ", genericForLoop.Variables );
            this._writer.Write ( " in " );
            this.VisitNode ( genericForLoop.Iteratable );
            this._writer.WriteLine ( " do" );
            this._writer.WithIndentation ( ( ) => this.VisitStatementList ( genericForLoop.Body ) );
            this._writer.WriteIndented ( "end" );
            this.WriteStatementLineEnd ( genericForLoop );
        }

        public virtual void VisitLocalVariableDeclaration ( LocalVariableDeclarationStatement localVariableDeclaration )
        {
            this._writer.WriteIndented ( "local " );
            this.WriteSeparatedNodeList ( ", ", localVariableDeclaration.Identifiers );
            if ( localVariableDeclaration.Values.Any ( ) )
            {
                this._writer.Write ( " = " );
                this.WriteSeparatedNodeList ( ", ", localVariableDeclaration.Values );
            }
            this.WriteStatementLineEnd ( localVariableDeclaration );
        }

        public virtual void VisitNumericFor ( NumericForLoopStatement numericForLoop )
        {
            this._writer.WriteIndented ( "for " );
            this.VisitIdentifier ( numericForLoop.Variable );
            this._writer.Write ( " = " );
            this.VisitNode ( numericForLoop.Initial );
            this._writer.Write ( ", " );
            this.VisitNode ( numericForLoop.Final );
            if ( numericForLoop.Step is Expression )
            {
                this._writer.Write ( ", " );
                this.VisitNode ( numericForLoop.Step );
            }
            this._writer.WriteLine ( " do" );
            this._writer.WithIndentation ( ( ) => this.VisitStatementList ( numericForLoop.Body ) );
            this._writer.WriteIndented ( "end" );
            this.WriteStatementLineEnd ( numericForLoop );
        }

        public virtual void VisitRepeatUntil ( RepeatUntilStatement repeatUntilLoop )
        {
            this._writer.WriteLineIndented ( "repeat" );
            this._writer.WithIndentation ( ( ) => this.VisitStatementList ( repeatUntilLoop.Body ) );
            this._writer.WriteIndented ( "until " );
            this.VisitNode ( repeatUntilLoop.Condition );
            this.WriteStatementLineEnd ( repeatUntilLoop );
        }

        public virtual void VisitReturn ( ReturnStatement returnStatement )
        {
            this._writer.WriteIndented ( "return " );
            this.WriteSeparatedNodeList ( ", ", returnStatement.Values );
            this.WriteStatementLineEnd ( returnStatement );
        }

        public virtual void VisitStatementList ( StatementList node )
        {
            foreach ( Statement statement in node.Body )
            {
                this.VisitNode ( statement );
            }
        }

        public virtual void VisitWhileLoop ( WhileLoopStatement whileLoop )
        {
            this._writer.WriteIndented ( "while " );
            this.VisitNode ( whileLoop.Condition );
            this._writer.WriteLine ( " do" );
            this._writer.WithIndentation ( ( ) => this.VisitStatementList ( whileLoop.Body ) );
            this._writer.WriteIndented ( "end" );
            this.WriteStatementLineEnd ( whileLoop );
        }

        #endregion ITreeVisitor

        public void Clear ( ) =>
            this._writer.Reset ( );

        public override String ToString ( ) =>
            this._writer.ToString ( );
    }
}