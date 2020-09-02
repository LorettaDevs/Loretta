using Loretta.Parsing.AST;
using Loretta.Parsing.AST.Tables;

namespace Loretta.Parsing.Visitor
{
    /// <summary>
    /// An interface for classes that implement the visitor pattern.
    /// </summary>
    public interface ITreeVisitor
    {
        /// <summary>
        /// Visits a nil literal expression.
        /// </summary>
        /// <param name="node">The nil expression to visit.</param>
        void VisitNil ( NilExpression node );

        /// <summary>
        /// Visits a boolean literal expression.
        /// </summary>
        /// <param name="node">The boolean literal expression to visit.</param>
        void VisitBoolean ( BooleanExpression node );

        /// <summary>
        /// Visits an identifier expression.
        /// </summary>
        /// <param name="node">The identifier expression to visit.</param>
        void VisitIdentifier ( IdentifierExpression node );

        /// <summary>
        /// Visits an indexing operation expression.
        /// </summary>
        /// <param name="node">The indexing operation to visit.</param>
        void VisitIndex ( IndexExpression node );

        /// <summary>
        /// Visits a number literal expression.
        /// </summary>
        /// <param name="node">The number literal expression to visit.</param>
        void VisitNumber ( NumberExpression node );

        /// <summary>
        /// Visits a string literal expression.
        /// </summary>
        /// <param name="node">The string literal expression to visit.</param>
        void VisitString ( StringExpression node );

        /// <summary>
        /// Visits a vararg expression.
        /// </summary>
        /// <param name="node">The vararg expression to visit.</param>
        void VisitVarArg ( VarArgExpression node );

        /// <summary>
        /// Visits a function call expression.
        /// </summary>
        /// <param name="node">The function call expression.</param>
        void VisitFunctionCall ( FunctionCallExpression node );

        /// <summary>
        /// Visits an unary operation expression.
        /// </summary>
        /// <param name="node">The unary operation expression to visit.</param>
        void VisitUnaryOperation ( UnaryOperationExpression node );

        /// <summary>
        /// Visits a grouped expression.
        /// </summary>
        /// <param name="node">The grouped expression to visit.</param>
        void VisitGroupedExpression ( GroupedExpression node );

        /// <summary>
        /// Visits a binary operation expression.
        /// </summary>
        /// <param name="node">The binary operation expression to visit.</param>
        void VisitBinaryOperation ( BinaryOperationExpression node );

        /// <summary>
        /// Visits a table field.
        /// </summary>
        /// <param name="node">The table field to visit.</param>
        void VisitTableField ( TableField node );

        /// <summary>
        /// Visits a table constructor expression.
        /// </summary>
        /// <param name="node">The table constructor expression to visit.</param>
        void VisitTableConstructor ( TableConstructorExpression node );

        /// <summary>
        /// Visits an anonymous function expression.
        /// </summary>
        /// <param name="node">The anonymous function expression to visit.</param>
        void VisitAnonymousFunction ( AnonymousFunctionExpression node );

        /// <summary>
        /// Visits an assignment statement.
        /// </summary>
        /// <param name="node">The assignment statement to visit.</param>
        void VisitAssignment ( AssignmentStatement node );

        /// <summary>
        /// Visits a compound assignment statement.
        /// </summary>
        /// <param name="compoundAssignmentStatement">The compound assignment statement to visit.</param>
        void VisitCompoundAssignmentStatement ( CompoundAssignmentStatement compoundAssignmentStatement );

        /// <summary>
        /// Visits a break statement.
        /// </summary>
        /// <param name="node">The break statement to visit.</param>
        void VisitBreak ( BreakStatement node );

        /// <summary>
        /// Visits a continue statement.
        /// </summary>
        /// <param name="node">The continue statement to visit.</param>
        void VisitContinue ( ContinueStatement node );

        /// <summary>
        /// Visits a do statement.
        /// </summary>
        /// <param name="node">The do statement to visit.</param>
        void VisitDo ( DoStatement node );

        /// <summary>
        /// Visits an expression statement node.
        /// </summary>
        /// <param name="node">The expression statement node to visit.</param>
        void VisitExpressionStatement ( ExpressionStatement node );

        /// <summary>
        /// Visits a function definition statement.
        /// </summary>
        /// <param name="node">The function definition statement to visit.</param>
        void VisitFunctionDefinition ( FunctionDefinitionStatement node );

        /// <summary>
        /// Visits a goto label.
        /// </summary>
        /// <param name="node">The goto label to visit.</param>
        void VisitGotoLabel ( GotoLabelStatement node );

        /// <summary>
        /// Visits a goto statement.
        /// </summary>
        /// <param name="node">The goto statement to visit.</param>
        void VisitGoto ( GotoStatement node );

        /// <summary>
        /// Visits an if statement.
        /// </summary>
        /// <param name="node">The if statement to visit.</param>
        void VisitIfStatement ( IfStatement node );

