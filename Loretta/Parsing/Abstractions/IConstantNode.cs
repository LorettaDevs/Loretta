using System;

namespace Loretta.Parsing
{
    public interface IConstantNode<T>
    {
        /// <summary>
        /// Raw form of the node value
        /// </summary>
        String Raw { get; }

        /// <summary>
        /// The node value
        /// </summary>
        T Value { get; }
    }
}
