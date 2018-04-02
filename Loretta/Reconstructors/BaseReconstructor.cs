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
        /// <summary>
        /// The indentation level. Use <see cref="Indent" /> and
        /// <see cref="Outdent" /> to change this.
        /// </summary>
        protected Int32 IndentLevel { get; private set; }

        /// <summary>
        /// The string that should be repeated the amount of times
        /// that the current <see cref="IndentLevel" /> is in
        /// </summary>
        public String IndentationSequence { get; set; } = "\t";

        /// <summary>
        /// The line indentation prefix (basically what you should
        /// use instead of yolo'ing your own indentation)
        /// </summary>
        protected String IndentString { get; private set; }

        /// <summary>
        /// The current stream being used
        /// </summary>
        private Stream Stream { get; set; }

        /// <summary>
        /// The current StreamWriter
        /// </summary>
        private StreamWriter StreamWriter { get; set; }

        protected BaseReconstructor ( )
        {
        }

        /// <summary>
        /// Constructs the code from a node. DO NOT CALL THIS
        /// INTERNALLY, CALL <see cref="ConstructInternal(ASTNode)" />.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="stream"></param>
        public virtual void Construct ( ASTNode node, Stream stream )
        {
            this.IndentLevel = 0;
            using ( this.StreamWriter = new StreamWriter ( stream, Encoding.GetEncoding ( 28591 ), 4096, true ) )
                this.ConstructInternal ( node );
        }

        /// <summary>
        /// Constructs the code from a node. DO NOT CALL THIS
        /// INTERNALLY, CALL <see cref="ConstructInternal(ASTNode)" />.
        /// </summary>
        /// <param name="node"></param>
        public virtual String Construct ( ASTNode node )
        {
            using ( var mem = new MemoryStream ( ) )
            using ( var reader = new StreamReader ( mem ) )
            using ( this.StreamWriter = new StreamWriter ( mem ) )
            {
                this.IndentLevel = 0;
                this.ConstructInternal ( node );

                mem.Seek ( 0, SeekOrigin.Begin );
                return reader.ReadToEnd ( );
            }
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

        #endregion Indentation System

        #region Stream Writing

        protected void Write ( Object value )
            => this.StreamWriter.Write ( value );

        protected void WriteLine ( )
            => this.StreamWriter.WriteLine ( );

        protected void WriteLine ( Object value )
            => this.StreamWriter.WriteLine ( value );

        protected void WriteIndent ( )
            => this.StreamWriter.Write ( this.IndentString );

        protected void WriteIndented ( Object value )
            => this.StreamWriter.Write ( this.IndentString + value );

        protected void WriteLineIndented ( )
            => this.StreamWriter.WriteLine ( this.IndentString );

        protected void WriteLineIndented ( Object value )
            => this.StreamWriter.WriteLine ( this.IndentString + value );

        #endregion Stream Writing

        /// <summary>
        /// Call this on the various nodes so that they are all
        /// written to the stream. DO NOT CALL
        /// <see cref="Construct(ASTNode)" /> INTERNALLY.
        /// </summary>
        /// <param name="node"></param>
        protected virtual void ConstructInternal ( ASTNode node )
        {
            #region Regex Generated Mess
            if ( node is DoStatement )
                this.ConstructDoStatement ( ( DoStatement ) node );
            else if ( node is StatementList )
                this.ConstructStatementList ( ( StatementList ) node );
            else if ( node is AssignmentStatement )
                this.ConstructAssignmentStatement ( ( AssignmentStatement ) node );
            else if ( node is LocalVariableStatement )
                this.ConstructLocalVariableStatement ( ( LocalVariableStatement ) node );
            else if ( node is VariableExpression )
                this.ConstructVariableExpression ( ( VariableExpression ) node );
            else if ( node is BinaryOperatorExpression )
                this.ConstructBinaryOperatorExpression ( ( BinaryOperatorExpression ) node );
            else if ( node is UnaryOperatorExpression )
                this.ConstructUnaryOperatorExpression ( ( UnaryOperatorExpression ) node );
            else if ( node is ForGenericStatement )
                this.ConstructForGenericStatement ( ( ForGenericStatement ) node );
            else if ( node is ForNumericStatement )
                this.ConstructForNumericStatement ( ( ForNumericStatement ) node );
            else if ( node is RepeatStatement )
                this.ConstructRepeatStatement ( ( RepeatStatement ) node );
            else if ( node is WhileStatement )
                this.ConstructWhileStatement ( ( WhileStatement ) node );
            else if ( node is Parsing.Nodes.Indexers.IndexExpression )
                this.ConstructIndexExpression ( ( Parsing.Nodes.Indexers.IndexExpression ) node );
            else if ( node is Parsing.Nodes.Indexers.MemberExpression )
                this.ConstructMemberExpression ( ( Parsing.Nodes.Indexers.MemberExpression ) node );
            else if ( node is IfClause )
                this.ConstructIfClause ( ( IfClause ) node );
            else if ( node is IfStatement )
                this.ConstructIfStatement ( ( IfStatement ) node );
            else if ( node is AnonymousFunctionExpression )
                this.ConstructAnonymousFunctionExpression ( ( AnonymousFunctionExpression ) node );
            else if ( node is FunctionCallExpression )
                this.ConstructFunctionCallExpression ( ( FunctionCallExpression ) node );
            else if ( node is LocalFunctionStatement )
                this.ConstructLocalFunctionStatement ( ( LocalFunctionStatement ) node );
            else if ( node is NamedFunctionStatement )
                this.ConstructNamedFunctionStatement ( ( NamedFunctionStatement ) node );
            else if ( node is StringFunctionCallExpression )
                this.ConstructStringFunctionCallExpression ( ( StringFunctionCallExpression ) node );
            else if ( node is TableFunctionCallExpression )
                this.ConstructTableFunctionCallExpression ( ( TableFunctionCallExpression ) node );
            else if ( node is BreakStatement )
                this.ConstructBreakStatement ( ( BreakStatement ) node );
            else if ( node is ContinueStatement )
                this.ConstructContinueStatement ( ( ContinueStatement ) node );
            else if ( node is GotoStatement )
                this.ConstructGotoStatement ( ( GotoStatement ) node );
            else if ( node is GotoLabelStatement )
                this.ConstructGotoLabelStatement ( ( GotoLabelStatement ) node );
            else if ( node is ReturnStatement )
                this.ConstructReturnStatement ( ( ReturnStatement ) node );
            else if ( node is BooleanExpression )
                this.ConstructBooleanExpression ( ( BooleanExpression ) node );
            else if ( node is Eof )
                this.ConstructEof ( ( Eof ) node );
            else if ( node is NilExpression )
                this.ConstructNilExpression ( ( NilExpression ) node );
            else if ( node is NumberExpression )
                this.ConstructNumberExpression ( ( NumberExpression ) node );
            else if ( node is ParenthesisExpression )
                this.ConstructParenthesisExpression ( ( ParenthesisExpression ) node );
            else if ( node is StringExpression )
                this.ConstructStringExpression ( ( StringExpression ) node );
            else if ( node is TableConstructorExpression )
                this.ConstructTableConstructorExpression ( ( TableConstructorExpression ) node );
            else if ( node is VarArgExpression )
                this.ConstructVarArgExpression ( ( VarArgExpression ) node );
            #endregion Regex Generated Mess
            else
                throw new Exception ( "Unrecognized node: " + node );
        }

        #region Regex Generated Mess

        public abstract void ConstructDoStatement ( DoStatement node );

        public abstract void ConstructStatementList ( StatementList node );

        public abstract void ConstructAssignmentStatement ( AssignmentStatement node );

        public abstract void ConstructLocalVariableStatement ( LocalVariableStatement node );

        public abstract void ConstructVariableExpression ( VariableExpression node );

        public abstract void ConstructBinaryOperatorExpression ( BinaryOperatorExpression node );

        public abstract void ConstructUnaryOperatorExpression ( UnaryOperatorExpression node );

        public abstract void ConstructForGenericStatement ( ForGenericStatement node );

        public abstract void ConstructForNumericStatement ( ForNumericStatement node );

        public abstract void ConstructRepeatStatement ( RepeatStatement node );

        public abstract void ConstructWhileStatement ( WhileStatement node );

        public abstract void ConstructIndexExpression ( Parsing.Nodes.Indexers.IndexExpression node );

        public abstract void ConstructMemberExpression ( Parsing.Nodes.Indexers.MemberExpression node );

        public abstract void ConstructIfClause ( IfClause node );

        public abstract void ConstructIfStatement ( IfStatement node );

        public abstract void ConstructAnonymousFunctionExpression ( AnonymousFunctionExpression node );

        public abstract void ConstructFunctionCallExpression ( FunctionCallExpression node );

        public abstract void ConstructLocalFunctionStatement ( LocalFunctionStatement node );

        public abstract void ConstructNamedFunctionStatement ( NamedFunctionStatement node );

        public abstract void ConstructStringFunctionCallExpression ( StringFunctionCallExpression node );

        public abstract void ConstructTableFunctionCallExpression ( TableFunctionCallExpression node );

        public abstract void ConstructBreakStatement ( BreakStatement node );

        public abstract void ConstructContinueStatement ( ContinueStatement node );

        public abstract void ConstructGotoStatement ( GotoStatement node );

        public abstract void ConstructGotoLabelStatement ( GotoLabelStatement node );

        public abstract void ConstructReturnStatement ( ReturnStatement node );

        public abstract void ConstructBooleanExpression ( BooleanExpression node );

        public abstract void ConstructEof ( Eof node );

        public abstract void ConstructNilExpression ( NilExpression node );

        public abstract void ConstructNumberExpression ( NumberExpression node );

        public abstract void ConstructParenthesisExpression ( ParenthesisExpression node );

        public abstract void ConstructStringExpression ( StringExpression node );

        public abstract void ConstructTableConstructorExpression ( TableConstructorExpression node );

        public abstract void ConstructVarArgExpression ( VarArgExpression node );

        #endregion Regex Generated Mess
    }
}
