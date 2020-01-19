using System;
using Loretta.Parsing.AST;
using Loretta.Parsing.AST.Tables;

namespace Loretta.Parsing.Visitor
{
    public abstract class TreeWalkerBase : ITreeVisitor
    {
        public virtual void VisitNode ( LuaASTNode node ) =>
            node?.Accept ( this );

        public virtual void VisitAnonymousFunction ( AnonymousFunctionExpression node )
        {
            foreach ( Expression argument in node.Arguments )
                this.VisitNode ( argument );
            this.VisitStatementList ( node.Body );
        }

        public virtual void VisitAssignment ( AssignmentStatement node )
        {
            foreach ( Expression variable in node.Variables )
                this.VisitNode ( variable );
            foreach ( Expression value in node.Values )
                this.VisitNode ( value );
        }

        public virtual void VisitBinaryOperation ( BinaryOperationExpression node )
        {
            this.VisitNode ( node.Left );
            this.VisitNode ( node.Right );
        }

        public virtual void VisitBoolean ( BooleanExpression node )
        {
        }

        public virtual void VisitBreak ( BreakStatement node )
        {
        }

        public virtual void VisitContinue ( ContinueStatement @continue )
        {
        }

        public virtual void VisitDo ( DoStatement node ) => this.VisitStatementList ( node.Body );

        public virtual void VisitExpressionStatement ( ExpressionStatement node ) => this.VisitNode ( node.Expression );

        public virtual void VisitFunctionCall ( FunctionCallExpression node )
        {
            this.VisitNode ( node.Function );
            foreach ( Expression argument in node.Arguments )
                this.VisitNode ( argument );
        }

        public virtual void VisitFunctionDefinition ( FunctionDefinitionStatement node )
        {
            this.VisitNode ( node.Name );
            foreach ( Expression argument in node.Arguments )
                this.VisitNode ( argument );
            this.VisitStatementList ( node.Body );
        }

        public virtual void VisitGotoLabel ( GotoLabelStatement node )
        {
        }

        public virtual void VisitGoto ( GotoStatement node )
        {
        }

        public virtual void VisitGroupedExpression ( GroupedExpression node ) => this.VisitNode ( node.InnerExpression );

        public virtual void VisitIdentifier ( IdentifierExpression node )
        {
        }

        public virtual void VisitIfStatement ( IfStatement node )
        {
            foreach ( IfClause clause in node.Clauses )
                this.VisitNode ( clause );
            this.VisitStatementList ( node.ElseBlock );
        }

        public virtual void VisitIndex ( IndexExpression node )
        {
            this.VisitNode ( node.Indexee );
            this.VisitNode ( node.Indexer );
        }

        public virtual void VisitGenericFor ( GenericForLoopStatement node )
        {
            foreach ( IdentifierExpression variable in node.Variables )
                this.VisitIdentifier ( variable );
        }

        public virtual void VisitLocalVariableDeclaration ( LocalVariableDeclarationStatement node ) => throw new NotImplementedException ( );

        public virtual void VisitNil ( NilExpression node ) => throw new NotImplementedException ( );

        public virtual void VisitNumber ( NumberExpression node ) => throw new NotImplementedException ( );

        public virtual void VisitNumericFor ( NumericForLoopStatement node ) => throw new NotImplementedException ( );

        public virtual void VisitRepeatUntil ( RepeatUntilStatement node ) => throw new NotImplementedException ( );

        public virtual void VisitReturn ( ReturnStatement node ) => throw new NotImplementedException ( );

        public virtual void VisitStatementList ( StatementList node ) => throw new NotImplementedException ( );

        public virtual void VisitString ( StringExpression node ) => throw new NotImplementedException ( );

        public virtual void VisitTableConstructor ( TableConstructorExpression node ) => throw new NotImplementedException ( );

        public virtual void VisitTableField ( TableField node ) => throw new NotImplementedException ( );

        public virtual void VisitUnaryOperation ( UnaryOperationExpression node ) => throw new NotImplementedException ( );

        public virtual void VisitVarArg ( VarArgExpression node ) => throw new NotImplementedException ( );

        public virtual void VisitWhileLoop ( WhileLoopStatement node ) => throw new NotImplementedException ( );
    }
}