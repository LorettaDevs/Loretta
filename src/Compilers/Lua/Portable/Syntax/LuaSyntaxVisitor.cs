namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// Represents a <see cref="LuaSyntaxNode"/> visitor that visits only the single
    /// <see cref="LuaSyntaxNode"/> passed into its Visit method and produces a value
    /// of the type specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="TResult">
    /// The type of the return value this visitor's Visit method.
    /// </typeparam>
    public abstract partial class LuaSyntaxVisitor<TResult>
    {
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public virtual TResult? Visit(SyntaxNode? node)
        {
            if (node is not null)
            {
                return ((LuaSyntaxNode) node).Accept(this);
            }

            // should not come here too often so we will put this at the end of the method.
            return default;
        }

        public virtual TResult? DefaultVisit(SyntaxNode node) => default;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore IDE0079 // Remove unnecessary suppression
    }

    /// <summary>
    /// Represents a <see cref="LuaSyntaxNode"/> visitor that visits only the single LuaSyntaxNode
    /// passed into its Visit method.
    /// </summary>
    public abstract partial class LuaSyntaxVisitor
    {
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public virtual void Visit(SyntaxNode? node)
        {
            if (node != null)
            {
                ((LuaSyntaxNode) node).Accept(this);
            }
        }

        public virtual void DefaultVisit(SyntaxNode node)
        {
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore IDE0079 // Remove unnecessary suppression
    }
}
