using System;
using Loretta.Env;
using Loretta.Parsing.Nodes;
using Loretta.Parsing.Nodes.Constants;
using Loretta.Parsing.Nodes.ControlStatements;
using Loretta.Parsing.Nodes.Functions;
using Loretta.Parsing.Nodes.IfStatements;
using Loretta.Parsing.Nodes.Loops;
using Loretta.Parsing.Nodes.Operators;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Analysis
{
    public abstract class BaseASTAnalyser
    {
        public LuaEnvironment Environment { get; internal set; }

        public EnvFile File { get; internal set; }

        public virtual Object[] Analyse ( ASTNode node, params Object[] args )
        {
            // Protection. JustInCase™
            if ( node == null )
                return null;

            this.AnalyseNode ( node, args );
            return this.InternalNodeAnalyseFunc ( node, args );
        }

        protected Object[] InternalNodeAnalyseFunc ( ASTNode node, params Object[] args )
        {
            #region Regex Generated Mess
            if ( node is DoStatement )
                return this.AnalyseDoStatement ( ( DoStatement ) node, args );
            else if ( node is StatementList )
                return this.AnalyseStatementList ( ( StatementList ) node, args );
            else if ( node is AssignmentStatement )
                return this.AnalyseAssignmentStatement ( ( AssignmentStatement ) node, args );
            else if ( node is LocalVariableStatement )
                return this.AnalyseLocalVariableStatement ( ( LocalVariableStatement ) node, args );
            else if ( node is VariableExpression )
                return this.AnalyseVariableExpression ( ( VariableExpression ) node, args );
            else if ( node is BinaryOperatorExpression )
                return this.AnalyseBinaryOperatorExpression ( ( BinaryOperatorExpression ) node, args );
            else if ( node is UnaryOperatorExpression )
                return this.AnalyseUnaryOperatorExpression ( ( UnaryOperatorExpression ) node, args );
            else if ( node is ForGenericStatement )
                return this.AnalyseForGenericStatement ( ( ForGenericStatement ) node, args );
            else if ( node is ForNumericStatement )
                return this.AnalyseForNumericStatement ( ( ForNumericStatement ) node, args );
            else if ( node is RepeatStatement )
                return this.AnalyseRepeatStatement ( ( RepeatStatement ) node, args );
            else if ( node is WhileStatement )
                return this.AnalyseWhileStatement ( ( WhileStatement ) node, args );
            else if ( node is Parsing.Nodes.Indexers.IndexExpression )
                return this.AnalyseIndexExpression ( ( Parsing.Nodes.Indexers.IndexExpression ) node, args );
            else if ( node is Parsing.Nodes.Indexers.MemberExpression )
                return this.AnalyseMemberExpression ( ( Parsing.Nodes.Indexers.MemberExpression ) node, args );
            else if ( node is IfClause )
                return this.AnalyseIfClause ( ( IfClause ) node, args );
            else if ( node is IfStatement )
                return this.AnalyseIfStatement ( ( IfStatement ) node, args );
            else if ( node is AnonymousFunctionExpression )
                return this.AnalyseAnonymousFunctionExpression ( ( AnonymousFunctionExpression ) node, args );
            else if ( node is FunctionCallExpression )
                return this.AnalyseFunctionCallExpression ( ( FunctionCallExpression ) node, args );
            else if ( node is LocalFunctionStatement )
                return this.AnalyseLocalFunctionStatement ( ( LocalFunctionStatement ) node, args );
            else if ( node is NamedFunctionStatement )
                return this.AnalyseNamedFunctionStatement ( ( NamedFunctionStatement ) node, args );
            else if ( node is StringFunctionCallExpression )
                return this.AnalyseStringFunctionCallExpression ( ( StringFunctionCallExpression ) node, args );
            else if ( node is TableFunctionCallExpression )
                return this.AnalyseTableFunctionCallExpression ( ( TableFunctionCallExpression ) node, args );
            else if ( node is BreakStatement )
                return this.AnalyseBreakStatement ( ( BreakStatement ) node, args );
            else if ( node is ContinueStatement )
                return this.AnalyseContinueStatement ( ( ContinueStatement ) node, args );
            else if ( node is GotoStatement )
                return this.AnalyseGotoStatement ( ( GotoStatement ) node, args );
            else if ( node is GotoLabelStatement )
                return this.AnalyseGotoLabelStatement ( ( GotoLabelStatement ) node, args );
            else if ( node is ReturnStatement )
                return this.AnalyseReturnStatement ( ( ReturnStatement ) node, args );
            else if ( node is BooleanExpression )
                return this.AnalyseBooleanExpression ( ( BooleanExpression ) node, args );
            else if ( node is Eof )
                return this.AnalyseEof ( ( Eof ) node, args );
            else if ( node is NilExpression )
                return this.AnalyseNilExpression ( ( NilExpression ) node, args );
            else if ( node is NumberExpression )
                return this.AnalyseNumberExpression ( ( NumberExpression ) node, args );
            else if ( node is ParenthesisExpression )
                return this.AnalyseParenthesisExpression ( ( ParenthesisExpression ) node, args );
            else if ( node is StringExpression )
                return this.AnalyseStringExpression ( ( StringExpression ) node, args );
            else if ( node is TableConstructorExpression )
                return this.AnalyseTableConstructorExpression ( ( TableConstructorExpression ) node, args );
            else if ( node is TableKeyValue )
                return this.AnalyseTableKeyValue ( ( TableKeyValue ) node, args );
            else if ( node is VarArgExpression )
                return this.AnalyseVarArgExpression ( ( VarArgExpression ) node, args );
            #endregion Regex Generated Mess
            else
                throw new Exception ( "Unknown node type: " + node );
        }

        /// <summary>
        /// Called for all nodes
        /// </summary>
        /// <param name="node"></param>
        /// <param name="args"></param>
        protected virtual Object[] AnalyseNode ( ASTNode node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] AnalyseDoStatement ( DoStatement node, params Object[] args )
        {
            this.Analyse ( node.Body );
            return null;
        }

        protected virtual Object[] AnalyseStatementList ( StatementList node, params Object[] args )
        {
            foreach ( ASTNode statement in node.Statements )
                this.Analyse ( statement );
            return null;
        }

        protected virtual Object[] AnalyseAssignmentStatement ( AssignmentStatement node, params Object[] args )
        {
            foreach ( ASTNode var in node.Variables )
                this.Analyse ( var );
            foreach ( ASTNode val in node.Assignments )
                this.Analyse ( val );
            return null;
        }

        protected virtual Object[] AnalyseLocalVariableStatement ( LocalVariableStatement node, params Object[] args )
        {
            foreach ( ASTNode var in node.Variables )
                this.Analyse ( var );
            foreach ( ASTNode val in node.Assignments )
                this.Analyse ( val );
            return null;
        }

        protected virtual Object[] AnalyseVariableExpression ( VariableExpression node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] AnalyseBinaryOperatorExpression ( BinaryOperatorExpression node, params Object[] args )
        {
            this.Analyse ( node.LeftOperand );
            this.Analyse ( node.RightOperand );
            return null;
        }

        protected virtual Object[] AnalyseUnaryOperatorExpression ( UnaryOperatorExpression node, params Object[] args )
        {
            this.Analyse ( node.Operand );
            return null;
        }

        protected virtual Object[] AnalyseForGenericStatement ( ForGenericStatement node, params Object[] args )
        {
            foreach ( VariableExpression var in node.Variables )
                this.Analyse ( var );

            foreach ( ASTNode gen in node.Generators )
                this.Analyse ( gen );

            this.Analyse ( node.Body );

            return null;
        }

        protected virtual Object[] AnalyseForNumericStatement ( ForNumericStatement node, params Object[] args )
        {
            this.Analyse ( node.InitialExpression );
            this.Analyse ( node.FinalExpression );
            this.Analyse ( node.IncrementExpression );
            this.Analyse ( node.Body );
            return null;
        }

        protected virtual Object[] AnalyseRepeatStatement ( RepeatStatement node, params Object[] args )
        {
            this.Analyse ( node.Body );
            this.Analyse ( node.Condition );
            return null;
        }

        protected virtual Object[] AnalyseWhileStatement ( WhileStatement node, params Object[] args )
        {
            this.Analyse ( node.Body );
            this.Analyse ( node.Condition );
            return null;
        }

        protected virtual Object[] AnalyseIndexExpression ( Parsing.Nodes.Indexers.IndexExpression node, params Object[] args )
        {
            this.Analyse ( node.Base );
            this.Analyse ( node.Indexer );
            return null;
        }

        protected virtual Object[] AnalyseMemberExpression ( Parsing.Nodes.Indexers.MemberExpression node, params Object[] args )
        {
            this.Analyse ( node.Base );
            return null;
        }

        protected virtual Object[] AnalyseIfClause ( IfClause node, params Object[] args )
        {
            this.Analyse ( node.Condition );
            this.Analyse ( node.Body );
            return null;
        }

        protected virtual Object[] AnalyseIfStatement ( IfStatement node, params Object[] args )
        {
            this.Analyse ( node.MainClause );
            foreach ( IfClause elif in node.ElseIfClauses )
                this.Analyse ( elif );
            this.Analyse ( node.ElseBlock );
            return null;
        }

        protected virtual Object[] AnalyseAnonymousFunctionExpression ( AnonymousFunctionExpression node, params Object[] args )
        {
            foreach ( ASTNode arg in node.Arguments )
                this.Analyse ( arg );
            this.Analyse ( node.Body );
            return null;
        }

        protected virtual Object[] AnalyseFunctionCallExpression ( FunctionCallExpression node, params Object[] args )
        {
            this.Analyse ( node.Base );
            foreach ( ASTNode arg in node.Arguments )
                this.Analyse ( arg );
            return null;
        }

        protected virtual Object[] AnalyseLocalFunctionStatement ( LocalFunctionStatement node, params Object[] args )
        {
            this.Analyse ( node.Identifier );
            foreach ( ASTNode arg in node.Arguments )
                this.Analyse ( arg );
            this.Analyse ( node.Body );
            return null;
        }

        protected virtual Object[] AnalyseNamedFunctionStatement ( NamedFunctionStatement node, params Object[] args )
        {
            this.Analyse ( node.Identifier );
            foreach ( ASTNode arg in node.Arguments )
                this.Analyse ( arg );
            this.Analyse ( node.Body );
            return null;
        }

        protected virtual Object[] AnalyseStringFunctionCallExpression ( StringFunctionCallExpression node, params Object[] args )
        {
            this.Analyse ( node.Base );
            this.Analyse ( node.Argument );
            return null;
        }

        protected virtual Object[] AnalyseTableFunctionCallExpression ( TableFunctionCallExpression node, params Object[] args )
        {
            this.Analyse ( node.Base );
            this.Analyse ( node.Argument );
            return null;
        }

        protected virtual Object[] AnalyseBreakStatement ( BreakStatement node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] AnalyseContinueStatement ( ContinueStatement node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] AnalyseGotoStatement ( GotoStatement node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] AnalyseGotoLabelStatement ( GotoLabelStatement node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] AnalyseReturnStatement ( ReturnStatement node, params Object[] args )
        {
            foreach ( ASTNode ret in node.Returns )
                this.Analyse ( ret );
            return null;
        }

        protected virtual Object[] AnalyseBooleanExpression ( BooleanExpression node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] AnalyseEof ( Eof node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] AnalyseNilExpression ( NilExpression node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] AnalyseNumberExpression ( NumberExpression node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] AnalyseParenthesisExpression ( ParenthesisExpression node, params Object[] args )
        {
            this.Analyse ( node.Expression );
            return null;
        }

        protected virtual Object[] AnalyseStringExpression ( StringExpression node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] AnalyseTableConstructorExpression ( TableConstructorExpression node, params Object[] args )
        {
            foreach ( TableKeyValue field in node.Fields )
                this.Analyse ( field );
            return null;
        }

        protected virtual Object[] AnalyseTableKeyValue ( TableKeyValue node, params Object[] args )
        {
            if ( !node.IsSequential )
                this.Analyse ( node.Key );
            this.Analyse ( node.Value );
            return null;
        }

        protected virtual Object[] AnalyseVarArgExpression ( VarArgExpression node, params Object[] args )
        {
            return null;
        }
    }
}
