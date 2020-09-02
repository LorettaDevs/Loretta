using System;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// The base class for an expression node.
    /// </summary>
    public abstract class Expression : LuaASTNode
    {
        /// <summary>
        /// Whether this expression has a constant value.
        /// </summary>
        public abstract Boolean IsConstant { get; }

        /// <summary>
        /// This expression's constant value.
        /// </summary>
        public abstract Object? ConstantValue { get; }
    }
}