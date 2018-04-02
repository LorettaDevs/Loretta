using System;
using Loretta.Env;
using Loretta.Parsing.Nodes.ControlStatements;

namespace Loretta.Analysis
{
    public class GotoTargetCheckAnalyser : BaseASTAnalyser
    {
        public GotoTargetCheckAnalyser ( ) : base ( )
        {
        }

        protected override Object[] AnalyseGotoStatement ( GotoStatement node, params Object[] args )
        {
            if ( node.Label.Node == null )
                this.File.Errors.Add ( new Error ( ErrorType.Error, node, $"Label {node.Label.Name} has no target node." ) );
            return null;
        }
    }
}
