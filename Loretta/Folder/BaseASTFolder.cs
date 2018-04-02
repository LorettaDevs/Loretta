using System;
using System.Collections.Generic;
using Loretta.Env;
using Loretta.Parsing.Nodes;
using Loretta.Parsing.Nodes.Constants;
using Loretta.Parsing.Nodes.ControlStatements;
using Loretta.Parsing.Nodes.Functions;
using Loretta.Parsing.Nodes.IfStatements;
using Loretta.Parsing.Nodes.Loops;
using Loretta.Parsing.Nodes.Operators;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Folder
{
    public abstract class BaseASTFolder
    {
        public LuaEnvironment Environment { get; internal set; }

        public EnvFile File { get; internal set; }

        protected BaseASTFolder ( )
        {
        }

        public virtual ASTNode Fold ( ASTNode node, params Object[] args )
        {
            // Protection. JustInCase™
            if ( node == null )
                return null;

            this.FoldNode ( node, args );
            ASTNode ret = this.InternalNodeFoldFunc ( node, args );

            // We may want to remove a node
            if ( ret == null )
                node.Parent.RemoveChild ( node );
            else if ( !node.Equals ( ret ) && node.Parent != null )
                node.ReplaceInParent ( ret );

            return ret;
        }

        protected ASTNode InternalNodeFoldFunc ( ASTNode node, params Object[] args )
        {
            #region Regex Generated Mess
            if ( node is DoStatement )
                return this.FoldDoStatement ( ( DoStatement ) node, args );
            else if ( node is StatementList )
                return this.FoldStatementList ( ( StatementList ) node, args );
            else if ( node is AssignmentStatement )
                return this.FoldAssignmentStatement ( ( AssignmentStatement ) node, args );
            else if ( node is LocalVariableStatement )
                return this.FoldLocalVariableStatement ( ( LocalVariableStatement ) node, args );
            else if ( node is VariableExpression )
                return this.FoldVariableExpression ( ( VariableExpression ) node, args );
            else if ( node is BinaryOperatorExpression )
                return this.FoldBinaryOperatorExpression ( ( BinaryOperatorExpression ) node, args );
            else if ( node is UnaryOperatorExpression )
                return this.FoldUnaryOperatorExpression ( ( UnaryOperatorExpression ) node, args );
            else if ( node is ForGenericStatement )
                return this.FoldForGenericStatement ( ( ForGenericStatement ) node, args );
            else if ( node is ForNumericStatement )
                return this.FoldForNumericStatement ( ( ForNumericStatement ) node, args );
            else if ( node is RepeatStatement )
                return this.FoldRepeatStatement ( ( RepeatStatement ) node, args );
            else if ( node is WhileStatement )
                return this.FoldWhileStatement ( ( WhileStatement ) node, args );
            else if ( node is Parsing.Nodes.Indexers.IndexExpression )
                return this.FoldIndexExpression ( ( Parsing.Nodes.Indexers.IndexExpression ) node, args );
            else if ( node is Parsing.Nodes.Indexers.MemberExpression )
                return this.FoldMemberExpression ( ( Parsing.Nodes.Indexers.MemberExpression ) node, args );
            else if ( node is IfClause )
                return this.FoldIfClause ( ( IfClause ) node, args );
            else if ( node is IfStatement )
                return this.FoldIfStatement ( ( IfStatement ) node, args );
            else if ( node is AnonymousFunctionExpression )
                return this.FoldAnonymousFunctionExpression ( ( AnonymousFunctionExpression ) node, args );
            else if ( node is FunctionCallExpression )
                return this.FoldFunctionCallExpression ( ( FunctionCallExpression ) node, args );
            else if ( node is LocalFunctionStatement )
                return this.FoldLocalFunctionStatement ( ( LocalFunctionStatement ) node, args );
            else if ( node is NamedFunctionStatement )
                return this.FoldNamedFunctionStatement ( ( NamedFunctionStatement ) node, args );
            else if ( node is StringFunctionCallExpression )
                return this.FoldStringFunctionCallExpression ( ( StringFunctionCallExpression ) node, args );
            else if ( node is TableFunctionCallExpression )
                return this.FoldTableFunctionCallExpression ( ( TableFunctionCallExpression ) node, args );
            else if ( node is BreakStatement )
                return this.FoldBreakStatement ( ( BreakStatement ) node, args );
            else if ( node is ContinueStatement )
                return this.FoldContinueStatement ( ( ContinueStatement ) node, args );
            else if ( node is GotoStatement )
                return this.FoldGotoStatement ( ( GotoStatement ) node, args );
            else if ( node is GotoLabelStatement )
                return this.FoldGotoLabelStatement ( ( GotoLabelStatement ) node, args );
            else if ( node is ReturnStatement )
                return this.FoldReturnStatement ( ( ReturnStatement ) node, args );
            else if ( node is BooleanExpression )
                return this.FoldBooleanExpression ( ( BooleanExpression ) node, args );
            else if ( node is Eof )
                return this.FoldEof ( ( Eof ) node, args );
            else if ( node is NilExpression )
                return this.FoldNilExpression ( ( NilExpression ) node, args );
            else if ( node is NumberExpression )
                return this.FoldNumberExpression ( ( NumberExpression ) node, args );
            else if ( node is ParenthesisExpression )
                return this.FoldParenthesisExpression ( ( ParenthesisExpression ) node, args );
            else if ( node is StringExpression )
                return this.FoldStringExpression ( ( StringExpression ) node, args );
            else if ( node is TableConstructorExpression )
                return this.FoldTableConstructorExpression ( ( TableConstructorExpression ) node, args );
            else if ( node is TableKeyValue )
                return this.FoldTableKeyValue ( ( TableKeyValue ) node, args );
            else if ( node is VarArgExpression )
                return this.FoldVarArgExpression ( ( VarArgExpression ) node, args );
            #endregion Regex Generated Mess
            else
                throw new Exception ( "Unknown node type: " + node );
        }

        /// <summary>
        /// Folds a list of astnodes and returns a new list with
        /// all non-nulled nodes
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        protected List<ASTNode> FoldNodeList ( IEnumerable<ASTNode> nodes )
        {
            var newList = new List<ASTNode> ( );
            foreach ( ASTNode node in nodes )
            {
                ASTNode newNode = this.Fold ( node );
                if ( newNode != null )
                    newList.Add ( newNode );
            }
            return newList;
        }

        /// <summary>
        /// Called for all nodes
        /// </summary>
        /// <param name="node"></param>
        /// <param name="args"></param>
        protected virtual ASTNode FoldNode ( ASTNode node, params Object[] args )
        {
            return null;
        }

        protected virtual ASTNode FoldDoStatement ( DoStatement node, params Object[] args )
        {
            if ( node.Body == null )
                throw new Exception ( "Cannot have a DoStatement with a null body." );
            node.SetBody ( this.FoldStatementList ( node.Body ) );

            if ( node.Body.Statements.Count == 0 )
                return null;
            return node;
        }

        protected virtual StatementList FoldStatementList ( StatementList node, params Object[] args )
        {
            List<ASTNode> newList = this.FoldNodeList ( node.Statements );
            node.SetStatements ( newList );
            return node;
        }

        protected virtual ASTNode FoldAssignmentStatement ( AssignmentStatement node, params Object[] args )
        {
            node.SetVariables ( this.FoldNodeList ( node.Variables ) );
            node.SetAssignments ( this.FoldNodeList ( node.Assignments ) );

            if ( node.Variables.Count == 0 )
                return null;
            return node;
        }

        protected virtual ASTNode FoldLocalVariableStatement ( LocalVariableStatement node, params Object[] args )
        {
            node.SetVariables ( this.FoldNodeList ( node.Variables ) );
            node.SetAssignments ( this.FoldNodeList ( node.Assignments ) );

            if ( node.Variables.Count == 0 )
                return null;
            return node;
        }

        protected virtual ASTNode FoldVariableExpression ( VariableExpression node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode FoldBinaryOperatorExpression ( BinaryOperatorExpression node, params Object[] args )
        {
            if ( node.LeftOperand != null )
                node.SetLeftOperand ( this.Fold ( node.LeftOperand ) );
            if ( node.RightOperand != null )
                node.SetRightOperand ( this.Fold ( node.RightOperand ) );
            if ( node.LeftOperand == null && node.RightOperand == null )
                throw new Exception ( "Cannot fold both operands to null." );

            if ( node.LeftOperand == null )
                return node.RightOperand;
            else if ( node.RightOperand == null )
                return node.LeftOperand;
            else
                return node;
        }

        protected virtual ASTNode FoldUnaryOperatorExpression ( UnaryOperatorExpression node, params Object[] args )
        {
            if ( node.Operand != null )
                node.SetOperand ( this.Fold ( node.Operand ) );
            if ( node.Operand == null )
                throw new Exception ( "Cannot have a unary expression with a null operand." );
            return node;
        }

        protected virtual ASTNode FoldForGenericStatement ( ForGenericStatement node, params Object[] args )
        {
            node.SetVariables ( this.FoldNodeList ( node.Variables ) );
            node.SetGenerators ( this.FoldNodeList ( node.Generators ) );

            if ( node.Variables.Count == 0 || node.Generators.Count == 0 )
                throw new Exception ( "Cannot have a generic for without variables or generators." );
            if ( node.Body == null )
                throw new Exception ( "Cannot have a generic for with a null body." );
            node.SetBody ( this.FoldStatementList ( node.Body ) );

            return node;
        }

        protected virtual ASTNode FoldForNumericStatement ( ForNumericStatement node, params Object[] args )
        {
            if ( node.Variable != null )
            {
                var var = ( VariableExpression ) this.Fold ( node.Variable );
                if ( var != null )
                    node.SetVariable ( var );
                else
                    throw new Exception ( "Cannot have a numeric for statement without a variable." );
            }
            else
                throw new Exception ( "Cannot have a numeric for statement without a variable." );

            if ( node.InitialExpression != null )
            {
                ASTNode init = this.Fold ( node.InitialExpression );
                if ( init != null )
                    node.SetInitialExpression ( init );
                else
                    throw new Exception ( "Cannot have a numeric for statement without an initial expression." );
            }
            else
                throw new Exception ( "Cannot have a numeric for statement without an initial expression." );

            if ( node.FinalExpression != null )
            {
                ASTNode final = this.Fold ( node.FinalExpression );
                if ( final != null )
                    node.SetFinalExpression ( final );
                else
                    throw new Exception ( "Cannot have a numeric for statement without a final expression." );
            }
            else
                throw new Exception ( "Cannot have a numeric for statement without a final expression." );

            if ( node.IncrementExpression != null )
                node.SetIncrementExpression ( this.Fold ( node.IncrementExpression ) );

            if ( node.Body == null )
                throw new Exception ( "Cannot have a numeric for statement with a null body." );
            node.SetBody ( this.FoldStatementList ( node.Body ) );
            return node;
        }

        protected virtual ASTNode FoldRepeatStatement ( RepeatStatement node, params Object[] args )
        {
            node.SetBody ( this.FoldStatementList ( node.Body ) );
            node.SetCondition ( this.Fold ( node.Condition ) );
            return node;
        }

        protected virtual ASTNode FoldWhileStatement ( WhileStatement node, params Object[] args )
        {
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            node.SetCondition ( this.Fold ( node.Condition ) );
            return node;
        }

        protected virtual ASTNode FoldIndexExpression ( Parsing.Nodes.Indexers.IndexExpression node, params Object[] args )
        {
            node.SetBase ( this.Fold ( node.Base ) );
            node.SetIndexer ( this.Fold ( node.Indexer ) );
            return node;
        }

        protected virtual ASTNode FoldMemberExpression ( Parsing.Nodes.Indexers.MemberExpression node, params Object[] args )
        {
            node.SetBase ( this.Fold ( node.Base ) );
            return node;
        }

        protected virtual ASTNode FoldIfClause ( IfClause node, params Object[] args )
        {
            node.SetCondition ( this.Fold ( node.Condition ) );
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            return node;
        }

        protected virtual ASTNode FoldIfStatement ( IfStatement node, params Object[] args )
        {
            if ( node.MainClause != null )
                node.SetMainClause ( ( IfClause ) this.Fold ( node.MainClause ) );
            else
                throw new Exception ( "Cannot have an if statement without a main clause." );

            node.SetElseIfClauses ( this.FoldNodeList ( node.ElseIfClauses ) );

            if ( node.ElseBlock != null )
                node.SetElseBlock ( ( StatementList ) this.Fold ( node.ElseBlock ) );

            if ( node.ElseBlock != null && node.ElseBlock.Statements.Count == 0 )
                node.SetElseBlock ( null );
            return node;
        }

        protected virtual ASTNode FoldAnonymousFunctionExpression ( AnonymousFunctionExpression node, params Object[] args )
        {
            node.SetArguments ( this.FoldNodeList ( node.Arguments ) );
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            return node;
        }

        protected virtual ASTNode FoldFunctionCallExpression ( FunctionCallExpression node, params Object[] args )
        {
            node.SetBase ( this.Fold ( node.Base ) );
            node.SetArguments ( this.FoldNodeList ( node.Arguments ) );
            return node;
        }

        protected virtual ASTNode FoldLocalFunctionStatement ( LocalFunctionStatement node, params Object[] args )
        {
            node.SetIdentifier ( ( VariableExpression ) this.Fold ( node.Identifier ) );
            node.SetArguments ( this.FoldNodeList ( node.Arguments ) );
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            return node;
        }

        protected virtual ASTNode FoldNamedFunctionStatement ( NamedFunctionStatement node, params Object[] args )
        {
            node.SetIdentifier ( this.Fold ( node.Identifier ) );
            node.SetArguments ( this.FoldNodeList ( node.Arguments ) );
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            return node;
        }

        protected virtual ASTNode FoldStringFunctionCallExpression ( StringFunctionCallExpression node, params Object[] args )
        {
            node.SetBase ( this.Fold ( node.Base ) );
            node.SetArgument ( ( StringExpression ) this.Fold ( node.Argument ) );
            return null;
        }

        protected virtual ASTNode FoldTableFunctionCallExpression ( TableFunctionCallExpression node, params Object[] args )
        {
            node.SetBase ( this.Fold ( node.Base ) );
            node.SetArgument ( ( TableConstructorExpression ) this.Fold ( node.Argument ) );
            return null;
        }

        protected virtual ASTNode FoldBreakStatement ( BreakStatement node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode FoldContinueStatement ( ContinueStatement node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode FoldGotoStatement ( GotoStatement node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode FoldGotoLabelStatement ( GotoLabelStatement node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode FoldReturnStatement ( ReturnStatement node, params Object[] args )
        {
            node.SetReturns ( this.FoldNodeList ( node.Returns ) );
            return node;
        }

        protected virtual ASTNode FoldBooleanExpression ( BooleanExpression node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode FoldEof ( Eof node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode FoldNilExpression ( NilExpression node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode FoldNumberExpression ( NumberExpression node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode FoldParenthesisExpression ( ParenthesisExpression node, params Object[] args )
        {
            node.SetExpression ( this.Fold ( node.Expression ) );
            return node;
        }

        protected virtual ASTNode FoldStringExpression ( StringExpression node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode FoldTableConstructorExpression ( TableConstructorExpression node, params Object[] args )
        {
            node.SetFields ( this.FoldNodeList ( node.Fields ) );
            return node;
        }

        protected virtual ASTNode FoldTableKeyValue ( TableKeyValue node, params Object[] args )
        {
            if ( !node.IsSequential )
                node.Key = this.Fold ( node.Key );
            node.Value = this.Fold ( node.Value );
            return node;
        }

        protected virtual ASTNode FoldVarArgExpression ( VarArgExpression node, params Object[] args )
        {
            return node;
        }
    }
}
