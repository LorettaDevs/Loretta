using Loretta.Parsing.AST;
using Loretta.Parsing.AST.Tables;

namespace Loretta.Parsing.Visitor
{
    public interface ITreeVisitor
    {
        void VisitNil ( NilExpression node );

        void VisitBoolean ( BooleanExpression node );

        void VisitIdentifier ( IdentifierExpression node );

        void VisitIndex ( IndexExpression node );

        void VisitNumber ( NumberExpression node );

        void VisitString ( StringExpression node );

        void VisitVarArg ( VarArgExpression node );

        void VisitFunctionCall ( FunctionCallExpression node );

        void VisitUnaryOperation ( UnaryOperationExpression node );

        void VisitGroupedExpression ( GroupedExpression node );

        void VisitBinaryOperation ( BinaryOperationExpression node );

        void VisitTableField ( TableField node );

        void VisitTableConstructor ( TableConstructorExpression node );

        void VisitAnonymousFunction ( AnonymousFunctionExpression node );

        void VisitAssignment ( AssignmentStatement node );

        void VisitCompoundAssignmentStatement ( CompoundAssignmentStatement compoundAssignmentStatement );

        void VisitBreak ( BreakStatement node );

        void VisitContinue ( ContinueStatement node );

        void VisitDo ( DoStatement node );

        void VisitExpressionStatement ( ExpressionStatement node );

        void VisitFunctionDefinition ( FunctionDefinitionStatement node );

        void VisitGotoLabel ( GotoLabelStatement node );

        void VisitGoto ( GotoStatement node );

        void VisitIfStatement ( IfStatement node );

        void VisitGenericFor ( GenericForLoopStatement node );

        void VisitLocalVariableDeclaration ( LocalVariableDeclarationStatement node );

        void VisitNode ( LuaASTNode node );

        void VisitNumericFor ( NumericForLoopStatement node );

        void VisitRepeatUntil ( RepeatUntilStatement node );

        void VisitReturn ( ReturnStatement node );

        void VisitStatementList ( StatementList node );

        void VisitWhileLoop ( WhileLoopStatement node );

        void VisitEmptyStatement ( EmptyStatement emptyStatement );
    }

    public interface ITreeVisitor<T>
    {
        T VisitNil ( NilExpression node );

        T VisitBoolean ( BooleanExpression node );

        T VisitIdentifier ( IdentifierExpression node );

        T VisitIndex ( IndexExpression node );

        T VisitNumber ( NumberExpression node );

        T VisitString ( StringExpression node );

        T VisitVarArg ( VarArgExpression node );

        T VisitFunctionCall ( FunctionCallExpression node );

        T VisitUnaryOperation ( UnaryOperationExpression node );

        T VisitGroupedExpression ( GroupedExpression node );

        T VisitBinaryOperation ( BinaryOperationExpression node );

        T VisitTableField ( TableField node );

        T VisitTableConstructor ( TableConstructorExpression node );

        T VisitAnonymousFunction ( AnonymousFunctionExpression node );

        T VisitAssignment ( AssignmentStatement node );

        T VisitCompoundAssignmentStatement ( CompoundAssignmentStatement compoundAssignmentStatement );

        T VisitBreak ( BreakStatement node );

        T VisitContinue ( ContinueStatement node );

        T VisitDo ( DoStatement node );

        T VisitExpressionStatement ( ExpressionStatement node );

        T VisitFunctionDefinition ( FunctionDefinitionStatement node );

        T VisitGotoLabel ( GotoLabelStatement node );

        T VisitGoto ( GotoStatement node );

        T VisitIfStatement ( IfStatement node );

        T VisitGenericFor ( GenericForLoopStatement node );

        T VisitLocalVariableDeclaration ( LocalVariableDeclarationStatement node );

        T VisitNode ( LuaASTNode node );

        T VisitNumericFor ( NumericForLoopStatement node );

        T VisitRepeatUntil ( RepeatUntilStatement node );

        T VisitReturn ( ReturnStatement node );

        T VisitStatementList ( StatementList node );

        T VisitWhileLoop ( WhileLoopStatement node );

        T VisitEmptyStatement ( EmptyStatement emptyStatement );
    }
}