using System;
using System.Collections.Immutable;
using System.Linq;
using GParse.IO;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.AST.Tables;
using Loretta.Utilities;

namespace Loretta.Parsing.Visitor
{
    /// <summary>
    /// A formatted lua code serializer. Transforms an AST into formatted code.
    /// </summary>
    public class FormattedLuaCodeSerializer : ITreeVisitor
    {
        private readonly CodeWriter _writer;

        /// <summary>
        /// The lua options being used by this serializer.
        /// </summary>
        public LuaOptions LuaOptions { get; }

        /// <summary>
        /// Initializes a new formatted lua code serializer.
        /// </summary>
        /// <param name="luaOptions">The lua options to be used by this serializer.</param>
        /// <param name="indentation">The indentation to use.</param>
        public FormattedLuaCodeSerializer ( LuaOptions luaOptions, String indentation = "\t" )
        {
            this._writer = new CodeWriter ( indentation );
            this.LuaOptions = luaOptions;
        }

        #region Code Serialization Helpers

        /// <summary>
        /// Writes the EOL of a statement adding the semicolon if required.
        /// </summary>
        /// <param name="statement">The statement whose EOL is to be written.</param>
        private void WriteStatementLineEnd ( Statement statement )
        {
            if ( statement.Semicolon is Token<LuaTokenType> )
                this._writer.WriteLine ( ";" );
            else
                this._writer.WriteLine ( );
        }

        /// <summary>
        /// Writes a list of nodes separated by the specified separator.
        /// </summary>
        /// <typeparam name="T">The type of the nodes in the list.</typeparam>
        /// <param name="separator">The separator.</param>
        /// <param name="nodes">The nodes.</param>
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

        /// <summary>
        /// Visits a node.
        /// </summary>
        /// <param name="node"></param>
        public virtual void VisitNode ( LuaASTNode node ) =>
            node.Accept ( this );

        #region Expressions

        /// <inheritdoc />
        public virtual void VisitAnonymousFunction ( AnonymousFunctionExpression anonymousFunction )
        {
            this._writer.Write ( "function ( " );
            this.WriteSeparatedNodeList ( ", ", anonymousFunction.Arguments );
            this._writer.WriteLine ( " )" );
            this._writer.WithIndentation ( ( ) => this.VisitNode ( anonymousFunction.Body ) );
            this._writer.WriteIndented ( "end" );
        }

        /// <inheritdoc />
        public virtual void VisitBinaryOperation ( BinaryOperationExpression binaryOperaion )
        {
            this.VisitNode ( binaryOperaion.Left );
            this._writer.Write ( ' ' );
            this._writer.Write ( binaryOperaion.Operator.Value );
            this._writer.Write ( ' ' );
            this.VisitNode ( binaryOperaion.Right );
        }

        /// <inheritdoc />
        public virtual void VisitBoolean ( BooleanExpression booleanExpression ) =>
            this._writer.Write ( booleanExpression.Value ? "true" : "false" );

        /// <inheritdoc />
        public virtual void VisitFunctionCall ( FunctionCallExpression node )
        {
            this.VisitNode ( node.Function );
            this._writer.Write ( " ( " );
            this.WriteSeparatedNodeList ( ", ", node.Arguments );
            this._writer.Write ( " )" );
        }

        /// <inheritdoc />
        public virtual void VisitGroupedExpression ( GroupedExpression node )
        {
            this._writer.Write ( "( " );
            this.VisitNode ( node.InnerExpression );
            this._writer.Write ( " )" );
        }

        /// <inheritdoc />
        public virtual void VisitIdentifier ( IdentifierExpression identifier ) =>
            this._writer.Write ( identifier.Identifier );

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual void VisitNil ( NilExpression nilExpression ) =>
            this._writer.Write ( "nil" );

        /// <inheritdoc />
        public virtual void VisitNumber ( NumberExpression numberExpression ) =>
            this._writer.Write ( numberExpression.Tokens.Single ( ).Raw );

        /// <inheritdoc />
        public virtual void VisitString ( StringExpression node ) =>
            this._writer.Write ( node.Tokens.Single ( ).Raw );

        /// <inheritdoc />
        public virtual void VisitTableConstructor ( TableConstructorExpression node )
        {
            this._writer.WriteLine ( "{" );
            this._writer.WithIndentation ( ( ) =>
            {
                for ( var i = 0; i < node.Fields.Length; i++ )
                {
                    this._writer.WriteIndentation ( );
                    this.VisitNode ( node.Fields[i] );
                    this._writer.WriteLine ( );
                }
            } );
            this._writer.WriteIndented ( "}" );
        }

