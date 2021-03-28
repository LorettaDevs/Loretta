// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;

namespace Loretta.Test.Utilities
{
    public static class TokenUtilities
    {
        public static void AssertTokensEqual(
            string expected, string actual)
        {
            var expectedTokens = GetTokens(expected);
            var actualTokens = GetTokens(actual);
            var max = Math.Min(expectedTokens.Count, actualTokens.Count);
            for (var i = 0; i < max; i++)
            {
                var expectedToken = expectedTokens[i].ToString();
                var actualToken = actualTokens[i].ToString();
                if (!string.Equals(expectedToken, actualToken))
                {
                    string actualAll = "";
                    string expectedAll = "";
                    for (var j = i - 3; j <= i + 5; j++)
                    {
                        if (j >= 0 && j < max)
                        {
                            if (j == i)
                            {
                                actualAll += "^" + actualTokens[j].ToString() + "^ ";
                                expectedAll += "^" + expectedTokens[j].ToString() + "^ ";
                            }
                            else
                            {
                                actualAll += actualTokens[j].ToString() + " ";
                                expectedAll += expectedTokens[j].ToString() + " ";
                            }
                        }
                    }

                    AssertEx.Fail($"Unexpected token.  Actual '{actualAll}' Expected '{expectedAll}'\r\nActual:\r\n{actual}");
                }
            }

            if (expectedTokens.Count != actualTokens.Count)
            {
                var expectedDisplay = string.Join(" ", expectedTokens.Select(t => t.ToString()));
                var actualDisplay = string.Join(" ", actualTokens.Select(t => t.ToString()));
                AssertEx.Fail(@"Wrong token count. Expected '{0}', Actual '{1}', Expected Text: '{2}', Actual Text: '{3}'",
                    expectedTokens.Count, actualTokens.Count, expectedDisplay, actualDisplay);
            }
        }

        public static IList<SyntaxToken> GetTokens(string text)
        {
            return SyntaxFactory.ParseTokens(text).ToList();
        }

        public static IList<SyntaxToken> GetTokens(SyntaxNode node)
        {
            return node.DescendantTokens().ToList();
        }

        internal static SyntaxNode GetSyntaxRoot(string expectedText, LuaParseOptions options = null)
        {
            return SyntaxFactory.ParseCompilationUnit(expectedText, options: options);
        }
    }
}
