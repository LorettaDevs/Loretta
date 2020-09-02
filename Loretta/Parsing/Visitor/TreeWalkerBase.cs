using Loretta.Parsing.AST;
using Loretta.Parsing.AST.Tables;

namespace Loretta.Parsing.Visitor
{
    /// <summary>
    /// The base class for tree walkers.
    /// </summary>
    public abstract class TreeWalkerBase : ITreeVisitor
    {
        /// <summary>
        /// Walks into a nil expression. Does nothing by default.
        /// </summary>
        /// <param name="node">The nil expression to walk into.</param>
        public virtual void VisitNil ( NilExpression node )
        {
        }

        /// <summary>
        /// Walks into a boolean expression. Does nothing by default.
        /// </summary>
        /// <param name="node">The boolean expression to walk into.</param>
        public virtual void VisitBoolean ( BooleanExpression node )
        {
        }

        /// <summary>
        /// Walks into an identifier expression. Does nothing by default.
        /// </summary>
        /// <param name="node">The identifier expression to walk into.</param>
        public virtual void VisitIdentifier ( IdentifierExpression node )
        {
        }

        /// <summary>
        /// Walks into an indexing operation expression. Walks into the <see
        /// cref="IndexExpression.Indexee" /> and then into the <see cref="IndexExpression.Indexer"
        /// /> by default.
        /// </summary>
        /// <param name="node">The indexing operation expression to walk into.</param>
        public virtual void VisitIndex ( IndexExpression node )
        {
            this.VisitNode ( node.Indexee );
            this.VisitNode ( node.Indexer );
        }

        /// <summary>
        /// Walks into a number expression. Does nothing by default.
        /// </summary>
        /// <param name="node">The number expression to walk into.</param>
        public virtual void VisitNumber ( NumberExpression node )
        {
        }

        /// <summary>
        /// Walks into a string expression. Does nothing by default.
        /// </summary>
        /// <param name="node">The string expression to walk into.</param>
        public virtual void VisitString ( StringExpression node )
        {
        }

        /// <summary>
        /// Walks into a vararg expression. Does nothing by default.
        /// </summary>
        /// <param name="node">The vararg expression to walk into.</param>
        public virtual void VisitVarArg ( VarArgExpression node )
        {
        }

        /// <summary>
        /// Walks into a function call expression. Walks into the <see
        /// cref="FunctionCallExpression.Function" /> being called then the <see
        /// cref="FunctionCallExpression.Arguments" /> in the collection's order by default.
        /// </summary>
        /// <param name="node">The function call expression to walk into.</param>
        public virtual void VisitFunctionCall ( FunctionCallExpression node )
        {
            this.VisitNode ( node.Function );
            foreach ( Expression argument in node.Arguments )
                this.VisitNode ( argument );
        }

        /// <summary>
        /// Walks into a unary operation expression. Walks into the <see
        /// cref="UnaryOperationExpression.Operand" /> by default.
        /// </summary>
        /// <param name="node">The unuary operation expression to walk into.</param>
        public virtual void VisitUnaryOperation ( UnaryOperationExpression node ) =>
            this.VisitNode ( node.Operand );

        /// <summary>
        /// Walks into a grouped expression. Walks into the <see
        /// cref="GroupedExpression.InnerExpression" /> by default.
        /// </summary>
        /// <param name="node">The grouped expression to walk into.</param>
        public virtual void VisitGroupedExpression ( GroupedExpression node ) =>
            this.VisitNode ( node.InnerExpression );

        /// <summary>
        /// Walks into a binary operation expression. Walks into the <see
        /// cref="BinaryOperationExpression.Left" /> operand then into the <see
        /// cref="BinaryOperationExpression.Right" /> operand by default.
        /// </summary>
        /// <param name="node">The nil expression to walk into.</param>
        public virtual void VisitBinaryOperation ( BinaryOperationExpression node )
        {
            this.VisitNode ( node.Left );
            this.VisitNode ( node.Right );
        }

        /// <summary>
        /// Walks into a table field. Walks into the <see cref="TableField.Key" /> (if any) then
        /// into the <see cref="TableField.Value" /> by default.
        /// </summary>
        /// <param name="node">The table field to walk into.</param>
        public virtual void VisitTableField ( TableField node )
        {
            if ( node.Key is Expression )
                this.VisitNode ( node.Key );
            this.VisitNode ( node.Value );
        }

        /// <summary>
        /// Walks into a table constructor expression. Walks into the table <see
        /// cref="TableConstructorExpression.Fields" /> in the order of the collection by default.
        /// </summary>
        /// <param name="node">The table constructor expression to walk into.</param>
        public virtual void VisitTableConstructor ( TableConstructorExpression node )
        {
            foreach ( TableField field in node.Fields )
            {
                this.VisitNode ( field );
            }
        }