        /// <summary>
        /// Visits a generic for loop statement.
        /// </summary>
        /// <param name="node">The generic for loop statement to visit.</param>
        void VisitGenericFor ( GenericForLoopStatement node );

        /// <summary>
        /// Visits a local variable declaration.
        /// </summary>
        /// <param name="node">The local variable declaration to visit.</param>
        void VisitLocalVariableDeclaration ( LocalVariableDeclarationStatement node );

        /// <summary>
        /// Visits an AST node.
        /// </summary>
        /// <param name="node">The AST node to visit.</param>
        void VisitNode ( LuaASTNode node );

        /// <summary>
        /// Visits a numeric for statement.
        /// </summary>
        /// <param name="node">The numeric for statement to visit.</param>
        void VisitNumericFor ( NumericForLoopStatement node );

        /// <summary>
        /// Visits a repeat until statement.
        /// </summary>
        /// <param name="node">The repeat until statement to visit.</param>
        void VisitRepeatUntil ( RepeatUntilStatement node );

        /// <summary>
        /// Visits a return statement.
        /// </summary>
        /// <param name="node">The return statement to visit.</param>
        void VisitReturn ( ReturnStatement node );

        /// <summary>
        /// Visits a statement list.
        /// </summary>
        /// <param name="node">The statement list to visit.</param>
        void VisitStatementList ( StatementList node );

        /// <summary>
        /// Visits a while loop statement.
        /// </summary>
        /// <param name="node">The while loop statement to visit.</param>
        void VisitWhileLoop ( WhileLoopStatement node );

        /// <summary>
        /// Visits an empty statement.
        /// </summary>
        /// <param name="emptyStatement">The empty statement to visit.</param>
        void VisitEmptyStatement ( EmptyStatement emptyStatement );
    }

    /// <summary>
    /// The interface for classes that implement the visitor pattern with a return value.
    /// </summary>
    /// <typeparam name="T">The value returned by the visitor methods.</typeparam>
    public interface ITreeVisitor<T>
    {
        /// <summary>
        /// Visits a nil literal expression.
        /// </summary>
        /// <param name="node">The nil expression to visit.</param>
        /// <returns>The result of visiting this nil literal expression.</returns>
        T VisitNil ( NilExpression node );

        /// <summary>
        /// Visits a boolean literal expression.
        /// </summary>
        /// <param name="node">The boolean literal expression to visit.</param>
        /// <returns>The result of visiting this boolean literal expression.</returns>
        T VisitBoolean ( BooleanExpression node );

        /// <summary>
        /// Visits an identifier expression.
        /// </summary>
        /// <param name="node">The identifier expression to visit.</param>
        /// <returns>The result of visiting this identifier expression.</returns>
        T VisitIdentifier ( IdentifierExpression node );

        /// <summary>
        /// Visits an indexing operation expression.
        /// </summary>
        /// <param name="node">The indexing operation expression to visit.</param>
        /// <returns>The result of visiting this indexing operation expression.</returns>
        T VisitIndex ( IndexExpression node );

        /// <summary>
        /// Visits a number literal expression.
        /// </summary>
        /// <param name="node">The number literal expression to visit.</param>
        /// <returns>The result of visiting this number literal expression.</returns>
        T VisitNumber ( NumberExpression node );

        /// <summary>
        /// Visits a string literal expression.
        /// </summary>
        /// <param name="node">The string literal expression to visit.</param>
        /// <returns>The result of visiting this string literal expression.</returns>
        T VisitString ( StringExpression node );

        /// <summary>
        /// Visits a vararg expression.
        /// </summary>
        /// <param name="node">The vararg expression to visit.</param>
        /// <returns>The result of visiting this vararg expression.</returns>
        T VisitVarArg ( VarArgExpression node );

        /// <summary>
        /// Visits a function call expression.
        /// </summary>
        /// <param name="node">The function call expression.</param>
        /// <returns>The result of visiting this function call expression.</returns>
        T VisitFunctionCall ( FunctionCallExpression node );

        /// <summary>
        /// Visits an unary operation expression.
        /// </summary>
        /// <param name="node">The unary operation expression to visit.</param>
        /// <returns>The result of visiting this unary operation expression.</returns>
        T VisitUnaryOperation ( UnaryOperationExpression node );

        /// <summary>
        /// Visits a grouped expression.
        /// </summary>
        /// <param name="node">The grouped expression to visit.</param>
        /// <returns>The result of visiting this grouped expression.</returns>
        T VisitGroupedExpression ( GroupedExpression node );

        /// <summary>
        /// Visits a binary operation expression.
        /// </summary>
        /// <param name="node">The binary operation expression to visit.</param>
        /// <returns>The result of visiting this binary operation expression.</returns>
        T VisitBinaryOperation ( BinaryOperationExpression node );

        /// <summary>
        /// Visits a table field.
        /// </summary>
        /// <param name="node">The table field to visit.</param>
        /// <returns>The result of visiting this table field.</returns>
        T VisitTableField ( TableField node );

