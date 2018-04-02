using System.Collections.Generic;
using Loretta.Parsing.Nodes;

namespace Loretta.Parsing.Abstractions
{
    public interface IAssignmentNode
    {
        List<ASTNode> Variables { get; }

        List<ASTNode> Assignments { get; }
    }
}
