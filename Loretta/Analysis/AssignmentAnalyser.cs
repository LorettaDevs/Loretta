using System;
using System.Collections.Generic;
using Loretta.Lexing;
using Loretta.Parsing.Nodes;
using Loretta.Parsing.Nodes.Constants;
using Loretta.Parsing.Nodes.Functions;
using Loretta.Parsing.Nodes.Indexers;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Analysis
{
    // WIP: don't use this.
    public class AssignmentAnalyser : BaseASTAnalyser
    {
        protected Dictionary<ASTNode, ASTNode> State = new Dictionary<ASTNode, ASTNode> ( );

        protected static MemberExpression GetMemberExpression ( ASTNode Base, String Indexer )
        {
            var exp = new MemberExpression ( Base.Parent, Base.Scope, new List<LToken> ( ) );
            exp.SetBase ( Base );
            exp.SetIndexer ( Indexer );
            return exp;
        }

        protected static IndexExpression GetIndexExpression ( ASTNode Base, ASTNode Indexer )
        {
            var exp = new IndexExpression ( Base.Parent, Base.Scope, new List<LToken> ( ) );
            exp.SetBase ( Base );
            exp.SetIndexer ( Indexer );
            return exp;
        }

        protected void ProcessAssignment ( ASTNode variable, ASTNode value )
        {
            if ( value is TableConstructorExpression valueTable )
            {
                foreach ( TableKeyValue field in valueTable.Fields )
                {
                    if ( field.Tokens.Count > 0 && field.Tokens[0].ID == "ident" )
                    {
                        MemberExpression member = GetMemberExpression ( variable, ( String ) field.Tokens[0].Value );
                        this.ProcessAssignment ( member, field.Value );
                    }
                    IndexExpression index = GetIndexExpression ( variable, field.Key );
                    this.ProcessAssignment ( index, field.Value );
                }
            }
            else
            {
                this.State[variable] = this.State.ContainsKey ( value ) ? this.State[value] : value;
            }
        }

        protected override Object[] AnalyseLocalVariableStatement ( LocalVariableStatement node, params Object[] args )
        {
            foreach ( ASTNode assign in node.Assignments )
                this.Analyse ( assign );
            for ( var i = 0; i < node.Variables.Count; i++ )
            {
                if ( i < node.Assignments.Count )
                {
                    this.ProcessAssignment ( node.Variables[i], node.Assignments[i] );
                }
                else
                {
                    this.State[node.Variables[i]] = NilExpression.Nil;
                }
            }
            return null;
        }

        protected override Object[] AnalyseAssignmentStatement ( AssignmentStatement node, params Object[] args )
        {
            foreach ( ASTNode assign in node.Assignments )
                this.Analyse ( assign );
            var isNonRootFunc = node.Scope.InternalData.GetValue ( "isFunction", false )
                && !node.Scope.InternalData.GetValue ( "isRoot", false );

            for ( var i = 0; i < node.Variables.Count; i++ )
            {
                if ( i < node.Assignments.Count )
                {
                    if ( this.State.ContainsKey ( node.Variables[i] ) )
                        this.State[node.Variables[i]].InternalData.RemoveValue ( "aa.isRiskyReplacement" );
                    this.State[node.Variables[i]] = node.Assignments[i];
                    this.State[node.Variables[i]].InternalData.SetValue ( "aa.isRiskyReplacement", isNonRootFunc );
                }
                else
                {
                    this.State[node.Variables[i]] = NilExpression.Nil;
                }
            }

            return null;
        }

        protected override Object[] AnalyseFunctionCallExpression ( FunctionCallExpression node, params Object[] args )
        {
            foreach ( ASTNode arg in node.Arguments )
            {
                ASTNode valAtTime = arg.InternalData.GetValue<ASTNode> ( "aa.valueAtTime", null );
                // A table could be possibly be modified by the function
                if ( ( valAtTime != null && valAtTime is TableConstructorExpression )
                    || ( this.State.ContainsKey ( arg ) && this.State[arg] is TableConstructorExpression ) )
                    arg.InternalData.SetValue ( "aa.isRiskyReplacement", true );
            }
            return null;
        }

        protected override Object[] AnalyseMemberExpression ( MemberExpression node, params Object[] args )
        {
            if ( this.State.ContainsKey ( node ) )
            {
                node.InternalData.SetValue ( "aa.isRiskyReplacement",
                    this.State[node].InternalData.GetValue ( "aa.isRiskyReplacement", false ) );
                node.InternalData.SetValue ( "aa.valueAtTime", this.State[node] );
            }
            return null;
        }

        protected override Object[] AnalyseIndexExpression ( IndexExpression node, params Object[] args )
        {
            if ( this.State.ContainsKey ( node ) )
            {
                node.InternalData.SetValue ( "aa.isRiskyReplacement",
                    this.State[node].InternalData.GetValue ( "aa.isRiskyReplacement", false ) );
                node.InternalData.SetValue ( "aa.valueAtTime", this.State[node] );
            }
            return null;
        }

        protected override Object[] AnalyseVariableExpression ( VariableExpression node, params Object[] args )
        {
            if ( this.State.ContainsKey ( node ) )
            {
                node.InternalData.SetValue ( "aa.isRiskyReplacement",
                    this.State[node].InternalData.GetValue ( "aa.isRiskyReplacement", false ) );
                node.InternalData.SetValue ( "aa.valueAtTime", this.State[node] );
            }
            return null;
        }
    }
}
