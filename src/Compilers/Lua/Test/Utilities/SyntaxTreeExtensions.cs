// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.UnitTests
{
    public static class SyntaxTreeExtensions
    {
        public static SyntaxTree WithReplace(this SyntaxTree syntaxTree, int offset, int length, string newText)
        {
            var oldFullText = syntaxTree.GetText();
            var newFullText = oldFullText.WithChanges(new TextChange(new TextSpan(offset, length), newText));
            return syntaxTree.WithChangedText(newFullText);
        }

        public static SyntaxTree WithReplaceFirst(this SyntaxTree syntaxTree, string oldText, string newText)
        {
            var oldFullText = syntaxTree.GetText().ToString();
            int offset = oldFullText.IndexOf(oldText, StringComparison.Ordinal);
            int length = oldText.Length;
            return WithReplace(syntaxTree, offset, length, newText);
        }

        public static SyntaxTree WithReplace(this SyntaxTree syntaxTree, int startIndex, string oldText, string newText)
        {
            var oldFullText = syntaxTree.GetText().ToString();
            int offset = oldFullText.IndexOf(oldText, startIndex, StringComparison.Ordinal); // Use an offset to find the first element to replace at
            int length = oldText.Length;
            return WithReplace(syntaxTree, offset, length, newText);
        }

        public static SyntaxTree WithInsertAt(this SyntaxTree syntaxTree, int offset, string newText) =>
            WithReplace(syntaxTree, offset, 0, newText);

        public static SyntaxTree WithInsertBefore(this SyntaxTree syntaxTree, string existingText, string newText)
        {
            var oldFullText = syntaxTree.GetText().ToString();
            int offset = oldFullText.IndexOf(existingText, StringComparison.Ordinal);
            return WithReplace(syntaxTree, offset, 0, newText);
        }

        public static SyntaxTree WithRemoveAt(this SyntaxTree syntaxTree, int offset, int length) =>
            WithReplace(syntaxTree, offset, length, string.Empty);

        public static SyntaxTree WithRemoveFirst(this SyntaxTree syntaxTree, string oldText) =>
            WithReplaceFirst(syntaxTree, oldText, string.Empty);

        internal static string Dump(this SyntaxNode node)
        {
            var visitor = new LuaSyntaxPrinter();
            visitor.Visit(node);
            return visitor.Dump();
        }

        internal static string Dump(this SyntaxTree tree) => tree.GetRoot().Dump();

        private class LuaSyntaxPrinter : LuaSyntaxWalker
        {
            private readonly PooledStringBuilder _builder;
            private int _indent = 0;

            internal LuaSyntaxPrinter()
            {
                _builder = PooledStringBuilder.GetInstance();
            }

            internal string Dump() => _builder.ToStringAndFree();

            public override void DefaultVisit(SyntaxNode node)
            {
                _builder.Builder.Append(' ', repeatCount: _indent);
                _builder.Builder.Append(node.Kind().ToString());
                if (node.IsMissing)
                {
                    _builder.Builder.Append(" (missing)");
                }
                else if (node is IdentifierNameSyntax name)
                {
                    _builder.Builder.Append(' ');
                    _builder.Builder.Append(name.ToString());
                }
                _builder.Builder.AppendLine();

                _indent += 2;
                base.DefaultVisit(node);
                _indent -= 2;
            }
        }
    }
}
