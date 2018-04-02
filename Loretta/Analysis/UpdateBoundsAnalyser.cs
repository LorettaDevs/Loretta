using System;
using Loretta.Parsing.Nodes;

namespace Loretta.Analysis
{
    // Updates the bounds of the nodes so
    // that they map to the correct places
    // in the source code (recommended)
    public class UpdateBoundsAnalyser : BaseASTAnalyser
    {
        public UpdateBoundsAnalyser ( ) : base ( )
        {
        }

        public override Object[] Analyse ( ASTNode AST, params Object[] args )
        {
            AST.RecomputeBounds ( );
            return null;
        }
    }
}
