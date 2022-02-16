// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// A program location in source code.
    /// </summary>
    [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
    public abstract class Location
    {
        internal Location()
        {
        }

        /// <summary>
        /// Location kind (None/SourceFile/MetadataFile).
        /// </summary>
        public abstract LocationKind Kind { get; }

        /// <summary>
        /// Returns true if the location represents a specific location in a source code file.
        /// </summary>
        [MemberNotNullWhen(true, nameof(SourceTree))]
        public bool IsInSource => SourceTree != null;

        /// <summary>
        /// The syntax tree this location is located in or <c>null</c> if not in a syntax tree.
        /// </summary>
        public virtual SyntaxTree? SourceTree => null;

        /// <summary>
        /// The location within the syntax tree that this location is associated with.
        /// </summary>
        /// <remarks>
        /// If <see cref="IsInSource"/> returns False this method returns an empty <see cref="TextSpan"/> which starts at position 0.
        /// </remarks>
        public virtual TextSpan SourceSpan => default;

        /// <summary>
        /// Gets the location in terms of path, line and column.
        /// </summary>
        /// <returns>
        /// <see cref="FileLinePositionSpan"/> that contains path, line and column information.
        /// 
        /// Returns an invalid span (see <see cref="FileLinePositionSpan.IsValid"/>) if the information is not available.
        /// 
        /// The values are not affected by line mapping directives (#line in C# or #ExternalSource in VB).
        /// </returns>
        public virtual FileLinePositionSpan GetLineSpan() => default;

        // Derived classes should provide value equality semantics.
        /// <inheritdoc/>
        public abstract override bool Equals(object? obj);
        /// <inheritdoc/>
        public abstract override int GetHashCode();

        /// <inheritdoc/>
        public override string ToString()
        {
            var result = Kind.ToString();
            if (IsInSource)
            {
                result += "(" + SourceTree?.FilePath + SourceSpan + ")";
            }
            else
            {
                var pos = GetLineSpan();
                if (pos.Path != null)
                {
                    // user-visible line and column counts are 1-based, but internally are 0-based.
                    result += "(" + pos.Path + "@" + (pos.StartLinePosition.Line + 1) + ":" + (pos.StartLinePosition.Character + 1) + ")";
                }
            }

            return result;
        }

        /// <summary>
        /// Checks whether two locations are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(Location? left, Location? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Checks whether two locations are not equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(Location? left, Location? right) => !(left == right);

        /// <summary>
        /// Returns the display string for the debugger.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetDebuggerDisplay()
        {
            var result = GetType().Name;
            var pos = GetLineSpan();
            if (pos.Path != null)
            {
                // user-visible line and column counts are 1-based, but internally are 0-based.
                result += "(" + pos.Path + "@" + (pos.StartLinePosition.Line + 1) + ":" + (pos.StartLinePosition.Character + 1) + ")";
            }

            return result;
        }

        /// <summary>
        /// A location of kind LocationKind.None. 
        /// </summary>
        public static Location None => NoLocation.Singleton;

        /// <summary>
        /// Creates an instance of a <see cref="Location"/> for a span in a <see cref="SyntaxTree"/>.
        /// </summary>
        public static Location Create(SyntaxTree syntaxTree!!, TextSpan textSpan) =>
            new SourceLocation(syntaxTree, textSpan);

        /// <summary>
        /// Creates an instance of a <see cref="Location"/> for a span in a file.
        /// </summary>
        public static Location Create(string filePath!!, TextSpan textSpan, LinePositionSpan lineSpan) =>
            new ExternalFileLocation(filePath, textSpan, lineSpan);
    }
}
