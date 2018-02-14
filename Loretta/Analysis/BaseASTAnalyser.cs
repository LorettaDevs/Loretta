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
        public LuaEnvironment Environment { get; }

        public EnvFile File { get; }

        protected BaseASTAnalyser ( LuaEnvironment env, EnvFile file )
        {
            this.Environment = env;
            this.File = file;
        }

        protected virtual Object[] Analyse ( ASTNode node, params Object[] args )
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
                return this.DoStatement ( ( DoStatement ) node, args );
            else if ( node is StatementList )
                return this.StatementList ( ( StatementList ) node, args );
            else if ( node is AssignmentStatement )
                return this.AssignmentStatement ( ( AssignmentStatement ) node, args );
            else if ( node is LocalVariableStatement )
                return this.LocalVariableStatement ( ( LocalVariableStatement ) node, args );
            else if ( node is VariableExpression )
                return this.VariableExpression ( ( VariableExpression ) node, args );
            else if ( node is BinaryOperatorExpression )
                return this.BinaryOperatorExpression ( ( BinaryOperatorExpression ) node, args );
            else if ( node is UnaryOperatorExpression )
                return this.UnaryOperatorExpression ( ( UnaryOperatorExpression ) node, args );
            else if ( node is ForGenericStatement )
                return this.ForGenericStatement ( ( ForGenericStatement ) node, args );
            else if ( node is ForNumericStatement )
                return this.ForNumericStatement ( ( ForNumericStatement ) node, args );
            else if ( node is RepeatStatement )
                return this.RepeatStatement ( ( RepeatStatement ) node, args );
            else if ( node is WhileStatement )
                return this.WhileStatement ( ( WhileStatement ) node, args );
            else if ( node is Parsing.Nodes.Indexers.IndexExpression )
                return this.IndexExpression ( ( Parsing.Nodes.Indexers.IndexExpression ) node, args );
            else if ( node is Parsing.Nodes.Indexers.MemberExpression )
                return this.MemberExpression ( ( Parsing.Nodes.Indexers.MemberExpression ) node, args );
            else if ( node is IfClause )
                return this.IfClause ( ( IfClause ) node, args );
            else if ( node is IfStatement )
                return this.IfStatement ( ( IfStatement ) node, args );
            else if ( node is AnonymousFunctionExpression )
                return this.AnonymousFunctionExpression ( ( AnonymousFunctionExpression ) node, args );
            else if ( node is FunctionCallExpression )
                return this.FunctionCallExpression ( ( FunctionCallExpression ) node, args );
            else if ( node is LocalFunctionStatement )
                return this.LocalFunctionStatement ( ( LocalFunctionStatement ) node, args );
            else if ( node is NamedFunctionStatement )
                return this.NamedFunctionStatement ( ( NamedFunctionStatement ) node, args );
            else if ( node is StringFunctionCallExpression )
                return this.StringFunctionCallExpression ( ( StringFunctionCallExpression ) node, args );
            else if ( node is TableFunctionCallExpression )
                return this.TableFunctionCallExpression ( ( TableFunctionCallExpression ) node, args );
            else if ( node is BreakStatement )
                return this.BreakStatement ( ( BreakStatement ) node, args );
            else if ( node is ContinueStatement )
                return this.ContinueStatement ( ( ContinueStatement ) node, args );
            else if ( node is GotoStatement )
                return this.GotoStatement ( ( GotoStatement ) node, args );
            else if ( node is GotoLabelStatement )
                return this.GotoLabelStatement ( ( GotoLabelStatement ) node, args );
            else if ( node is ReturnStatement )
                return this.ReturnStatement ( ( ReturnStatement ) node, args );
            else if ( node is BooleanExpression )
                return this.BooleanExpression ( ( BooleanExpression ) node, args );
            else if ( node is Eof )
                return this.Eof ( ( Eof ) node, args );
            else if ( node is NilExpression )
                return this.NilExpression ( ( NilExpression ) node, args );
            else if ( node is NumberExpression )
                return this.NumberExpression ( ( NumberExpression ) node, args );
            else if ( node is ParenthesisExpression )
                return this.ParenthesisExpression ( ( ParenthesisExpression ) node, args );
            else if ( node is StringExpression )
                return this.StringExpression ( ( StringExpression ) node, args );
            else if ( node is TableConstructorExpression )
                return this.TableConstructorExpression ( ( TableConstructorExpression ) node, args );
            else if ( node is TableKeyValue )
                return this.TableKeyValue ( ( TableKeyValue ) node, args );
            else if ( node is VarArgExpression )
                return this.VarArgExpression ( ( VarArgExpression ) node, args );
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

        protected virtual Object[] DoStatement ( DoStatement node, params Object[] args )
        {
            this.Analyse ( node.Body );
            return null;
        }

        protected virtual Object[] StatementList ( StatementList node, params Object[] args )
        {
            foreach ( ASTNode statement in node.Statements )
                this.Analyse ( statement );
            return null;
        }

        protected virtual Object[] AssignmentStatement ( AssignmentStatement node, params Object[] args )
        {
            foreach ( ASTNode var in node.Variables )
                this.Analyse ( var );
            foreach ( ASTNode val in node.Assignments )
                this.Analyse ( val );
            return null;
        }

        protected virtual Object[] LocalVariableStatement ( LocalVariableStatement node, params Object[] args )
        {
            foreach ( ASTNode var in node.Variables )
                this.Analyse ( var );
            foreach ( ASTNode val in node.Assignments )
                this.Analyse ( val );
            return null;
        }

        protected virtual Object[] VariableExpression ( VariableExpression node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] BinaryOperatorExpression ( BinaryOperatorExpression node, params Object[] args )
        {
            this.Analyse ( node.LeftOperand );
            this.Analyse ( node.RightOperand );
            return null;
        }

        protected virtual Object[] UnaryOperatorExpression ( UnaryOperatorExpression node, params Object[] args )
        {
            this.Analyse ( node.Operand );
            return null;
        }

        protected virtual Object[] ForGenericStatement ( ForGenericStatement node, params Object[] args )
        {
            foreach ( VariableExpression var in node.Variables )
                this.Analyse ( var );

            foreach ( ASTNode gen in node.Generators )
                this.Analyse ( gen );

            this.Analyse ( node.Body );

            return null;
        }

        protected virtual Object[] ForNumericStatement ( ForNumericStatement node, params Object[] args )
        {
            this.Analyse ( node.InitialExpression );
            this.Analyse ( node.FinalExpression );
            this.Analyse ( node.IncrementExpression );
            this.Analyse ( node.Body );
            return null;
        }

        protected virtual Object[] RepeatStatement ( RepeatStatement node, params Object[] args )
        {
            this.Analyse ( node.Body );
            this.Analyse ( node.Condition );
            return null;
        }

        protected virtual Object[] WhileStatement ( WhileStatement node, params Object[] args )
        {
            this.Analyse ( node.Body );
            this.Analyse ( node.Condition );
            return null;
        }

        protected virtual Object[] IndexExpression ( Parsing.Nodes.Indexers.IndexExpression node, params Object[] args )
        {
            this.Analyse ( node.Base );
            this.Analyse ( node.Indexer );
            return null;
        }

        protected virtual Object[] MemberExpression ( Parsing.Nodes.Indexers.MemberExpression node, params Object[] args )
        {
            this.Analyse ( node.Base );
            return null;
        }

        protected virtual Object[] IfClause ( IfClause node, params Object[] args )
        {
            this.Analyse ( node.Condition );
            this.Analyse ( node.Body );
            return null;
        }

        protected virtual Object[] IfStatement ( IfStatement node, params Object[] args )
        {
            this.Analyse ( node.MainClause );
            foreach ( IfClause elif in node.ElseIfClauses )
                this.Analyse ( elif );
            this.Analyse ( node.ElseBlock );
            return null;
        }

        protected virtual Object[] AnonymousFunctionExpression ( AnonymousFunctionExpression node, params Object[] args )
        {
            foreach ( ASTNode arg in node.Arguments )
                this.Analyse ( arg );
            this.Analyse ( node.Body );
            return null;
        }

        protected virtual Object[] FunctionCallExpression ( FunctionCallExpression node, params Object[] args )
        {
            this.Analyse ( node.Base );
            foreach ( ASTNode arg in node.Arguments )
                this.Analyse ( arg );
            return null;
        }

        protected virtual Object[] LocalFunctionStatement ( LocalFunctionStatement node, params Object[] args )
        {
            this.Analyse ( node.Identifier );
            foreach ( ASTNode arg in node.Arguments )
                this.Analyse ( arg );
            this.Analyse ( node.Body );
            return null;
        }

        protected virtual Object[] NamedFunctionStatement ( NamedFunctionStatement node, params Object[] args )
        {
            this.Analyse ( node.Identifier );
            foreach ( ASTNode arg in node.Arguments )
                this.Analyse ( arg );
            this.Analyse ( node.Body );
            return null;
        }

        protected virtual Object[] StringFunctionCallExpression ( StringFunctionCallExpression node, params Object[] args )
        {
            this.Analyse ( node.Base );
            this.Analyse ( node.Argument );
            return null;
        }

        protected virtual Object[] TableFunctionCallExpression ( TableFunctionCallExpression node, params Object[] args )
        {
            this.Analyse ( node.Base );
            this.Analyse ( node.Argument );
            return null;
        }

        protected virtual Object[] BreakStatement ( BreakStatement node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] ContinueStatement ( ContinueStatement node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] GotoStatement ( GotoStatement node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] GotoLabelStatement ( GotoLabelStatement node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] ReturnStatement ( ReturnStatement node, params Object[] args )
        {
            foreach ( ASTNode ret in node.Returns )
                this.Analyse ( ret );
            return null;
        }

        protected virtual Object[] BooleanExpression ( BooleanExpression node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] Eof ( Eof node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] NilExpression ( NilExpression node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] NumberExpression ( NumberExpression node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] ParenthesisExpression ( ParenthesisExpression node, params Object[] args )
        {
            this.Analyse ( node.Expression );
            return null;
        }

        protected virtual Object[] StringExpression ( StringExpression node, params Object[] args )
        {
            return null;
        }

        protected virtual Object[] TableConstructorExpression ( TableConstructorExpression node, params Object[] args )
        {
            foreach ( TableKeyValue field in node.Fields )
                this.Analyse ( field );
            return null;
        }

        protected virtual Object[] TableKeyValue ( TableKeyValue node, params Object[] args )
        {
            if ( !node.Sequential )
                this.Analyse ( node.Key );
            this.Analyse ( node.Value );
            return null;
        }

        protected virtual Object[] VarArgExpression ( VarArgExpression node, params Object[] args )
        {
            return null;
        }
    }
}