        /// <inheritdoc />
        public virtual void VisitTableField ( TableField node )
        {
            switch ( node.KeyType )
            {
                case TableFieldKeyType.Expression:
                    this._writer.Write ( "[" );
                    this.VisitNode ( node.Key! );
                    this._writer.Write ( "] = " );
                    break;

                case TableFieldKeyType.Identifier:
                    this.VisitNode ( node.Key! );
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

        /// <inheritdoc />
        public virtual void VisitUnaryOperation ( UnaryOperationExpression node )
        {
            switch ( node.Fix )
            {
                case UnaryOperationFix.Prefix:
                    this._writer.Write ( node.Operator.Value );
                    if ( StringUtils.IsIdentifier ( this.LuaOptions.UseLuaJitIdentifierRules, node.Operator.Raw ) )
                        this._writer.Write ( ' ' );
                    this.VisitNode ( node.Operand );
                    break;

                case UnaryOperationFix.Postfix:
                    this.VisitNode ( node.Operand );
                    if ( StringUtils.IsIdentifier ( this.LuaOptions.UseLuaJitIdentifierRules, node.Operator.Raw ) )
                        this._writer.Write ( ' ' );
                    this._writer.Write ( node.Operator.Value );
                    break;

                default:
                    throw new InvalidOperationException ( "Invalid unary operator fix." );
            }
        }

        /// <inheritdoc />
        public virtual void VisitVarArg ( VarArgExpression varArg ) =>
            this._writer.Write ( "..." );

        #endregion Expressions

        /// <inheritdoc />
        public virtual void VisitAssignment ( AssignmentStatement assignmentStatement )
        {
            this._writer.WriteIndentation ( );
            this.WriteSeparatedNodeList ( ", ", assignmentStatement.Variables );
            this._writer.Write ( " = " );
            this.WriteSeparatedNodeList ( ", ", assignmentStatement.Values );
            this.WriteStatementLineEnd ( assignmentStatement );
        }

        /// <inheritdoc />
        public void VisitCompoundAssignmentStatement ( CompoundAssignmentStatement compoundAssignmentStatement )
        {
            this._writer.WriteIndentation ( );
            this.VisitNode ( compoundAssignmentStatement.Assignee );
            this._writer.Write ( " " );
            this._writer.Write ( compoundAssignmentStatement.OperatorToken.Id );
            this._writer.Write ( " " );
            this.VisitNode ( compoundAssignmentStatement.ValueExpression );
            this.WriteStatementLineEnd ( compoundAssignmentStatement );
        }

        /// <inheritdoc />
        public virtual void VisitBreak ( BreakStatement breakStatement )
        {
            this._writer.WriteIndented ( "break" );
            this.WriteStatementLineEnd ( breakStatement );
        }

        /// <inheritdoc />
        public virtual void VisitContinue ( ContinueStatement continueStatement )
        {
            this._writer.WriteIndented ( "continue" );
            this.WriteStatementLineEnd ( continueStatement );
        }

        /// <inheritdoc />
        public virtual void VisitDo ( DoStatement doStatement )
        {
            this._writer.WriteLineIndented ( "do" );
            this._writer.WithIndentation ( ( ) => this.VisitNode ( doStatement.Body ) );
            this._writer.WriteIndented ( "end" );
            this.WriteStatementLineEnd ( doStatement );
        }

        /// <inheritdoc />
        public virtual void VisitExpressionStatement ( ExpressionStatement expressionStatement )
        {
            this._writer.WriteIndentation ( );
            this.VisitNode ( expressionStatement.Expression );
            this.WriteStatementLineEnd ( expressionStatement );
        }

        /// <inheritdoc />
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
            this._writer.WithIndentation ( ( ) => this.VisitNode ( functionDeclaration.Body ) );
            this._writer.WriteIndented ( "end" );
            this.WriteStatementLineEnd ( functionDeclaration );
        }

        /// <inheritdoc />
        public virtual void VisitGotoLabel ( GotoLabelStatement gotoLabelStatement )
        {
            this._writer.WriteIndented ( "::" );
            this._writer.Write ( gotoLabelStatement.Label.Identifier );
            this._writer.Write ( "::" );
            this.WriteStatementLineEnd ( gotoLabelStatement );
        }

        /// <inheritdoc />
        public virtual void VisitGoto ( GotoStatement gotoStatement )
        {
            this._writer.WriteIndented ( "goto " );
            this._writer.Write ( gotoStatement.Target.Identifier );
            this.WriteStatementLineEnd ( gotoStatement );
        }

        /// <inheritdoc />
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
                this._writer.WithIndentation ( ( ) => this.VisitNode ( clause.Body ) );
            }

