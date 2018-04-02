using System;
using System.Collections.Generic;
using System.Text;

namespace Loretta.Parsing.Nodes.Functions
{
    public interface IFunctionDefinition
    {
        List<ASTNode> Arguments { get; }

        StatementList Body { get; }
    }
}
