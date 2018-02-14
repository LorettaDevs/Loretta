using System;
using Loretta.Env;
using Loretta.Parsing.Nodes;

namespace Loretta.Analysis
{
    public class UpdateBoundsAnalyser : BaseASTAnalyser
    {
        protected UpdateBoundsAnalyser ( LuaEnvironment env, EnvFile file ) : base ( env, file )
        {
        }

        protected override Object[] Analyse ( ASTNode AST, params Object[] args )
        {
            AST.RecomputeBounds ( );
            return null;
        }
    }
}