            if ( ifStatement.ElseBlock is StatementList )
            {
                this._writer.WriteLineIndented ( "else" );
                this._writer.WithIndentation ( ( ) => this.VisitNode ( ifStatement.ElseBlock ) );
            }

            this._writer.WriteIndented ( "end" );
            this.WriteStatementLineEnd ( ifStatement );
        }

        /// <inheritdoc />
        public virtual void VisitGenericFor ( GenericForLoopStatement genericForLoop )
        {
            this._writer.WriteIndented ( "for " );
            this.WriteSeparatedNodeList ( ", ", genericForLoop.Variables );
            this._writer.Write ( " in " );
            this.WriteSeparatedNodeList ( ", ", genericForLoop.Expressions );
            this._writer.WriteLine ( " do" );
            this._writer.WithIndentation ( ( ) => this.VisitNode ( genericForLoop.Body ) );
            this._writer.WriteIndented ( "end" );
            this.WriteStatementLineEnd ( genericForLoop );
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual void VisitNumericFor ( NumericForLoopStatement numericForLoop )
        {
            this._writer.WriteIndented ( "for " );
            this.VisitNode ( numericForLoop.Variable );
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
            this._writer.WithIndentation ( ( ) => this.VisitNode ( numericForLoop.Body ) );
            this._writer.WriteIndented ( "end" );
            this.WriteStatementLineEnd ( numericForLoop );
        }

        /// <inheritdoc />
        public virtual void VisitRepeatUntil ( RepeatUntilStatement repeatUntilLoop )
        {
            this._writer.WriteLineIndented ( "repeat" );
            this._writer.WithIndentation ( ( ) => this.VisitNode ( repeatUntilLoop.Body ) );
            this._writer.WriteIndented ( "until " );
            this.VisitNode ( repeatUntilLoop.Condition );
            this.WriteStatementLineEnd ( repeatUntilLoop );
        }

        /// <inheritdoc />
        public virtual void VisitReturn ( ReturnStatement returnStatement )
        {
            this._writer.WriteIndented ( "return " );
            this.WriteSeparatedNodeList ( ", ", returnStatement.Values );
            this.WriteStatementLineEnd ( returnStatement );
        }

        /// <inheritdoc />
        public virtual void VisitStatementList ( StatementList node )
        {
            foreach ( Statement statement in node.Body )
            {
                this.VisitNode ( statement );
            }
        }

        /// <inheritdoc />
        public virtual void VisitWhileLoop ( WhileLoopStatement whileLoop )
        {
            this._writer.WriteIndented ( "while " );
            this.VisitNode ( whileLoop.Condition );
            this._writer.WriteLine ( " do" );
            this._writer.WithIndentation ( ( ) => this.VisitNode ( whileLoop.Body ) );
            this._writer.WriteIndented ( "end" );
            this.WriteStatementLineEnd ( whileLoop );
        }

        /// <inheritdoc />
        public void VisitEmptyStatement ( EmptyStatement emptyStatement )
        {
            this._writer.WriteIndentation ( );
            this.WriteStatementLineEnd ( emptyStatement );
        }

        #endregion ITreeVisitor

        /// <summary>
        /// Clears the string contents written.
        /// </summary>
        public void Clear ( ) =>
            this._writer.Reset ( );

        /// <summary>
        /// Obtains the string contents of the visited nodes up to now.
        /// </summary>
        /// <returns></returns>
        public override String ToString ( ) =>
            this._writer.ToString ( );

        /// <summary>
        /// Formats a node into code with the provided lua options.
        /// </summary>
        /// <param name="luaOptions">The lua options to be used.</param>
        /// <param name="node">The node to be formatted.</param>
        /// <returns></returns>
        public static String Format ( LuaOptions luaOptions, LuaASTNode node )
        {
            var serializer = new FormattedLuaCodeSerializer ( luaOptions );
            serializer.VisitNode ( node );
            return serializer.ToString ( );
        }
    }
}