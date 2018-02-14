using System;
using System.Collections.Generic;
using System.Text;
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
        public LuaEnvironment Environment { get; }

        public EnvFile File { get; }

        protected BaseASTFolder ( LuaEnvironment env, EnvFile file )
        {
            this.Environment = env;
            this.File = file;
        }

        protected virtual ASTNode Fold ( ASTNode node, params Object[] args )
        {
            // Protection. JustInCase™
            if ( node == null )
                return null;

            this.FoldNode ( node, args );
            ASTNode ret = this.InternalNodeFoldFunc ( node, args );

            if ( !node.Equals ( ret ) && node.Parent != null )
                node.ReplaceInParent ( ret );

            return ret;
        }

        protected ASTNode InternalNodeFoldFunc ( ASTNode node, params Object[] args )
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
        protected virtual ASTNode FoldNode ( ASTNode node, params Object[] args )
        {
            return null;
        }

        protected virtual ASTNode DoStatement ( DoStatement node, params Object[] args )
        {
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            return node;
        }

        protected virtual ASTNode StatementList ( StatementList node, params Object[] args )
        {
            for ( var i = 0; i < node.Statements.Count; i++ )
                node.Statements[i] = this.Fold ( node.Statements[i] );
            return node;
        }

        protected virtual ASTNode AssignmentStatement ( AssignmentStatement node, params Object[] args )
        {
            for ( var i = 0; i < node.Variables.Count; i++ )
                node.Variables[i] = this.Fold ( node.Variables[i] );
            for ( var i = 0; i < node.Assignments.Count; i++ )
                node.Assignments[i] = this.Fold ( node.Assignments[i] );
            return node;
        }

        protected virtual ASTNode LocalVariableStatement ( LocalVariableStatement node, params Object[] args )
        {
            for ( var i = 0; i < node.Variables.Count; i++ )
                node.Variables[i] = ( VariableExpression ) this.Fold ( node.Variables[i] );
            for ( var i = 0; i < node.Assignments.Count; i++ )
                node.Assignments[i] = this.Fold ( node.Assignments[i] );
            return node;
        }

        protected virtual ASTNode VariableExpression ( VariableExpression node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode BinaryOperatorExpression ( BinaryOperatorExpression node, params Object[] args )
        {
            node.SetLeftOperand ( this.Fold ( node.LeftOperand ) );
            node.SetRightOperand ( this.Fold ( node.RightOperand ) );
            return node;
        }

        protected virtual ASTNode UnaryOperatorExpression ( UnaryOperatorExpression node, params Object[] args )
        {
            node.SetOperand ( this.Fold ( node.Operand ) );
            return node;
        }

        protected virtual ASTNode ForGenericStatement ( ForGenericStatement node, params Object[] args )
        {
            for ( var i = 0; i < node.Variables.Count; i++ )
                node.Variables[i] = ( VariableExpression ) this.Fold ( node.Variables[i] );
            for ( var i = 0; i < node.Generators.Count; i++ )
                node.Generators[i] = this.Fold ( node.Generators[i] );
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            return node;
        }

        protected virtual ASTNode ForNumericStatement ( ForNumericStatement node, params Object[] args )
        {
            node.SetInitialExpression ( this.Fold ( node.InitialExpression ) );
            node.SetFinalExpression ( this.Fold ( node.FinalExpression ) );
            node.SetIncrementExpression ( this.Fold ( node.IncrementExpression ) );
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            return node;
        }

        protected virtual ASTNode RepeatStatement ( RepeatStatement node, params Object[] args )
        {
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            node.SetCondition ( this.Fold ( node.Condition ) );
            return node;
        }

        protected virtual ASTNode WhileStatement ( WhileStatement node, params Object[] args )
        {
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            node.SetCondition ( this.Fold ( node.Condition ) );
            return node;
        }

        protected virtual ASTNode IndexExpression ( Parsing.Nodes.Indexers.IndexExpression node, params Object[] args )
        {
            node.SetBase ( this.Fold ( node.Base ) );
            node.SetIndexer ( this.Fold ( node.Indexer ) );
            return node;
        }

        protected virtual ASTNode MemberExpression ( Parsing.Nodes.Indexers.MemberExpression node, params Object[] args )
        {
            node.SetBase ( this.Fold ( node.Base ) );
            return node;
        }

        protected virtual ASTNode IfClause ( IfClause node, params Object[] args )
        {
            node.SetCondition ( this.Fold ( node.Condition ) );
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            return node;
        }

        protected virtual ASTNode IfStatement ( IfStatement node, params Object[] args )
        {
            node.SetMainClause ( ( IfClause ) this.Fold ( node.MainClause ) );
            for ( var i = 0; i < node.ElseIfClauses.Count; i++ )
                node.ElseIfClauses[i] = ( IfClause ) this.Fold ( node.ElseIfClauses[i] );
            node.SetElseBlock ( ( StatementList ) this.Fold ( node.ElseBlock ) );
            return node;
        }

        protected virtual ASTNode AnonymousFunctionExpression ( AnonymousFunctionExpression node, params Object[] args )
        {
            for ( var i = 0; i < node.Arguments.Count; i++ )
                node.Arguments[i] = this.Fold ( node.Arguments[i] );
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            return node;
        }

        protected virtual ASTNode FunctionCallExpression ( FunctionCallExpression node, params Object[] args )
        {
            node.SetBase ( this.Fold ( node.Base ) );
            for ( var i = 0; i < node.Arguments.Count; i++ )
                node.Arguments[i] = this.Fold ( node.Arguments[i] );
            return node;
        }

        protected virtual ASTNode LocalFunctionStatement ( LocalFunctionStatement node, params Object[] args )
        {
            node.SetIdentifier ( ( VariableExpression ) this.Fold ( node.Identifier ) );
            for ( var i = 0; i < node.Arguments.Count; i++ )
                node.Arguments[i] = this.Fold ( node.Arguments[i] );
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            return node;
        }

        protected virtual ASTNode NamedFunctionStatement ( NamedFunctionStatement node, params Object[] args )
        {
            node.SetIdentifier ( ( VariableExpression ) this.Fold ( node.Identifier ) );
            for ( var i = 0; i < node.Arguments.Count; i++ )
                node.Arguments[i] = this.Fold ( node.Arguments[i] );
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            return node;
        }

        protected virtual ASTNode StringFunctionCallExpression ( StringFunctionCallExpression node, params Object[] args )
        {
            node.SetBase ( this.Fold ( node.Base ) );
            node.SetArgument ( ( StringExpression ) this.Fold ( node.Argument ) );
            return null;
        }

        protected virtual ASTNode TableFunctionCallExpression ( TableFunctionCallExpression node, params Object[] args )
        {
            node.SetBase ( this.Fold ( node.Base ) );
            node.SetArgument ( ( TableConstructorExpression ) this.Fold ( node.Argument ) );
            return null;
        }

        protected virtual ASTNode BreakStatement ( BreakStatement node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode ContinueStatement ( ContinueStatement node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode GotoStatement ( GotoStatement node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode GotoLabelStatement ( GotoLabelStatement node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode ReturnStatement ( ReturnStatement node, params Object[] args )
        {
            for ( var i = 0; i < node.Returns.Count; i++ )
                node.Returns[i] = this.Fold ( node.Returns[i] );
            return node;
        }

        protected virtual ASTNode BooleanExpression ( BooleanExpression node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode Eof ( Eof node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode NilExpression ( NilExpression node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode NumberExpression ( NumberExpression node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode ParenthesisExpression ( ParenthesisExpression node, params Object[] args )
        {
            node.SetExpression ( this.Fold ( node.Expression ) );
            return node;
        }

        protected virtual ASTNode StringExpression ( StringExpression node, params Object[] args )
        {
            return node;
        }

        protected virtual ASTNode TableConstructorExpression ( TableConstructorExpression node, params Object[] args )
        {
            for ( var i = 0; i < node.Fields.Count; i++ )
                node.Fields[i] = ( TableKeyValue ) this.Fold ( node.Fields[i] );
            return node;
        }

        protected virtual ASTNode TableKeyValue ( TableKeyValue node, params Object[] args )
        {
            if ( !node.Sequential )
                node.Key = this.Fold ( node.Key );
            node.Value = this.Fold ( node.Key );
            return node;
        }

        protected virtual ASTNode VarArgExpression ( VarArgExpression node, params Object[] args )
        {
            return node;
        }
    }
}
