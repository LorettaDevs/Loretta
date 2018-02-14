using System;
using Loretta.Env;
using Loretta.Parsing;
using Loretta.Parsing.Nodes;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Analysis.RefCount
{
    public class RefCountAnalysis2 : BaseASTAnalyser
    {
        public RefCountAnalysis2 ( LuaEnvironment env, EnvFile file ) : base ( env, file )
        {
        }

        protected override Object[] VariableExpression ( VariableExpression node, params Object[] args )
        {
            if ( args.Length == 0 )
            {
                InternalData id = node.Variable.InternalData;
                id.SetValue ( "refcount2.references", 1 + id.GetValue ( "refcount2.references", 0 ) );
            }
            return null;
        }

        protected override Object[] LocalVariableStatement ( LocalVariableStatement node, params Object[] args )
        {
            foreach ( VariableExpression var in node.Variables )
                this.Analyse ( var, true );
            foreach ( ASTNode assign in node.Assignments )
                this.Analyse ( assign );
            return null;
        }

        protected override Object[] AssignmentStatement ( AssignmentStatement node, params Object[] args )
        {
            foreach ( ASTNode var in node.Variables )
                this.Analyse ( var, true );
            foreach ( ASTNode ass in node.Assignments )
                this.Analyse ( ass ); // with lube, please
            return null;
        }
    }
}