        /// <summary>
        /// Walks into an anonymous function expression. Walks into the <see
        /// cref="AnonymousFunctionExpression.Arguments" /> in the order of the collection and then
        /// into the <see cref="AnonymousFunctionExpression.Body" /> by default.
        /// </summary>
        /// <param name="node">The anonymous function expression to walk into.</param>
        public virtual void VisitAnonymousFunction ( AnonymousFunctionExpression node )
        {
            foreach ( Expression argument in node.Arguments )
            {
                this.VisitNode ( argument );
            }

            this.VisitNode ( node.Body );
        }

        /// <summary>
        /// Walks into an assignment statement. Walks into the <see
        /// cref="AssignmentStatement.Variables" /> in the order of the collection and then into the
        /// <see cref="AssignmentStatement.Values" /> in the order of the collection by default.
        /// </summary>
        /// <param name="node">The assignment statement to walk into.</param>
        public virtual void VisitAssignment ( AssignmentStatement node )
        {
            foreach ( Expression variable in node.Variables )
            {
                this.VisitNode ( variable );
            }

            foreach ( Expression value in node.Values )
            {
                this.VisitNode ( value );
            }
        }

        /// <summary>
        /// Walks into a compound assignment statement. Walks into the <see
        /// cref="CompoundAssignmentStatement.Assignee" /> and then into the <see
        /// cref="CompoundAssignmentStatement.ValueExpression" /> by default.
        /// </summary>
        /// <param name="node">The compound assignment statement to walk into.</param>
        public void VisitCompoundAssignmentStatement ( CompoundAssignmentStatement node )
        {
            this.VisitNode ( node.Assignee );
            this.VisitNode ( node.ValueExpression );
        }

        /// <summary>
        /// Walks into a break statement. Does nothing by default.
        /// </summary>
        /// <param name="node">The break statement to walk into.</param>
        public virtual void VisitBreak ( BreakStatement node )
        {
        }

        /// <summary>
        /// Walks into a continue statement. Does nothing by default.
        /// </summary>
        /// <param name="node">The continue statement to walk into.</param>
        public virtual void VisitContinue ( ContinueStatement node )
        {
        }

        /// <summary>
        /// Walks into a do statement. Walks into the <see cref="DoStatement.Body" /> by default.
        /// </summary>
        /// <param name="node">The do statement to walk into.</param>
        public virtual void VisitDo ( DoStatement node ) =>
            this.VisitNode ( node.Body );

        /// <summary>
        /// Walks into an expression statement. Visits the <see
        /// cref="ExpressionStatement.Expression" /> by default.
        /// </summary>
        /// <param name="node">The expression statement to walk into.</param>
        public virtual void VisitExpressionStatement ( ExpressionStatement node ) =>
            this.VisitNode ( node.Expression );

        /// <summary>
        /// Walks into a function definition statement. Walks into the <see
        /// cref="FunctionDefinitionStatement.Name" /> then into the function <see
        /// cref="FunctionDefinitionStatement.Arguments" /> in the collection's order and then into
        /// the <see cref="FunctionDefinitionStatement.Body" /> by default.
        /// </summary>
        /// <param name="node">The break statement to walk into.</param>
        public virtual void VisitFunctionDefinition ( FunctionDefinitionStatement node )
        {
            this.VisitNode ( node.Name );
            foreach ( Expression argument in node.Arguments )
            {
                this.VisitNode ( argument );
            }
            this.VisitNode ( node.Body );
        }

        /// <summary>
        /// Walks into a goto label. Does nothing by default.
        /// </summary>
        /// <param name="node">The goto label to walk into.</param>
        public virtual void VisitGotoLabel ( GotoLabelStatement node )
        {
        }

        /// <summary>
        /// Walks into a goto statement. Does nothing by default.
        /// </summary>
        /// <param name="node">The goto statement to walk into.</param>
        public virtual void VisitGoto ( GotoStatement node )
        {
        }

        /// <summary>
        /// Walks into an if statement. Walks into each <see cref="IfStatement.Clauses" />' <see
        /// cref="IfClause.Condition" /> and <see cref="IfClause.Body" /> in the collection's order
        /// and then into the <see cref="IfStatement.ElseBlock" /> if any by default.
        /// </summary>
        /// <param name="node">The if statement to walk into.</param>
        public virtual void VisitIfStatement ( IfStatement node )
        {
            foreach ( IfClause clause in node.Clauses )
            {
                this.VisitNode ( clause.Condition );
                this.VisitNode ( clause.Body );
            }

            if ( node.ElseBlock != null )
            {
                this.VisitNode ( node.ElseBlock );
            }
        }

