using Loretta.Parsing.Nodes;
using Loretta.Parsing.Nodes.Constants;
using Loretta.Parsing.Nodes.ControlStatements;
using Loretta.Parsing.Nodes.Functions;
using Loretta.Parsing.Nodes.IfStatements;
using Loretta.Parsing.Nodes.Indexers;
using Loretta.Parsing.Nodes.Loops;
using Loretta.Parsing.Nodes.Operators;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Reconstructors
{
    // TODO: Implement LuaReconstructorOptions usage
    public class FormattedLuaReconstructor : BaseReconstructor
    {
        protected readonly LuaReconstructorOptions Settings;

        public FormattedLuaReconstructor ( LuaReconstructorOptions options ) : base ( )
        {
            this.Settings = options;
        }

        #region Expressions

        public override void ConstructNumberExpression ( NumberExpression node )
        {
            this.Write ( node.Value.ToString ( ) );
        }

        public override void ConstructStringExpression ( StringExpression node )
        {
            this.Write ( node.StartDelimiter );
            this.Write ( node.EscapedValue );
            this.Write ( node.EndDelimiter );
        }

        public override void ConstructNilExpression ( NilExpression node )
        {
            this.Write ( "nil" );
        }

        public override void ConstructBooleanExpression ( BooleanExpression node )
        {
            this.Write ( node.Value ? "true" : "false" );
        }

        public override void ConstructVarArgExpression ( VarArgExpression node )
        {
            this.Write ( "..." );
        }

        public override void ConstructTableConstructorExpression ( TableConstructorExpression node )
        {
            // First token is always {
            this.Write ( '{' );

            if ( node.Fields.Count > 1 )
                this.WriteLine ( );
            else if ( node.Fields.Count == 1 && !node.Fields[0].IsSequential )
                this.Write ( ' ' );

            this.Indent ( );
            for ( var i = 0; i < node.Fields.Count; i++ )
            {
                TableKeyValue field = node.Fields[i];

                if ( field.IsSequential )
                {// No key to write
                }
                else if ( field.Key.Tokens[0].ID == "ident" )
                {
                    this.WriteIndented ( ( ( StringExpression ) field.Key ).Value );
                    this.Write ( " = " );
                }
                else
                {
                    this.WriteIndented ( '[' );
                    this.ConstructInternal ( field.Key );
                    this.Write ( "] = " );
                }

                this.ConstructInternal ( field.Value );
                this.WriteLine ( field.Separator );
            }
            this.Outdent ( );

            this.WriteIndented ( '}' );
        }

        public override void ConstructParenthesisExpression ( ParenthesisExpression node )
        {
            this.Write ( " ( " );
            this.ConstructInternal ( node.Expression );
            this.Write ( " ) " );
        }

        public override void ConstructBinaryOperatorExpression ( BinaryOperatorExpression node )
        {
            this.ConstructInternal ( node.LeftOperand );
            this.Write ( ' ' );
            this.Write ( node.Operator );
            this.Write ( ' ' );
            this.ConstructInternal ( node.RightOperand );
        }

        public override void ConstructAnonymousFunctionExpression ( AnonymousFunctionExpression node )
        {
            this.Write ( "function ( " );
            for ( var i = 0; i < node.Arguments.Count; i++ )
            {
                this.ConstructInternal ( node.Arguments[i] );
                if ( i < node.Arguments.Count - 1 )
                    this.Write ( ", " );
            }
            this.WriteLine ( " )" );

            this.Indent ( );
            this.ConstructInternal ( node.Body );
            this.Outdent ( );

            this.WriteIndented ( "end" );
        }

        public override void ConstructFunctionCallExpression ( FunctionCallExpression node )
        {
            this.ConstructInternal ( node.Base );
            this.Write ( " ( " );
            for ( var i = 0; i < node.Arguments.Count; i++ )
            {
                this.ConstructInternal ( node.Arguments[i] );
                if ( i < node.Arguments.Count - 1 )
                    this.Write ( ", " );
            }
            this.Write ( " ) " );
        }

        public override void ConstructStringFunctionCallExpression ( StringFunctionCallExpression node )
        {
            this.ConstructInternal ( node.Base );
            this.Write ( ' ' );
            this.ConstructStringExpression ( node.Argument );
            if ( node.HasSemicolon )
                this.Write ( ';' );
        }

        public override void ConstructTableFunctionCallExpression ( TableFunctionCallExpression node )
        {
            this.ConstructInternal ( node.Base );
            this.Write ( ' ' );
            this.ConstructTableConstructorExpression ( node.Argument );
            if ( node.HasSemicolon )
                this.Write ( ';' );
        }

        public override void ConstructIndexExpression ( IndexExpression node )
        {
            this.ConstructInternal ( node.Base );
            this.Write ( '[' );
            this.ConstructInternal ( node.Indexer );
            this.Write ( ']' );
        }

        public override void ConstructMemberExpression ( MemberExpression node )
        {
            this.ConstructInternal ( node.Base );
            this.Write ( node.SelfRef ? ':' : '.' );
            this.Write ( node.Indexer );
        }

        public override void ConstructUnaryOperatorExpression ( UnaryOperatorExpression node )
        {
            this.Write ( node.Operator );
            this.ConstructInternal ( node.Operand );
        }

        public override void ConstructVariableExpression ( VariableExpression node )
        {
            this.Write ( node.Variable.Name );
        }

        #endregion Expressions

        public override void ConstructReturnStatement ( ReturnStatement node )
        {
            this.WriteIndented ( "return " );

            for ( var i = 0; i < node.Returns.Count; i++ )
            {
                this.ConstructInternal ( node.Returns[i] );
                if ( i < node.Returns.Count - 1 )
                    this.Write ( ", " );
            }

            if ( node.HasSemicolon )
                this.Write ( ";" );
        }

        public override void ConstructAssignmentStatement ( AssignmentStatement node )
        {
            this.WriteIndent ( );

            for ( var i = 0; i < node.Variables.Count; i++ )
            {
                this.ConstructInternal ( node.Variables[i] );
                if ( i < node.Variables.Count - 1 )
                    this.Write ( ", " );
            }

            if ( node.Assignments.Count > 0 )
            {
                this.Write ( " = " );
                for ( var i = 0; i < node.Assignments.Count; i++ )
                {
                    this.ConstructInternal ( node.Assignments[i] );
                    if ( i < node.Assignments.Count - 1 )
                        this.Write ( ", " );
                }
            }

            if ( node.HasSemicolon )
                this.Write ( ";" );
        }

        public override void ConstructBreakStatement ( BreakStatement node )
        {
            this.WriteIndented ( "break" );
            if ( node.HasSemicolon )
                this.Write ( ";" );
        }

        public override void ConstructContinueStatement ( ContinueStatement node )
        {
            this.WriteIndented ( "continue" );
            if ( node.HasSemicolon )
                this.Write ( ";" );
        }

        public override void ConstructDoStatement ( DoStatement node )
        {
            this.WriteLineIndented ( "do" );
            this.Indent ( );
            this.ConstructInternal ( node.Body );
            this.Outdent ( );
            this.WriteIndented ( "end" );
            if ( node.HasSemicolon )
                this.Write ( ";" );
        }

        public override void ConstructEof ( Eof node )
        {
            // Do nothing.
        }

        public override void ConstructForGenericStatement ( ForGenericStatement node )
        {
            this.WriteIndented ( "for " );
            for ( var i = 0; i < node.Variables.Count; i++ )
            {
                this.ConstructVariableExpression ( node.Variables[i] );
                if ( i < node.Variables.Count - 1 )
                    this.Write ( ", " );
            }
            this.Write ( " in " );
            for ( var i = 0; i < node.Generators.Count; i++ )
            {
                this.ConstructInternal ( node.Generators[i] );
                if ( i < node.Generators.Count - 1 )
                    this.Write ( ", " );
            }
            this.WriteLine ( " do" );
            this.Indent ( );
            this.ConstructStatementList ( node.Body );
            this.Outdent ( );
            this.WriteIndented ( "end" );
            if ( node.HasSemicolon )
                this.Write ( ";" );
        }

        public override void ConstructForNumericStatement ( ForNumericStatement node )
        {
            this.WriteIndented ( "for " );
            this.ConstructVariableExpression ( node.Variable );
            this.Write ( " = " );
            this.ConstructInternal ( node.InitialExpression );
            this.Write ( ", " );
            this.ConstructInternal ( node.FinalExpression );
            if ( node.IncrementExpression != null )
            {
                this.Write ( ", " );
                this.ConstructInternal ( node.IncrementExpression );
            }
            this.WriteLine ( " do" );
            this.Indent ( );
            this.ConstructStatementList ( node.Body );
            this.Outdent ( );
            this.WriteIndented ( "end" );
            if ( node.HasSemicolon )
                this.Write ( ";" );
        }

        public override void ConstructGotoLabelStatement ( GotoLabelStatement node )
        {
            this.WriteIndented ( "::" );
            this.Write ( node.Label.Name );
            this.Write ( "::" );
            if ( node.HasSemicolon )
                this.Write ( ";" );
        }

        public override void ConstructGotoStatement ( GotoStatement node )
        {
            this.WriteIndented ( "goto " );
            this.Write ( node.Label.Name );
            if ( node.HasSemicolon )
                this.Write ( ";" );
        }

        public override void ConstructIfClause ( IfClause node )
        {
            this.Write ( "if " );
            this.ConstructInternal ( node.Condition );
            this.WriteLine ( " then" );
            this.Indent ( );
            this.ConstructInternal ( node.Body );
            this.Outdent ( );
        }

        public override void ConstructIfStatement ( IfStatement node )
        {
            this.WriteIndent ( );
            this.ConstructIfClause ( node.MainClause );

            foreach ( IfClause elif in node.ElseIfClauses )
            {
                this.Write ( "else" );
                this.ConstructIfClause ( elif );
            }

            if ( node.ElseBlock != null )
            {
                this.WriteLineIndented ( "else" );
                this.Indent ( );
                this.ConstructStatementList ( node.ElseBlock );
                this.Outdent ( );
            }
            this.WriteIndented ( "end" );
            if ( node.HasSemicolon )
                this.Write ( ";" );
        }

        public override void ConstructLocalFunctionStatement ( LocalFunctionStatement node )
        {
            this.WriteIndented ( "local function " );
            this.ConstructVariableExpression ( node.Identifier );
            this.Write ( " ( " );
            for ( var i = 0; i < node.Arguments.Count; i++ )
            {
                this.ConstructInternal ( node.Arguments[i] );
                if ( i < node.Arguments.Count - 1 )
                    this.Write ( ", " );
            }
            this.WriteLine ( " )" );
            this.Indent ( );
            this.ConstructStatementList ( node.Body );
            this.Outdent ( );
            this.WriteIndented ( "end" );
            if ( node.HasSemicolon )
                this.Write ( ';' );
        }

        public override void ConstructLocalVariableStatement ( LocalVariableStatement node )
        {
            this.WriteIndented ( "local " );
            for ( var i = 0; i < node.Variables.Count; i++ )
            {
                this.ConstructVariableExpression ( ( VariableExpression ) node.Variables[i] );
                if ( i < node.Variables.Count - 1 )
                    this.Write ( ", " );
            }
            if ( node.Assignments.Count > 0 )
            {
                this.Write ( " = " );
                for ( var i = 0; i < node.Assignments.Count; i++ )
                {
                    this.ConstructInternal ( node.Assignments[i] );
                    if ( i < node.Assignments.Count - 1 )
                        this.Write ( ", " );
                }
            }
            if ( node.HasSemicolon )
                this.Write ( ';' );
        }

        public override void ConstructNamedFunctionStatement ( NamedFunctionStatement node )
        {
            this.WriteIndented ( "function " );
            this.ConstructInternal ( node.Identifier );
            this.Write ( " ( " );
            for ( var i = 0; i < node.Arguments.Count; i++ )
            {
                this.ConstructInternal ( node.Arguments[i] );
                if ( i < node.Arguments.Count - 1 )
                    this.Write ( ", " );
            }
            this.WriteLine ( " )" );
            this.Indent ( );
            this.ConstructStatementList ( node.Body );
            this.Outdent ( );
            this.WriteIndented ( "end" );
            if ( node.HasSemicolon )
                this.Write ( ';' );
        }

        public override void ConstructRepeatStatement ( RepeatStatement node )
        {
            this.WriteIndented ( "repeat" );
            this.Indent ( );
            this.ConstructStatementList ( node.Body );
            this.Outdent ( );
            this.WriteIndented ( "until " );
            this.ConstructInternal ( node.Condition );
            if ( node.HasSemicolon )
                this.Write ( ';' );
        }

        public override void ConstructStatementList ( StatementList node )
        {
            foreach ( ASTNode statement in node.Statements )
            {
                this.ConstructInternal ( statement );
                this.WriteLine ( );
            }
        }

        public override void ConstructWhileStatement ( WhileStatement node )
        {
            this.WriteIndented ( "while " );
            this.ConstructInternal ( node.Condition );
            this.WriteLine ( " do" );
            this.Indent ( );
            this.ConstructStatementList ( node.Body );
            this.Outdent ( );
            this.WriteIndented ( "end" );
            if ( node.HasSemicolon )
                this.Write ( ';' );
        }
    }
}
