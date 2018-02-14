using System;
using Loretta.Env;
using Loretta.Parsing.Nodes.ControlStatements;

namespace Loretta.Analysis
{
    public class GotoTargetCheckAnalyser : BaseASTAnalyser
    {
        protected GotoTargetCheckAnalyser ( LuaEnvironment env, EnvFile file ) : base ( env, file )
        {
        }

        protected override Object[] GotoStatement ( GotoStatement node, params Object[] args )
        {
            if ( node.Label.Node == null )
                this.File.Errors.Add ( new Error ( ErrorType.Error, node, $"Label {node.Label.Name} has no target node." ) );
            return null;
        }
    }
}
