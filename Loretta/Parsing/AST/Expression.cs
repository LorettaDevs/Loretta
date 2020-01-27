using System;

namespace Loretta.Parsing.AST
{
    public abstract class Expression : LuaASTNode
    {
        public abstract Boolean IsConstant { get; }
        public abstract Object ConstantValue { get; }
    }
}
