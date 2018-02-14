using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Env;
using Loretta.Lexing;
using Loretta.Parsing;
using Loretta.Parsing.Nodes;
using Loretta.Parsing.Nodes.Constants;
using Loretta.Parsing.Nodes.ControlStatements;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Analysis.RefCount
{
    internal class RefCountAnalyser1 : BaseASTAnalyser
    {
        public RefCountAnalyser1 ( LuaEnvironment env, EnvFile file ) : base ( env, file )
        {
        }

        public static Dictionary<ASTNode, NilExpression> Nils = new Dictionary<ASTNode, NilExpression> ( );

        public static NilExpression GetNil ( ASTNode parent )
        {
            if ( !Nils.ContainsKey ( parent ) )
                Nils[parent] = new NilExpression ( parent, parent.Scope, new List<LToken> ( new[] {
                    new LToken ( "nil", "nil", "nil", GParse.Lexing.TokenType.Keyword, GParse.Lexing.SourceRange.Zero )
                } ) );
            return Nils[parent];
        }

        protected override Object[] GotoLabelStatement ( GotoLabelStatement node, params Object[] args )
        {
            Label label = node.Label;

            Int32 refs = label.InternalData.GetValue ( "refcount.totalReferences", 0 );
            label.InternalData.SetValue ( "refcount.totalReferences", refs++ );

            return null;
        }

        protected override Object[] GotoStatement ( GotoStatement node, params Object[] args )
        {
            Label label = node.Label;

            Int32 refs = label.InternalData.GetValue ( "refcount.totalReferences", 0 );
            label.InternalData.SetValue ( "refcount.totalReferences", refs++ );

            return null;
        }

        protected override Object[] VariableExpression ( VariableExpression node, params Object[] args )
        {
            if ( args.Length == 0 )
            {
                Variable var = node.Variable;
                ASTNode value = var.InternalData.GetValue<ASTNode> ( "refcount.lastValue", GetNil ( node.Parent ) );
                Dictionary<ASTNode, Int32> refs = var.InternalData.GetValue ( "refcount.totalReferences", new Dictionary<ASTNode, Int32> ( ) );

                if ( !refs.ContainsKey ( value ) )
                    refs[value] = 0;
                refs[value]++;
            }
            return null;
        }

        protected override Object[] LocalVariableStatement ( LocalVariableStatement node, params Object[] args )
        {
            for ( var i = 0; i < node.Assignments.Count; i++ )
            {
                ASTNode expr = node.Assignments[i];
                this.Analyse ( expr );

                if ( node.Variables.Count > i )
                {
                    Variable var = node.Variables[i].Variable;
                    var.InternalData.SetValue ( "refcount.lastValue", expr );
                }
            }

            // Assign nil to the ones that don't have an
            // expression to be assigned to
            for ( var i = 0; i < node.Variables.Count; i++ )
            {
                VariableExpression var = node.Variables[i];
                if ( i > node.Assignments.Count )
                    var.Variable.InternalData.SetValue ( "refcount.lastValue", GetNil ( node ) );

                this.Analyse ( var, true, GetNil ( node ) );
            }

            return null;
        }

        protected override Object[] AssignmentStatement ( AssignmentStatement node, params Object[] args )
        {
            for ( var i = 0; i < node.Assignments.Count; i++ )
            {
                ASTNode expr = node.Assignments[i];
                if ( i < node.Variables.Count && node.Variables[i] is VariableExpression var )
                    var.InternalData.SetValue ( "refcount.lastValue", expr );

                this.Analyse ( expr );
            }

            for ( var i = 0; i < node.Variables.Count; i++ )
            {
                ASTNode expr = node.Variables[i];
                if ( i >= node.Assignments.Count && expr is VariableExpression var )
                    var.InternalData.SetValue ( "refcount.lastValue", GetNil ( node ) );

                this.Analyse ( expr, true );
            }

            return null;
        }
    }
}
