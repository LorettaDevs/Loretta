using Loretta.Parsing.AST;
using Loretta.Parsing.AST.Tables;

namespace Loretta.Parsing.Visitor
{
    public abstract class TreeWalkerBase : ITreeVisitor
    {
        public virtual void VisitNil ( NilExpression node )
        {
        }

        public virtual void VisitBoolean ( BooleanExpression node )
        {
        }

        public virtual void VisitIdentifier ( IdentifierExpression node )
        {
        }

        public virtual void VisitIndex ( IndexExpression node )
        {
            this.VisitNode ( node.Indexee );
            this.VisitNode ( node.Indexer );
        }

        public virtual void VisitNumber ( NumberExpression node )
        {
        }

        public virtual void VisitString ( StringExpression node )
        {
        }

        public virtual void VisitVarArg ( VarArgExpression node )
        {
        }

        public virtual void VisitFunctionCall ( FunctionCallExpression node )
        {
            this.VisitNode ( node.Function );
            foreach ( Expression argument in node.Arguments )
                this.VisitNode ( argument );
        }

        public virtual void VisitUnaryOperation ( UnaryOperationExpression node ) =>
            this.VisitNode ( node.Operand );

        public virtual void VisitGroupedExpression ( GroupedExpression node ) =>
            this.VisitNode ( node.InnerExpression );

        public virtual void VisitBinaryOperation ( BinaryOperationExpression node )
        {
            this.VisitNode ( node.Left );
            this.VisitNode ( node.Right );
        }

        public virtual void VisitTableField ( TableField node )
        {
            this.VisitNode ( node.Key );
            this.VisitNode ( node.Value );
        }

        public virtual void VisitTableConstructor ( TableConstructorExpression node )
        {
            foreach ( TableField field in node.Fields )
            {
                this.VisitTableField ( field );
            }
        }

        public virtual void VisitAnonymousFunction ( AnonymousFunctionExpression node )
        {
            foreach ( Expression argument in node.Arguments )
            {
                this.VisitNode ( argument );
            }

            this.VisitStatementList ( node.Body );
        }

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

        public virtual void VisitBreak ( BreakStatement node )
        {
        }

        public virtual void VisitContinue ( ContinueStatement node )
        {
        }

        public virtual void VisitDo ( DoStatement node ) =>
            this.VisitStatementList ( node.Body );

        public virtual void VisitExpressionStatement ( ExpressionStatement node ) =>
            this.VisitNode ( node.Expression );

        public virtual void VisitFunctionDefinition ( FunctionDefinitionStatement node )
        {
            this.VisitNode ( node.Name );
            foreach ( Expression argument in node.Arguments )
            {
                this.VisitNode ( argument );
            }
            this.VisitStatementList ( node.Body );
        }

        public virtual void VisitGotoLabel ( GotoLabelStatement node )
        {
        }

        public virtual void VisitGoto ( GotoStatement node )
        {
        }

        public virtual void VisitIfStatement ( IfStatement node )
        {
            foreach ( IfClause clause in node.Clauses )
            {
                this.VisitNode ( clause.Condition );
                this.VisitStatementList ( clause.Body );
            }

            if ( node.ElseBlock != null )
            {
                this.VisitStatementList ( node.ElseBlock );
            }
        }

        public virtual void VisitGenericFor ( GenericForLoopStatement node )
        {
            foreach ( IdentifierExpression variable in node.Variables )
            {
                this.VisitIdentifier ( variable );
            }

            this.VisitNode ( node.Iteratable );
            this.VisitStatementList ( node.Body );
        }

        public virtual void VisitLocalVariableDeclaration ( LocalVariableDeclarationStatement node )
        {
            foreach ( IdentifierExpression identifier in node.Identifiers )
            {
                this.VisitIdentifier ( identifier );
            }

            foreach ( Expression value in node.Values )
            {
                this.VisitNode ( value );
            }
        }

        public virtual void VisitNode ( LuaASTNode node ) =>
            node?.Accept ( this );

        public virtual void VisitNumericFor ( NumericForLoopStatement node )
        {
            this.VisitIdentifier ( node.Variable );
            this.VisitNode ( node.Initial );
            this.VisitNode ( node.Final );
            if ( node.Step != null )
                this.VisitNode ( node.Step );
            this.VisitStatementList ( node.Body );
        }

        public virtual void VisitRepeatUntil ( RepeatUntilStatement node )
        {
            this.VisitStatementList ( node.Body );
            this.VisitNode ( node.Condition );
        }

        public virtual void VisitReturn ( ReturnStatement node )
        {
            foreach ( Expression value in node.Values )
                this.VisitNode ( value );
        }

        public virtual void VisitStatementList ( StatementList node )
        {
            foreach ( Statement statement in node.Body )
            {
                this.VisitNode ( statement );
            }
        }

        public virtual void VisitWhileLoop ( WhileLoopStatement node )
        {
            this.VisitNode ( node.Condition );
            this.VisitStatementList ( node.Body );
        }
    }
}