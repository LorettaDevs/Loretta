using System;
using System.IO;
using System.Text;
using Loretta.Parsing.Nodes;
using Loretta.Parsing.Nodes.Constants;
using Loretta.Parsing.Nodes.ControlStatements;
using Loretta.Parsing.Nodes.Functions;
using Loretta.Parsing.Nodes.IfStatements;
using Loretta.Parsing.Nodes.Loops;
using Loretta.Parsing.Nodes.Operators;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Reconstructors
{
    public abstract class BaseReconstructor
    {
        public LuaEnvironment Environment { get; }

        /// <summary>
        /// The indentation level. You'll regret setting this
        /// directly (just saying...)
        /// </summary>
        protected Int32 IndentLevel { get; set; }

        /// <summary>
        /// The string that should be repeated the amount of times
        /// that the current <see cref="IndentLevel" /> is in
        /// </summary>
        protected String IndentationSequence { get; set; }

        /// <summary>
        /// The line indentation prefix (basically what you should
        /// use instead of yolo'ing your own indentation)
        /// </summary>
        protected String IndentString { get; private set; }

        protected BaseReconstructor ( LuaEnvironment environment )
        {
            this.Environment = environment;
        }

        /// <summary>
        /// Constructs the code from a node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="args"></param>
        public virtual void Construct ( ASTNode node, params Object[] args )
        {
            this.IndentLevel = 0;
            this.InternalConstructChoose ( node, args );
        }

        #region Indentation System

        /// <summary>
        /// Increases the indentation level
        /// </summary>
        protected void Indent ( )
        {
            this.IndentLevel++;
            this.UpdateIndentString ( );
        }

        /// <summary>
        /// Decreases the indentation level
        /// </summary>
        protected void Outdent ( )
        {
            this.IndentLevel--;
            this.UpdateIndentString ( );
        }

        /// <summary>
        /// Updates the <see cref="IndentString" /> to match the
        /// current indentation level (automatically called when
        /// you use <see cref="Indent" /> and <see cref="Outdent" />)
        /// </summary>
        protected void UpdateIndentString ( )
        {
            var build = new StringBuilder ( );
            for ( var i = 0; i < this.IndentLevel; i++ )
                build.Append ( this.IndentationSequence );
            this.IndentString = build.ToString ( );
        }

        /// <summary>
        /// Call this on the various nodes so that they are all
        /// written to the stream
        /// </summary>
        /// <param name="node"></param>
        /// <param name="args"></param>
        protected virtual void InternalConstructChoose ( ASTNode node, params Object[] args )
        {
            #region Regex Generated Mess
            if ( node is DoStatement )
                this.DoStatement ( ( DoStatement ) node, args );
            else if ( node is StatementList )
                this.StatementList ( ( StatementList ) node, args );
            else if ( node is AssignmentStatement )
                this.AssignmentStatement ( ( AssignmentStatement ) node, args );
            else if ( node is LocalVariableStatement )
                this.LocalVariableStatement ( ( LocalVariableStatement ) node, args );
            else if ( node is VariableExpression )
                this.VariableExpression ( ( VariableExpression ) node, args );
            else if ( node is BinaryOperatorExpression )
                this.BinaryOperatorExpression ( ( BinaryOperatorExpression ) node, args );
            else if ( node is UnaryOperatorExpression )
                this.UnaryOperatorExpression ( ( UnaryOperatorExpression ) node, args );
            else if ( node is ForGenericStatement )
                this.ForGenericStatement ( ( ForGenericStatement ) node, args );
            else if ( node is ForNumericStatement )
                this.ForNumericStatement ( ( ForNumericStatement ) node, args );
            else if ( node is RepeatStatement )
                this.RepeatStatement ( ( RepeatStatement ) node, args );
            else if ( node is WhileStatement )
                this.WhileStatement ( ( WhileStatement ) node, args );
            else if ( node is Parsing.Nodes.Indexers.IndexExpression )
                this.IndexExpression ( ( Parsing.Nodes.Indexers.IndexExpression ) node, args );
            else if ( node is Parsing.Nodes.Indexers.MemberExpression )
                this.MemberExpression ( ( Parsing.Nodes.Indexers.MemberExpression ) node, args );
            else if ( node is IfClause )
                this.IfClause ( ( IfClause ) node, args );
            else if ( node is IfStatement )
                this.IfStatement ( ( IfStatement ) node, args );
            else if ( node is AnonymousFunctionExpression )
                this.AnonymousFunctionExpression ( ( AnonymousFunctionExpression ) node, args );
            else if ( node is FunctionCallExpression )
                this.FunctionCallExpression ( ( FunctionCallExpression ) node, args );
            else if ( node is LocalFunctionStatement )
                this.LocalFunctionStatement ( ( LocalFunctionStatement ) node, args );
            else if ( node is NamedFunctionStatement )
                this.NamedFunctionStatement ( ( NamedFunctionStatement ) node, args );
            else if ( node is StringFunctionCallExpression )
                this.StringFunctionCallExpression ( ( StringFunctionCallExpression ) node, args );
            else if ( node is TableFunctionCallExpression )
                this.TableFunctionCallExpression ( ( TableFunctionCallExpression ) node, args );
            else if ( node is BreakStatement )
                this.BreakStatement ( ( BreakStatement ) node, args );
            else if ( node is ContinueStatement )
                this.ContinueStatement ( ( ContinueStatement ) node, args );
            else if ( node is GotoStatement )
                this.GotoStatement ( ( GotoStatement ) node, args );
            else if ( node is GotoLabelStatement )
                this.GotoLabelStatement ( ( GotoLabelStatement ) node, args );
            else if ( node is ReturnStatement )
                this.ReturnStatement ( ( ReturnStatement ) node, args );
            else if ( node is BooleanExpression )
                this.BooleanExpression ( ( BooleanExpression ) node, args );
            else if ( node is Eof )
                this.Eof ( ( Eof ) node, args );
            else if ( node is NilExpression )
                this.NilExpression ( ( NilExpression ) node, args );
            else if ( node is NumberExpression )
                this.NumberExpression ( ( NumberExpression ) node, args );
            else if ( node is ParenthesisExpression )
                this.ParenthesisExpression ( ( ParenthesisExpression ) node, args );
            else if ( node is StringExpression )
                this.StringExpression ( ( StringExpression ) node, args );
            else if ( node is TableConstructorExpression )
                this.TableConstructorExpression ( ( TableConstructorExpression ) node, args );
            else if ( node is TableKeyValue )
                this.TableKeyValue ( ( TableKeyValue ) node, args );
            else if ( node is VarArgExpression )
                this.VarArgExpression ( ( VarArgExpression ) node, args );
            #endregion Regex Generated Mess
            else
                throw new Exception ( "Unrecognized node: " + node );
        }

        #region Regex Generated Mess

        public abstract void DoStatement ( DoStatement node, params Object[] args );

        public abstract void StatementList ( StatementList node, params Object[] args );

        public abstract void AssignmentStatement ( AssignmentStatement node, params Object[] args );

        public abstract void LocalVariableStatement ( LocalVariableStatement node, params Object[] args );

        public abstract void VariableExpression ( VariableExpression node, params Object[] args );

        public abstract void BinaryOperatorExpression ( BinaryOperatorExpression node, params Object[] args );

        public abstract void UnaryOperatorExpression ( UnaryOperatorExpression node, params Object[] args );

        public abstract void ForGenericStatement ( ForGenericStatement node, params Object[] args );

        public abstract void ForNumericStatement ( ForNumericStatement node, params Object[] args );

        public abstract void RepeatStatement ( RepeatStatement node, params Object[] args );

        public abstract void WhileStatement ( WhileStatement node, params Object[] args );

        public abstract void IndexExpression ( Parsing.Nodes.Indexers.IndexExpression node, params Object[] args );

        public abstract void MemberExpression ( Parsing.Nodes.Indexers.MemberExpression node, params Object[] args );

        public abstract void IfClause ( IfClause node, params Object[] args );

        public abstract void IfStatement ( IfStatement node, params Object[] args );

        public abstract void AnonymousFunctionExpression ( AnonymousFunctionExpression node, params Object[] args );

        public abstract void FunctionCallExpression ( FunctionCallExpression node, params Object[] args );

        public abstract void LocalFunctionStatement ( LocalFunctionStatement node, params Object[] args );

        public abstract void NamedFunctionStatement ( NamedFunctionStatement node, params Object[] args );

        public abstract void StringFunctionCallExpression ( StringFunctionCallExpression node, params Object[] args );

        public abstract void TableFunctionCallExpression ( TableFunctionCallExpression node, params Object[] args );

        public abstract void BreakStatement ( BreakStatement node, params Object[] args );

        public abstract void ContinueStatement ( ContinueStatement node, params Object[] args );

        public abstract void GotoStatement ( GotoStatement node, params Object[] args );

        public abstract void GotoLabelStatement ( GotoLabelStatement node, params Object[] args );

        public abstract void ReturnStatement ( ReturnStatement node, params Object[] args );

        public abstract void BooleanExpression ( BooleanExpression node, params Object[] args );

        public abstract void Eof ( Eof node, params Object[] args );

        public abstract void NilExpression ( NilExpression node, params Object[] args );

        public abstract void NumberExpression ( NumberExpression node, params Object[] args );

        public abstract void ParenthesisExpression ( ParenthesisExpression node, params Object[] args );

        public abstract void StringExpression ( StringExpression node, params Object[] args );

        public abstract void TableConstructorExpression ( TableConstructorExpression node, params Object[] args );

        public abstract void TableKeyValue ( TableKeyValue node, params Object[] args );

        public abstract void VarArgExpression ( VarArgExpression node, params Object[] args );

        #endregion Regex Generated Mess
    }
}