        /// <summary>
        /// Visits a table constructor expression.
        /// </summary>
        /// <param name="node">The table constructor expression to visit.</param>
        /// <returns>The result of visiting this table constructor expression.</returns>
        T VisitTableConstructor ( TableConstructorExpression node );

        /// <summary>
        /// Visits an anonymous function expression.
        /// </summary>
        /// <param name="node">The anonymous function expression to visit.</param>
        /// <returns>The result of visiting this anonymous function expression.</returns>
        T VisitAnonymousFunction ( AnonymousFunctionExpression node );

        /// <summary>
        /// Visits an assignment statement.
        /// </summary>
        /// <param name="node">The assignment statement to visit.</param>
        /// <returns>The result of visiting this assignment statement.</returns>
        T VisitAssignment ( AssignmentStatement node );

        /// <summary>
        /// Visits a compound assignment statement.
        /// </summary>
        /// <param name="compoundAssignmentStatement">The compound assignment statement to visit.</param>
        /// <returns>The result of visiting this compound assignment statement.</returns>
        T VisitCompoundAssignmentStatement ( CompoundAssignmentStatement compoundAssignmentStatement );

        /// <summary>
        /// Visits a break statement.
        /// </summary>
        /// <param name="node">The break statement to visit.</param>
        /// <returns>The result of visiting this break statement.</returns>
        T VisitBreak ( BreakStatement node );

        /// <summary>
        /// Visits a continue statement.
        /// </summary>
        /// <param name="node">The continue statement to visit.</param>
        /// <returns>The result of visiting this continue statement.</returns>
        T VisitContinue ( ContinueStatement node );

        /// <summary>
        /// Visits a do statement.
        /// </summary>
        /// <param name="node">The do statement to visit.</param>
        /// <returns>The result of visiting this do statement.</returns>
        T VisitDo ( DoStatement node );

        /// <summary>
        /// Visits an expression statement node.
        /// </summary>
        /// <param name="node">The expression statement node to visit.</param>
        /// <returns>The result of visiting this expression statement.</returns>
        T VisitExpressionStatement ( ExpressionStatement node );

        /// <summary>
        /// Visits a function definition statement.
        /// </summary>
        /// <param name="node">The function definition statement to visit.</param>
        /// <returns>The result of visiting this function definition statement.</returns>
        T VisitFunctionDefinition ( FunctionDefinitionStatement node );

        /// <summary>
        /// Visits a goto label.
        /// </summary>
        /// <param name="node">The goto label to visit.</param>
        /// <returns>The result of visiting this goto label.</returns>
        T VisitGotoLabel ( GotoLabelStatement node );

        /// <summary>
        /// Visits a goto statement.
        /// </summary>
        /// <param name="node">The goto statement to visit.</param>
        /// <returns>The result of visiting this goto statement.</returns>
        T VisitGoto ( GotoStatement node );

        /// <summary>
        /// Visits an if statement.
        /// </summary>
        /// <param name="node">The if statement to visit.</param>
        /// <returns>The result of visiting this if statement.</returns>
        T VisitIfStatement ( IfStatement node );

        /// <summary>
        /// Visits a generic for loop statement.
        /// </summary>
        /// <param name="node">The generic for loop statement to visit.</param>
        /// <returns>The result of visiting this generic for loop statement.</returns>
        T VisitGenericFor ( GenericForLoopStatement node );

        /// <summary>
        /// Visits a local variable declaration.
        /// </summary>
        /// <param name="node">The local variable declaration to visit.</param>
        /// <returns>The result of visiting this local variable declaration statement.</returns>
        T VisitLocalVariableDeclaration ( LocalVariableDeclarationStatement node );

        /// <summary>
        /// Visits an AST node.
        /// </summary>
        /// <param name="node">The AST node to visit.</param>
        /// <returns>The result of visiting this AST node.</returns>
        T VisitNode ( LuaASTNode node );

        /// <summary>
        /// Visits a numeric for loop statement.
        /// </summary>
        /// <param name="node">The numeric for statement to visit.</param>
        /// <returns>The result of visiting this numeric for loop statement.</returns>
        T VisitNumericFor ( NumericForLoopStatement node );

        /// <summary>
        /// Visits a repeat until statement.
        /// </summary>
        /// <param name="node">The repeat until statement to visit.</param>
        T VisitRepeatUntil ( RepeatUntilStatement node );

        /// <summary>
        /// Visits a return statement.
        /// </summary>
        /// <param name="node">The return statement to visit.</param>
        T VisitReturn ( ReturnStatement node );

        /// <summary>
        /// Visits a statement list.
        /// </summary>
        /// <param name="node">The statement list to visit.</param>
        T VisitStatementList ( StatementList node );

        /// <summary>
        /// Visits a while loop statement.
        /// </summary>
        /// <param name="node">The while loop statement to visit.</param>
        T VisitWhileLoop ( WhileLoopStatement node );

        /// <summary>
        /// Visits an empty statement.
        /// </summary>
        /// <param name="emptyStatement">The empty statement to visit.</param>
        T VisitEmptyStatement ( EmptyStatement emptyStatement );
    }
}