        /// <summary>
        /// Walks into a generic for statement. Walks into the <see
        /// cref="GenericForLoopStatement.Variables" /> in the collection's order and then into the
        /// <see cref="GenericForLoopStatement.Expressions" /> in the collection's order and then
        /// into the <see cref="GenericForLoopStatement.Body" /> by default.
        /// </summary>
        /// <param name="node">The generic for statement to walk into.</param>
        public virtual void VisitGenericFor ( GenericForLoopStatement node )
        {
            foreach ( IdentifierExpression variable in node.Variables )
            {
                this.VisitNode ( variable );
            }
            foreach ( Expression iteratable in node.Expressions )
            {
                this.VisitNode ( iteratable );
            }
            this.VisitNode ( node.Body );
        }

        /// <summary>
        /// Walks into a local variable declaration statement. Walks into the <see
        /// cref="LocalVariableDeclarationStatement.Identifiers" /> in the collection's order and
        /// then into the <see cref="LocalVariableDeclarationStatement.Values" /> in the
        /// collection's order by default.
        /// </summary>
        /// <param name="node">The local variable declaration statement to walk into.</param>
        public virtual void VisitLocalVariableDeclaration ( LocalVariableDeclarationStatement node )
        {
            foreach ( IdentifierExpression identifier in node.Identifiers )
            {
                this.VisitNode ( identifier );
            }

            foreach ( Expression value in node.Values )
            {
                this.VisitNode ( value );
            }
        }

        /// <summary>
        /// Walks into a node.
        /// </summary>
        /// <param name="node">The node to walk into.</param>
        public virtual void VisitNode ( LuaASTNode node ) =>
            node?.Accept ( this );

        /// <summary>
        /// Walks into a numeric for loop statement. Walks into the <see
        /// cref="NumericForLoopStatement.Variable" /> then into <see
        /// cref="NumericForLoopStatement.Initial" /> then into <see
        /// cref="NumericForLoopStatement.Final" /> then into the <see
        /// cref="NumericForLoopStatement.Step" /> (if any) and then into the <see
        /// cref="NumericForLoopStatement.Body" />.
        /// </summary>
        /// <param name="node">The numeric for loop statement to walk into.</param>
        public virtual void VisitNumericFor ( NumericForLoopStatement node )
        {
            this.VisitNode ( node.Variable );
            this.VisitNode ( node.Initial );
            this.VisitNode ( node.Final );
            if ( node.Step != null )
                this.VisitNode ( node.Step );
            this.VisitNode ( node.Body );
        }

        /// <summary>
        /// Walks into a repeat until statement. Walks into the <see
        /// cref="RepeatUntilStatement.Body" /> and then into the <see
        /// cref="RepeatUntilStatement.Condition" /> by default.
        /// </summary>
        /// <param name="node">The repeat until statement to walk into.</param>
        public virtual void VisitRepeatUntil ( RepeatUntilStatement node )
        {
            this.VisitNode ( node.Body );
            this.VisitNode ( node.Condition );
        }

        /// <summary>
        /// Walks into a return statement. Walks into the <see cref="ReturnStatement.Values" /> in
        /// the collection's order by default.
        /// </summary>
        /// <param name="node">The repeat until statement to walk into.</param>
        public virtual void VisitReturn ( ReturnStatement node )
        {
            foreach ( Expression value in node.Values )
                this.VisitNode ( value );
        }

        /// <summary>
        /// Walks into a statement list. Walks into the <see cref="StatementList.Body" />'s
        /// statements in the collection's order by default.
        /// </summary>
        /// <param name="node">The statement list to walk into.</param>
        public virtual void VisitStatementList ( StatementList node )
        {
            foreach ( Statement statement in node.Body )
            {
                this.VisitNode ( statement );
            }
        }

        /// <summary>
        /// Walks into a while loop statement. Walks into the <see
        /// cref="WhileLoopStatement.Condition" /> then into the <see cref="WhileLoopStatement.Body"
        /// /> by default.
        /// </summary>
        /// <param name="node">The while loop statement to walk into.</param>
        public virtual void VisitWhileLoop ( WhileLoopStatement node )
        {
            this.VisitNode ( node.Condition );
            this.VisitNode ( node.Body );
        }

        /// <summary>
        /// Walks into an empty statement. Does nothing by default.
        /// </summary>
        /// <param name="emptyStatement">The empty statement to walk into.</param>
        public void VisitEmptyStatement ( EmptyStatement emptyStatement )
        {
        }
    }
}