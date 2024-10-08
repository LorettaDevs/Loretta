﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis
{
    internal static class SyntaxTreeExtensions
    {
        /// <summary>
        /// Verify nodes match source.
        /// </summary>
        [Conditional("DEBUG")]
        internal static void VerifySource(this SyntaxTree tree, IEnumerable<TextChangeRange>? changes = null)
        {
            var root = tree.GetRoot();
            var text = tree.GetText();
            var fullSpan = new TextSpan(0, text.Length);
            SyntaxNode? node = null;

            // If only a subset of the document has changed,
            // just check that subset to reduce verification cost.
            if (changes != null)
            {
                var change = TextChangeRange.Collapse(changes).Span;
                if (change != fullSpan)
                {
                    // Find the lowest node in the tree that contains the changed region.
                    node = root.DescendantNodes(n => n.FullSpan.Contains(change)).LastOrDefault();
                }
            }

            node ??= root;

            var span = node.FullSpan;
            var textSpanOpt = span.Intersection(fullSpan);
            int index;
            char found = default;
            char expected = default;
            if (textSpanOpt == null)
            {
                index = 0;
            }
            else
            {
                var fromText = text.ToString(textSpanOpt.Value);
                var fromNode = node.ToFullString();
                index = FindFirstDifference(fromText, fromNode);
                if (index >= 0)
                {
                    found = fromNode[index];
                    expected = fromText[index];
                }
            }

            if (index >= 0)
            {
                index += span.Start;
                string message;
                if (index < text.Length)
                {
                    var position = text.Lines.GetLinePosition(index);
                    var line = text.Lines[position.Line];
                    var allText = text.ToString(); // Entire document as string to allow inspecting the text in the debugger.
                    message = $"Unexpected difference at offset {index}: Line {position.Line + 1}, Column {position.Character + 1} \"{line}\"  (Found: [{found}] Expected: [{expected}])";
                }
                else
                {
                    message = "Unexpected difference past end of the file";
                }
                LorettaDebug.Assert(false, message);
            }
        }

        /// <summary>
        /// Return the index of the first difference between
        /// the two strings, or -1 if the strings are the same.
        /// </summary>
        private static int FindFirstDifference(string s1, string s2)
        {
            var n1 = s1.Length;
            var n2 = s2.Length;
            var n = Math.Min(n1, n2);
            for (int i = 0; i < n; i++)
            {
                if (s1[i] != s2[i])
                {
                    return i;
                }
            }
            return (n1 == n2) ? -1 : n + 1;
        }
    }
}
