// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Text;
using Xunit;

namespace Loretta.Test.Utilities
{
    /// <summary>
    /// Assert style type to deal with the lack of features in xUnit's Assert type
    /// </summary>
    public static class AssertEx
    {
        public static void Equal(bool[,] expected, Func<int, int, bool> getResult, int size)
        {
            Equal<bool>(expected, getResult, (b1, b2) => b1 == b2, b => b ? "true" : "false", "{0,-6:G}", size);
        }

        public static void Equal<T>(T[,] expected, Func<int, int, T> getResult, Func<T, T, bool> valuesEqual, Func<T, string> printValue, string format, int size)
        {
            bool mismatch = false;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (!valuesEqual(expected[i, j], getResult(i, j)))
                    {
                        mismatch = true;
                    }
                }
            }

            if (mismatch)
            {
                var builder = new StringBuilder();
                builder.AppendLine("Actual result: ");
                for (int i = 0; i < size; i++)
                {
                    builder.Append("{ ");
                    for (int j = 0; j < size; j++)
                    {
                        string resultWithComma = printValue(getResult(i, j));
                        if (j < size - 1)
                        {
                            resultWithComma += ",";
                        }

                        builder.Append(string.Format(format, resultWithComma));
                        if (j < size - 1)
                        {
                            builder.Append(' ');
                        }
                    }
                    builder.AppendLine("},");
                }

                Assert.True(false, builder.ToString());
            }
        }
    }
}
